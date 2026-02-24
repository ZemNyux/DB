using System.Runtime.InteropServices;
using System.Text;

public delegate bool CallBack(int hwnd, int lParam);

public class OldFunctionsHolder
{
    [DllImport("user32.dll")]
    public static extern int EnumWindows(CallBack x, int nothing);

    [DllImport("user32")]
    public static extern int GetWindowText(int hWnd, StringBuilder text, int count);
}

class Program
{
    static int counter = 1;

    static void Main()
    {
        var myCallBack = new CallBack(Report);
        OldFunctionsHolder.EnumWindows(myCallBack, 0);
    }

    static bool Report(int hwnd, int lParam)
    {
        var text = new StringBuilder(255);
        OldFunctionsHolder.GetWindowText(hwnd, text, 255);
        Console.Write(counter++ + ". handle: {0,8}, caption: {1}\n", hwnd, text);
        Thread.Sleep(100);
        return true;
    }
}

/*
Спочатку були визначені Win32-функції EnumWindows і GetWindowText за допомогою атрибуту Dlllmport.
Потім - делегат Callback і метод Report. Після цього залишається тільки в методі Main створити
екземпляр делегата Callback (передаючи йому метод Report), і викликати метод EnumWindows.
Для кожного вікна, знайденого в системі, Windows викличе метод Report.

Метод Report цікавий, оскільки використовує клас StringBuilder для створення рядка
фіксованої довжини, який він передає функції GetWindowText. Тому функція GetWindowText
визначається так:

static extern int GetWindowText(int hWnd, StringBuilder text, int count);

Причина в тому, що функції DLL не дозволяється змінювати рядок, так що не вийде використовувати
цей тип. І навіть якщо спробувати здійснити передачу за посиланням, код, що викликає, не може
ініціалізувати рядок з правильним розміром. Тут в справу вступає клас StringBuilder.
Оскільки довжина тексту не перевищує максимального значення, переданого конструктору
StringBuilder, об'єкт StringBuilder може бути розіменуваний і модифікований викликаною функцією.
*/