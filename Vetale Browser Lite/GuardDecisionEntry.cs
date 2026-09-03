using System;
using LiteDB;

namespace Vetale_Browser_Lite
{
    /// <summary>DB #3 — redirect-guard decisions log (encrypted sharded LiteDB).</summary>
    public sealed class GuardDecisionEntry
    {
        public ObjectId Id { get; set; } = ObjectId.NewObjectId();
        public DateTime At { get; set; }
        public string FromHost { get; set; } = string.Empty;
        public string ToHost { get; set; } = string.Empty;
        public bool Allowed { get; set; }
    }
}
