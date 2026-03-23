using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        string path = "numbers.txt";

        if (!File.Exists(path))
        {
            Console.WriteLine("Файл не знайдено");
            return;
        }

        List<int> numbers = File.ReadAllLines(path).Select(int.Parse).ToList();

        Console.WriteLine("Числа: " + string.Join(", ", numbers));

        var parallelNumbers = numbers.AsParallel();

        int sum = parallelNumbers.Sum();
        long product = parallelNumbers.Aggregate(1L, (acc, x) => acc * x);
        double average = parallelNumbers.Average();
        int max = parallelNumbers.Max();
        int min = parallelNumbers.Min();
        int uniqueCount = parallelNumbers.Distinct().Count();

        Console.WriteLine("\nРезультати:");
        Console.WriteLine($"Сума: {sum}");
        Console.WriteLine($"Добуток: {product}");
        Console.WriteLine($"Середнє: {average:F2}");
        Console.WriteLine($"Максимум: {max}");
        Console.WriteLine($"Мінімум: {min}");
        Console.WriteLine($"Унікальних значень: {uniqueCount}");
    }
}