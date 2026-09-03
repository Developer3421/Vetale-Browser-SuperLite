using System.Windows;
using System.Windows.Input;

namespace Vetale_Browser_Lite
{
    public partial class AddBookmarkWindow : Window
    {
        public AddBookmarkWindow(string title, string url)
        {
            InitializeComponent();
            TitleBox.Text = title ?? string.Empty;
            UrlBox.Text = url ?? string.Empty;
            TitleBox.Focus();
            TitleBox.SelectAll();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var url = UrlBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show(LocalizationManager.Get("Msg_UrlMissing"), LocalizationManager.Get("Msg_Title"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (!BookmarkStore.Add(TitleBox.Text, url))
            {
                MessageBox.Show(LocalizationManager.Get("Msg_AlreadyExists"), LocalizationManager.Get("Msg_Title"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch { }
        }
    }
}
