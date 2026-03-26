using ApiPujas.Data;
using ApiPujas.DTOs;
using ApiPujas.Enums;
using ApiPujas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ApiPujas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BidController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BidController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================
        // CURRENT STATUS (ULTRA OPTIMIZED)
        // =========================================
        [HttpGet("status/{productId}")]
        public async Task<IActionResult> GetStatus(int productId)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == productId)
                .Select(p => new
                {
                    p.InitialPrice,
                    p.productState,
                    p.EndDate
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound();

            var latestBid = await _context.Bids
                .AsNoTracking()
                .Where(b => b.ProductId == productId)
                .OrderByDescending(b => b.Amount)
                .Select(b => new
                {
                    b.Amount,
                    BuyerName = b.Buyer.Name
                })
                .FirstOrDefaultAsync();

            if (latestBid == null)
            {
                return Ok(new
                {
                    currentPrice = product.InitialPrice,
                    lastBidderName = "Nadie aún"
                });
            }

            return Ok(new
            {
                currentPrice = latestBid.Amount,
                lastBidderName = latestBid.BuyerName
            });
        }

        // =========================================
        // CREATE BID (SAFE + FAST)
        // =========================================
        [HttpPost]
        public async Task<IActionResult> CreateBid([FromBody] CreateBidDto dto)
        {
            var product = await _context.Products
                .Where(p => p.Id == dto.ProductId)
                .Select(p => new
                {
                    p.Id,
                    p.InitialPrice,
                    p.productState,
                    p.EndDate
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound("Producto no encontrado");

            if (product.productState != ProductState.Active)
                return BadRequest("La subasta no está activa");

            if (product.EndDate <= DateTime.UtcNow)
                return BadRequest("La subasta ha terminado");

            var highestBid = await _context.Bids
                .Where(b => b.ProductId == dto.ProductId)
                .MaxAsync(b => (decimal?)b.Amount) ?? product.InitialPrice;

            if (dto.Amount <= highestBid)
                return BadRequest($"La puja debe ser mayor que {highestBid}");

            var bid = new Bid
            {
                ProductId = dto.ProductId,
                BuyerId = dto.BuyerId,
                Amount = dto.Amount,
                Date = DateTime.UtcNow
            };

            _context.Bids.Add(bid);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                isSuccess = true,
                data = bid
            });
        }

        // =========================================
        // USER BIDS (FAST VERSION 🚀)
        // =========================================
        [HttpGet("user-bids/{userId}")]
        public async Task<IActionResult> GetUserBids(int userId)
        {
            var stopwatch = Stopwatch.StartNew();

            var result = await _context.Bids
                .AsNoTracking()
                .Where(b => b.BuyerId == userId)
                .GroupBy(b => b.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,

                    ProductTitle = g.Select(x => x.Product.Title).FirstOrDefault(),
                    Photo = g.Select(x => x.Product.Photo).FirstOrDefault(),
                    Category = g.Select(x => x.Product.Category).FirstOrDefault(),
                    EndDate = g.Select(x => x.Product.EndDate).FirstOrDefault(),
                    Status = g.Select(x => x.Product.productState).FirstOrDefault(),

                    MyHighestBid = g.Max(x => x.Amount)
                })
                .OrderByDescending(x => x.EndDate)
                .ToListAsync();

            stopwatch.Stop();
            Console.WriteLine($"⏱️ USER BIDS QUERY: {stopwatch.ElapsedMilliseconds} ms");

            return Ok(result);
        }
    }
}