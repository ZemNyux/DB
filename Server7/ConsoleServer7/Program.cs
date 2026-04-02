using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class ChatServer
{
    private readonly UdpClient _udpClient;
    private readonly ConcurrentDictionary<IPEndPoint, string> _clients = new();
    private readonly ConcurrentBag<string> _messageHistory = new();

    public ChatServer(int port = 9000)
    {
        _udpClient = new UdpClient(port);
        Console.WriteLine($"Сервер запущено на порту {port}");
    }

    public async Task StartAsync()
    {
        while (true)
        {
            try
            {
                var result = await _udpClient.ReceiveAsync();
                var clientEndPoint = result.RemoteEndPoint;
                var message = Encoding.UTF8.GetString(result.Buffer);

                // Реєстрація нового клієнта
                if (!_clients.ContainsKey(clientEndPoint))
                {
                    _clients[clientEndPoint] = "Unknown"; // тимчасово, поки не отримаємо нік
                    Console.WriteLine($"Новий клієнт підключився: {clientEndPoint}");
                }

                // Обробка команди exit
                if (message.Trim().ToLower() == "exit")
                {
                    _clients.TryRemove(clientEndPoint, out _);
                    Console.WriteLine($"Клієнт від'єднався: {clientEndPoint}");
                    continue;
                }

                // Якщо повідомлення містить нік і колір (формат: NICK|COLOR|текст)
                if (message.Contains("|"))
                {
                    var parts = message.Split('|', 3);
                    if (parts.Length == 3)
                    {
                        string nickname = parts[0];
                        string colorStr = parts[1];
                        string text = parts[2];

                        _clients[clientEndPoint] = nickname;

                        string formattedMessage = $"[{DateTime.Now:HH:mm:ss}] {nickname}: {text}";
                        _messageHistory.Add(formattedMessage);

                        // Розсилка всім клієнтам
                        await BroadcastMessageAsync(formattedMessage, clientEndPoint);
                        continue;
                    }
                }

                // Якщо просто текст (для сумісності)
                string simpleMsg = $"[{DateTime.Now:HH:mm:ss}] {_clients[clientEndPoint]}: {message}";
                _messageHistory.Add(simpleMsg);
                await BroadcastMessageAsync(simpleMsg, clientEndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка сервера: {ex.Message}");
            }
        }
    }

    private async Task BroadcastMessageAsync(string message, IPEndPoint sender)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        foreach (var client in _clients.Keys)
        {
            if (!client.Equals(sender))
            {
                try
                {
                    await _udpClient.SendAsync(data, data.Length, client);
                }
                catch { /* ігноруємо помилки відправки */ }
            }
        }
    }

    public void Stop()
    {
        _udpClient.Close();
    }
}

class Program
{
    static async Task Main()
    {
        var server = new ChatServer(9000);
        await server.StartAsync();
    }
}