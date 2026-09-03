using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Vetale_Browser_Lite.Data;
using System.Collections.ObjectModel;

namespace Vetale_Browser_Lite
{
    /// <summary>DB #2 — bookmarks (encrypted sharded LiteDB).</summary>
    public static class BookmarkStore
    {
        private const string Collection = "bookmarks";
        private static readonly ShardedEncryptedDb Db = new("bookmarks");

        private static readonly string LegacyJsonPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Vetale Browser Lite", "bookmarks.json");

        public static ObservableCollection<BookmarkEntry> Items { get; } = new();

        static BookmarkStore()
        {
            Load();
        }

        public static void Load()
        {
            MigrateLegacyJsonOnce();
            var all = Db.QueryAll<BookmarkEntry>(Collection)
                .Where(b => !string.IsNullOrWhiteSpace(b?.Url))
                .OrderByDescending(b => b.CreatedAt)
                .ToList();
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Items.Clear();
                foreach (var b in all)
                    Items.Add(b);
            });
        }

        /// <summary>One-time import of the old unencrypted bookmarks.json, then retires it.</summary>
        private static void MigrateLegacyJsonOnce()
        {
            try
            {
                if (!File.Exists(LegacyJsonPath))
                    return;
                var json = File.ReadAllText(LegacyJsonPath);
                var list = JsonSerializer.Deserialize<BookmarkEntry[]>(json);
                if (list != null)
                {
                    foreach (var b in list.Where(b => !string.IsNullOrWhiteSpace(b?.Url)))
                    {
                        b.Id = LiteDB.ObjectId.NewObjectId();
                        try { Db.Insert(Collection, b); } catch { }
                    }
                }
                File.Move(LegacyJsonPath, LegacyJsonPath + ".migrated.bak", overwrite: true);
            }
            catch { /* start clean on any migration error */ }
        }

        public static bool Add(string title, string url)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return false;
            url = url.Trim();
            try
            {
                if (Db.Any<BookmarkEntry>(Collection, b => string.Equals(b.Url, url, StringComparison.OrdinalIgnoreCase)))
                    return false; // already bookmarked
                var entry = new BookmarkEntry { Title = (title ?? string.Empty).Trim(), Url = url, CreatedAt = DateTime.Now };
                entry.Favicon = FaviconService.GetCached(url);
                Db.Insert(Collection, entry);
                Application.Current?.Dispatcher.Invoke(() => Items.Insert(0, entry));
                // Fetch the real site icon in the background (Chromium-independent fallback chain)
                System.Threading.Tasks.Task.Run(async () =>
                {
                    var bytes = await FaviconService.GetFaviconBytesAsync(url, null).ConfigureAwait(false);
                    if (bytes == null)
                        return;
                    entry.Favicon = bytes;
                    try { Db.Update(Collection, entry); } catch { }
                });
                return true;
            }
            catch { return false; }
        }

        public static void Remove(BookmarkEntry entry)
        {
            if (entry == null)
                return;
            try { Db.Delete<BookmarkEntry>(Collection, entry.Id); } catch { }
            Application.Current?.Dispatcher.Invoke(() => Items.Remove(entry));
        }
    }
}
