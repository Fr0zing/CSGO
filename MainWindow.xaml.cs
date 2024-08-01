using CSGOCheatDetector.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CSGOCheatDetector
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            MainFrame.Navigate(new Uri("Views/HomePage.xaml", UriKind.Relative));
        }

        private void ButtonHomePage_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/HomePage.xaml", UriKind.Relative));
        }

        private void ButtonFileCheckPage_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/FileCheckPage.xaml", UriKind.Relative));
        }

        private void ButtonLogFoldersPage_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/LogFoldersPage.xaml", UriKind.Relative));
        }

        private void ButtonSteamAccountsPage_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/SteamAccountsPage.xaml", UriKind.Relative));
        }

        private void ButtonAdditionalProgramsPage_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/AdditionalProgramsPage.xaml", UriKind.Relative));
        }

        private void ButtonGameProcessCheckPage_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/GameProcessCheckPage.xaml", UriKind.Relative));
        }

        private void ButtonBrowserPage_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BrowserPage());
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
