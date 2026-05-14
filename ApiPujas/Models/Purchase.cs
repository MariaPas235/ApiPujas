using ApiPujas.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace ApiPujas.Models
{

    /// <summary>
    /// Entidad que representa la compra generada cuando un usuario gana una subasta.
    /// </summary>
    public class Purchase
    {

        /// <summary>
        /// Identificador único de la compra.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Fecha y hora en UTC en que se registró la compra. Por defecto, el momento actual.
        /// </summary>
        [Required]
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Estado actual de la compra dentro de su ciclo de vida (ej: pendiente, pagada, cancelada).
        /// </summary>
        [Required]
        public PurchaseState purchaseState { get; set; }

        /// <summary>
        /// Importe total a abonar por el comprador, correspondiente a la puja ganadora.
        /// </summary>
        [Required]
        public decimal TotalToPay { get; set; }

        /// <summary>
        /// Datos adicionales de la transacción de pago. Máximo 500 caracteres. Campo opcional.
        /// </summary>
        [MaxLength(500)]
        public string? Data { get; set; }

        /// <summary>
        /// Número de orden asociado al pago en la pasarela (ej: Redsys). Máximo 100 caracteres. Campo opcional.
        /// </summary>
        [MaxLength(100)]
        public string? OrderNumber { get; set; }

        /// <summary>
        /// Identificador del usuario comprador que ganó la subasta.
        /// </summary>
        [Required]
        public int BuyerId { get; set; }

        /// <summary>
        /// Usuario comprador asociado a la compra.
        /// </summary>
        public User Buyer { get; set; }

        /// <summary>
        /// Identificador del producto adquirido en la subasta.
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// Producto asociado a la compra.
        /// </summary>
        public Product Product { get; set; }
    }
}