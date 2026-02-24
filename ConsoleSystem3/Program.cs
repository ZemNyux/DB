using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ChangeWindowTitleExample
{
    class Program
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("програма зміни заголовка вікна Блокнота");

            IntPtr hWnd = FindWindow("Notepad", null);

            string newTitle = "Мой Блокнот!";

            bool success = SetWindowText(hWnd, newTitle);

            if (success)
            {
                Console.WriteLine($"\nЗаголовок змінено на:\n«{newTitle}»");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("\nНе вдалося змінити заголовок");
            }
        }
    }
}