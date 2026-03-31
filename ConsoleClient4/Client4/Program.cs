using System.Net.Sockets;
using System.Net;
using System.Text;

class Client
{
    private const int DEFAULT_BUFLEN = 512;
    private const int DEFAULT_PORT = 27015;

    static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "КЛІЄНТСЬКА СТОРОНА";

        string nickname;
        do
        {
            Console.Write("Введіть ваш нікнейм: ");
            nickname = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(nickname))
                Console.WriteLine("Нікнейм не може бути порожнім, спробуйте ще раз.");
        }
        while (string.IsNullOrEmpty(nickname));

        Console.WriteLine($"Привіт, {nickname} Підключаємось до сервера...");

        string? pendingMessage = null;
        var inputCancellation = new CancellationTokenSource();

        while (true)
        {
            using var client = new TcpClient();
            try
            {
                await client.ConnectAsync(IPAddress.Loopback, DEFAULT_PORT);
                Console.WriteLine("Підключення до сервера встановлено.");
                inputCancellation = new CancellationTokenSource();

                using var stream = client.GetStream();

                var receivingTask = Task.Run(async () =>
                {
                    while (client.Connected)
                    {
                        var buffer = new byte[DEFAULT_BUFLEN];
                        int bytesReceived = await stream.ReadAsync(buffer, 0, buffer.Length);

                        if (bytesReceived > 0)
                        {
                            string response = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                            Console.WriteLine($"\nВідповідь від сервера: {response}");
                        }
                        else
                        {
                            Console.WriteLine("З'єднання з сервером перервано.");
                            inputCancellation.Cancel();
                            break;
                        }
                    }
                });

                if (pendingMessage != null)
                {
                    string time = DateTime.Now.ToString("HH:mm:ss");
                    string formattedPending = $"[{nickname}][{time}] {pendingMessage}";
                    byte[] pendingBytes = Encoding.UTF8.GetBytes(formattedPending);
                    await stream.WriteAsync(pendingBytes, 0, pendingBytes.Length);
                    Console.WriteLine($"Збережене повідомлення надіслано: {formattedPending}");
                    pendingMessage = null;
                }

                while (client.Connected)
                {
                    Console.Write("Введіть повідомлення для надсилання серверу: ");
                    var readTask = Task.Run(() => Console.ReadLine(), inputCancellation.Token);
                    var completedTask = await Task.WhenAny(readTask, receivingTask);

                    if (completedTask == receivingTask)
                    {
                        if (!readTask.IsCompleted)
                            inputCancellation.Cancel();

                        pendingMessage = await readTask;
                        break;
                    }

                    var message = readTask.Result;

                    if (string.IsNullOrEmpty(message))
                        break;

                    string sendTime = DateTime.Now.ToString("HH:mm:ss");
                    string formattedMessage = $"[{nickname}][{sendTime}] {message}";
                    byte[] messageBytes = Encoding.UTF8.GetBytes(formattedMessage);
                    await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
                    Console.WriteLine($"Повідомлення надіслано: {formattedMessage}");
                }

                await receivingTask;
            }
            catch
            {
                Console.WriteLine("Сервер недоступний. Спроба підключення знову через 3 секунди...");
                await Task.Delay(3000);
            }
        }
    }
}