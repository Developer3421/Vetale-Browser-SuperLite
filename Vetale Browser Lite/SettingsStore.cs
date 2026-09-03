using System.Linq;
using Vetale_Browser_Lite.Data;

namespace Vetale_Browser_Lite
{
    /// <summary>DB #4 — persistent app settings (language, etc.).</summary>
    public static class SettingsStore
    {
        private const string Collection = "settings";
        private static readonly ShardedEncryptedDb Db = new("settings");

        public static string Get(string key, string defaultValue = "")
        {
            try
            {
                var all = Db.QueryAll<SettingEntry>(Collection);
                var row = all.FirstOrDefault(s => s.Key == key);
                return row != null ? row.Value ?? defaultValue : defaultValue;
            }
            catch { return defaultValue; }
        }

        public static void Set(string key, string value)
        {
            try
            {
                var row = new SettingEntry { Key = key, Value = value ?? string.Empty };
                if (!Db.Update(Collection, row))
                    Db.Insert(Collection, row);
            }
            catch { /* never break the app on settings errors */ }
        }
    }
}
