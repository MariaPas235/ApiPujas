using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiPujas.Models
{
    public class User
    {

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Password { get; set; }

        [Required]
        public string Address { get; set; }
        [Required]
        public DateTime Register_date { get; set; } = DateTime.Now;


        public float Reputation { get; set; } = 0;
        public Boolean Verificated { get; set; } = false;





    }
}
