using CommonResources.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApiApp.Context
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) => Database.EnsureCreated();

        public DbSet<Product> Products { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasData(
                    new Product { Id = Guid.Parse("F6B9F927-C3CC-47BF-AF9B-D57F6D20DC10"), Name = "Coca-Cola", Price = 2 },
                    new Product { Id = Guid.Parse("13224D97-93BF-401F-B250-B4DD77699259"), Name = "Fish", Price = 12 },
                    new Product { Id = Guid.Parse("1CCE564E-B379-4599-9AEE-048DAE276CE9"), Name = "Burger", Price = 5 }
            );
        }
    }
}
