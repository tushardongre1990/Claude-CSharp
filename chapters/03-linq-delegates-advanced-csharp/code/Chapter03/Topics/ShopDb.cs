using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Chapter03.Topics;

// Shared by topics 8 and 9: a tiny product catalogue in an in-memory SQLite database.
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}

public class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite has no decimal type. Storing as REAL (double) lets SQLite compare and sort prices.
        // On SQL Server you would use .HasPrecision(18, 2) instead.
        modelBuilder.Entity<Product>().Property(p => p.Price).HasConversion<double>();
    }
}

// Records the SQL text of every query EF Core sends, so the demos can print it
public sealed class SqlCapture : DbCommandInterceptor
{
    public List<string> Commands { get; } = [];

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Commands.Add(command.CommandText);
        return result;
    }
}

public static class ShopDb
{
    public static (ShopDbContext Db, SqlCapture Sql) CreateSeeded()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();                         // the in-memory database lives as long as this connection

        var sql = new SqlCapture();
        var options = new DbContextOptionsBuilder<ShopDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(sql)
            .Options;

        var db = new ShopDbContext(options);
        db.Database.EnsureCreated();
        db.Products.AddRange(
            new Product { Name = "ThinkPad X1",    Category = "Laptops",     Price = 145_000m, IsActive = true },
            new Product { Name = "MacBook Air",    Category = "Laptops",     Price = 99_000m,  IsActive = true },
            new Product { Name = "Budget Laptop",  Category = "Laptops",     Price = 32_000m,  IsActive = false },
            new Product { Name = "USB-C Hub",      Category = "Accessories", Price = 2_499m,   IsActive = true },
            new Product { Name = "Wireless Mouse", Category = "Accessories", Price = 899m,     IsActive = true });
        db.SaveChanges();
        db.ChangeTracker.Clear();
        sql.Commands.Clear();
        return (db, sql);
    }
}
