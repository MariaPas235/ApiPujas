using System;
using System.ComponentModel.DataAnnotations;

namespace ApiPujas.Models
{

    /// <summary>
    /// Entidad que representa la valoración que un comprador realiza sobre un vendedor tras una subasta.
    /// </summary>
    public class Rating
    {

        /// <summary>
        /// Identificador único de la valoración.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Puntuación otorgada al vendedor. Valor entre 1 (mínimo) y 5 (máximo).
        /// </summary>
        [Required]
        [Range(1, 5)]
        public int Score { get; set; }

        /// <summary>
        /// Comentario opcional del comprador sobre la experiencia con el vendedor. Máximo 500 caracteres.
        /// </summary>
        [MaxLength(500)]
        public string? Comment { get; set; }

        /// <summary>
        /// Fecha y hora en UTC en que se registró la valoración. Por defecto, el momento actual.
        /// </summary>
        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Identificador del usuario vendedor que recibe la valoración.
        /// </summary>
        [Required]
        public int SellerId { get; set; }

        /// <summary>
        /// Usuario vendedor asociado a la valoración.
        /// </summary>
        public User Seller { get; set; }

        /// <summary>
        /// Identificador del usuario comprador que emite la valoración.
        /// </summary>
        [Required]
        public int BuyerId { get; set; }

        /// <summary>
        /// Usuario comprador que realiza la valoración.
        /// </summary>
        public User Buyer { get; set; }
    }
}