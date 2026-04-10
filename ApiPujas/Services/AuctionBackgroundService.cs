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

        private static string GenerateOrderNumber()
        {
            // Ej: "0407XXXXXXXX" -> fecha + ticks truncados
            var now = DateTime.UtcNow;
            var suffix = Math.Abs(Guid.NewGuid().GetHashCode()) % 100000000;
            return $"{now:MMdd}{suffix:D8}".Substring(0, 12);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[AuctionService] Iniciado a {time}", DateTime.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var now = DateTime.UtcNow;

                    // =========================
                    // 🟡 ACTIVAR SUBASTAS
                    // =========================
                    var toActivate = await context.Products
                        .Where(p => p.productState == ProductState.Scheduled
                                    && p.StartDate <= now)
                        .ToListAsync(stoppingToken);

                    foreach (var product in toActivate)
                    {
                        product.productState = ProductState.Active;

                        _logger.LogInformation(
                            "[AUCTION] Producto {id} -> ACTIVE",
                            product.Id);
                    }

                    // =========================
                    // 🔴 CERRAR SUBASTAS
                    // =========================
                    var toClose = await context.Products
                        .Where(p => p.productState == ProductState.Active
                                    && p.EndDate <= now)
                        .ToListAsync(stoppingToken);

                    foreach (var product in toClose)
                    {
                        product.productState = ProductState.Closed;

                        _logger.LogInformation(
                            "[AUCTION] Producto {id} -> CLOSED",
                            product.Id);

                        // =========================
                        // 🧠 PUJA GANADORA
                        // =========================
                        var winningBid = await context.Bids
                            .Where(b => b.ProductId == product.Id)
                            .OrderByDescending(b => b.Amount)
                            .ThenBy(b => b.Date)
                            .FirstOrDefaultAsync(stoppingToken);

                        if (winningBid != null)
                        {
                            var exists = await context.Purchases
                                .AnyAsync(p => p.ProductId == product.Id, stoppingToken);

                            if (!exists)
                            {
                                var purchase = new Purchase
                                {
                                    PurchaseDate = DateTime.UtcNow,
                                    purchaseState = PurchaseState.Pending,
                                    ProductId = product.Id,
                                    BuyerId = winningBid.BuyerId,
                                    OrderNumber = GenerateOrderNumber(),
                                    OperationId = 0,
                                    Data = "3102023",
                                    TotalToPay = winningBid.Amount

                                };  

                                context.Purchases.Add(purchase);

                                _logger.LogInformation(
                                    "[PURCHASE] Creada para producto {id}, buyer {buyer}",
                                    product.Id,
                                    winningBid.BuyerId);
                            }
                            else
                            {
                                _logger.LogInformation(
                                    "[PURCHASE] Ya existía para producto {id}",
                                    product.Id);
                            }
                        }
                        else
                        {
                            _logger.LogInformation(
                                "[PURCHASE] Producto {id} sin pujas",
                                product.Id);
                        }
                    }

                    // =========================
                    // 💾 GUARDAR CAMBIOS
                    // =========================
                    await context.SaveChangesAsync(stoppingToken);

                    // =========================
                    // 📡 SIGNALR UPDATE
                    // =========================
                    if (toActivate.Any() || toClose.Any())
                    {
                        await _hubContext.Clients.All.SendAsync("RefreshProducts");

                        _logger.LogInformation(
                            "[SIGNALR] Refresh enviado a clientes");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "[AuctionService] ERROR: {msg}",
                        ex.InnerException?.Message);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}