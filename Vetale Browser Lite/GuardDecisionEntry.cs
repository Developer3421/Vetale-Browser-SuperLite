using System;

namespace Vetale_Browser_Lite
{
    /// <summary>DB #3 — redirect-guard decisions log (encrypted SQLite).</summary>
    public sealed class GuardDecisionEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime At { get; set; }
        public string FromHost { get; set; } = string.Empty;
        public string ToHost { get; set; } = string.Empty;
        public bool Allowed { get; set; }
    }
}
