using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("Управління процесами (оновлення кожні ~10 секунд)");
        Console.WriteLine("-------------------------------------------------\n");

        while (true)
        {
            ShowProcessesAndMenu();
            Thread.Sleep(10000);
            Console.Clear();
        }
    }

    static void ShowProcessesAndMenu()
    {
        Process[] processes = Process.GetProcesses().OrderBy(p => p.ProcessName).ToArray();

        Console.WriteLine($"Знайдено процесів: {processes.Length}");
        Console.WriteLine("  PID       Ім'я процесу                  Пріоритет");
        Console.WriteLine("------------------------------------------------------------");

        int line = 0;
        foreach (var p in processes)
        {
            try
            {
                string name = p.ProcessName.Length > 28 ? p.ProcessName.Substring(0, 25) + "..." : p.ProcessName.PadRight(28);

                Console.WriteLine($"{p.Id,8}   {name}   {p.PriorityClass,-12}");
            }
            catch
            {
                Thread.Sleep(10000);
            }

            line++;
            if (line % 20 == 0 && line < processes.Length)
            {
                Console.WriteLine("\nНатисніть Enter для продовження списку");
                Console.ReadLine();
                Console.Clear();
                Console.WriteLine("Продовження списку\n");
                Console.WriteLine("  PID       Ім'я процесу                  Пріоритет (Base)");
                Console.WriteLine("------------------------------------------------------------");
            }
        }

        Console.WriteLine("\nКоманди:");
        Console.WriteLine("  k <PID>     — завершити процес");
        Console.WriteLine("  p <PID>     — змінити пріоритет");
        Console.WriteLine("  s <ім'я>    — запустити новий процес");
        Console.WriteLine("  r           — оновити список зараз");
        Console.WriteLine("  q           — вийти з програми");
        Console.WriteLine("  Enter — оновити через 10 секунд");

        Console.Write("\nВведіть команду: ");
        string input = Console.ReadLine()?.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(input))
            return;

        if (input == "q")
        {
            Console.WriteLine("Завершення програми");
            Environment.Exit(0);
        }

        if (input == "r")
            return;

        string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            Console.WriteLine("Неправильний формат команди");
            Console.ReadKey(true);
            return;
        }

        string cmd = parts[0];
        string arg = parts[1];

        if (!int.TryParse(arg, out int pid) && (cmd == "k" || cmd == "p"))
        {
            Console.WriteLine("Очікується числовий PID");
            Console.ReadKey(true);
            return;
        }

        switch (cmd)
        {
            case "k":
                KillProcess(pid);
                break;

            case "p":
                ChangePriority(pid);
                break;

            case "s":
                StartNewProcess(arg);
                break;

            default:
                Console.WriteLine("Невідома команда");
                Console.ReadKey(true);
                break;
        }
    }

    static void KillProcess(int pid)
    {
        try
        {
            Process p = Process.GetProcessById(pid);
            Console.Write($"Завершуємо {p.ProcessName} (PID {pid}) ... ");

            if (p.CloseMainWindow())
            {
                if (p.WaitForExit(3000))
                {
                    return;
                }
            }

            p.Kill();
            p.WaitForExit(2000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка: {ex.Message}");
        }
        Console.ReadKey(true);
    }

    static void ChangePriority(int pid)
    {
        try
        {
            Process p = Process.GetProcessById(pid);
            Console.WriteLine($"Поточний пріоритет: {p.PriorityClass}");

            Console.WriteLine("Новий пріоритет:");
            Console.WriteLine("  1 = Idle");
            Console.WriteLine("  2 = BelowNormal");
            Console.WriteLine("  3 = AboveNormal");
            Console.WriteLine("  4 = High");
            Console.WriteLine("  5 = RealTime");
            Console.Write("\nВибір (1-6): ");

            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > 5)
            {
                Console.WriteLine("Невірний вибір");
                return;
            }

            ProcessPriorityClass newPriority = choice switch
            {
                1 => ProcessPriorityClass.Idle,
                2 => ProcessPriorityClass.BelowNormal,
                3 => ProcessPriorityClass.AboveNormal,
                4 => ProcessPriorityClass.High,
                5 => ProcessPriorityClass.RealTime,
            };

            p.PriorityClass = newPriority;
            Console.WriteLine($"Пріоритет змінено на {newPriority}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка: {ex.Message} (можливо, недостатньо прав)");
        }
        Console.ReadKey(true);
    }

    static void StartNewProcess(string processName)
    {
        try
        {
            string fileName = processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? processName : processName + ".exe";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = true
            };

            Console.WriteLine($"Спроба запуску: {fileName}");
            Process.Start(startInfo);
            Console.WriteLine("Запущено");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не вдалося запустити: {ex.Message}");
        }
        Console.ReadKey(true);
    }
}