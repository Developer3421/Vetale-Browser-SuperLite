using System.Collections.Generic;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Handler;

namespace Vetale_Browser_Lite
{
    /// <summary>
    /// Receives favicon URLs from Chromium and stores the downloaded icon
    /// bytes on the matching history entry.
    /// </summary>
    public sealed class VetaleDisplayHandler : DisplayHandler
    {
        protected override void OnFaviconUrlChange(IWebBrowser chromiumWebBrowser, IBrowser browser, IList<string> urls)
        {
            // NOTE: runs on the CEF UI thread — never touch WPF dependency
            // properties here (e.g. ChromiumWebBrowser.Address throws).
            // IBrowser/IFrame are safe to read from any thread.
            var pageUrl = browser?.MainFrame?.Url;
            if (string.IsNullOrWhiteSpace(pageUrl))
                return;
            Task.Run(async () =>
            {
                var bytes = await FaviconService.GetFaviconBytesAsync(pageUrl, urls).ConfigureAwait(false);
                if (bytes != null)
                    HistoryStore.SetFavicon(pageUrl, bytes);
            });
        }
    }
}
