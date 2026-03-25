using ApiPujas.Data;
using ApiPujas.Enums;
using ApiPujas.Hubs; // Asegúrate de importar tu Hub
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
        private readonly IHubContext<AuctionHub> _hubContext; // Inyectamos el Hub

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

                    // Traemos productos que cambian de estado
                    var productsToUpdate = await context.Products
                        .Where(p => (p.productState == ProductState.Scheduled && p.StartDate <= now)
                                 || (p.productState == ProductState.Active && p.EndDate <= now))
                        .ToListAsync();

                    if (productsToUpdate.Any())
                    {
                        foreach (var product in productsToUpdate)
                        {
                            if (product.productState == ProductState.Scheduled)
                            {
                                product.productState = ProductState.Active;
                                _logger.LogInformation("[SignalR] Producto {id} pasado a ACTIVO", product.Id);
                            }
                            else if (product.productState == ProductState.Active)
                            {
                                product.productState = ProductState.Closed;
                                _logger.LogInformation("[SignalR] Producto {id} pasado a CERRADO", product.Id);
                            }
                        }

                        await context.SaveChangesAsync();

                        // 🔥 NOTIFICACIÓN POR SIGNALR
                        // Enviamos un evento llamado "RefreshProducts" a todos los conectados
                        await _hubContext.Clients.All.SendAsync("RefreshProducts", stoppingToken);
                        _logger.LogInformation("[SignalR] Notificación enviada a todos los clientes.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[AuctionBackgroundService] ERROR");
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}