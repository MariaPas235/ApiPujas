using ApiPujas.Data;
using ApiPujas.DTOs;
using ApiPujas.Enums;
using ApiPujas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ApiPujas.Controllers
{

    /// <summary>
    /// Controlador para gestionar las pujas sobre productos en subasta.
    /// Permite consultar el estado actual de una subasta, registrar nuevas pujas
    /// y obtener el historial de pujas activas de un usuario.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BidController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor del controlador BidController.
        /// </summary>
        /// <param name="context">Contexto de base de datos de la aplicación.</param>

        public BidController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene el estado actual de la subasta de un producto,
        /// incluyendo el precio más alto alcanzado y el nombre del último pujador.
        /// </summary>
        /// <param name="productId">Identificador único del producto.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Precio inicial si no hay pujas, o precio actual y nombre del último pujador.</description></item>
        ///   <item><description><c>404 Not Found</c>: El producto no existe.</description></item>
        /// </list>
        /// </returns>
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
                });
            }

            return Ok(new
            {
                currentPrice = latestBid.Amount,
                lastBidderName = latestBid.BuyerName
            });
        }

        /// <summary>
        /// Registra una nueva puja sobre un producto en subasta.
        /// Valida que el producto exista, que la subasta esté activa y no haya finalizado,
        /// y que el importe ofertado supere la puja más alta actual.
        /// </summary>
        /// <param name="dto">Datos de la puja: identificador de producto, comprador e importe.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Puja registrada correctamente, devuelve <c>isSuccess</c> y los datos de la puja.</description></item>
        ///   <item><description><c>400 Bad Request</c>: Subasta inactiva, finalizada o importe insuficiente.</description></item>
        ///   <item><description><c>404 Not Found</c>: El producto no existe.</description></item>
        /// </list>
        /// </returns>
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

        /// <summary>
        /// Obtiene todas las pujas activas de un usuario agrupadas por producto,
        /// devolviendo únicamente la puja más alta de cada subasta en curso.
        /// </summary>
        /// <param name="userId">Identificador único del usuario comprador.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Lista de productos con subasta activa en los que el usuario ha pujado,
        ///   ordenada por fecha de finalización descendente.</description></item>
        /// </list>
        /// </returns>
        [HttpGet("user-bids/{userId}")]
        public async Task<IActionResult> GetUserBids(int userId)
        {
            var stopwatch = Stopwatch.StartNew();

            var result = await _context.Bids
             .AsNoTracking()
             .Where(b => b.BuyerId == userId &&
                         b.Product.productState == ProductState.Active)
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
            return Ok(result);
        }
    }
}