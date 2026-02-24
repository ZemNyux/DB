using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static Program;

public static class DllImports
{
    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc enumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

    [DllImport("user32.dll")]
    public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr CreatePolygonRgn(POINT[] lpPoints, int nCount, int fnPolyFillMode);

    [DllImport("gdi32.dll")]
    public static extern bool RectInRegion(IntPtr hRegion, ref RECT rect);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
}

public class Program
{
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    public struct POINT
    {
        public int x;
        public int y;
    }

    public class Data
    {
        public IntPtr hWnd;
        public RECT rect;
        public Direction direction;
        public IntPtr bang;
        public int step;
    }

    static List<Data> hList = new List<Data>();
    static int high = 100;
    static int width = 100;
    static RECT rectScr;

    static void OnProcessExit(object? sender, EventArgs e)
    {
        var processes = Process.GetProcessesByName("notepad");

        foreach (var process in processes)
        {
            try
            {
                process.Kill();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не вдалося завершити процес {process.ProcessName}: {ex.Message}");
            }
        }
    }

    static void Main()
    {
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        var rand = new Random();
        DllImports.GetWindowRect(DllImports.GetDesktopWindow(), ref rectScr);

        for (int i = 0; i < 5; i++)
        {
            Process.Start("notepad");
        }
        Thread.Sleep(1500);

        var moveTimer = new Timer(TimerProcCallback, null, 0, 15);

        DllImports.EnumWindows(EnumWindowsProcCallback, IntPtr.Zero);

        Thread.Sleep(100000);
    }

    static bool EnumWindowsProcCallback(IntPtr hWnd, IntPtr lParam)
    {
        const int bufferSize = 30;
        var className = new StringBuilder(bufferSize);
        DllImports.GetClassName(hWnd, className, bufferSize);

        if (DllImports.IsWindowVisible(hWnd) && className.ToString() == "Notepad")
        {
            var app = new Data
            {
                hWnd = hWnd
            };
            DllImports.GetWindowRect(hWnd, ref app.rect);
            app.direction = (Direction)new Random().Next(1, 5);
            app.bang = IntPtr.Zero;
            app.step = new Random().Next(1, 15);

            hList.Add(app);
            DllImports.MoveWindow(hWnd, new Random().Next(rectScr.right) - width, new Random().Next(rectScr.bottom) - high, width, high, true);
        }
        return true;
    }

    static void TimerProcCallback(object state)
    {
        TimerProc();
    }

    static void TimerProc()
    {
        for (int i = 0; i < hList.Count; i++)
        {
            var app = hList[i];

            var rect = new RECT();
            DllImports.GetWindowRect(app.hWnd, ref rect);

            int newX = rect.left;
            int newY = rect.top;

            switch (app.direction)
            {
                case Direction.LeftUp:
                    {
                        if (rect.left <= rectScr.left + 10)
                            app.direction = Direction.RightUp;
                        else if (rect.top <= rectScr.top + 10)
                            app.direction = Direction.LeftDown;
                        else
                        {
                            newX -= app.step;
                            newY -= app.step;
                        }
                        break;
                    }
                case Direction.LeftDown:
                    {
                        if (rect.left <= rectScr.left + 10)
                            app.direction = Direction.RightDown;
                        else if (rect.bottom >= rectScr.bottom - 50)
                            app.direction = Direction.LeftUp;
                        else
                        {
                            newX -= app.step;
                            newY += app.step;
                        }
                        break;
                    }
                case Direction.RightUp:
                    {
                        if (rect.right >= rectScr.right - 10)
                            app.direction = Direction.LeftUp;
                        else if (rect.top <= rectScr.top + 10)
                            app.direction = Direction.RightDown;
                        else
                        {
                            newX += app.step;
                            newY -= app.step;
                        }
                        break;
                    }
                case Direction.RightDown:
                    {
                        if (rect.right >= rectScr.right - 10)
                            app.direction = Direction.LeftDown;
                        else if (rect.bottom >= rectScr.bottom - 50)
                            app.direction = Direction.RightUp;
                        else
                        {
                            newX += app.step;
                            newY += app.step;
                        }
                        break;
                    }
            }

            newX = Math.Max(rectScr.left, Math.Min(newX, rectScr.right - width));
            newY = Math.Max(rectScr.top, Math.Min(newY, rectScr.bottom - high));

            DllImports.MoveWindow(app.hWnd, newX, newY, width, high, true);
        }
    }

    public enum Direction
    {
        LeftUp = 1,
        LeftDown,
        RightUp,
        RightDown
    }
}