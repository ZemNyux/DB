using System;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        int[] numbers = { 5, 10, 3, 8, 15, 2, 20 };

        Console.WriteLine("Масив: " + string.Join(", ", numbers));

        Task<int> minTask = Task.Run(() =>
        {
            Console.WriteLine($"[Min] Потік: {Task.CurrentId}");
            return numbers.Min();
        });

        Task<int> maxTask = Task.Run(() =>
        {
            Console.WriteLine($"[Max] Потік: {Task.CurrentId}");
            return numbers.Max();
        });

        Task<double> avgTask = Task.Run(() =>
        {
            Console.WriteLine($"[Avg] Потік: {Task.CurrentId}");
            return numbers.Average();
        });

        Task<int> sumTask = Task.Run(() =>
        {
            Console.WriteLine($"[Sum] Потік: {Task.CurrentId}");
            return numbers.Sum();
        });

        await Task.WhenAll(minTask, maxTask, avgTask, sumTask);

        Console.WriteLine($"\nМінімум: {minTask.Result}");
        Console.WriteLine($"Максимум: {maxTask.Result}");
        Console.WriteLine($"Середнє: {avgTask.Result:F2}");
        Console.WriteLine($"Сума: {sumTask.Result}");
    }
}