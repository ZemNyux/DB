using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using System.Diagnostics;

class Server
{
    private const int DEFAULT_BUFLEN = 512;
    private const int DEFAULT_PORT = 27015;
    private static ConcurrentQueue<(TcpClient sender, byte[] data)> messageQueue = new ConcurrentQueue<(TcpClient, byte[])>();
    private static TcpListener? listener;
    private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private static ConcurrentDictionary<int, (TcpClient client, string ip, int port)> clients = new ConcurrentDictionary<int, (TcpClient, string, int)>();
    private static int clientCounter = 0;

    static async Task Main()
    {
        string processName = Process.GetCurrentProcess().ProcessName;
        var processes = Process.GetProcessesByName(processName);
        if (processes.Length > 1)
        {
            Console.WriteLine("Сервер вже запущено.");
            return;
        }

        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "СЕРВЕРНА СТОРОНА";
        Console.WriteLine("Процес сервера запущено!");

        _ = Task.Run(async () =>
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                string? input = Console.ReadLine();
                if (input?.ToLower() == "exit")
                {
                    Console.WriteLine("Процес сервера завершує роботу...");
                    await StopServerAsync();
                    cancellationTokenSource.Cancel();
                    break;
                }
            }
        }, cancellationTokenSource.Token);

        Console.CancelKeyPress += async (sender, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("Сервер завершує роботу...");
            await StopServerAsync();
            cancellationTokenSource.Cancel();
        };

        try
        {
            listener = new TcpListener(IPAddress.Any, DEFAULT_PORT);
            listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Start();
            Console.WriteLine("Будь ласка, запустіть одну або кілька клієнтських програм.");

            _ = ProcessMessages();

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    int clientId = Interlocked.Increment(ref clientCounter);
                    var clientEndPoint = (IPEndPoint)client.Client.RemoteEndPoint!;
                    clients.TryAdd(clientId, (client, clientEndPoint.Address.ToString(), clientEndPoint.Port));
                    Console.WriteLine($"Клієнт #{clientId} підключився: IP {clientEndPoint.Address}, Порт {clientEndPoint.Port}");
                    _ = HandleClientAsync(client, clientId);
                }
                catch (OperationCanceledException) { }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted || ex.SocketErrorCode == SocketError.OperationAborted) { }
            }
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
        {
            Console.WriteLine("Порт 27015 вже використовується.");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            if (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
                Console.ReadKey();
            }
        }
        finally
        {
            await StopServerAsync();
            cancellationTokenSource.Dispose();
        }
    }

    private static async Task HandleClientAsync(TcpClient client, int clientId)
    {
        NetworkStream? stream = null;
        try
        {
            stream = client.GetStream();
            while (!cancellationTokenSource.Token.IsCancellationRequested && client.Connected)
            {
                var buffer = new byte[DEFAULT_BUFLEN];
                int bytesReceived = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationTokenSource.Token).ConfigureAwait(false);

                if (bytesReceived > 0)
                {
                    messageQueue.Enqueue((client, buffer[..bytesReceived]));
                    Console.WriteLine($"Клієнт #{clientId}: Додано повідомлення до черги.");
                }
                else
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка з клієнтом #{clientId}: {ex.Message}");
        }
        finally
        {
            stream?.Dispose();
            client.Close();
            clients.TryRemove(clientId, out _);
            Console.WriteLine($"Клієнт #{clientId} від'єднався.");
        }
    }

    private static async Task StopServerAsync()
    {
        try
        {
            cancellationTokenSource.Cancel();

            foreach (var clientInfo in clients.Values)
            {
                try
                {
                    clientInfo.client.Close();
                    clientInfo.client.Dispose();
                    Console.WriteLine($"Клієнт з IP {clientInfo.ip}:{clientInfo.port} закрито.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка при закритті клієнта {clientInfo.ip}:{clientInfo.port}: {ex.Message}");
                }
            }
            clients.Clear();

            listener?.Stop();
            listener = null;

            Console.WriteLine("Сервер повністю зупинено.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка при зупинці сервера: {ex.Message}");
        }
        finally
        {
            await Task.Delay(100).ConfigureAwait(false);
        }
    }

    private static async Task ProcessMessages()
    {
        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                if (messageQueue.TryDequeue(out var item))
                {
                    var (sender, buffer) = item;
                    if (!sender.Connected) continue;

                    string message = Encoding.UTF8.GetString(buffer);
                    var senderEndPoint = (IPEndPoint)sender.Client.RemoteEndPoint!;
                    int senderId = clients.FirstOrDefault(x => x.Value.ip == senderEndPoint.Address.ToString() && x.Value.port == senderEndPoint.Port).Key;
                    Console.WriteLine($"Клієнт #{senderId} надіслав повідомлення: {message}");

                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                    foreach (var kvp in clients)
                    {
                        if (kvp.Key == senderId) continue;

                        var (targetClient, ip, port) = kvp.Value;
                        if (!targetClient.Connected) continue;

                        try
                        {
                            var stream = targetClient.GetStream();
                            await stream.WriteAsync(messageBytes, 0, messageBytes.Length, cancellationTokenSource.Token).ConfigureAwait(false);
                            Console.WriteLine($"Повідомлення переслано клієнту #{kvp.Key}");
                        }
                        catch
                        {
                            Console.WriteLine($"Не вдалося надіслати повідомлення клієнту #{kvp.Key}.");
                        }
                    }
                }

                await Task.Delay(15, cancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка в ProcessMessages: {ex.Message}");
            }
        }
    }
}