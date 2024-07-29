using CSGOCheatDetector.Views;
using System.Windows;

namespace CSGOCheatDetector
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new SteamAccountsPage());
            MainFrame.Navigate(new HomePage());
        }

        private void ButtonHomePage_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new HomePage();
        }

        private void ButtonFileCheckPage_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new FileCheckPage();
        }

        private void ButtonLogFoldersPage_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new LogFoldersPage();
        }

        private void ButtonSteamAccountsPage_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new SteamAccountsPage();
        }

        private void ButtonAdditionalProgramsPage_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new AdditionalProgramsPage();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
