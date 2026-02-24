using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace P45Library
{
    internal class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            // підключаємо бібліотеку динамічно, без імпорту та залежностей
            var path = @"C:\Users\Alex\Desktop\P45Library\bin\Debug\net10.0\P45Library.dll";

            var context = new AssemblyLoadContext("MyContext", isCollectible: true);
            var assembly = context.LoadFromAssemblyPath(path); // !!!!!!!!!!!!!!!!

            if (assembly == null) Console.WriteLine("OOPS!");

            Console.WriteLine("бібліотека завантажена успішно.");

            var type = assembly.GetType("P45Library.Cat");
            if (type == null)
            {
                Console.WriteLine("не вдалося знайти тип Cat.");
                return;
            }

            // виводимо методи типу FileFacade для перевірки
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            Console.WriteLine("методи Cat:");
            foreach (var method in methods)
            {
                Console.WriteLine($"- {method.Name}");
            }

            // створюємо рефлексивно екземпляр Cat
            object? cat = Activator.CreateInstance(type);
            // Cat c = new Cat();

            var meowMethod = type.GetMethod("Meow",
                BindingFlags.Public | BindingFlags.Instance);

            // викликаємо метод
            meowMethod.Invoke(cat, null);   // null — бо метод без параметрів
            meowMethod.Invoke(cat, null);
            meowMethod.Invoke(cat, null);
            meowMethod.Invoke(cat, null);

            context.Unload(); // !!!!!!!!!!!!!!!! 
        }
    }
}