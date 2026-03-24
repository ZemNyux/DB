using System.Net;
using System.Net.Sockets;
using System.Text; // простір імен для роботи з кодуваннями

class Client
{
    private const int MESSAGE_LENGTH = 512;
    private const int SERVER_PORT = 27015;
    
    static void Main()
    {
        Console.Title = "CLIENT";
        Console.OutputEncoding = Encoding.UTF8; // кирилиця

        try
        {
            var ipAddress = IPAddress.Parse("127.0.0.1"); // IP-адреса локального хоста (127.0.0.1), яка використовується для підключення до сервера на поточному пристрої
            var remoteEndPoint = new IPEndPoint(ipAddress, SERVER_PORT);

            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            clientSocket.Connect(remoteEndPoint); // ініціює підключення клієнта до сервера за вказаною кінцевою точкою (IP-адреса та порт)

            while (true)
            {
                // відправка запиту на сервер
                Console.Write("Введіть повідомлення для сервера: ");
                string? message = Console.ReadLine();
                byte[] messageBytes = Encoding.UTF8.GetBytes(message!);
                clientSocket.Send(messageBytes);
                Console.WriteLine($"Повідомлення надіслано: {message}");

                // отримання відповіді від сервера
                var buffer = new byte[MESSAGE_LENGTH];
                int bytesReceived = clientSocket.Receive(buffer);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                Console.WriteLine($"Відповідь від сервера: {response}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Сталася помилка: {ex.Message}");
        }
    }
}