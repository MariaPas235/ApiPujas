using ApiPujas.Data;
using ApiPujas.Enums;
using ApiPujas.Models;
using ApiPujas.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiPujas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PurchaseController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPut("{id}")]
        public async Task<ResponseDto> UpdateBidState(int id)
        {
            try
            {
                var purchase = await _context.Purchases
                    .Include(p => p.Product)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (purchase == null)
                    return new ResponseDto
                    {
                        IsSuccess = false,
                        Message = "Purchase not found"
                    };

                if (purchase.purchaseState == PurchaseState.Finalized)
                    return new ResponseDto
                    {
                        IsSuccess = false,
                        Message = "Purchase is already finalized"
                    };

                purchase.purchaseState = PurchaseState.Finalized;
                purchase.Product.productState = ProductState.Sended;

                await _context.SaveChangesAsync();

                return new ResponseDto
                {
                    IsSuccess = true,
                    Message = "Purchase finalized successfully",
                    Data = purchase
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
        [HttpGet("product/{productId}")]
        public async Task<ResponseDto> GetByProduct(int productId)
        {
            var response = new ResponseDto();
            try
            {
                var purchase = await _context.Purchases
                    .Include(p => p.Product)
                    .Include(p => p.Buyer)
                    .FirstOrDefaultAsync(p => p.ProductId == productId);

                response.IsSuccess = true;
                response.Data = purchase;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        [HttpGet("user/finalized/{userId}")]
        public async Task<ResponseDto> GetFinalized(int userId)
        {
            var response = new ResponseDto();
            try
            {
                var purchases = await _context.Purchases
                    .Include(p => p.Product)
                    .Include(p => p.Buyer)   // ← añade esto
                    .Where(p => p.BuyerId == userId && p.purchaseState == PurchaseState.Finalized)
                    .OrderByDescending(p => p.PurchaseDate)
                    .ToListAsync();

                response.IsSuccess = true;
                response.Data = purchases;
                response.Message = purchases.Any()
                    ? $"Compras pagadas encontradas: {purchases.Count}"
                    : "No hay compras pagadas para este usuario";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpGet("user/{userId}")]
        public async Task<ResponseDto> GetByUser(int userId)
        {
            var response = new ResponseDto();
            try
            {
                var purchases = await _context.Purchases
                    .Include(p => p.Product)
                    .Include(p => p.Buyer)   // ← añade esto
                    .Where(p => p.BuyerId == userId && p.purchaseState == PurchaseState.Pending)
                    .OrderByDescending(p => p.PurchaseDate)
                    .ToListAsync();

                response.IsSuccess = true;
                response.Data = purchases;
                response.Message = purchases.Any()
                    ? $"Compras pendientes encontradas: {purchases.Count}"
                    : "No hay compras pendientes para este usuario";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}