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
public ResponseDto GetRandomProducts([FromQuery] int count = 10, [FromQuery] int? userId = null)
{
    try
    {
        // Definimos los estados que queremos mostrar
        var allowedStates = new[] { ProductState.Active, ProductState.Scheduled };

        // Filtramos productos:
        // - Estado Activo O Programado
        // - Fecha de fin mayor a hoy (para no mostrar subastas terminadas)
        var query = _context.Products
            .Where(p => allowedStates.Contains(p.productState) && p.EndDate > DateTime.Now);

        // Excluir productos del usuario que está logueado (si userId viene)
        if (userId.HasValue)
        {
            query = query.Where(p => p.SellerId != userId.Value);
        }

        // Orden aleatorio y límite
        var products = query
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
            try
            {
                var productList = _context.Products
                    .Where(p => p.SellerId == userId)
                    .ToList();

                if (productList == null || !productList.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Este usuario no tiene productos registrados.";
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
            try
            {
                // Intentamos convertir el string 'status' al Enum ProductState
                if (!Enum.TryParse<ProductState>(status, true, out var stateEnum))
                {
                    _response.IsSuccess = false;
                    _response.Message = $"El estado '{status}' no es un estado de producto válido.";
                    return _response;
                }

                // Filtramos por SellerId y por el Enum productState
                var products = _context.Products
                    .Where(p => p.SellerId == userId && p.productState == stateEnum)
                    .ToList();

                if (products == null || !products.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = $"No se encontraron productos para el usuario {userId} con estado {stateEnum}.";
                    return _response;
                }

                _response.IsSuccess = true;
                _response.Data = products;
                _response.Message = $"Se encontraron {products.Count} productos.";
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Error: " + ex.Message;
            }
            return _response;
        }
        [HttpDelete("{id}")]
        public async Task<ResponseDto> Delete(int id)
        {
            try
            {
                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Producto no encontrado";
                    return _response;
                }

                _context.Products.Remove(existingProduct);
                await _context.SaveChangesAsync();

                _response.IsSuccess = true;
                _response.Message = "Producto eliminado correctamente";
                _response.Data = existingProduct;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            }

            return _response;
        }
        [HttpPut("{id}")]
        public async Task<ResponseDto> Put(int id, [FromBody] Product product)
        {
            try
            {
                ModelState.Remove("Seller");
                ModelState.Remove("Bids");

                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Datos de entrada inválidos";
                    return _response;
                }

                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Producto no encontrado";
                    return _response;
                }

                // Actualizar campos permitidos
                existingProduct.Title = product.Title;
                existingProduct.Description = product.Description;
                existingProduct.InitialPrice = product.InitialPrice;
                existingProduct.StartDate = product.StartDate;
                existingProduct.EndDate = product.EndDate;
                existingProduct.Photo = product.Photo;
                existingProduct.Category = product.Category;

                // Solo cambiar estado si viene
                existingProduct.productState = product.productState;

                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();

                _response.IsSuccess = true;
                _response.Data = existingProduct;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            }

            return _response;
        }

        [HttpGet("GetProductsByState/{state}")]
        public ResponseDto GetProductsByState(string state)
        {
            if (!Enum.TryParse<ProductState>(state, true, out var stateEnum))
                return new ResponseDto { IsSuccess = false, Message = "Estado inválido" };

            var products = _context.Products.Where(p => p.productState == stateEnum).ToList();
            return new ResponseDto { IsSuccess = true, Data = products };
        }

        [HttpPost]
        public async Task<ResponseDto> Post([FromBody] Product product)
        {
            try
            {
                ModelState.Remove("Seller");
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
                _response.Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            }

            return _response;
        }
    }
}