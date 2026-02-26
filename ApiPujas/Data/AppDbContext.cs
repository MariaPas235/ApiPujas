using ApiPujas.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiPujas.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Tablas
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Valoration> Valorations { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Bid> Bids { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Valorations: ya no hay cascada, NO ACTION
            modelBuilder.Entity<Valoration>()
                .HasOne(v => v.UserBuyer)
                .WithMany()
                .HasForeignKey(v => v.UserIdBuyer)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Valoration>()
                .HasOne(v => v.UserSeller)
                .WithMany()
                .HasForeignKey(v => v.UserIdSeller)
                .OnDelete(DeleteBehavior.NoAction);

            // Bids: quitar cascada para evitar conflicto
            modelBuilder.Entity<Bid>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Product)
                .WithMany(p => p.Bids)
                .HasForeignKey(b => b.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // esta sí puede quedar
        }
    }
}