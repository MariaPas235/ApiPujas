using Microsoft.EntityFrameworkCore;
using ApiPujas.Models;
using System.Linq;

namespace ApiPujas.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // 🗄️ Tablas
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===============================
            // ⚙️ 1. Configuración global decimal
            // ===============================
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }

            // ===============================
            // 🔗 2. RELACIONES
            // ===============================

            // Product → Seller (User)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Seller)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bid → Buyer (User)
            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Buyer)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bid → Product ✅ (IMPORTANTE: evita ProductId1)
            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Product)
                .WithMany(p => p.Bids)
                .HasForeignKey(b => b.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Purchase → Buyer
            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Buyer)
                .WithMany()
                .HasForeignKey(p => p.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Purchase → Product
            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Product)
                .WithMany()
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Rating → Buyer
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Buyer)
                .WithMany()
                .HasForeignKey(r => r.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Rating → Seller
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Seller)
                .WithMany()
                .HasForeignKey(r => r.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===============================
            // ⚡ 3. ÍNDICES (CLAVE RENDIMIENTO)
            // ===============================

            modelBuilder.Entity<Bid>()
                .HasIndex(b => b.BuyerId);

            modelBuilder.Entity<Bid>()
                .HasIndex(b => b.ProductId);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SellerId);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.EndDate);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            
        }
    }
}