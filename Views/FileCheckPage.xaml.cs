using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using CSGOCheatDetector.ViewModels;

namespace CSGOCheatDetector.Views
{
    public partial class FileCheckPage : Page
    {
        private readonly FileCheckViewModel viewModel;

        public FileCheckPage()
        {
            InitializeComponent();
            viewModel = ServiceLocator.ServiceProvider.GetRequiredService<FileCheckViewModel>();
            DataContext = viewModel;
        }
    }
}