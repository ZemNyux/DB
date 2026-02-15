using System;
using System.Linq;

class Program
{
    static void Main()
    {
        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();

            Console.WriteLine("Enter login:");
            string username = Console.ReadLine();

            Console.WriteLine("Enter password:");
            string password = Console.ReadLine();

            if (password.Length < 6 || !password.Any(char.IsDigit))
            {
                Console.WriteLine("Password must be at least 6 characters and contain at least one digit.");
                return;
            }

            var user = db.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                var newUser = new User
                {
                    Username = username,
                    Password = PasswordHelper.HashPassword(password),
                    CreatedAt = DateTime.Now
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                Console.WriteLine("User registered successfully!");
            }
            else
            {
                string hashedInput = PasswordHelper.HashPassword(password);

                if (user.Password == hashedInput)
                    Console.WriteLine("Login successful!");
                else
                    Console.WriteLine("Wrong password!");
            }
        }
    }
}
