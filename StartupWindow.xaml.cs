using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;

namespace CSGOCheatDetector
{
    public partial class StartupWindow : Window
    {
        private static StartupWindow instance;
        private static readonly object lockObj = new object();
        private string requestId;
        private ClientWebSocket clientWebSocket;

        public StartupWindow()  // Публичный конструктор по умолчанию
        {
            InitializeComponent();
            Debug.WriteLine("StartupWindow initialized successfully");

            // Запуск WebSocket подключения
            Task.Run(InitializeWebSocketAsync);
        }

        public static StartupWindow GetInstance()
        {
            lock (lockObj)
            {
                if (instance == null)
                {
                    instance = new StartupWindow();
                    instance.Closed += (sender, e) => instance = null; // Сброс instance при закрытии
                }
                return instance;
            }
        }

        private async void RequestAccessButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("RequestAccessButton clicked");
            await CheckAccessAsync();
        }

        private async Task CheckAccessAsync()
        {
            Debug.WriteLine("CheckAccessAsync started");

            string username = "user123";
            string url = "http://localhost:3000/request-access";

            using (HttpClient client = new HttpClient())
            {
                var requestData = new StringContent($"username={username}", Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage response = await client.PostAsync(url, requestData);

                if (response.IsSuccessStatusCode)
                {
                    requestId = await response.Content.ReadAsStringAsync();
                    UpdateStatus("Запрос отправлен администратору.", "Ожидайте подтверждения.");
                    Debug.WriteLine($"Request ID: {requestId}");
                    await CheckRequestStatusAsync();
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    UpdateStatus("Ошибка: " + response.StatusCode, "Сообщение от сервера: " + errorResponse);
                    Debug.WriteLine($"Error: {response.StatusCode}, {errorResponse}");
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        UpdateStatus("Доступ временно заблокирован.", "Пожалуйста, обратитесь к администратору.");
                    }
                }
            }
        }

        private async Task CheckRequestStatusAsync()
        {
            string url = $"http://localhost:3000/check-status?requestId={requestId}";

            using (HttpClient client = new HttpClient())
            {
                while (true)
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string status = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"Request status: {status}");
                        if (status == "approved")
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (!Application.Current.Windows.OfType<MainWindow>().Any())
                                {
                                    MainWindow mainWindow = new MainWindow();
                                    mainWindow.Show();
                                    instance = null;
                                    this.Close();
                                }
                            });
                            break;
                        }
                        else if (status == "denied")
                        {
                            UpdateStatus("Запрос отклонен.", "Пожалуйста, обратитесь к администратору.");
                            break;
                        }
                    }

                    await Task.Delay(5000);
                }
            }
        }

        private async Task InitializeWebSocketAsync()
        {
            clientWebSocket = new ClientWebSocket();
            await clientWebSocket.ConnectAsync(new Uri("ws://localhost:3000"), CancellationToken.None);

            byte[] buffer = new byte[1024];
            while (clientWebSocket.State == WebSocketState.Open)
            {
                var result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (message == "{\"type\":\"close\"}")
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Application.Current.Shutdown();
                        });
                        break;
                    }
                }
            }
        }

        private void UpdateStatus(string message1, string message2)
        {
            Dispatcher.Invoke(() =>
            {
                statusLabel1.Text = message1;
                statusLabel2.Text = message2;
            });
        }
    }
}
