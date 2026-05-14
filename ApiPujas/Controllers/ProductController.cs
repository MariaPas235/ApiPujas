using ApiPujas.Data;
using ApiPujas.Enums;
using ApiPujas.Models;
using ApiPujas.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiPujas.Controllers
{

    /// <summary>
    /// Controlador para gestionar los productos en subasta.
    /// Permite crear, consultar, actualizar y eliminar productos,
    /// así como filtrarlos por usuario, estado, categoría o término de búsqueda.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor del controlador ProductController.
        /// </summary>
        /// <param name="context">Contexto de base de datos de la aplicación.</param>
        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Devuelve una selección aleatoria de productos activos o programados
        /// cuya fecha de finalización aún no ha expirado.
        /// Opcionalmente excluye los productos publicados por un usuario concreto.
        /// </summary>
        /// <param name="count">Número de productos a devolver. Por defecto 10.</param>
        /// <param name="userId">Identificador del vendedor a excluir de los resultados. Opcional.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Lista aleatoria de productos con <c>isSuccess</c> y <c>data</c>.</description></item>
        /// </list>
        /// </returns>
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

        /// <summary>
        /// Obtiene todos los productos publicados por un vendedor específico,
        /// independientemente de su estado.
        /// </summary>
        /// <param name="userId">Identificador único del vendedor.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Lista de productos con <c>isSuccess</c>, <c>data</c> y <c>count</c>.</description></item>
        /// </list>
        /// </returns>
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

        /// <summary>
        /// Obtiene los productos de un vendedor filtrados por un estado concreto del ciclo de vida
        /// de la subasta (por ejemplo: Active, Scheduled, Finished).
        /// </summary>
        /// <param name="userId">Identificador único del vendedor.</param>
        /// <param name="status">Nombre del estado del producto como cadena de texto (case-insensitive).</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Lista filtrada de productos.</description></item>
        ///   <item><description><c>400 Bad Request</c>: El valor de <paramref name="status"/> no corresponde a un <see cref="ProductState"/> válido.</description></item>
        /// </list>
        /// </returns>
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

        /// <summary>
        /// Crea un nuevo producto en estado <see cref="ProductState.Scheduled"/>
        /// a partir de los datos proporcionados en el cuerpo de la solicitud.
        /// </summary>
        /// <param name="dto">Datos del producto a crear: título, descripción, precio inicial, fechas, foto, categoría y vendedor.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>isSuccess = true</c>: Producto creado correctamente, devuelve la entidad persistida.</description></item>
        ///   <item><description><c>isSuccess = false</c>: Error durante la creación, devuelve el mensaje de excepción.</description></item>
        /// </list>
        /// </returns>
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

        /// <summary>
        /// Devuelve todos los productos activos o programados agrupados por categoría,
        /// incluyendo el conteo y el listado de productos de cada una.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Colección de objetos con <c>category</c>, <c>count</c> y <c>products</c>.</description></item>
        /// </list>
        /// </returns>
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

        /// <summary>
        /// Actualiza todos los campos editables de un producto existente identificado por su ID.
        /// </summary>
        /// <param name="id">Identificador único del producto a actualizar.</param>
        /// <param name="dto">Nuevos valores del producto: título, descripción, precio, fechas, foto, categoría y estado.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Producto actualizado correctamente.</description></item>
        ///   <item><description><c>404 Not Found</c>: No existe ningún producto con el ID indicado.</description></item>
        /// </list>
        /// </returns>
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

        /// <summary>
        /// Elimina permanentemente un producto de la base de datos por su ID.
        /// </summary>
        /// <param name="id">Identificador único del producto a eliminar.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Producto eliminado correctamente.</description></item>
        ///   <item><description><c>404 Not Found</c>: No existe ningún producto con el ID indicado.</description></item>
        /// </list>
        /// </returns>==
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
        /// <summary>
        /// Busca productos activos o programados cuyo título contenga el término indicado.
        /// Devuelve un máximo de 10 resultados.
        /// </summary>
        /// <param name="term">Cadena de texto a buscar en el título del producto.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Lista de productos coincidentes, o lista vacía si el término es nulo o en blanco.</description></item>
        /// </list>
        /// </returns>
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