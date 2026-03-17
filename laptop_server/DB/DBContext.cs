using LaptopServer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LaptopServer.DB
{
    public class LaptopsDBContext : IdentityDbContext<IdentityUser>
    {
        public LaptopsDBContext(DbContextOptions<LaptopsDBContext> options) : base(options) { }
        public DbSet<LaptopEntity> Laptops { get; set; }
        public DbSet<CartItemEntity> Carts { get; set; }
        public DbSet<OrderItemEntity> OrderItems { get; set; }
        public DbSet<OrderEntity> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CartItemEntity>()
                .HasKey(op => new { op.CartId, op.LaptopId });

            modelBuilder.Entity<OrderEntity>()
                .HasMany(a => a.OrderItems)
                .WithOne(b => b.Order)
                .HasForeignKey(b => b.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}