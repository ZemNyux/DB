using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("Створення процесу notepad.exe...");
        Process process = new Process();
        process.StartInfo.FileName = "notepad.exe";

        // process.StartInfo.Verb = "runas"; // запуск від імені адміністратора

        // FileName - вказує шлях до виконуваного файлу (наприклад, "notepad.exe"), який буде запущено, це обов’язкове поле
        // Arguments - вказує аргументи командного рядка, що передаються процесу, який запускається (наприклад, "file.txt")
        // WorkingDirectory - вказує робочу директорію для процесу, що запускається. Якщо не вказано, використовується поточна директорія

        // UseShellExecute - вказує, чи використовувати оболонку операційної системи для запуску процесу
        // якщо true, можна запускати процеси, що не є виконуваними файлами (наприклад, URL-посилання, документи)
        // якщо false, використовується низькорівневий запуск

        // RedirectStandardOutput - вказує, чи потрібно перенаправити стандартний вивід процесу в батьківський процес
        // RedirectStandardError - вказує, чи потрібно перенаправити стандартний потік помилок процесу
        // RedirectStandardInput - вказує, чи потрібно перенаправити стандартний ввід процесу
        // CreateNoWindow - вказує, чи потрібно створювати вікно для процесу, що запускається. Якщо true, вікно не відображатиметься
        // WindowStyle - визначає стиль вікна процесу (наприклад, нормальне вікно, згорнуте або приховане)
        // EnvironmentVariables - дозволяє задати або отримати змінні середовища для процесу, що запускається
        // UserName - вказує ім’я користувача, від імені якого має бути запущено процес
        // Password - вказує пароль для аутентифікації користувача (якщо потрібно)

        process.Start();

        Thread.Sleep(2000);

        Console.WriteLine("Виберіть спосіб завершення процесу:");
        Console.WriteLine("1 - Очікувати завершення процесу (рекомендований спосіб)");
        Console.WriteLine("2 - Спробувати завершити процес через CloseMainWindow()");
        Console.WriteLine("3 - Примусово завершити один процес через Kill()");
        Console.WriteLine("4 - Завершити всі процеси за ім’ям");
        Console.WriteLine("5 - Примусово завершити через TerminateProcess()");
        Console.WriteLine("6 - Завершити поточний процес через ExitProcess()");

        var choice = Console.ReadKey().KeyChar;
        Console.WriteLine();

        switch (choice)
        {
            case '1':
                Console.WriteLine("Очікування завершення процесу природним шляхом...");
                process.WaitForExit(); // поточний процес продовжить роботу, щойно закриється блокнот
                break;
            case '2':
                Console.WriteLine("Спроба закрити процес через CloseMainWindow()...");
                if (!process.CloseMainWindow())
                {
                    // Paint закриється, а блокнот чи калькулятор — ні, тому що вони є UWP (Universal Windows Platform) застосунками, а не звичайними Win32-застосунками. UWP-застосунки мають суворіші обмеження щодо доступу та безпеки, і для їх завершення потрібен особливий підхід.
                    Console.WriteLine("Не вдалося закрити через CloseMainWindow()");
                }
                break;
            case '3':
                Console.WriteLine("Примусове завершення через Kill()...");
                // process.Kill(); // для Paint і більшості застосунків підійде
                // якщо спробувати завершити конкретний процес через process.Kill(), система може запобігти цьому, якщо процес захищений (актуально для UWP)

                // тому, іноді щоб зупинити один процес, необхідно запустити інший процес :)
                var taskkillProcess = new Process();
                taskkillProcess.StartInfo.FileName = "taskkill";
                taskkillProcess.StartInfo.Arguments = "/IM notepad.exe /F"; // завершуємо процес калькулятора/блокнота з прапорцем Force
                taskkillProcess.StartInfo.UseShellExecute = false;
                taskkillProcess.StartInfo.CreateNoWindow = true;
                taskkillProcess.Start();
                break;
            case '4':
                Console.WriteLine("Пошук усіх процесів блокнота...");
                Process[] processes = Process.GetProcessesByName("notepad");

                // якщо знайти всі процеси з певним ім’ям, вони будуть отримані в контексті різних екземплярів, які можуть бути простішими для завершення (порівняно з одним конкретним захищеним екземпляром)
                if (processes.Length == 0)
                {
                    Console.WriteLine("Процеси блокнота не знайдено.");
                }
                else
                {
                    Console.WriteLine($"Знайдено {processes.Length} процесів блокнота. Закриваємо...");
                    foreach (var p in processes)
                    {
                        try
                        {
                            p.Kill();
                            Console.WriteLine($"Процес {p.Id} закрито.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Помилка при завершенні процесу {p.Id}: {ex.Message}");
                        }
                    }
                }
                break;
            case '5':
                try
                {
                    // метод Process.Kill() є стандартним методом .NET для завершення
                    // процесу. Він завершує процес за ID, але може не спрацювати для деяких
                    // захищених процесів, таких як UWP-застосунки (наприклад, блокнот,
                    // калькулятор та інші застосунки, що працюють у захищеному середовищі). Причина
                    // у тому, що такі процеси мають особливі обмеження на доступ і управління,
                    // і для їх завершення потрібен особливий підхід.
                    // переваги:
                    // - простота використання
                    // - добре працює зі звичайними Win32-процесами
                    // - обробляється засобами .NET
                    // недоліки:
                    // - не завжди працює із захищеними процесами (наприклад, UWP)
                    // - можуть виникати проблеми з правами доступу при спробі завершити процеси, що працюють із іншими рівнями безпеки

                    // метод TerminateProcess є низькорівневим викликом Windows API, який
                    // завершує процес безпосередньо на рівні операційної системи. Вимагає
                    // використання DllImport і роботи з низькорівневими API Windows. Може бути
                    // складнішим у використанні та не таким «чистим» з точки зору архітектури, як
                    // використання стандартних методів .NET. Не надає такого ж рівня
                    // контролю, як Kill(), наприклад, не можна коректно обробити завершення
                    // процесу (наприклад, дочекатися виходу)

                    bool result = TerminateProcess(process.Handle, 1);
                    if (!result)
                    {
                        Console.WriteLine($"Не вдалося завершити процес через TerminateProcess. Код помилки: {Marshal.GetLastWin32Error()}");
                    }
                    else
                    {
                        Console.WriteLine("Процес було примусово завершено.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка при завершенні процесу: {ex.Message}");
                }
                break;
            case '6':
                try
                {
                    Console.WriteLine("Завершуємо поточний процес через Environment.Exit...");
                    Environment.Exit(0);
                    // Application.Exit(); // для застосунків WF і WPF

                    // Console.WriteLine("Завершуємо процес через ExitProcess...");
                    // ExitProcess(1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка при завершенні процесу: {ex.Message}");
                }
                break;
            default:
                Console.WriteLine("Невірний вибір.");
                break;
        }

        Console.WriteLine("Головний потік завершив виконання.");
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern void ExitProcess(uint uExitCode);
}