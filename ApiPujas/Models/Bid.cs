using System;
using System.ComponentModel.DataAnnotations;

namespace ApiPujas.Models
{
    /// <summary>
    /// Entidad que representa una puja realizada sobre un producto en subasta.
    /// </summary>
    public class Bid
    {
        /// <summary>
        /// Identificador único de la puja.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Importe ofertado en la puja. Debe ser superior al precio de salida y a la puja anterior.
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// Fecha y hora en UTC en que se registró la puja. Por defecto, el momento actual.
        /// </summary>
        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Identificador del producto sobre el que se realiza la puja.
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// Producto asociado a la puja.
        /// </summary>
        public Product Product { get; set; }

        /// <summary>
        /// Identificador del usuario comprador que realiza la puja.
        /// </summary>
        [Required]
        public int BuyerId { get; set; }

        /// <summary>
        /// Usuario comprador asociado a la puja.
        /// </summary>
        public User Buyer { get; set; }
    }
}