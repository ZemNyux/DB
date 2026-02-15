using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=AuthDb;Trusted_Connection=True;");
    }
}
