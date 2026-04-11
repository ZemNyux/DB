using System.Net.Sockets;
using System.Text;

public class ClientNetwork
{
    private TcpClient? _client;
    private StreamWriter? _writer;
    private StreamReader? _reader;

    public async Task StartAsync()
    {
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync("localhost", 27015);
        }
        catch
        {
            Console.WriteLine("Cannot connect to server! Make sure server is running.");
            return;
        }

        var stream = _client.GetStream();
        _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        _reader = new StreamReader(stream, Encoding.UTF8);

        Console.WriteLine("Connected! Use arrow keys to move. You are BLUE 😊");

        var readTask = ReadFromServerAsync();
        var inputTask = HandleInputAsync();

        await Task.WhenAny(readTask, inputTask);
    }

    private GameState? _lastState;

    private async Task ReadFromServerAsync()
    {
        try
        {
            while (true)
            {
                // Читаем заголовок
                string? header = await _reader!.ReadLineAsync();
                if (header == null) break;

                // Читаем карту (Height строк по Width символов = одна строка)
                string? mapLine = await _reader.ReadLineAsync();
                if (mapLine == null) break;

                string full = header + "\n" + mapLine;
                _lastState = GameState.Deserialize(full);

                RenderMap(_lastState);

                if (_lastState.GameOver)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n GAME OVER! {_lastState.Result.Replace('_', ' ')}");
                    Console.ResetColor();
                    break;
                }
            }
        }
        catch { }
    }

    private async Task HandleInputAsync()
    {
        while (true)
        {
            if (_lastState?.GameOver == true) break;
            if (!Console.KeyAvailable) { await Task.Delay(50); continue; }

            var key = Console.ReadKey(true).Key;
            if (_lastState == null) continue;

            int nx = _lastState.BlueX, ny = _lastState.BlueY;
            switch (key)
            {
                case ConsoleKey.LeftArrow: nx--; break;
                case ConsoleKey.RightArrow: nx++; break;
                case ConsoleKey.UpArrow: ny--; break;
                case ConsoleKey.DownArrow: ny++; break;
                default: continue;
            }

            try { await _writer!.WriteLineAsync($"{nx} {ny}"); }
            catch { break; }
        }
    }

    private void RenderMap(GameState state)
    {
        Console.Clear();
        Console.WriteLine($"=== MAZE GAME === Blue score: {state.BlueScore} | Red score: {state.RedScore}");
        Console.WriteLine("You are BLUE 😊 (arrow keys) | RED = server player");

        for (int y = 0; y < GameMap.Height; y++)
        {
            for (int x = 0; x < GameMap.Width; x++)
            {
                if (x == state.BlueX && y == state.BlueY)
                { Console.ForegroundColor = ConsoleColor.Blue; Console.Write("😊"); Console.ResetColor(); }
                else if (x == state.RedX && y == state.RedY)
                { Console.ForegroundColor = ConsoleColor.Red; Console.Write("😊"); Console.ResetColor(); }
                else
                {
                    char c = state.Map[y, x];
                    Console.ForegroundColor = c switch
                    {
                        '#' => ConsoleColor.DarkGray,
                        'T' => ConsoleColor.Yellow,
                        'F' => ConsoleColor.Green,
                        _ => ConsoleColor.White
                    };
                    Console.Write(c == '#' ? "██" : c == 'T' ? "📦" : c == 'F' ? "🏁" : "  ");
                    Console.ResetColor();
                }
            }
            Console.WriteLine();
        }
    }
}