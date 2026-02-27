using ApiPujas.Data;
using ApiPujas.Models;
using ApiPujas.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiPujas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase // Usamos ControllerBase para APIs
    {
        private readonly AppDbContext _context;
        private ResponseDto _response;

        public ProductController(AppDbContext context)
        {
            _context = context;
            _response = new ResponseDto();
        }

        [HttpGet("GetRandom")]
        public ResponseDto GetRandomProducts([FromQuery] int count = 10)
        {
            try
            {
                // Filtramos por fecha y mezclamos aleatoriamente
                var products = _context.Products
                    .Where(p => p.End_date > DateTime.Now)
                    .AsEnumerable()
                    .OrderBy(p => Guid.NewGuid())
                    .Take(count)
                    .ToList();

                _response.Data = products;
                _response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
    }
}