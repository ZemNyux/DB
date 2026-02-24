using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace WindowManagerLab
{
    class Program
    {
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        const int SW_MINIMIZE = 6;
        const int SW_RESTORE = 9;
        const int SW_MAXIMIZE = 3;

        const uint WM_CLOSE = 0x0010;
        const uint WM_SYSCOMMAND = 0x0112;
        const uint SC_MINIMIZE = 0xF020;

        static List<(IntPtr hWnd, string Title)> windows = new List<(IntPtr, string)>();

        static bool EnumCallback(IntPtr hWnd, IntPtr lParam)
        {
            if (GetWindowTextLength(hWnd) < 1)
                return true;

            var sb = new StringBuilder(256);
            GetWindowText(hWnd, sb, sb.Capacity);

            string title = sb.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(title))
            {
                windows.Add((hWnd, title));
            }
            return true;
        }

        static void PrintWindows()
        {
            windows.Clear();

            EnumWindows(EnumCallback, IntPtr.Zero);

            Console.WriteLine($"\nзнайдено видимих вікон: {windows.Count}");

            for (int i = 0; i < windows.Count; i++)
            {
                Console.WriteLine($"  {i + 1,3})  {windows[i].Title,-60}  (hWnd: 0x{windows[i].hWnd.ToInt64():X})");
            }
        }

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            while (true)
            {
                PrintWindows();

                if (windows.Count == 0)
                {
                    Console.WriteLine("\nнемає видимих вікон натисніть Enter");
                    Console.ReadLine();

                    continue;
                }

                Console.WriteLine("\nкоманди (введіть номер + команду):");
                Console.WriteLine("  5 move 100 800 600      перемістити/змінити розмір");
                Console.WriteLine("  5 hide                       сховати");
                Console.WriteLine("  5 show                       показати");
                Console.WriteLine("  5 min                        згорнути");
                Console.WriteLine("  5 max                        розгорнути на весь екран");
                Console.WriteLine("  5 restore                    відновити");
                Console.WriteLine("  5 close                      закрити (PostMessage WM_CLOSE)");
                Console.WriteLine("  5 rename \"Новий заголовок\"   перейменувати");
                Console.WriteLine("  r                            оновити список");
                Console.WriteLine("  q                            вихід\n");

                Console.Write("ваша команда: ");
                string line = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(line))
                    continue;
                if (line.ToLower() == "q") 
                    break;
                if (line.ToLower() == "r")
                    continue;

                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    Console.WriteLine("помилка: потрібно вказати номер вікна та команду");
                    Thread.Sleep(1800);
                    continue;
                }

                if (!int.TryParse(parts[0], out int num) || num < 1 || num > windows.Count)
                {
                    Console.WriteLine($"помилка: неправильний номер вікна (має бути від 1 до {windows.Count})");
                    Thread.Sleep(1800);
                    continue;
                }

                var target = windows[num - 1];
                IntPtr h = target.hWnd;

                string cmd = parts[1].ToLowerInvariant();

                Console.WriteLine($"вибрано вікно #{num}: \"{target.Title}\"");
                Console.WriteLine($"команда: {cmd}");

                try
                {
                    switch (cmd)
                    {
                        case "move":
                            if (parts.Length < 6)
                            {
                                Console.WriteLine("формат: <номер> move X Y ширина висота");
                                break;
                            }
                            if (int.TryParse(parts[2], out int x) &&
                                int.TryParse(parts[3], out int y) &&
                                int.TryParse(parts[4], out int w) &&
                                int.TryParse(parts[5], out int hgt))
                            {
                                bool ok = MoveWindow(h, x, y, w, hgt, true);
                                Console.WriteLine(ok ? "вікно переміщено/змінено розмір" : "помилка в MoveWindow");
                            }
                            else
                            {
                                Console.WriteLine("неправильні числа для координат/розміру");
                            }
                            break;

                        case "hide":
                            ShowWindow(h, SW_HIDE);
                            Console.WriteLine("вікно сховано");
                            break;

                        case "show":
                            ShowWindow(h, SW_SHOW);
                            Console.WriteLine("вікно показано");
                            break;

                        case "min":
                            ShowWindow(h, SW_MINIMIZE);
                            Console.WriteLine("вікно згорнуто");
                            break;

                        case "max":
                            ShowWindow(h, SW_MAXIMIZE);
                            Console.WriteLine("вікно максимізовано");
                            break;

                        case "restore":
                            ShowWindow(h, SW_RESTORE);
                            Console.WriteLine("вікно відновлено");
                            break;

                        case "close":
                            PostMessage(h, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                            Console.WriteLine("надіслано команду закриття (WM_CLOSE)");
                            break;

                        case "rename":
                            if (parts.Length < 3)
                            {
                                Console.WriteLine("формат: <номер> rename новий заголовок (можна в лапках)");
                                break;
                            }
                            string newTitle = string.Join(" ", parts, 2, parts.Length - 2);
                            bool okRename = SetWindowText(h, newTitle);
                            Console.WriteLine(okRename ? $"Заголовок змінено на: \"{newTitle}\"" : "не вдалося змінити заголовок (можливо, сучасне UWP-вікно)");
                            break;

                        default:
                            Console.WriteLine($"невідома команда: {cmd}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("виникла помилка: " + ex.Message);
                }

                Console.WriteLine("\nнатисніть Enter для продовження");
                Console.ReadLine();
            }
        }
    }
}