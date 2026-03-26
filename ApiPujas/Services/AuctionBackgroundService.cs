using ApiPujas.Data;
using ApiPujas.Enums;
using ApiPujas.Hubs;
using ApiPujas.Models;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<AuctionHub> _hubContext;

        public AuctionBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<AuctionBackgroundService> logger,
            IHubContext<AuctionHub> hubContext)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _hubContext = hubContext;
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

                    var productsToUpdate = await context.Products
                        .Where(p =>
                            (p.productState == ProductState.Scheduled && p.StartDate <= now) ||
                            (p.productState == ProductState.Active && p.EndDate <= now))
                        .ToListAsync();

                    if (productsToUpdate.Any())
                    {
                        foreach (var product in productsToUpdate)
                        {
                            // 🟡 PASAR A ACTIVO
                            if (product.productState == ProductState.Scheduled &&
                                product.StartDate <= now)
                            {
                                product.productState = ProductState.Active;

                                _logger.LogInformation(
                                    "[SignalR] Producto {id} pasado a ACTIVO",
                                    product.Id);
                            }

                            // 🔴 CERRAR SUBASTA
                            else if (product.productState == ProductState.Active &&
                                     product.EndDate <= now)
                            {
                                product.productState = ProductState.Closed;

                                _logger.LogInformation(
                                    "[SignalR] Producto {id} pasado a CERRADO",
                                    product.Id);

                                // =========================
                                // 🧠 1. PUJA GANADORA
                                // =========================
                                var winningBid = await context.Bids
                                    .Where(b => b.ProductId == product.Id)
                                    .OrderByDescending(b => b.Amount)
                                    .ThenBy(b => b.Date)
                                    .FirstOrDefaultAsync();

                                // =========================
                                // 💰 2. CREAR PURCHASE
                                // =========================
                                if (winningBid != null)
                                {
                                    var alreadyExists = await context.Purchases
                                        .AnyAsync(p => p.ProductId == product.Id);

                                    if (!alreadyExists)
                                    {
                                        var purchase = new Purchase
                                        {
                                            PurchaseDate = DateTime.UtcNow,
                                            purchaseState = PurchaseState.Pending,
                                            ProductId = product.Id,
                                            BuyerId = winningBid.BuyerId,
                                            OrderNumber = Guid.NewGuid().ToString(),
                                            OperationId = 0,
                                            Data = null
                                        };

                                        context.Purchases.Add(purchase);

                                        _logger.LogInformation(
                                            "[PURCHASE] Creada compra para producto {id} - buyer {buyer}",
                                            product.Id,
                                            winningBid.BuyerId);
                                    }
                                }
                                else
                                {
                                    _logger.LogInformation(
                                        "[PURCHASE] Producto {id} sin pujas",
                                        product.Id);
                                }
                            }
                        }

                        // 💾 Guardar TODO (estado + purchases)
                        await context.SaveChangesAsync();

                        // 📡 SIGNALR refresh frontend
                        await _hubContext.Clients.All.SendAsync("RefreshProducts", stoppingToken);

                        _logger.LogInformation("[SignalR] Notificación enviada a todos los clientes.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[AuctionBackgroundService] ERROR");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}