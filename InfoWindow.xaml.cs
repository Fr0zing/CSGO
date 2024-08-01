using System.Windows;

namespace CSGOCheatDetector
{
    public partial class InfoWindow : Window
    {
        public InfoWindow()
        {
            InitializeComponent();
        }

        public void SetInfo(string title, string content)
        {
            InfoTitle.Text = title;
            InfoContent.Text = content;
        }
    }
}
