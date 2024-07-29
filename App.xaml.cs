using System.Windows;

namespace CSGOCheatDetector
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Удаляем создание окна здесь
            // RequestAccessWindow requestAccessWindow = new RequestAccessWindow();
            // requestAccessWindow.Show();
        }
    }
}
