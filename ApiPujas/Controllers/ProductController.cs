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
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================
        // RANDOM PRODUCTS (MUY OPTIMIZADO)
        // =========================================
        [HttpGet("GetRandom")]
        public async Task<IActionResult> GetRandomProducts(int count = 10, int? userId = null)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(p =>
                    (p.productState == ProductState.Active ||
                     p.productState == ProductState.Scheduled) &&
                    p.EndDate > DateTime.UtcNow);

            if (userId.HasValue)
                query = query.Where(p => p.SellerId != userId.Value);

            var products = await query
                .OrderBy(p => Guid.NewGuid())
                .Take(count)
                .ToListAsync();

            return Ok(new
            {
                isSuccess = true,
                data = products
            });
        }

        // =========================================
        // PRODUCTS BY USER
        // =========================================
        [HttpGet("GetProductsByUser/{userId}")]
        public async Task<IActionResult> GetProductsByUser(int userId)
        {
            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.SellerId == userId)
                .ToListAsync();

            return Ok(new
            {
                isSuccess = true,
                data = products,
                count = products.Count
            });
        }

        // =========================================
        // BY USER + STATUS
        // =========================================
        [HttpGet("GetProductsByUserAndStatus/{userId}/{status}")]
        public async Task<IActionResult> GetProductsByUserAndStatus(int userId, string status)
        {
            if (!Enum.TryParse<ProductState>(status, true, out var state))
                return BadRequest("Estado inválido");

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.SellerId == userId && p.productState == state)
                .ToListAsync();

            return Ok(new
            {
                isSuccess = true,
                data = products
            });
        }

        // =========================================
        // CREATE PRODUCT
        // =========================================
        [HttpPost]
        public async Task<ResponseDto> Post([FromBody] CreateProductDto dto)
        {
            try
            {
                var product = new Product
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    InitialPrice = dto.InitialPrice,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Photo = dto.Photo,
                    Category = dto.Category,
                    SellerId = dto.SellerId,
                    productState = ProductState.Scheduled
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return new ResponseDto
                {
                    IsSuccess = true,
                    Data = product
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
        // =========================================
        // UPDATE PRODUCT
        // =========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            var existing = await _context.Products.FindAsync(id);

            if (existing == null)
                return NotFound();

            existing.Title = product.Title;
            existing.Description = product.Description;
            existing.InitialPrice = product.InitialPrice;
            existing.StartDate = product.StartDate;
            existing.EndDate = product.EndDate;
            existing.Photo = product.Photo;
            existing.Category = product.Category;
            existing.productState = product.productState;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                isSuccess = true,
                data = existing
            });
        }

        // =========================================
        // DELETE PRODUCT
        // =========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                isSuccess = true,
                message = "Producto eliminado"
            });
        }
    }
}