using ApiPujas.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPujas.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InitialPrice { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; } // Antes era End_date

        [Required]
    
        public ProductState productState { get; set; } // Antes era State

        public string? Photo { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [Required]
        public int SellerId { get; set; } // Antes era UserId

        [ForeignKey("SellerId")]
        public virtual User Seller { get; set; }
    }
}