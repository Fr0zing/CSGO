using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CSGOCheatDetector
{
    public partial class RequestAccessWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();
        private string clientId;

        public RequestAccessWindow()
        {
            InitializeComponent();
        }

        private async void RequestAccessButton_Click(object sender, RoutedEventArgs e)
        {
            RequestButton.Visibility = Visibility.Collapsed;
            LoadingBar.Visibility = Visibility.Visible;
            StatusText.Visibility = Visibility.Collapsed;

            var username = Environment.UserName; // Используем имя пользователя из Windows
            clientId = Guid.NewGuid().ToString(); // Генерация уникального client_id
            var requestBody = new
            {
                username = username,
                client_id = clientId
            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("http://80.76.43.223/api/access-request", content);
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var responseData = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseData>(responseString);
                    if (responseData.status == "pending")
                    {
                        // Ждем обновления статуса
                        await WaitForApproval();
                    }
                    else
                    {
                        ShowStatusMessage("Доступ отклонен. Пожалуйста, свяжитесь с администратором.");
                        ResetUI();
                    }
                }
                else
                {
                    ShowStatusMessage("Ошибка: " + responseString);
                    ResetUI();
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Ошибка: {ex.Message}");
                ResetUI();
            }
        }

        private async Task WaitForApproval()
        {
            var isApproved = false;

            while (!isApproved)
            {
                try
                {
                    var response = await client.GetAsync($"http://80.76.43.223/api/access-request-status/{clientId}");
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseData = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseData>(responseString);
                    if (responseData.status == "approved")
                    {
                        isApproved = true;
                        OpenMainWindow();
                    }
                    else if (responseData.status == "denied")
                    {
                        ShowStatusMessage("Доступ отклонен. Пожалуйста, свяжитесь с администратором.");
                        ResetUI();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    ShowStatusMessage($"Ошибка: {ex.Message}");
                    ResetUI();
                    break;
                }

                await Task.Delay(5000); // Ждем 5 секунд перед повторной проверкой
            }
        }

        private void ResetUI()
        {
            RequestButton.Visibility = Visibility.Visible;
            LoadingBar.Visibility = Visibility.Collapsed;
        }

        private void ShowStatusMessage(string message)
        {
            StatusText.Text = message;
            StatusText.Visibility = Visibility.Visible;
        }

        private void OpenMainWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            });
        }
    }

    public class ResponseData
    {
        public string status { get; set; }
    }
}
