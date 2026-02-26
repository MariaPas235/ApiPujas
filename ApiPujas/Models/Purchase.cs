using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiPujas.Models
{
    public class Purchase
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DateTime Buy_date { get; set; }

        public string State { get; set; }

        [MaxLength(450)]
        public string? Id_operation{ get; set; }

        [Required]
        public string Data { get; set; }

        [Required]
        public string Order { get; set; }

        public long UserIdBuyer { get; set; }


        [ForeignKey("UserIdBuyer")]
        public User User { get; set; }
    }
}
