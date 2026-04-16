using ApiPujas.Data;
using ApiPujas.DTOs;
using ApiPujas.Models;
using ApiPujas.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiPujas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/review/seller/{sellerId}
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

                // Calcular media y actualizar reputación del seller
                var allRatings = await _context.Ratings
                    .Where(r => r.SellerId == purchase.Product.SellerId)
                    .ToListAsync();

                var average = allRatings.Average(r => r.Score);

                var seller = await _context.Users.FindAsync(purchase.Product.SellerId);
                seller.Reputation = Math.Round((decimal)average, 2);

                // ✅ Si la reputación es 5, marcar al usuario como verificado
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