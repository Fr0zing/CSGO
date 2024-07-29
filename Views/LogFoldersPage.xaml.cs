using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CSGOCheatDetector.Views
{
    public partial class LogFoldersPage : Page
    {
        private static bool isSearching = false; // Флаг для отслеживания состояния поиска

        public LogFoldersPage()
        {
            InitializeComponent();
            Loaded += LogFoldersPage_Loaded;
        }

        private void LogFoldersPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (isSearching)
            {
                UpdateSearchUI(true);
            }
        }

        private void OpenFolder(string folderPath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonPrefetch_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = @"C:\Windows\Prefetch";
            OpenFolder(folderPath);
        }

        private void ButtonEventLogs_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = @"C:\Windows\System32\winevt\Logs";
            OpenFolder(folderPath);
        }

        private void ButtonWERReportArchive_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = @"C:\ProgramData\Microsoft\Windows\WER\ReportArchive";
            OpenFolder(folderPath);
        }

        private void ButtonAppData_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            OpenFolder(folderPath);
        }

        private void ButtonRecent_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
            OpenFolder(folderPath);
        }

        private void ButtonHistory_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\AppData\Local\Microsoft\Windows\History";
            OpenFolder(folderPath);
        }

        private void ButtonProcesses_Click(object sender, RoutedEventArgs e)
        {
            string filePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}\processes.txt";
            string command = "powershell.exe";
            string args = $"-NoProfile -Command \"Get-Process | Out-File -FilePath '{filePath}'\"";

            ProcessStartInfo startInfo = new ProcessStartInfo(command, args)
            {
                RedirectStandardOutput = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }

                // Открытие файла
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating or opening the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ButtonCheckCSGO_Click(object sender, RoutedEventArgs e)
        {
            if (isSearching)
            {
                return; // Если уже идет поиск, ничего не делать
            }

            isSearching = true;
            UpdateSearchUI(true);

            List<string> csgoFolders = await Task.Run(() => FindCSGOFolders());

            isSearching = false;
            UpdateSearchUI(false);

            if (csgoFolders.Count > 0)
            {
                foreach (var folder in csgoFolders)
                {
                    OpenFolder(folder);
                }
            }
            else
            {
                MessageBox.Show("Папка Counter-Strike Global Offensive не найдена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateSearchUI(bool isSearching)
        {
            if (isSearching)
            {
                CheckAndOpenCSGOButton.Content = "Идет поиск...";
                CheckAndOpenCSGOButton.IsEnabled = false;
            }
            else
            {
                CheckAndOpenCSGOButton.Content = "Check and Open CS:GO";
                CheckAndOpenCSGOButton.IsEnabled = true;
            }
        }

        private List<string> FindCSGOFolders()
        {
            List<string> csgoFolders = new List<string>();

            foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed))
            {
                try
                {
                    // First look for Steam folder
                    string steamFolder = FindFolder("Steam", drive.Name);
                    if (!string.IsNullOrEmpty(steamFolder))
                    {
                        string csgoPath = Path.Combine(steamFolder, "steamapps", "common", "Counter-Strike Global Offensive");
                        if (Directory.Exists(csgoPath) && File.Exists(Path.Combine(csgoPath, "csgo.exe")))
                        {
                            csgoFolders.Add(csgoPath);
                        }
                    }

                    // Then look for CS:GO folder directly
                    var csgoPaths = FindFolders("Counter-Strike Global Offensive", drive.Name);
                    csgoFolders.AddRange(csgoPaths.Where(p => File.Exists(Path.Combine(p, "csgo.exe"))));
                }
                catch (Exception ex)
                {
                    // Ignore errors
                }
            }

            return csgoFolders;
        }

        private string FindFolder(string folderName, string rootPath)
        {
            try
            {
                var directories = Directory.EnumerateDirectories(rootPath, "*", SearchOption.TopDirectoryOnly)
                                           .Where(d => new DirectoryInfo(d).Name.Equals(folderName, StringComparison.OrdinalIgnoreCase));
                return directories.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Ignore access errors
            }

            return null;
        }

        private List<string> FindFolders(string folderName, string rootPath)
        {
            List<string> foundFolders = new List<string>();
            Stack<string> dirs = new Stack<string>();

            dirs.Push(rootPath);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                try
                {
                    foreach (var dir in Directory.EnumerateDirectories(currentDir))
                    {
                        if (new DirectoryInfo(dir).Name.Equals(folderName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (File.Exists(Path.Combine(dir, "csgo.exe")))
                            {
                                foundFolders.Add(dir);
                            }
                        }
                        dirs.Push(dir);
                    }
                }
                catch (Exception ex)
                {
                    // Ignore access errors
                }
            }

            return foundFolders;
        }
    }
}
