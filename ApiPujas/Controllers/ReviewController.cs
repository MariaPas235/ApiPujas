using ApiPujas.Data;
using ApiPujas.DTOs;
using ApiPujas.Models;
using ApiPujas.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiPujas.Controllers
{

    /// <summary>
    /// Controlador para gestionar las reseñas y valoraciones de vendedores.
    /// Permite consultar las reseñas recibidas por un vendedor y crear nuevas valoraciones
    /// vinculadas a una compra completada, actualizando automáticamente la reputación del vendedor.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor del controlador ReviewController.
        /// </summary>
        /// <param name="context">Contexto de base de datos de la aplicación.</param>
        public ReviewController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todas las reseñas recibidas por un vendedor, ordenadas por fecha descendente
        /// e incluyendo los datos del comprador que emitió cada valoración.
        /// </summary>
        /// <param name="sellerId">Identificador único del vendedor.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>isSuccess = true</c>: Lista de reseñas del vendedor, o lista vacía si no tiene ninguna.</description></item>
        ///   <item><description><c>isSuccess = false</c>: Error interno al acceder a la base de datos.</description></item>
        /// </list>
        /// </returns>
        /// 

        [HttpGet("seller/{sellerId}")]
        public async Task<ResponseDto> GetReviewsBySeller(int sellerId)
        {
            try
            {
                var reviews = await _context.Ratings
                    .Where(r => r.SellerId == sellerId)
                    .Include(r => r.Buyer)
                    .OrderByDescending(r => r.Date)
                    .ToListAsync();

                return new ResponseDto
                {
                    IsSuccess = true,
                    Data = reviews
                };
            }
            catch (Exception ex)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Crea una nueva reseña vinculada a una compra existente y recalcula la reputación
        /// del vendedor como la media de todas sus valoraciones.
        /// Si la reputación resultante es 5, el vendedor es marcado automáticamente como verificado.
        /// </summary>
        /// <param name="dto">Datos de la reseña: identificador de compra, puntuación y comentario.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>isSuccess = true</c>: Reseña creada correctamente; el mensaje indica si el vendedor ha alcanzado la verificación.</description></item>
        ///   <item><description><c>isSuccess = false</c>: Compra no encontrada o error interno.</description></item>
        /// </list>
        /// </returns>
        [HttpPost]
        public async Task<ResponseDto> CreateReview([FromBody] CreateReviewDto dto)
        {
            try
            {
                var purchase = await _context.Purchases
                    .Include(p => p.Buyer)
                    .Include(p => p.Product)
                        .ThenInclude(p => p.Seller)
                    .FirstOrDefaultAsync(p => p.Id == dto.PurchaseId);

                if (purchase == null)
                    return new ResponseDto
                    {
                        IsSuccess = false,
                        Message = "Purchase not found"
                    };

                var rating = new Rating
                {
                    Score = dto.Score,
                    Comment = dto.Comment,
                    SellerId = purchase.Product.SellerId,
                    BuyerId = purchase.BuyerId,
                    Date = DateTime.UtcNow
                };

                _context.Ratings.Add(rating);
                await _context.SaveChangesAsync();

                var allRatings = await _context.Ratings
                    .Where(r => r.SellerId == purchase.Product.SellerId)
                    .ToListAsync();

                var average = allRatings.Average(r => r.Score);

                var seller = await _context.Users.FindAsync(purchase.Product.SellerId);
                seller.Reputation = Math.Round((decimal)average, 2);

                if (seller.Reputation == 5)
                {
                    seller.IsVerified = true;
                }

                await _context.SaveChangesAsync();

                return new ResponseDto
                {
                    IsSuccess = true,
                    Message = seller.IsVerified
                        ? "Review created successfully. Seller is now verified!"
                        : "Review created successfully",
                    Data = rating
                };
            }
            catch (Exception ex)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}