using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Vetale_Browser_Lite
{
    /// <summary>
    /// Downloads page favicons (Chromium reports their URLs via
    /// DisplayHandler.OnFaviconUrlChange, with /favicon.ico fallback)
    /// and caches them per host in memory.
    /// </summary>
    public static class FaviconService
    {
        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };
        private static readonly ConcurrentDictionary<string, byte[]> Cache = new(StringComparer.OrdinalIgnoreCase);
        private const int MaxBytes = 256 * 1024;

        public static byte[] GetCached(string pageUrl)
        {
            try
            {
                var host = new Uri(pageUrl).Host;
                return Cache.TryGetValue(host, out var b) ? b : null;
            }
            catch { return null; }
        }

        public static async Task<byte[]> GetFaviconBytesAsync(string pageUrl, System.Collections.Generic.IList<string> faviconUrls)
        {
            string host;
            try { host = new Uri(pageUrl).Host; }
            catch { return null; }

            if (Cache.TryGetValue(host, out var cached))
                return cached;

            var candidates = (faviconUrls ?? Enumerable.Empty<string>())
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Take(3)
                .ToList();
            try
            {
                var fallback = new Uri(new Uri(pageUrl), "/favicon.ico").AbsoluteUri;
                if (!candidates.Contains(fallback))
                    candidates.Add(fallback);
            }
            catch { }

            foreach (var url in candidates)
            {
                try
                {
                    using var resp = await Http.GetAsync(url).ConfigureAwait(false);
                    if (!resp.IsSuccessStatusCode)
                        continue;
                    var bytes = await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    if (bytes != null && bytes.Length > 0 && bytes.Length <= MaxBytes && LooksLikeImage(bytes))
                    {
                        // Don't stop at the first hit (often a tiny 16x16):
                        // take the largest file — it is usually the highest resolution.
                        if (!Cache.TryGetValue(host, out var best) || bytes.Length > best.Length)
                            Cache[host] = bytes;
                    }
                }
                catch { /* try next candidate */ }
            }
            Cache.TryGetValue(host, out var result);
            return result;
        }

        private static bool LooksLikeImage(byte[] b)
        {
            if (b.Length < 4) return false;
            // PNG, ICO, JPEG, GIF, BMP, WEBP, SVG
            if (b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47) return true;
            if (b[0] == 0x00 && b[1] == 0x00 && b[2] == 0x01 && b[3] == 0x00) return true;
            if (b[0] == 0xFF && b[1] == 0xD8) return true;
            if (b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46) return true;
            if (b[0] == 0x42 && b[1] == 0x4D) return true;
            if (b.Length > 12 && b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46) return true;
            var head = System.Text.Encoding.ASCII.GetString(b, 0, Math.Min(b.Length, 200)).TrimStart();
            if (head.StartsWith("<svg", StringComparison.OrdinalIgnoreCase) || head.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
