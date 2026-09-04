using System;
using Vetale_Browser_Lite.Data;

namespace Vetale_Browser_Lite
{
    /// <summary>DB #3 — log of user allow/deny answers to redirect prompts.</summary>
    public static class GuardDecisionStore
    {
        private const string Collection = "decisions";
        private static readonly SqliteDb Db = new("guard");

        public static void Log(string fromHost, string toHost, bool allowed)
        {
            try
            {
                Db.Insert(Collection, new GuardDecisionEntry
                {
                    At = DateTime.Now,
                    FromHost = fromHost ?? string.Empty,
                    ToHost = toHost ?? string.Empty,
                    Allowed = allowed
                });
            }
            catch { /* never break browsing on logging errors */ }
        }
    }
}
