using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CSGOCheatDetector.ViewModels
{
    public class SteamAccountsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<SteamAccount> _steamAccounts;
        private bool _isLoading;
        private bool _isLoaded;

        private static readonly HttpClient client = new HttpClient();
        private const string apiKey = "11C9E7A200CD540D858131E51B40FF6E"; // Замените на ваш ключ API

        public ObservableCollection<SteamAccount> SteamAccounts
        {
            get => _steamAccounts;
            set
            {
                _steamAccounts = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenProfileCommand { get; }

        public SteamAccountsViewModel()
        {
            SteamAccounts = new ObservableCollection<SteamAccount>();
            OpenProfileCommand = new RelayCommand<SteamAccount>(OpenProfile);
            Task.Run(() => LoadSteamAccounts());
        }

        private async void LoadSteamAccounts()
        {
            if (_isLoaded) return;

            IsLoading = true;

            var accounts = await Task.Run(() => GetSteamAccounts());

            await LoadBanStatuses(accounts);

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var account in accounts)
                {
                    SteamAccounts.Add(account);
                }
                IsLoading = false;
                _isLoaded = true;
            });
        }

        private List<SteamAccount> GetSteamAccounts()
        {
            var accounts = new List<SteamAccount>();
            var steamPaths = new List<string>();

            // Стандартные пути установки Steam
            var standardPaths = new[]
            {
                @"C:\Program Files (x86)\Steam",
                @"C:\Program Files\Steam",
                @"D:\Steam", // Добавьте другие возможные пути
            };

            // Проверка стандартных путей
            foreach (var path in standardPaths)
            {
                if (Directory.Exists(path) && File.Exists(Path.Combine(path, "steam.exe")))
                {
                    steamPaths.Add(path);
                }
            }

            // Если стандартные пути не дали результатов, выполняем полный обход дисков
            if (steamPaths.Count == 0)
            {
                var drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed && d.IsReady);

                Parallel.ForEach(drives, drive =>
                {
                    var root = drive.RootDirectory.FullName;
                    steamPaths.AddRange(FindSteamPaths(root));
                });
            }

            foreach (var path in steamPaths)
            {
                try
                {
                    string steamConfigPath = Path.Combine(path, "config", "loginusers.vdf");
                    if (File.Exists(steamConfigPath))
                    {
                        string[] lines = File.ReadAllLines(steamConfigPath);
                        string steamIDPattern = "\"\\d{17}\"";
                        string personaNamePattern = "\"PersonaName\"\\s+\"([^\"]+)\"";

                        string steamID = string.Empty;
                        string personaName = string.Empty;

                        foreach (string line in lines)
                        {
                            if (Regex.IsMatch(line, steamIDPattern))
                            {
                                steamID = Regex.Match(line, steamIDPattern).Value.Replace("\"", "");
                            }

                            if (Regex.IsMatch(line, personaNamePattern))
                            {
                                personaName = Regex.Match(line, personaNamePattern).Groups[1].Value;
                            }

                            if (!string.IsNullOrEmpty(steamID) && !string.IsNullOrEmpty(personaName))
                            {
                                var avatarPath = Path.Combine(path, "config", "avatarcache", $"{steamID}.png");
                                BitmapImage avatar = null;
                                if (File.Exists(avatarPath))
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        avatar = new BitmapImage(new Uri(avatarPath));
                                    });
                                }

                                accounts.Add(new SteamAccount { SteamID = steamID, PersonaName = personaName, Avatar = avatar });
                                steamID = string.Empty;
                                personaName = string.Empty;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore errors
                }
            }

            return accounts;
        }

        private List<string> FindSteamPaths(string rootPath)
        {
            var steamPaths = new List<string>();
            var directories = new Stack<string>();
            directories.Push(rootPath);

            while (directories.Count > 0)
            {
                var currentDir = directories.Pop();

                try
                {
                    foreach (var dir in Directory.GetDirectories(currentDir))
                    {
                        if (Directory.Exists(Path.Combine(dir, "config")) && File.Exists(Path.Combine(dir, "steam.exe")))
                        {
                            steamPaths.Add(dir);
                        }
                        directories.Push(dir);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Ignore access errors
                }
                catch (PathTooLongException)
                {
                    // Ignore path too long errors
                }
            }

            return steamPaths;
        }

        private async Task LoadBanStatuses(List<SteamAccount> accounts)
        {
            var steamIDs = accounts.Select(a => a.SteamID).ToList();
            var chunks = steamIDs.Select((id, index) => new { id, index })
                                  .GroupBy(x => x.index / 100)
                                  .Select(g => g.Select(x => x.id).ToList())
                                  .ToList();

            foreach (var chunk in chunks)
            {
                string ids = string.Join(",", chunk);
                string apiUrl = $"https://api.steampowered.com/ISteamUser/GetPlayerBans/v1/?key={apiKey}&steamids={ids}";

                var response = await client.GetStringAsync(apiUrl);
                var banData = Newtonsoft.Json.JsonConvert.DeserializeObject<BanResponse>(response);

                foreach (var banInfo in banData.Players)
                {
                    var account = accounts.FirstOrDefault(a => a.SteamID == banInfo.SteamId);
                    if (account != null)
                    {
                        account.VACBanned = banInfo.VACBanned;
                        account.GameBanCount = banInfo.NumberOfGameBans;
                    }
                }
            }
        }

        private void OpenProfile(SteamAccount account)
        {
            var steamProfileUrl = $"https://steamcommunity.com/profiles/{account.SteamID}";
            Process.Start(new ProcessStartInfo(steamProfileUrl) { UseShellExecute = true });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class SteamAccount
    {
        public string PersonaName { get; set; }
        public string SteamID { get; set; }
        public bool VACBanned { get; set; }
        public int GameBanCount { get; set; }
        public BitmapImage Avatar { get; set; }
    }

    public class BanResponse
    {
        public List<BanInfo> Players { get; set; }
    }

    public class BanInfo
    {
        public string SteamId { get; set; }
        public bool VACBanned { get; set; }
        public int NumberOfGameBans { get; set; }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
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
