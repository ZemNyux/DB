using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    private static UdpClient? _udpClient;
    private static IPEndPoint? _serverEndPoint;
    private static string _nickname = "";
    private static int _colorCode = 7; // за замовчуванням білий

    static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "UDP ЧАТ - Клієнт";

        try
        {
            _udpClient = new UdpClient();
            _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 9000);

            // === Реєстрація клієнта ===
            Console.Write("Введіть ваш нікнейм: ");
            _nickname = Console.ReadLine()?.Trim() ?? "Анонім";

            Console.Write("Введіть колір повідомлень (1-15): ");
            if (int.TryParse(Console.ReadLine(), out int color) && color >= 1 && color <= 15)
                _colorCode = color;
            else
                Console.WriteLine("Невірний колір, використовується стандартний (7)");

            // Відправляємо інформацію про себе на сервер
            string registrationMsg = $"{_nickname}|{_colorCode}|Приєднався до чату";
            byte[] regData = Encoding.UTF8.GetBytes(registrationMsg);
            await _udpClient.SendAsync(regData, regData.Length, _serverEndPoint);

            Console.WriteLine($"\nВи підключилися як {_nickname}. Колір: {_colorCode}");
            Console.WriteLine("Напишіть 'exit' для виходу.\n");

            // Запускаємо отримання повідомлень
            _ = Task.Run(ReceiveMessagesAsync);

            // Головний цикл введення повідомлень
            while (true)
            {
                string? input = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(input))
                    continue;

                if (input.ToLower() == "exit")
                {
                    string exitMsg = $"{_nickname}|{_colorCode}|Вийшов з чату";
                    byte[] exitData = Encoding.UTF8.GetBytes(exitMsg);
                    await _udpClient.SendAsync(exitData, exitData.Length, _serverEndPoint);
                    break;
                }

                // Відправляємо звичайне повідомлення
                string messageToSend = $"{_nickname}|{_colorCode}|{input}";
                byte[] data = Encoding.UTF8.GetBytes(messageToSend);
                await _udpClient.SendAsync(data, data.Length, _serverEndPoint);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка: {ex.Message}");
        }
        finally
        {
            _udpClient?.Close();
            Console.WriteLine("З'єднання закрито. Натисніть будь-яку клавішу...");
            Console.ReadKey();
        }
    }

    private static async Task ReceiveMessagesAsync()
    {
        try
        {
            while (true)
            {
                var result = await _udpClient!.ReceiveAsync();
                string message = Encoding.UTF8.GetString(result.Buffer);

                // Виводимо повідомлення з кольором
                Console.ForegroundColor = (ConsoleColor)_colorCode;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }
        catch (Exception)
        {
            // Ігноруємо помилки при отриманні (наприклад, при закритті)
        }
    }
}