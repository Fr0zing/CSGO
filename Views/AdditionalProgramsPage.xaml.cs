using System.Windows.Controls;
using CSGOCheatDetector.ViewModels;

namespace CSGOCheatDetector.Views
{
    public partial class AdditionalProgramsPage : Page
    {
        public AdditionalProgramsPage()
        {
            InitializeComponent();
            DataContext = new AdditionalProgramsViewModel();
        }
    }
}
