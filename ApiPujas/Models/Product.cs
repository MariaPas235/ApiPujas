using ApiPujas.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiPujas.Models
{

    /// <summary>
    /// Entidad que representa un producto publicado en subasta.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Identificador único del producto.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Título descriptivo del producto mostrado en el listado de subastas. Máximo 200 caracteres.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// Descripción detallada del producto, condición y características relevantes.
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Precio de salida de la subasta. Ninguna puja podrá ser igual o inferior a este valor.
        /// </summary>

        [Required]
        public decimal InitialPrice { get; set; }

        /// <summary>
        /// Fecha y hora en UTC en que la subasta comenzará a aceptar pujas.
        /// </summary>

        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Fecha y hora en UTC en que la subasta cerrará y dejará de aceptar pujas.
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Estado actual del producto dentro del ciclo de vida de la subasta.
        /// </summary>
        [Required]
        public ProductState productState { get; set; }

        /// <summary>
        /// URL o ruta de la imagen representativa del producto. Campo opcional.
        /// </summary>
        public string? Photo { get; set; }

        /// <summary>
        /// Categoría a la que pertenece el producto. Máximo 100 caracteres. Campo opcional.
        /// </summary>
        [MaxLength(100)]
        public string? Category { get; set; }

        /// <summary>
        /// Identificador del usuario vendedor que publica el producto.
        /// </summary>
        [Required]
        public int SellerId { get; set; }

        /// <summary>
        /// Usuario vendedor propietario del producto en subasta.
        /// </summary>
        public User Seller { get; set; }

        /// <summary>
        /// Colección de pujas realizadas sobre este producto.
        /// </summary>
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}