using System;
using System.Windows;
using System.Windows.Input;
using CefSharp;
using CefSharp.Wpf;

namespace Vetale_Browser_Lite
{
    public partial class MainWindow : Window
    {
        private const string HomeUrl = "https://duckduckgo.com/";
        private readonly RedirectGuardRequestHandler _redirectGuard = new();

        private ChromiumWebBrowser CefBrowser => (ChromiumWebBrowser)Browser;

        public MainWindow()
        {
            InitializeComponent();

            CefBrowser.LifeSpanHandler = new CustomLifeSpanHandler();
            CefBrowser.RequestHandler = _redirectGuard;
            CefBrowser.DisplayHandler = new VetaleDisplayHandler();

            BackButton.Click += (s, e) => { if (CefBrowser.CanGoBack) CefBrowser.Back(); };
            ForwardButton.Click += (s, e) => { if (CefBrowser.CanGoForward) CefBrowser.Forward(); };
            RefreshButton.Click += (s, e) => CefBrowser.Reload();
            GoButton.Click += (s, e) => Navigate(AddressBar.Text);
            HistoryButton.Click += (s, e) => new HistoryWindow { Owner = this }.ShowDialog();
            HomeButton.Click += (s, e) => Navigate(HomeUrl);
            LocalizationManager.LanguageChanged += (s, e) => Dispatcher.Invoke(RefreshLanguageButtons);
            RefreshLanguageButtons();            AddBookmarkButton.Click += (s, e) => new AddBookmarkWindow(CefBrowser.Title, AddressBar.Text) { Owner = this }.ShowDialog();
            BookmarksButton.Click += (s, e) => new BookmarksWindow { Owner = this }.ShowDialog();

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

            CefBrowser.FrameLoadEnd += (s, e) =>
            {
                if (e.Frame?.IsMain == true)
                {
                    var url = e.Url;
                    Dispatcher.Invoke(() => HistoryWindow.Add(url, CefBrowser.Title));
                }
            };

            Navigate(HomeUrl);
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

        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button b && b.Tag is string lang)
                LocalizationManager.Apply(lang);
        }

        private void AgreementButton_Click(object sender, RoutedEventArgs e)
        {
            new UserAgreementWindow { Owner = this }.ShowDialog();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow { Owner = this }.ShowDialog();
        }

        private void RefreshLanguageButtons()
        {
            MarkLanguageButton(LangUkButton, "uk");
            MarkLanguageButton(LangEnButton, "en");
            MarkLanguageButton(LangDeButton, "de");
            MarkLanguageButton(LangRuButton, "ru");
            MarkLanguageButton(LangTrButton, "tr");
        }

        private void MarkLanguageButton(System.Windows.Controls.Button b, string lang)
        {
            if (b == null)
                return;
            bool active = string.Equals(LocalizationManager.Current, lang, StringComparison.OrdinalIgnoreCase);
            b.Opacity = active ? 1.0 : 0.55;
            b.BorderThickness = active ? new Thickness(1.5) : new Thickness(0);
            b.BorderBrush = active ? System.Windows.Media.Brushes.White : null;
        }

        public void NavigateTo(string url)
        {
            _redirectGuard.MarkUserNavigation(url);
            CefBrowser.Load(url);
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
                uri = new Uri("https://duckduckgo.com/?q=" + q);
            }

            NavigateTo(uri.AbsoluteUri);
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
