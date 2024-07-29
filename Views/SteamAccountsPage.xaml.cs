using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CSGOCheatDetector.ViewModels;

namespace CSGOCheatDetector.Views
{
    public partial class SteamAccountsPage : Page
    {
        private static SteamAccountsViewModel viewModel = new SteamAccountsViewModel();

        public SteamAccountsPage()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is SteamAccount account)
            {
                ((SteamAccountsViewModel)DataContext).OpenProfileCommand.Execute(account);
            }
        }

    }
}
