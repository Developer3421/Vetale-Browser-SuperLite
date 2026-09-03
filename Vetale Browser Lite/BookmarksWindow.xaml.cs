using System.Windows;
using System.Windows.Input;

namespace Vetale_Browser_Lite
{
    public partial class BookmarksWindow : Window
    {
        public BookmarksWindow()
        {
            InitializeComponent();
            BookmarksList.ItemsSource = BookmarkStore.Items;
        }

        private void BookmarksList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelected();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e) => OpenSelected();

        private void OpenSelected()
        {
            if (BookmarksList.SelectedItem is BookmarkEntry entry && Owner is MainWindow main)
            {
                main.NavigateTo(entry.Url);
                Close();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (BookmarksList.SelectedItem is BookmarkEntry entry)
                BookmarkStore.Remove(entry);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch { }
        }
    }
}
