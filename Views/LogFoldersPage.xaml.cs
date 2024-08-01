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
        private static bool isSearching = false;

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

        private void ButtonEventLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "eventvwr.msc",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть журнал событий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            if (isSearching) return;

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

        private async void ButtonOpenScreenshots_Click(object sender, RoutedEventArgs e)
        {
            if (isSearching) return;

            isSearching = true;
            UpdateSearchUI(true);

            string screenshotsFolder = await Task.Run(() => FindScreenshotsFolder());

            isSearching = false;
            UpdateSearchUI(false);

            if (!string.IsNullOrEmpty(screenshotsFolder))
            {
                OpenFolder(screenshotsFolder);
            }
            else
            {
                MessageBox.Show("Папка скриншотов не найдена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateSearchUI(bool isSearching)
        {
            CheckAndOpenCSGOButton.Content = isSearching ? "Идет поиск..." : "Check and Open CS:GO";
            CheckAndOpenCSGOButton.IsEnabled = !isSearching;
        }

        private List<string> FindCSGOFolders()
        {
            List<string> csgoFolders = new List<string>();

            var standardPaths = new List<string>
            {
                @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive",
                @"D:\SteamLibrary\steamapps\common\Counter-Strike Global Offensive",
                @"E:\SteamLibrary\steamapps\common\Counter-Strike Global Offensive"
            };

            foreach (var path in standardPaths)
            {
                if (Directory.Exists(path) && File.Exists(Path.Combine(path, "csgo.exe")))
                {
                    csgoFolders.Add(path);
                    return csgoFolders;
                }
            }

            foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed))
            {
                try
                {
                    string steamFolder = FindFolder("Steam", drive.Name);
                    if (!string.IsNullOrEmpty(steamFolder))
                    {
                        string csgoPath = Path.Combine(steamFolder, "steamapps", "common", "Counter-Strike Global Offensive");
                        if (Directory.Exists(csgoPath) && File.Exists(Path.Combine(csgoPath, "csgo.exe")))
                        {
                            csgoFolders.Add(csgoPath);
                            return csgoFolders;
                        }
                    }

                    var csgoPaths = FindFolders("Counter-Strike Global Offensive", drive.Name);
                    csgoFolders.AddRange(csgoPaths.Where(p => File.Exists(Path.Combine(p, "csgo.exe"))));
                }
                catch (Exception)
                {
                }
            }

            return csgoFolders;
        }

        private string FindScreenshotsFolder()
        {
            List<string> csgoFolders = FindCSGOFolders();
            foreach (var csgoFolder in csgoFolders)
            {
                string screenshotsPath = Path.Combine(csgoFolder, "csgo", "screenshots");
                if (Directory.Exists(screenshotsPath))
                {
                    return screenshotsPath;
                }
            }

            return null;
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
        private void ButtonOpenDataUsage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:datausage",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть окно настроек: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
