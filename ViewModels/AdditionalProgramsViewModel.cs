using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace CSGOCheatDetector.ViewModels
{
    public class AdditionalProgramsViewModel : BaseViewModel
    {
        public RelayCommand RunBrowserDownloadsViewCommand { get; }
        public RelayCommand RunDevManViewCommand { get; }
        public RelayCommand RunExecutedProgramsListCommand { get; }
        public RelayCommand RunJumpListsViewCommand { get; }
        public RelayCommand RunMUICacheViewCommand { get; }
        public RelayCommand RunShellbagCommand { get; }
        public RelayCommand RunUserAssistViewCommand { get; }

        public AdditionalProgramsViewModel()
        {
            RunBrowserDownloadsViewCommand = new RelayCommand(_ => RunProgram("BrowserDownloadsView.exe"));
            RunDevManViewCommand = new RelayCommand(_ => RunProgram("DevManView.exe"));
            RunExecutedProgramsListCommand = new RelayCommand(_ => RunProgram("ExecutedProgramsList.exe"));
            RunJumpListsViewCommand = new RelayCommand(_ => RunProgram("JumpListsView.exe"));
            RunMUICacheViewCommand = new RelayCommand(_ => RunProgram("MUICacheView.exe"));
            RunShellbagCommand = new RelayCommand(_ => RunProgram("shellbag.exe"));
            RunUserAssistViewCommand = new RelayCommand(_ => RunProgram("UserAssistView.exe"));
        }

        private void RunProgram(string programName)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string appPath = Path.Combine(appDirectory, "Apps", programName);

            if (File.Exists(appPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(appPath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to start program: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show($"Program {programName} not found at {appPath}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}