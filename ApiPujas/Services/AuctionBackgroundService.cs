using ApiPujas.Models;
using ApiPujas.Enums;
using ApiPujas.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class AuctionBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AuctionBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;

            // Activar subastas
            var productsToStart = await context.Products
                .Where(p => p.StartDate <= now && p.productState == ProductState.Scheduled)
                .ToListAsync();

            foreach (var product in productsToStart)
            {
                product.productState = ProductState.Active;

                bool hasBids = await context.Bids.AnyAsync(b => b.ProductId == product.Id);

                if (!hasBids)
                {
                    var bid = new Bid
                    {
                        ProductId = product.Id,
                        Amount = product.InitialPrice,
                        Date = now,
                        BuyerId = product.SellerId
                    };

                    context.Bids.Add(bid);
                }
            }

            // Cerrar subastas
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
                    Console.WriteLine($"Producto {product.Id} ganado por usuario {winningBid.BuyerId} con {winningBid.Amount}");
                }
            }

            await context.SaveChangesAsync();

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}