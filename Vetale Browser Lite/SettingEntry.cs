namespace Vetale_Browser_Lite
{
    /// <summary>DB #4 — app settings row (encrypted SQLite).</summary>
    public sealed class SettingEntry
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
