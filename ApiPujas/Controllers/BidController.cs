using ApiPujas.Data;
using ApiPujas.DTOs;
using ApiPujas.Enums;
using ApiPujas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [HttpGet("status/{productId}")]
        public async Task<ActionResult> GetStatus(int productId)
        {
            // Buscamos la puja más alta (o la más reciente por fecha)
            var latestBid = await _context.Bids
                .Include(b => b.Buyer) // Para tener el nombre del usuario
                .Where(b => b.ProductId == productId)
                .OrderByDescending(b => b.Amount) // Ordenamos por precio de mayor a menor
                .FirstOrDefaultAsync(); // Nos quedamos solo con la primera (la más alta)

            if (latestBid == null)
            {
                // Si no hay pujas, devolvemos el precio inicial del producto
                var product = await _context.Products.FindAsync(productId);
                return Ok(new { currentPrice = product.InitialPrice, lastBidderName = "Nadie aún" });
            }

            // Enviamos solo los dos datos que el Front necesita
            return Ok(new
            {
                currentPrice = latestBid.Amount,
                lastBidderName = latestBid.Buyer.Name
            });
        }
        [HttpPost]
        public async Task<IActionResult> CreateBid([FromBody] CreateBidDto dto)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product == null)
                return NotFound("Producto no encontrado");

            if (product.productState != ProductState.Active)
                return BadRequest("La subasta no está activa");

            if (product.EndDate <= DateTime.UtcNow)
                return BadRequest("La subasta ha terminado");

            var highestBid = await _context.Bids
                .Where(b => b.ProductId == dto.ProductId)
                .OrderByDescending(b => b.Amount)
                .FirstOrDefaultAsync();

            decimal minAmount = highestBid != null
                ? highestBid.Amount
                : product.InitialPrice;

            if (dto.Amount <= minAmount)
                return BadRequest($"La puja debe ser mayor que {minAmount}");

            var bid = new Bid
            {
                ProductId = dto.ProductId,
                BuyerId = dto.BuyerId,
                Amount = dto.Amount,
                Date = DateTime.UtcNow
            };

            _context.Bids.Add(bid);

            await _context.SaveChangesAsync();

            return Ok(bid);
        }
    }

}