using Microsoft.EntityFrameworkCore;
using ApiPujas.Models;
using ApiPujas.Enums; // Asegúrate de que tus Enums estén en este namespace
using System.Linq;

namespace ApiPujas.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tablas de la Base de Datos
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configuración de precisión para todos los decimales (Dinero/Reputación)
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }

            // 2. Relación Producto -> Vendedor (User)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Seller)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Relación Puja -> Comprador (User)
            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Buyer)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Relación Puja -> Producto
            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Product)
                .WithMany()
                .HasForeignKey(b => b.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // Si se borra el producto, se borran sus pujas

            // 5. Relaciones de Valoración (Rating)
            // Relación con el que califica (Buyer)
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Buyer)
                .WithMany()
                .HasForeignKey(r => r.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con el calificado (Seller)
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Seller)
                .WithMany()
                .HasForeignKey(r => r.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. Relaciones de Compra (Purchase)
            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Buyer)
                .WithMany()
                .HasForeignKey(p => p.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Product)
                .WithMany()
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}