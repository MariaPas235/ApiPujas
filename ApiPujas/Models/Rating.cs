using System;
using System.ComponentModel.DataAnnotations;

namespace ApiPujas.Models
{
    public class Rating
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Score { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        // Usuario valorado (Seller)
        [Required]
        public int SellerId { get; set; }
        public User Seller { get; set; }

        // Usuario que valora (Buyer)
        [Required]
        public int BuyerId { get; set; }
        public User Buyer { get; set; }
    }
}