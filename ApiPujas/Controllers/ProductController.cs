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

        [HttpGet("GetProductsByUser/{userId}")]
        public ResponseDto GetProductsByUser(int userId)
        {
            var _response = new ResponseDto();
            try
            {
                var productList = _context.Products
                    .Where(p => p.UserId == userId)
                    .ToList();

                if (productList == null || !productList.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Este usuario no tiene productos registrados.";
                    _response.Data = null;
                    return _response;
                }

                _response.IsSuccess = true;
                _response.Data = productList;
                _response.Message = $"Se encontraron {productList.Count} productos.";
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Error al obtener productos: " + ex.Message;
            }
            return _response;
        }

        [HttpGet("GetProductsByUserAndStatus/{userId}/{status}")]
        public ResponseDto GetProductsByUserAndStatus(int userId, string status)
        {
            var response = new ResponseDto();
            try
            {
                // 1. Intentamos convertir el 'status' al Enum ProductState
                // Esto permite que el usuario envíe "1", "Activo" o "activo"
                if (!Enum.TryParse<ProductState>(status, true, out var stateEnum))
                {
                    response.IsSuccess = false;
                    response.Message = $"El estado '{status}' no es válido.";
                    return response;
                }

                // 2. Filtramos por el usuario Y por el estado
                var products = _context.Products
                    .Where(p => p.UserId == userId && p.State == stateEnum)
                    .ToList();

                if (products == null || !products.Any())
                {
                    response.IsSuccess = false;
                    response.Message = $"No se encontraron productos para el usuario {userId} con estado {stateEnum}.";
                    return response;
                }

                response.IsSuccess = true;
                response.Data = products;
                response.Message = $"Se encontraron {products.Count} productos.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Error: " + ex.Message;
            }
            return response;
        }


        [HttpPost]
        public async Task<ResponseDto> Post([FromBody] Product product)
        {
            try
            {
                ModelState.Remove("User");
                ModelState.Remove("Bids");

                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Datos de entrada inválidos";
                    return _response;
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _response.Data = product;
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