using System.Windows;
using System.Windows.Input;

namespace Vetale_Browser_Lite
{
    public partial class UserAgreementWindow : Window
    {
        private const string AcceptedKey = "agreementAcceptedV1";

        public bool Accepted { get; private set; }

        public UserAgreementWindow()
        {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            Accepted = true;
            SettingsStore.Set(AcceptedKey, "1");
            DialogResult = true;
            Close();
        }

        private void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            Accepted = false;
            DialogResult = false;
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch { }
        }

        /// <summary>Shows the dialog on first launch. Returns false when the app must exit.</summary>
        public static bool EnsureAccepted()
        {
            if (SettingsStore.Get(AcceptedKey, string.Empty) == "1")
                return true;
            var w = new UserAgreementWindow();
            bool? result = w.ShowDialog();
            return result == true && w.Accepted;
        }
    }
}
