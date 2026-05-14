using ApiPujas.Data;
using ApiPujas.Enums;
using ApiPujas.Models;
using ApiPujas.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiPujas.Controllers
{
    /// <summary>
    /// Controlador para gestionar las compras resultantes de subastas finalizadas.
    /// Permite consultar compras por producto o usuario, filtrar por estado
    /// y marcar una compra como finalizada tras el pago.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor del controlador PurchaseController.
        /// </summary>
        /// <param name="context">Contexto de base de datos de la aplicación.</param>
        public PurchaseController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Marca una compra como <see cref="PurchaseState.Finalized"/> y actualiza el estado
        /// del producto asociado a <see cref="ProductState.Sended"/>.
        /// No permite finalizar una compra que ya se encuentre en estado finalizado.
        /// </summary>
        /// <param name="id">Identificador único de la compra a actualizar.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>isSuccess = true</c>: Compra finalizada correctamente, devuelve la entidad actualizada.</description></item>
        ///   <item><description><c>isSuccess = false</c>: Compra no encontrada, ya finalizada, o error interno.</description></item>
        /// </list>
        /// </returns>
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


        /// <summary>
        /// Obtiene la compra asociada a un producto concreto,
        /// incluyendo los datos del producto y del comprador.
        /// </summary>
        /// <param name="productId">Identificador único del producto.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>isSuccess = true</c>: Compra encontrada o <c>null</c> si no existe ninguna.</description></item>
        ///   <item><description><c>isSuccess = false</c>: Error interno al acceder a la base de datos.</description></item>
        /// </list>
        /// </returns>
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

        /// <summary>
        /// Obtiene todas las compras finalizadas de un usuario ordenadas por fecha de compra descendente,
        /// incluyendo los datos del producto y del comprador.
        /// </summary>
        /// <param name="userId">Identificador único del comprador.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>isSuccess = true</c>: Lista de compras finalizadas o mensaje indicando que no hay ninguna.</description></item>
        ///   <item><description><c>isSuccess = false</c>: Error interno al acceder a la base de datos.</description></item>
        /// </list>
        /// </returns>
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

        /// <summary>
        /// Obtiene todas las compras pendientes de pago de un usuario ordenadas por fecha de compra descendente,
        /// incluyendo los datos del producto y del comprador.
        /// </summary>
        /// <param name="userId">Identificador único del comprador.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>isSuccess = true</c>: Lista de compras pendientes o mensaje indicando que no hay ninguna.</description></item>
        ///   <item><description><c>isSuccess = false</c>: Error interno al acceder a la base de datos.</description></item>
        /// </list>
        /// </returns>
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