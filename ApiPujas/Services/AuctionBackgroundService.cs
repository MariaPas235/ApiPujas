using ApiPujas.Data;
using ApiPujas.Models;
using ApiPujas.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApiPujas.Services
{
    public class AuctionBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AuctionBackgroundService> _logger;

        public AuctionBackgroundService(IServiceScopeFactory scopeFactory, ILogger<AuctionBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[AuctionBackgroundService] Servicio iniciado a {time}", DateTime.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var now = DateTime.UtcNow;
                    _logger.LogInformation("[AuctionBackgroundService] Revisando productos a {time}", now);

                    // 🔹 Activar subastas programadas
                    var productsToStart = await context.Products
                        .Where(p => p.StartDate <= now && p.productState == ProductState.Scheduled)
                        .ToListAsync();

                    foreach (var product in productsToStart)
                    {
                        _logger.LogInformation("[AuctionBackgroundService] Activando producto {id} ({title})", product.Id, product.Title);
                        product.productState = ProductState.Active;
                    }

                    // 🔹 Cerrar subastas activas que ya terminaron
                    var productsToClose = await context.Products
                        .Where(p => p.EndDate <= now && p.productState == ProductState.Active)
                        .ToListAsync();

                    foreach (var product in productsToClose)
                    {
                        product.productState = ProductState.Closed;

                        var winningBid = await context.Bids
                            .Where(b => b.ProductId == product.Id)
                            .OrderByDescending(b => b.Amount)
                            .FirstOrDefaultAsync();

                        if (winningBid != null)
                        {
                            _logger.LogInformation("[AuctionBackgroundService] Producto {id} cerrado. Ganador: Usuario {buyer} con {amount}",
                                product.Id, winningBid.BuyerId, winningBid.Amount);
                        }
                        else
                        {
                            _logger.LogInformation("[AuctionBackgroundService] Producto {id} cerrado. Sin pujas.", product.Id);
                        }
                    }

                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[AuctionBackgroundService] ERROR procesando subastas");
                }

                // Espera 30 segundos antes de revisar nuevamente
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

            _logger.LogInformation("[AuctionBackgroundService] Servicio detenido a {time}", DateTime.UtcNow);
        }
    }
}