using System;
using System.Windows;
using System.Windows.Input;
using CefSharp;
using CefSharp.Wpf;

namespace Vetale_Browser_Lite
{
    public partial class MainWindow : Window
    {
        private ChromiumWebBrowser CefBrowser => (ChromiumWebBrowser)Browser;

        public MainWindow()
        {
            InitializeComponent();

            CefBrowser.LifeSpanHandler = new CustomLifeSpanHandler();

            BackButton.Click += (s, e) => { if (CefBrowser.CanGoBack) CefBrowser.Back(); };
            ForwardButton.Click += (s, e) => { if (CefBrowser.CanGoForward) CefBrowser.Forward(); };
            RefreshButton.Click += (s, e) => CefBrowser.Reload();
            GoButton.Click += (s, e) => Navigate(AddressBar.Text);

            AddressBar.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    Navigate(AddressBar.Text);
            };

            CefBrowser.LoadingStateChanged += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    BackButton.IsEnabled = e.CanGoBack;
                    ForwardButton.IsEnabled = e.CanGoForward;
                });
            };

            CefBrowser.AddressChanged += (s, e) =>
            {
                Dispatcher.Invoke(() => AddressBar.Text = e.NewValue as string ?? string.Empty);
            };

            Navigate("https://lite.duckduckgo.com/lite");
        }

        // Window control handlers
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeRestoreWindow();
            }
            else
            {
                try { DragMove(); } catch { }
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            MaximizeRestoreWindow();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MaximizeRestoreWindow()
        {
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void Navigate(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return;

            input = input.Trim();

            if (!Uri.TryCreate(input, UriKind.Absolute, out var uri))
            {
                if (Uri.TryCreate("https://" + input, UriKind.Absolute, out var uri2))
                    uri = uri2;
            }

            if (uri == null)
            {
                var q = Uri.EscapeDataString(input);
                uri = new Uri("https://lite.duckduckgo.com/lite?q=" + q);
            }

            CefBrowser.Load(uri.AbsoluteUri);
        }
    }

    public class CustomLifeSpanHandler : ILifeSpanHandler
    {
        public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            newBrowser = null;
            // Load the target URL in the current browser instead of opening a popup
            chromiumWebBrowser.Load(targetUrl);
            return true; // Cancel the popup
        }

        public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            return false;
        }

        public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }
    }
}
