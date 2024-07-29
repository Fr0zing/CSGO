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

namespace CSGOCheatDetector.ViewModels
{
    public class FileCheckViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<SuspiciousFile> _suspiciousFiles;
        private ObservableCollection<string> _suspiciousNames;
        private string _status;
        private bool _isSearching;
        private CancellationTokenSource _cancellationTokenSource;
        private List<string> _excludedNames; // переменная для хранения исключаемых названий файлов
        private List<string> _excludedFolderNames; // переменная для хранения исключаемых имен папок

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
                "Glow Hack", "Auto Pistol"
            };
            _excludedNames = new List<string> { "Microsoft", "System" }; // примеры исключаемых названий файлов
            _excludedFolderNames = new List<string> { "Anaconda", "Microsoft", "anaconda3", "dota 2 beta", "Windows Kits", "Adobe" }; // примеры исключаемых имен папок
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
            SuspiciousFiles.Clear(); // Очищаем список подозрительных файлов перед началом поиска
            Status = "Статус: Поиск...";
            IsSearching = true;

            string[] rootPaths = Directory.GetLogicalDrives();
            string[] allowedExtensions = { ".exe", ".ahk", ".lua", ".dll" };
            string[] systemDirectories = { @"C:\Windows" };
            long minFileSize = 1024;

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var suspiciousFiles = new ConcurrentBag<SuspiciousFile>();

                foreach (var rootPath in rootPaths)
                {
                    var filesFromDisk = await Task.Run(() => SearchSuspiciousFiles(rootPath, SuspiciousNames.ToArray(), allowedExtensions, systemDirectories, minFileSize, Token));
                    foreach (var file in filesFromDisk)
                    {
                        if (!suspiciousFiles.Contains(file)) // Проверка на уникальность
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
                    if (!SuspiciousFiles.Contains(file)) // Проверка на уникальность перед добавлением
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

        public void SaveToFile()
        {
            if (SuspiciousFiles.Count == 0)
            {
                MessageBox.Show("Нет данных для сохранения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(appDirectory, "SuspiciousFiles.txt");

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

        private List<SuspiciousFile> SearchSuspiciousFiles(string rootPath, string[] names, string[] extensions, string[] systemDirectories, long minFileSize, CancellationToken cancellationToken)
        {
            var suspiciousFiles = new ConcurrentBag<SuspiciousFile>();
            var directories = new BlockingCollection<string> { rootPath };

            var processedDirectories = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            var processedFiles = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            int processedItems = 0;

            while (directories.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                var currentDir = directories.Take();

                // Пропускаем директории, содержащие исключаемые имена
                if (_excludedFolderNames.Any(excludedName => currentDir.Split(Path.DirectorySeparatorChar).Any(dir => dir.IndexOf(excludedName, StringComparison.OrdinalIgnoreCase) >= 0)))
                {
                    continue;
                }

                if (!processedDirectories.TryAdd(currentDir, true))
                {
                    continue;
                }

                try
                {
                    var subDirectories = Directory.EnumerateDirectories(currentDir).ToList();
                    var files = Directory.EnumerateFiles(currentDir).ToList();

                    Parallel.ForEach(subDirectories, dir =>
                    {
                        directories.Add(dir);
                        Interlocked.Increment(ref processedItems);
                        UpdateStatus(processedItems);
                    });

                    Parallel.ForEach(files, file =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        if (!processedFiles.TryAdd(file, true))
                        {
                            return;
                        }

                        try
                        {
                            var fileInfo = new FileInfo(file);

                            if ((fileInfo.Attributes & FileAttributes.System) == FileAttributes.System ||
                                (fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                            {
                                return;
                            }

                            if (systemDirectories.Any(dir => fileInfo.FullName.StartsWith(dir, StringComparison.OrdinalIgnoreCase)))
                            {
                                return;
                            }

                            if (!extensions.Contains(fileInfo.Extension.ToLower()))
                            {
                                return;
                            }

                            if (fileInfo.Length < minFileSize)
                            {
                                return;
                            }

                            // Проверка на подозрительное имя файла и исключаемое имя файла
                            if (names.Any(name => fileInfo.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0) &&
                                !_excludedNames.Any(excludedName => fileInfo.Name.IndexOf(excludedName, StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                var suspiciousFile = new SuspiciousFile
                                {
                                    Name = fileInfo.Name,
                                    Size = fileInfo.Length / 1024,
                                    CreationDate = fileInfo.CreationTime,
                                    ModificationDate = fileInfo.LastWriteTime,
                                    AccessDate = fileInfo.LastAccessTime,
                                    Extension = fileInfo.Extension,
                                    FullPath = fileInfo.FullName
                                };

                                suspiciousFiles.Add(suspiciousFile);
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (!SuspiciousFiles.Contains(suspiciousFile)) // Проверка на уникальность перед добавлением
                                    {
                                        SuspiciousFiles.Add(suspiciousFile);
                                    }
                                });
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Пропускаем файлы, к которым нет доступа
                        }
                        catch (PathTooLongException)
                        {
                            // Пропускаем файлы с слишком длинным путем
                        }
                        catch (IOException)
                        {
                            // Пропускаем файлы с ошибками ввода-вывода
                        }
                        catch (Exception)
                        {
                            // Пропускаем файлы с неожиданными ошибками
                        }

                        Interlocked.Increment(ref processedItems);
                        UpdateStatus(processedItems);

                        Thread.Sleep(1);
                    });
                }
                catch (UnauthorizedAccessException)
                {
                    // Пропускаем директории, к которым нет доступа
                }
                catch (PathTooLongException)
                {
                    // Пропускаем директории с слишком длинным путем
                }
                catch (IOException)
                {
                    // Пропускаем директории с ошибками ввода-вывода
                }
                catch (Exception)
                {
                    // Пропускаем директории с неожиданными ошибками
                }
            }

            return suspiciousFiles.ToList();
        }

        private void UpdateStatus(int processedItems)
        {
            Application.Current.Dispatcher.Invoke(() => Status = $"Статус: Поиск... Обработано {processedItems} элементов");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class SuspiciousFile : IEquatable<SuspiciousFile>
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ModificationDate { get; set; }
        public DateTime AccessDate { get; set; }
        public string Extension { get; set; }
        public string FullPath { get; set; }

        public bool Equals(SuspiciousFile other)
        {
            if (other is null) return false;
            return FullPath == other.FullPath;
        }

        public override bool Equals(object obj) => Equals(obj as SuspiciousFile);
        public override int GetHashCode() => FullPath.GetHashCode();
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute) : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
