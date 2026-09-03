using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using CefSharp;
using CefSharp.Handler;

namespace Vetale_Browser_Lite
{
    /// <summary>
    /// Blocks silent cross-site redirects (open redirects / tab-napping style):
    /// a main-frame navigation to a DIFFERENT host that was NOT started by the
    /// user (no user gesture, is a redirect) requires confirmation.
    /// Navigations typed/clicked in the UI call MainWindow.MarkUserNavigation
    /// so they are always allowed.
    /// </summary>
    public sealed class RedirectGuardRequestHandler : RequestHandler
    {
        private string _userAllowedHost = string.Empty;
        private DateTime _userAllowedAt = DateTime.MinValue;
        private static readonly TimeSpan UserAllowance = TimeSpan.FromSeconds(15);

        public void MarkUserNavigation(string url)
        {
            try { _userAllowedHost = new Uri(url).Host.ToLowerInvariant(); }
            catch { _userAllowedHost = string.Empty; }
            _userAllowedAt = DateTime.UtcNow;
        }

        protected override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            try
            {
                if (!frame.IsMain)
                    return false;

                var url = request.Url ?? string.Empty;
                if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    return false;

                var host = new Uri(url).Host.ToLowerInvariant();

                // Explicit user navigation (address bar / history double-click): allow.
                if (!string.IsNullOrEmpty(_userAllowedHost) &&
                    (DateTime.UtcNow - _userAllowedAt) < UserAllowance &&
                    string.Equals(host, _userAllowedHost, StringComparison.OrdinalIgnoreCase))
                    return false;

                // Direct user click on a link: allow.
                if (userGesture && !isRedirect)
                    return false;

                // Silent redirect to another host: ask the user.
                if (isRedirect && !userGesture)
                {
                    var current = browser.MainFrame?.Url ?? string.Empty;
                    string currentHost = string.Empty;
                    try { currentHost = new Uri(current).Host.ToLowerInvariant(); } catch { }

                    if (!string.IsNullOrEmpty(currentHost) &&
                        !string.Equals(host, currentHost, StringComparison.OrdinalIgnoreCase))
                    {
                        bool allow = false;
                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            var answer = MessageBox.Show(
                                string.Format(LocalizationManager.Get("Guard_Text"), currentHost, host),
                                LocalizationManager.Get("Guard_Title"),
                                MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            allow = answer == MessageBoxResult.Yes;
                        });
                        GuardDecisionStore.Log(currentHost, host, allow);
                        if (!allow)
                            return true; // cancel navigation
                        MarkUserNavigation(url);
                    }
                }
            }
            catch { /* fail-open: never break browsing on guard errors */ }
            return false;
        }

        protected override bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture) => false;
    }
}
