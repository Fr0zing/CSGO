using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace CSGOCheatDetector.Services
{
    public class BrowserService
    {
        public List<string> GetInstalledBrowsers()
        {
            List<string> browsers = new List<string>();
            HashSet<string> seenBrowsers = new HashSet<string>(); // Множество для отслеживания уникальных браузеров

            string registryKeyPath = @"SOFTWARE\Clients\StartMenuInternet";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKeyPath))
            {
                if (key != null)
                {
                    foreach (string subkeyName in key.GetSubKeyNames())
                    {
                        using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                        {
                            if (subkey != null)
                            {
                                string browserName = (string)subkey.GetValue(null);
                                string browserPath = (string)subkey.OpenSubKey(@"shell\open\command")?.GetValue(null);
                                if (!string.IsNullOrEmpty(browserName) && !string.IsNullOrEmpty(browserPath))
                                {
                                    if (!browserName.Contains("Razer"))
                                    {
                                        // Проверяем, если браузер Microsoft Edge или Internet Explorer, то заменяем на Microsoft Edge
                                        if (browserName.Contains("Edge") || browserName.Contains("Internet Explorer"))
                                        {
                                            browserName = "Microsoft Edge";
                                            browserPath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
                                        }

                                        string browserKey = $"{browserName} ({browserPath})";
                                        if (!seenBrowsers.Contains(browserKey))
                                        {
                                            browsers.Add(browserKey);
                                            seenBrowsers.Add(browserKey);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Отдельно проверяем наличие Яндекс Браузера, если он не найден в стандартном реестре
            string yandexPath = $@"C:\Users\{Environment.UserName}\AppData\Local\Yandex\YandexBrowser\Application\browser.exe";
            if (System.IO.File.Exists(yandexPath))
            {
                string yandexBrowser = $"Yandex Browser ({yandexPath})";
                if (!seenBrowsers.Contains(yandexBrowser))
                {
                    browsers.Add(yandexBrowser);
                    seenBrowsers.Add(yandexBrowser);
                }
            }

            return browsers;
        }
    }
}
