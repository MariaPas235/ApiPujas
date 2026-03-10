using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPujas.Models
{
    public class Rating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Score { get; set; }

        public string Comment { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        // Relationship to the person being rated (Seller)
        [Required]
        public int SellerId { get; set; }

        [ForeignKey("SellerId")]
        public virtual User Seller { get; set; }

        // Relationship to the person giving the rating (Buyer)
        [Required]
        public int BuyerId { get; set; }

        [ForeignKey("BuyerId")]
        public virtual User Buyer { get; set; }
    }
}