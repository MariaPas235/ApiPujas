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
        // PRODUCTS BY CATEGORY
        // =========================================
        [HttpGet("GetByCategory")]
        public async Task<IActionResult> GetByCategory()
        {
            var categories = new List<string>
    {
        "Motor", "Tecnologia", "Hogar", "Moda", "Ninos",
        "Deporte", "Coleccionismo", "Libros", "Construccion",
        "Industria", "Otros"
    };

            var products = await _context.Products
                .AsNoTracking()
                .Where(p =>
                    (p.productState == ProductState.Active ||
                     p.productState == ProductState.Scheduled) &&
                    p.EndDate > DateTime.UtcNow)
                .ToListAsync();

            var grouped = categories.Select(cat => new
            {
                category = cat,
                count = products.Count(p => p.Category == cat),
                products = products.Where(p => p.Category == cat).ToList()
            });

            return Ok(new
            {
                isSuccess = true,
                data = grouped
            });
        }
        // =========================================
        // UPDATE PRODUCT
        // =========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Title = dto.Title;
            existing.Description = dto.Description;
            existing.InitialPrice = dto.InitialPrice;
            existing.StartDate = dto.StartDate;
            existing.EndDate = dto.EndDate;
            existing.Photo = dto.Photo;
            existing.Category = dto.Category;
            existing.productState = dto.productState;

            await _context.SaveChangesAsync();
            return Ok(new { isSuccess = true, data = existing });
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

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProducts([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Ok(new List<Product>());
            }

       
            var results = await _context.Products
                .Where(p => p.Title.Contains(term) &&
                           (p.productState == ProductState.Active || p.productState == ProductState.Scheduled))
                .Take(10)
                .ToListAsync();

            return Ok(results);
        }
    }
}