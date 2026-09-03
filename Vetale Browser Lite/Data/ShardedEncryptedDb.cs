using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;

namespace Vetale_Browser_Lite.Data
{
    /// <summary>
    /// Encrypted LiteDB with infinite sharding: each shard file is capped at
    /// 500 MB. When the newest shard reaches the limit, a new one is created
    /// (name_0001.db, name_0002.db, ...). Reads always span ALL shards, so old
    /// data stays readable forever. Files are AES-encrypted with the
    /// user-level key (see SecureKeyProvider) — no key file exists.
    /// </summary>
    public sealed class ShardedEncryptedDb
    {
        public const long ShardLimitBytes = 500L * 1024 * 1024; // 500 MB

        private readonly string _dir;
        private readonly string _name;
        private readonly object _gate = new();

        public ShardedEncryptedDb(string name)
        {
            _name = name;
            _dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Vetale Browser SuperLite", "db");
            Directory.CreateDirectory(_dir);
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

        private LiteDatabase Open(int index) =>
            new($"Filename={ShardPath(index)};Password={SecureKeyProvider.Password}");

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

        public void Insert<T>(string collection, T doc)
        {
            lock (_gate)
            {
                using var db = Open(WritableShard());
                db.GetCollection<T>(collection).Insert(doc);
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
                    using var db = Open(shards[i]);
                    result.AddRange(db.GetCollection<T>(collection).FindAll());
                }
                catch { /* skip unreadable shard, keep others readable */ }
            }
            return result;
        }

        public bool Any<T>(string collection, Func<T, bool> predicate)
        {
            int[] shards;
            lock (_gate) { shards = ExistingShards(); }
            for (int i = shards.Length - 1; i >= 0; i--)
            {
                try
                {
                    using var db = Open(shards[i]);
                    if (db.GetCollection<T>(collection).FindAll().Any(predicate))
                        return true;
                }
                catch { }
            }
            return false;
        }

        public bool Delete<T>(string collection, BsonValue id)
        {
            int[] shards;
            lock (_gate) { shards = ExistingShards(); }
            foreach (var s in shards)
            {
                try
                {
                    using var db = Open(s);
                    if (db.GetCollection<T>(collection).Delete(id))
                        return true;
                }
                catch { }
            }
            return false;
        }

        /// <summary>Updates a document by Id in whichever shard holds it.</summary>
        public bool Update<T>(string collection, T doc)
        {
            int[] shards;
            lock (_gate) { shards = ExistingShards(); }
            foreach (var s in shards)
            {
                try
                {
                    using var db = Open(s);
                    if (db.GetCollection<T>(collection).Update(doc))
                        return true;
                }
                catch { }
            }
            return false;
        }

        /// <summary>Drops the collection in every shard.</summary>
        public void Clear(string collection)
        {
            lock (_gate)
            {
                foreach (var s in ExistingShards())
                {
                    try
                    {
                        using var db = Open(s);
                        db.DropCollection(collection);
                    }
                    catch { }
                }
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
