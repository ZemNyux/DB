using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        List<string> words = new List<string>
        {
            "apple", "banana", "orange", "apple", "kiwi", "banana", "grape"
        };

        Console.WriteLine("Початкова колекція:");
        Console.WriteLine(string.Join(", ", words));

        Task<List<string>> distinctTask = Task.Run(() =>
        {
            return words.Distinct().ToList();
        });

        var uniqueWords = await distinctTask;

        Console.WriteLine("\nПісля видалення дублікатів:");
        Console.WriteLine(string.Join(", ", uniqueWords));

        Task<List<string>> sortTask = Task.Run(() =>
        {
            return uniqueWords.OrderBy(w => w).ToList();
        });

        var sortedWords = await sortTask;

        Console.WriteLine("\nВідсортована колекція:");
        Console.WriteLine(string.Join(", ", sortedWords));

        Console.WriteLine("\nВведіть слово для пошуку:");
        string searchWord = Console.ReadLine();

        Task<int> searchTask = Task.Run(() =>
        {
            return sortedWords.BinarySearch(searchWord);
        });

        int index = await searchTask;

        if (index >= 0)
        {
            Console.WriteLine($"Слово '{searchWord}' знайдено на позиції {index}");
        }
        else
        {
            Console.WriteLine($"Слово '{searchWord}' не знайдено");
        }
    }
}