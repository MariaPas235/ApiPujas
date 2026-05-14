using Microsoft.EntityFrameworkCore;
using ApiPujas.Models;
using System.Linq;

namespace ApiPujas.Data
{

    /// <summary>
    /// Contexto principal de base de datos de la aplicación ApiPujas.
    /// Expone las tablas del modelo relacional y centraliza la configuración
    /// de tipos de columna, relaciones entre entidades e índices de rendimiento.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Constructor del contexto de base de datos.
        /// </summary>
        /// <param name="options">Opciones de configuración del contexto, inyectadas por el contenedor de dependencias.</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        /// <summary>Tabla de usuarios de la plataforma (compradores y vendedores).</summary>
        public DbSet<User> Users { get; set; }

        /// <summary>Tabla de productos publicados en subasta.</summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>Tabla de pujas realizadas sobre productos en subasta.</summary>
        public DbSet<Bid> Bids { get; set; }

        /// <summary>Tabla de compras generadas al finalizar una subasta.</summary>
        public DbSet<Purchase> Purchases { get; set; }

        /// <summary>Tabla de valoraciones emitidas por compradores sobre vendedores.</summary>
        public DbSet<Rating> Ratings { get; set; }


        /// <summary>
        /// Configura el modelo relacional de la base de datos mediante Fluent API.
        /// Se aplican tres bloques de configuración en orden:
        /// <list type="number">
        ///   <item><description>
        ///     <b>Tipos decimal</b>: todos los campos <c>decimal</c> y <c>decimal?</c>
        ///     se mapean a <c>decimal(18,2)</c> para garantizar precisión monetaria uniforme.
        ///   </description></item>
        ///   <item><description>
        ///     <b>Relaciones</b>: define las claves foráneas y comportamientos de borrado
        ///     entre <see cref="Product"/>↔<see cref="User"/> (vendedor),
        ///     <see cref="Bid"/>↔<see cref="User"/> (comprador),
        ///     <see cref="Bid"/>↔<see cref="Product"/> (cascade),
        ///     <see cref="Purchase"/>↔<see cref="User"/>/<see cref="Product"/>,
        ///     y <see cref="Rating"/>↔comprador/vendedor. Todas usan <c>Restrict</c>
        ///     salvo la relación Bid→Product, que usa <c>Cascade</c> para eliminar
        ///     las pujas al borrar el producto.
        ///   </description></item>
        ///   <item><description>
        ///     <b>Índices</b>: crea índices sobre <c>Bid.BuyerId</c>, <c>Bid.ProductId</c>,
        ///     <c>Product.SellerId</c>, <c>Product.EndDate</c> para acelerar las consultas
        ///     más frecuentes, y un índice único sobre <c>User.Email</c> para evitar duplicados.
        ///   </description></item>
        /// </list>
        /// </summary>
        /// <param name="modelBuilder">Constructor del modelo usado por Entity Framework Core.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Seller)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Buyer)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Product)
                .WithMany(p => p.Bids)
                .HasForeignKey(b => b.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

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

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Buyer)
                .WithMany()
                .HasForeignKey(r => r.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Seller)
                .WithMany()
                .HasForeignKey(r => r.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

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