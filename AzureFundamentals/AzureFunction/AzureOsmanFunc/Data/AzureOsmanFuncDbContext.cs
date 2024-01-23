using AzureOsmanFunc.Models;
using Microsoft.EntityFrameworkCore;

namespace AzureOsmanFunc.Data
{
    public class AzureOsmanFuncDbContext : DbContext
    {
        public AzureOsmanFuncDbContext(DbContextOptions<AzureOsmanFuncDbContext> options) : base(options)
        {

        }

        public DbSet<SalesRequest> SalesRequests { get; set; }
        public DbSet<GroceryItem> GroceryItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SalesRequest>(entity =>
            {
                entity.HasKey(c => c.Id);
            });

            modelBuilder.Entity<GroceryItem>(entity =>
            {
                entity.HasKey(c => c.Id);
            });
        }
    }
}
