using ApiPujas.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPujas.Models
{
    public class Product
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // AutoIncrement
        public long Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]  
        public string Image { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Evita truncamiento de valores
        public decimal Start_price { get; set; }

        [Required]
        public DateTime Start_date { get; set; }

        [Required]
        public DateTime End_date { get; set; }

        [Required]
        public ProductState State { get; set; }

        [Required]
        public long UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        // Colección de Bids asociadas a este producto
        public ICollection<Bid>? Bids { get; set; }

        // Opcional: colección de Valorations asociadas al producto, si quieres
        // public ICollection<Valoration> Valorations { get; set; }
    }
}