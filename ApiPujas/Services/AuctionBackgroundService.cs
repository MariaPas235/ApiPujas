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
    /// <summary>
    /// Servicio en segundo plano que gestiona el ciclo de vida automático de las subastas.
    /// Se ejecuta cada 10 segundos y realiza tres operaciones principales:
    /// activar subastas programadas, cerrar subastas vencidas y generar la compra del ganador.
    /// </summary>
    public class AuctionBackgroundService : BackgroundService
    {
        /// <summary>
        /// Fábrica de scopes para resolver servicios con ciclo de vida Scoped (como AppDbContext).
        /// </summary>
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        /// Logger para registrar el estado y errores del servicio.
        /// </summary>
        private readonly ILogger<AuctionBackgroundService> _logger;

        /// <summary>
        /// Contexto de SignalR para notificar en tiempo real a los clientes conectados.
        /// </summary>
        private readonly IHubContext<AuctionHub> _hubContext;

        /// <summary>
        /// Inicializa el servicio con sus dependencias necesarias.
        /// </summary>
        public AuctionBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<AuctionBackgroundService> logger,
            IHubContext<AuctionHub> hubContext)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Genera un número de orden único de 12 caracteres combinando la fecha actual y un sufijo aleatorio.
        /// </summary>
        /// <returns>Cadena de 12 caracteres usada como número de orden.</returns>
        private static string GenerateOrderNumber()
        {
            var now = DateTime.UtcNow;
            var suffix = Math.Abs(Guid.NewGuid().GetHashCode()) % 100000000;
            return $"{now:MMdd}{suffix:D8}".Substring(0, 12);
        }

        // <summary>
        /// Bucle principal del servicio. Se ejecuta cada 10 segundos mientras la aplicación esté activa.
        /// En cada ciclo:
        /// <list type="bullet">
        ///   <item><description>Activa productos en estado <see cref="ProductState.Scheduled"/> cuya <c>StartDate</c> ya ha llegado.</description></item>
        ///   <item><description>Cierra productos en estado <see cref="ProductState.Active"/> cuya <c>EndDate</c> ha expirado.</description></item>
        ///   <item><description>Genera una <see cref="Purchase"/> en estado <see cref="PurchaseState.Pending"/> para el comprador con la puja más alta.</description></item>
        ///   <item><description>Notifica a todos los clientes conectados vía SignalR si hubo cambios de estado.</description></item>
        /// </list>
        /// </summary>
        /// <param name="stoppingToken">Token de cancelación que detiene el servicio al apagar la aplicación.</param>
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
                        _logger.LogInformation("[AUCTION] Producto {id} -> ACTIVE", product.Id);
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
                    }
                                  
                    await context.SaveChangesAsync(stoppingToken);

                    // =========================
                    // 🧠 PUJA GANADORA
                    // =========================
                    foreach (var product in toClose)
                    {
                        _logger.LogInformation("[AUCTION] Producto {id} -> CLOSED", product.Id);

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
                                long timestampMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                string timestampPart = timestampMillis.ToString();
                                string timestampStr = timestampPart.Substring(Math.Max(0, timestampPart.Length - 8));

                                Random rnd = new Random();
                                string randomStr = rnd.Next(0, 10000).ToString("D4");

                                var purchase = new Purchase
                                {
                                    PurchaseDate = DateTime.UtcNow,
                                    purchaseState = PurchaseState.Pending,
                                    ProductId = product.Id,
                                    BuyerId = winningBid.BuyerId,
                                    OrderNumber = $"{timestampStr}{randomStr}",
                                    Data = "3102023",
                                    TotalToPay = winningBid.Amount
                                };

                                context.Purchases.Add(purchase);

                                _logger.LogInformation(
                                    "[PURCHASE] Creada para producto {id}, buyer {buyer}",
                                    product.Id, winningBid.BuyerId);
                            }
                            else
                            {
                                _logger.LogInformation(
                                    "[PURCHASE] Ya existía para producto {id}", product.Id);
                            }
                        }
                        else
                        {
                            _logger.LogInformation(
                                "[PURCHASE] Producto {id} sin pujas", product.Id);
                        }
                    }

                    // =========================
                    // 💾 GUARDAR PURCHASES
                    // =========================
                    await context.SaveChangesAsync(stoppingToken);

                    // =========================
                    // 📡 SIGNALR UPDATE
                    // =========================
                    if (toActivate.Any() || toClose.Any())
                    {
                        await _hubContext.Clients.All.SendAsync("RefreshProducts");
                        _logger.LogInformation("[SIGNALR] Refresh enviado a clientes");
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