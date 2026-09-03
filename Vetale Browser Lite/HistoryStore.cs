using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Vetale_Browser_Lite.Data;

namespace Vetale_Browser_Lite
{
    /// <summary>DB #1 — browsing history (encrypted sharded LiteDB).</summary>
    public static class HistoryStore
    {
        private const string Collection = "visits";
        private static readonly ShardedEncryptedDb Db = new("history");

        public static ObservableCollection<HistoryEntry> Items { get; } = new();

        static HistoryStore()
        {
            Reload();
        }

        public static void Reload()
        {
            var all = Db.QueryAll<HistoryEntry>(Collection)
                .OrderByDescending(h => h.VisitedAt)
                .ToList();
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Items.Clear();
                foreach (var h in all)
                    Items.Add(h);
            });
        }

        public static void Add(string url, string title)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return;
            if (url.StartsWith("about:", StringComparison.OrdinalIgnoreCase))
                return;

            HistoryEntry dupe = null;
            Application.Current?.Dispatcher.Invoke(() =>
            {
                dupe = Items.FirstOrDefault(); // newest is first
            });

            // Don't spam duplicates from reloads / same-document navigations
            if (dupe != null && string.Equals(dupe.Url, url, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(title) && dupe.Title != title)
                {
                    dupe.Title = title;
                }
                return;
            }

            var entry = new HistoryEntry { VisitedAt = DateTime.Now, Url = url, Title = title ?? string.Empty, Favicon = FaviconService.GetCached(url) };
            try { Db.Insert(Collection, entry); } catch { return; }
            Application.Current?.Dispatcher.Invoke(() => Items.Insert(0, entry));
        }

        public static void SetFavicon(string url, byte[] favicon)
        {
            if (favicon == null || favicon.Length == 0)
                return;
            HistoryEntry target = null;
            Application.Current?.Dispatcher.Invoke(() =>
            {
                foreach (var h in Items)
                {
                    if (string.Equals(h.Url, url, StringComparison.OrdinalIgnoreCase))
                    {
                        target = h;
                        break;
                    }
                }
            });
            if (target == null)
                return;
            target.Favicon = favicon;
            try { Db.Update(Collection, target); } catch { }
        }

        public static void Clear()
        {
            try { Db.Clear(Collection); } catch { }
            Application.Current?.Dispatcher.Invoke(() => Items.Clear());
        }
    }
}
