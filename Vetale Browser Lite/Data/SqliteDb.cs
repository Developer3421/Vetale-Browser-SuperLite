using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace Vetale_Browser_Lite.Data
{
    /// <summary>
    /// Sharded SQLite document store with infinite sharding: each shard file
    /// is capped at 500 MB. When the newest shard reaches the limit, a new
    /// one is created (name_0001.db, name_0002.db, ...). Reads always span
    /// ALL shards (newest first), so old data stays readable forever.
    /// Each document is stored as JSON encrypted with the user-level DPAPI
    /// key (see DpapiBox) — the .db files contain only ciphertext and NO key
    /// file exists anywhere. Files live in %LocalAppData%\Vetale Browser
    /// SuperLite\db (user level).
    /// </summary>
    public sealed class SqliteDb
    {
        public const long ShardLimitBytes = 500L * 1024 * 1024; // 500 MB

        private readonly string _dir;
        private readonly string _name;
        private readonly object _gate = new();

        public SqliteDb(string name)
        {
            _name = name;
            _dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Vetale Browser SuperLite", "db");
            Directory.CreateDirectory(_dir);
            AdoptUnshardedFile();
        }

        /// <summary>One-time: move a pre-sharding name.db to name_0001.db.</summary>
        private void AdoptUnshardedFile()
        {
            try
            {
                var plain = Path.Combine(_dir, _name + ".db");
                if (File.Exists(plain) && ExistingShards().Length == 0)
                    File.Move(plain, ShardPath(1));
            }
            catch { }
        }

        private string ShardPath(int index) => Path.Combine(_dir, $"{_name}_{index:D4}.db");

        private int[] ExistingShards()
        {
            try
            {
                return Directory.GetFiles(_dir, $"{_name}_*.db")
                    .Select(f =>
                    {
                        var stem = Path.GetFileNameWithoutExtension(f);
                        return int.TryParse(stem.Substring(_name.Length + 1), out var i) ? i : -1;
                    })
                    .Where(i => i >= 0)
                    .OrderBy(i => i)
                    .ToArray();
            }
            catch { return Array.Empty<int>(); }
        }

        private SqliteConnection Open(int index)
        {
            var conn = new SqliteConnection($"Data Source={ShardPath(index)}");
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS docs " +
                    "(collection TEXT NOT NULL, id TEXT NOT NULL, payload TEXT NOT NULL, " +
                    "PRIMARY KEY (collection, id))";
                cmd.ExecuteNonQuery();
            }
            return conn;
        }

        private int WritableShard()
        {
            var shards = ExistingShards();
            if (shards.Length == 0)
                return 1;
            var latest = shards[^1];
            try
            {
                if (new FileInfo(ShardPath(latest)).Length >= ShardLimitBytes)
                    return latest + 1;
            }
            catch { /* treat as writable */ }
            return latest;
        }

        /// <summary>Document id: "Id" property, else "Key" (settings rows).</summary>
        private static string GetId<T>(T doc)
        {
            var t = typeof(T);
            var idProp = t.GetProperty("Id") ?? t.GetProperty("Key");
            var id = idProp?.GetValue(doc) as string;
            if (!string.IsNullOrEmpty(id))
                return id;
            // Assign a fresh id back to the document when possible.
            id = Guid.NewGuid().ToString("N");
            try { t.GetProperty("Id")?.SetValue(doc, id); } catch { }
            return id;
        }

        private static string Encode<T>(T doc) =>
            DpapiBox.Protect(JsonSerializer.Serialize(doc));

        private static T Decode<T>(string payload) =>
            JsonSerializer.Deserialize<T>(DpapiBox.Unprotect(payload));

        public void Insert<T>(string collection, T doc)
        {
            lock (_gate)
            {
                using var conn = Open(WritableShard());
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO docs (collection, id, payload) VALUES ($c, $id, $p)";
                cmd.Parameters.AddWithValue("$c", collection);
                cmd.Parameters.AddWithValue("$id", GetId(doc));
                cmd.Parameters.AddWithValue("$p", Encode(doc));
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Reads newest shard first so callers can Take() recent items cheaply.</summary>
        public List<T> QueryAll<T>(string collection)
        {
            var result = new List<T>();
            int[] shards;
            lock (_gate) { shards = ExistingShards(); }
            for (int i = shards.Length - 1; i >= 0; i--)
            {
                try
                {
                    using var conn = Open(shards[i]);
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT payload FROM docs WHERE collection = $c ORDER BY rowid";
                    cmd.Parameters.AddWithValue("$c", collection);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        try
                        {
                            var doc = Decode<T>(reader.GetString(0));
                            if (doc != null)
                                result.Add(doc);
                        }
                        catch { /* skip unreadable row, keep others readable */ }
                    }
                }
                catch { /* skip unreadable shard, keep others readable */ }
            }
            return result;
        }

        public bool Any<T>(string collection, Func<T, bool> predicate)
        {
            try { return QueryAll<T>(collection).Any(predicate); }
            catch { return false; }
        }

        public bool Delete(string collection, string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;
            int[] shards;
            lock (_gate) { shards = ExistingShards(); }
            foreach (var s in shards)
            {
                try
                {
                    using var conn = Open(s);
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM docs WHERE collection = $c AND id = $id";
                    cmd.Parameters.AddWithValue("$c", collection);
                    cmd.Parameters.AddWithValue("$id", id);
                    if (cmd.ExecuteNonQuery() > 0)
                        return true;
                }
                catch { }
            }
            return false;
        }

        /// <summary>Replaces the document payload by id in whichever shard holds it.</summary>
        public bool Update<T>(string collection, T doc)
        {
            int[] shards;
            lock (_gate) { shards = ExistingShards(); }
            foreach (var s in shards)
            {
                try
                {
                    using var conn = Open(s);
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE docs SET payload = $p WHERE collection = $c AND id = $id";
                    cmd.Parameters.AddWithValue("$p", Encode(doc));
                    cmd.Parameters.AddWithValue("$c", collection);
                    cmd.Parameters.AddWithValue("$id", GetId(doc));
                    if (cmd.ExecuteNonQuery() > 0)
                        return true;
                }
                catch { }
            }
            return false;
        }

        /// <summary>Drops the collection in every shard.</summary>
        public void Clear(string collection)
        {
            int[] shards;
            lock (_gate) { shards = ExistingShards(); }
            foreach (var s in shards)
            {
                try
                {
                    using var conn = Open(s);
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM docs WHERE collection = $c";
                    cmd.Parameters.AddWithValue("$c", collection);
                    cmd.ExecuteNonQuery();
                }
                catch { }
            }
        }

        public string[] Files
        {
            get
            {
                lock (_gate) { return ExistingShards().Select(ShardPath).ToArray(); }
            }
        }
    }
}
