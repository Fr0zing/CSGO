using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CSGOCheatDetector.Commands;
using CSGOCheatDetector.Models;
using CSGOCheatDetector.Services;

namespace CSGOCheatDetector.ViewModels
{
    public class FileCheckViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<SuspiciousFile> _suspiciousFiles;
        private ObservableCollection<string> _suspiciousNames;
        private string _status;
        private bool _isSearching;
        private CancellationTokenSource _cancellationTokenSource;
        private List<string> _excludedNames;
        private List<string> _excludedFolderNames;
        private List<string> _suspiciousPatterns;

        public ObservableCollection<SuspiciousFile> SuspiciousFiles
        {
            get => _suspiciousFiles;
            set
            {
                _suspiciousFiles = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> SuspiciousNames
        {
            get => _suspiciousNames;
            set
            {
                _suspiciousNames = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public bool IsSearching
        {
            get => _isSearching;
            set
            {
                _isSearching = value;
                OnPropertyChanged();
            }
        }

        public CancellationToken Token => _cancellationTokenSource.Token;

        public ICommand SearchCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand SaveCommand { get; }

        public FileCheckViewModel()
        {
            SuspiciousFiles = new ObservableCollection<SuspiciousFile>();
            SuspiciousNames = new ObservableCollection<string>
            {
                "Aimbot", "Aimware", "PerfectAim", "Lethality", "Skeet.cc", "Iniuria", "Wallhack", "x22Cheats",
                "Interwebz", "IWantCheats", "Triggerbot", "FlixHack", "HyperCheats", "ESP", "Project-Infinity",
                "Big Milk", "Chod’s Cheats", "Bunnyhop", "HyperHook", "AIMJUNKIES", "Radar Hack", "Unityhacks",
                "Aimhax", "No Recoil", "No Spread", "Spinbot", "Skin Changer", "Osiris", "Anti-Aim", "Silent Aim",
                "Glow Hack", "Auto Pistol", "Advance.Tech", "Airflow Beta", "Alphen", "CSGOSimple", "D1gital",
                "Echozy.pw", "Ekknod", "Extender", "Fatality.win", "FluidAim", "ILikeFeet", "Luno Free", "NAIM Free",
                "Onetap v3", "Pandora.gg", "Plaguecheat.cc", "Qo0", "RaweTrip", "RyzeXTR", "YeahNOT", "cshSkins",
                "exloader", "Extreme Injector", "process hacker", "Guided Hacking Injector"
            };
            _excludedNames = new List<string> { "Microsoft", "System" };
            _excludedFolderNames = new List<string> { "Anaconda", "Microsoft", "anaconda3", "dota 2 beta", "Windows Kits", "Adobe" };
            _suspiciousPatterns = new List<string>(SuspiciousNames);
            Status = "Статус: Ожидание";
            IsSearching = false;
            _cancellationTokenSource = new CancellationTokenSource();
            SearchCommand = new RelayCommand(async _ => await StartSearchAsync(), _ => !IsSearching);
            StopCommand = new RelayCommand(_ => CancelSearch(), _ => IsSearching);
            SaveCommand = new RelayCommand(_ => SaveToFile(), _ => !IsSearching && SuspiciousFiles.Count > 0);
        }

        public void StartNewSearch()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void CancelSearch()
        {
            _cancellationTokenSource.Cancel();
            IsSearching = false;
            Status = "Статус: Отменено";
        }

        public async Task StartSearchAsync()
        {
            StartNewSearch();
            SuspiciousFiles.Clear();
            Status = "Статус: Поиск...";
            IsSearching = true;

            string[] rootPaths = Directory.GetLogicalDrives();
            string[] allowedExtensions = { ".exe", ".ahk", ".lua", ".dll", ".bat", ".cfg" };
            string[] systemDirectories = { @"C:\Windows" };
            long minFileSize = 1024;

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var suspiciousFiles = new ConcurrentBag<SuspiciousFile>();
                var fileSearchService = new FileSearchService(_excludedNames, _excludedFolderNames, _suspiciousPatterns, UpdateStatus);

                foreach (var rootPath in rootPaths)
                {
                    var filesFromDisk = await Task.Run(() => fileSearchService.SearchSuspiciousFiles(rootPath, allowedExtensions, systemDirectories, minFileSize, Token));
                    foreach (var file in filesFromDisk)
                    {
                        if (!suspiciousFiles.Contains(file))
                        {
                            suspiciousFiles.Add(file);
                        }
                    }
                }

                stopwatch.Stop();

                if (Token.IsCancellationRequested)
                {
                    Status = "Статус: Отменено";
                    IsSearching = false;
                    return;
                }

                foreach (var file in suspiciousFiles)
                {
                    if (!SuspiciousFiles.Contains(file))
                    {
                        SuspiciousFiles.Add(file);
                    }
                }

                Status = $"Статус: Завершено за {stopwatch.Elapsed.TotalSeconds} секунд. Найдено {suspiciousFiles.Count} подозрительных файлов.";
            }
            catch (OperationCanceledException)
            {
                Status = "Статус: Отменено";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
                Status = "Статус: Ошибка";
            }
            finally
            {
                IsSearching = false;
            }
        }

        private void UpdateStatus(int processedItems)
        {
            Application.Current.Dispatcher.Invoke(() => Status = $"Статус: Поиск... Обработано {processedItems} элементов");
        }

        public void SaveToFile()
        {
            if (SuspiciousFiles.Count == 0)
            {
                MessageBox.Show("Нет данных для сохранения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, "SuspiciousFiles.txt");

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (var file in SuspiciousFiles)
                    {
                        writer.WriteLine($"Имя: {file.Name}");
                        writer.WriteLine($"Размер (KB): {file.Size}");
                        writer.WriteLine($"Дата создания: {file.CreationDate}");
                        writer.WriteLine($"Дата изменения: {file.ModificationDate}");
                        writer.WriteLine($"Дата доступа: {file.AccessDate}");
                        writer.WriteLine($"Полный путь: {file.FullPath}");
                        writer.WriteLine();
                    }
                }

                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
