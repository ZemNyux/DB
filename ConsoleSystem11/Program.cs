// ------------------------------------------------------------------------------------------
//class MyThreads
//{
//    static void Main()
//    {
//        Console.WriteLine("Hello from Main " + Thread.CurrentThread.ManagedThreadId); // 2

//        for (int i = 0; i < 500000; i++)
//            new Thread(Fun).Start(); // 9

//        // Fun(); // 2

//        Thread.Sleep(10000);
//    }

//    private static void Fun()
//    {
//        Console.WriteLine("Hello from method Fun!");
//        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
//        Thread.Sleep(new Random().Next(1000, 10000));
//        Console.WriteLine("End of additional thread");
//    }
//}
// ------------------------------------------------------------------------------------------
//class MyThreads
//{
//    static int result = -1;

//    static void Main()
//    {
//        var t = new Thread(Fun);
//        t.Start(); // start separate thread to execute method Fun
//        for (int i = 0; i < 10; i++)
//        {
//            Console.WriteLine("main doing something...");
//            Thread.Sleep(500);
//        }

//        t.Join(2000);

//        Console.WriteLine("RESULT OF SECOND THREAD: " + result);
//        Console.WriteLine("END OF MAIN");
//    }

//    private static void Fun()
//    {
//        Console.WriteLine("Hello from method Fun (second thread)");

//        Thread.Sleep(2500);
//        result = 42;

//        Console.WriteLine("End of second thread");
//    }
//}
// ------------------------------------------------------------------------------------------
//using System.Text;

//class Program
//{
//    static object critical = new object();
//    static bool run = true;

//    static void Run(object? nameObj)
//    {
//        string name = nameObj + "" ?? "Потік";
//        long state = 0;
//        const int step = 10000;

//        for (long l = 0; l < long.MaxValue && run; l++)
//        {
//            state = l;

//            if (l % step == 0)
//            {
//                lock (critical) // лок потрібен для синхронізаці потоків, курсор в консолі - це ресурс, що поділяється на три потоки, і в якийсь момент часу курсор може бути перехоплений
//                { // дана область видимості - це так звана критична секція, більш ніж один потік за раз сюди не зайде
//                    if (name == "Максимальний") Console.SetCursorPosition(0, 1);
//                    else if (name == "Нормальний") Console.SetCursorPosition(0, 2);
//                    else if (name == "Мінімальний") Console.SetCursorPosition(0, 3);
//                    Console.WriteLine($"{name}: {state / step}");
//                }
//            }
//        }
//    }

//    static void Main()
//    {
//        Console.OutputEncoding = Encoding.UTF8;
//        Console.WriteLine("Запуск потоків з різними пріорітетами...");
//        Console.Title = "Натисніть будь-яку клавішу для продовження";
//        Console.CursorVisible = false;

//        var t1 = new Thread(Run) { IsBackground = true, Priority = ThreadPriority.Highest };
//        var t2 = new Thread(Run) { IsBackground = true, Priority = ThreadPriority.Normal };
//        var t3 = new Thread(Run) { IsBackground = true, Priority = ThreadPriority.Lowest };

//        t1.Start("Максимальний");
//        t2.Start("Нормальний");
//        t3.Start("Мінімальний");

//        Console.ReadKey();
//        run = false;
//        Console.SetCursorPosition(0, 10);
//        Console.WriteLine("\nПрограму завершено.");
//        Thread.Sleep(100000);
//    }
//}
// ------------------------------------------------------------------------------------------
//using System.Runtime.InteropServices;

//class ThreadPriorityExample
//{
//    static object locker = new();
//    static Random random = new();

//    const int STD_OUTPUT_HANDLE = -11;
//    static IntPtr consoleHandle = GetStdHandle(STD_OUTPUT_HANDLE);

//    [StructLayout(LayoutKind.Sequential)]
//    public struct COORD
//    {
//        public short X;
//        public short Y;

//        public COORD(short x, short y)
//        {
//            X = x;
//            Y = y;
//        }
//    }

//    const ushort RED = 0x0C;
//    const ushort BLUE = 0x09;

//    static int redCount = 0;
//    static int blueCount = 0;
//    static int totalCount = 0;

//    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
//    public static extern IntPtr GetStdHandle(int nStdHandle);

//    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
//    public static extern bool WriteConsoleOutputCharacter(IntPtr hConsoleOutput, string lpCharacter, uint nLength, COORD dwWriteCoord, out uint lpNumberOfCharsWritten);

//    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
//    public static extern bool WriteConsoleOutputAttribute(IntPtr hConsoleOutput, ushort[] lpAttribute, uint nLength, COORD dwWriteCoord, out uint lpNumberOfAttrsWritten);

//    static void Main()
//    {
//        new Thread(() => PrintColor(RED)) { Priority = ThreadPriority.Highest }.Start(); // Normal --- Lowest !!!
//        new Thread(() => PrintColor(BLUE)) { Priority = ThreadPriority.Lowest }.Start(); // Normal --- Highest

//        while (true)
//        {
//            double redPercent = (double)redCount / totalCount * 100;
//            double bluePercent = (double)blueCount / totalCount * 100;
//            double redOverBlue = redPercent - bluePercent;

//            string title = $"RED DOGS: {redCount} --- BLUE DOGS: {blueCount}" +
//                           $" --- TOTAL: {totalCount}" +
//                           $" --- RED%: {redPercent:F2}%" +
//                           $" --- BLUE%: {bluePercent:F2}%" +
//                           $" --- RED ADVANTAGE%: {redOverBlue:F2}%";
//            Console.Title = title;

//            Thread.Sleep(1); // пауза в 1мс зробить додаткове навантаження на потоки процесу, конкуренція червоних та синіх буде помітніше
//        }
//    }

//    static void PrintColor(ushort color)
//    {
//        while (true)
//        {
//            lock (locker)
//            {
//                var x = (short)random.Next(0, 80);
//                var y = (short)random.Next(0, 25);

//                ushort[] colorAttr = [color];

//                WriteConsoleOutputCharacter(consoleHandle, "@", 1, new COORD(x, y), out uint written);
//                WriteConsoleOutputAttribute(consoleHandle, colorAttr, 1, new COORD(x, y), out uint attrsWritten);

//                if (color == RED) redCount++;
//                else blueCount++;

//                totalCount++;
//            }
//        }
//    }
//}
// ------------------------------------------------------------------------------------------