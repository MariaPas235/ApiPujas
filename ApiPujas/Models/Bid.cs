using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPujas.Models
{
    public class Bid
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long BidId { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public long UserId { get; set; }      // FK a Users

        [Required]
        public long ProductId { get; set; }   // FK a Products

        [Required]
        public DateTime Date { get; set; }

        // Propiedades de navegación
        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}