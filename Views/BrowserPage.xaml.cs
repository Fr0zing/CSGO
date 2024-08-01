using CSGOCheatDetector.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace CSGOCheatDetector.Views
{
    public partial class BrowserPage : Page
    {
        private string selectedBrowserPath;

        public BrowserPage()
        {
            InitializeComponent();
            LoadInstalledBrowsers();
        }

        private void LoadInstalledBrowsers()
        {
            var browserService = new BrowserService();
            var browsers = browserService.GetInstalledBrowsers();

            BrowserList.Children.Clear();
            foreach (var browser in browsers)
            {
                var button = new Button
                {
                    Content = browser,
                    Margin = new Thickness(0, 5, 0, 5)
                };
                button.Click += (s, e) => SelectBrowser(browser);
                BrowserList.Children.Add(button);
            }
        }

        private void SelectBrowser(string browser)
        {
            selectedBrowserPath = GetBrowserPath(browser);
            InfoTextBlock.Text = $"Выбранный браузер: {browser}";
        }

        private string GetBrowserPath(string browser)
        {
            // Извлечение пути из строки браузера
            var startIndex = browser.IndexOf('(') + 1;
            var endIndex = browser.LastIndexOf(')');
            if (startIndex > 0 && endIndex > startIndex)
            {
                return browser.Substring(startIndex, endIndex - startIndex).Trim();
            }
            return null;
        }

        private void OpenSite(string url)
        {
            if (!string.IsNullOrEmpty(selectedBrowserPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = selectedBrowserPath,
                        Arguments = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось открыть браузер: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите браузер.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenZelenkaAndLolz_Click(object sender, RoutedEventArgs e)
        {
            OpenSite("https://zelenka.guru/");
            OpenSite("https://lolz.market/steam/");
            OpenSite("https://lolz.market/");
        }

        private void OpenOplataInfo_Click(object sender, RoutedEventArgs e)
        {
            OpenSite("https://oplata.info/");
        }

        private void OpenFunPay_Click(object sender, RoutedEventArgs e)
        {
            OpenSite("https://funpay.com/");
        }

        private void OpenVKSites_Click(object sender, RoutedEventArgs e)
        {
            OpenSite("https://vk.com/rawetrip");
            OpenSite("https://vk.com/aurorareborn");
            OpenSite("https://vk.com/yeahnotcsgo");
        }

        private void OpenBrokenCore_Click(object sender, RoutedEventArgs e)
        {
            OpenSite("https://brokencore.club/");
        }

        private void OpenEzCheats_Click(object sender, RoutedEventArgs e)
        {
            OpenSite("https://ezcheats.ru/");
        }

        private void OpenSelectPlace_Click(object sender, RoutedEventArgs e)
        {
            OpenSite("https://select-place.ru/");
        }

        private void OpenAllSites_Click(object sender, RoutedEventArgs e)
        {
            OpenSite("https://zelenka.guru/");
            OpenSite("https://lolz.market/steam/");
            OpenSite("https://lolz.market/");
            OpenSite("https://oplata.info/");
            OpenSite("https://funpay.com/");
            OpenSite("https://vk.com/rawetrip");
            OpenSite("https://vk.com/aurorareborn");
            OpenSite("https://vk.com/yeahnotcsgo");
            OpenSite("https://brokencore.club/");
            OpenSite("https://ezcheats.ru/");
            OpenSite("https://select-place.ru/");
        }
    }
}
