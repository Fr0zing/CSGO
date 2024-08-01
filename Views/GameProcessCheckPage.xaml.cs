using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WindowsInput;
using WindowsInput.Native;

namespace CSGOCheatDetector.Views
{
    public partial class GameProcessCheckPage : Page
    {
        private InputSimulator _inputSimulator;

        public GameProcessCheckPage()
        {
            InitializeComponent();
            _inputSimulator = new InputSimulator();
        }

        private void ButtonLaunchSystemInformer_Click(object sender, RoutedEventArgs e)
        {
            string systemInformerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SystemInformer", "SystemInformer.exe");
            if (File.Exists(systemInformerPath))
            {
                Process.Start(systemInformerPath);
                InfoTextBlock.Text = "SystemInformer запущен.";
            }
            else
            {
                InfoTextBlock.Text = "SystemInformer не найден.";
            }
        }

        private void ButtonAutoKeyPressMultiple_Click(object sender, RoutedEventArgs e)
        {
            var gameProcess = Process.GetProcessesByName("csgo");
            if (gameProcess.Length > 0)
            {
                InfoTextBlock.Text = "Процесс CS:GO запущен. Автонажатие нескольких клавиш активировано.";
                AutoPressMultipleKeys();
            }
            else
            {
                InfoTextBlock.Text = "Процесс CS:GO не найден. Пожалуйста, запустите CS:GO и повторите попытку.";
            }
        }

        private void AutoPressMultipleKeys()
        {
            Task.Run(async () =>
            {
                var keys = new[]
                {
                    VirtualKeyCode.INSERT,
                    VirtualKeyCode.HOME,
                    VirtualKeyCode.DELETE,
                    VirtualKeyCode.END
                };

                while (true)
                {
                    foreach (var key in keys)
                    {
                        _inputSimulator.Keyboard.KeyPress(key);
                        await Task.Delay(500); // Задержка 0.5 секунд между нажатиями клавиш
                    }

                    await Task.Delay(5000); // Задержка 5 секунд между циклами нажатий
                }
            });
        }
    }
}
