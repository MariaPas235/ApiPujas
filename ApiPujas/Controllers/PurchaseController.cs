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

        // GET: api/purchase/user/5
        [HttpGet("user/{userId}")]
        public async Task<ResponseDto> GetByUser(int userId)
        {
            var response = new ResponseDto();

            try
            {
                var purchases = await _context.Purchases
                    .Include(p => p.Product)
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