using LiteDB;

namespace Vetale_Browser_Lite
{
    /// <summary>DB #4 — app settings row (encrypted sharded LiteDB).</summary>
    public sealed class SettingEntry
    {
        [BsonId]
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
