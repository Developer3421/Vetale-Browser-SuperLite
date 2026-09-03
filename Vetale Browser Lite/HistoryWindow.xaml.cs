using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Vetale_Browser_Lite
{
    public partial class HistoryWindow : Window
    {
        public static ObservableCollection<HistoryEntry> Items => HistoryStore.Items;

        public HistoryWindow()
        {
            InitializeComponent();
            HistoryList.ItemsSource = Items;
        }

        public static void Add(string url, string title) => HistoryStore.Add(url, title);

        private void HistoryList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (HistoryList.SelectedItem is HistoryEntry entry && Owner is MainWindow main)
            {
                main.NavigateTo(entry.Url);
                Close();
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e) => HistoryList_MouseDoubleClick(sender, null);

        private void ClearButton_Click(object sender, RoutedEventArgs e) => HistoryStore.Clear();

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch { }
        }
    }
}
