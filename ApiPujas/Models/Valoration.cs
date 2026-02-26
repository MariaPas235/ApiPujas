using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPujas.Models
{
    public class Valoration
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 

        public long Id { get; set; }

        public long Punctuation { get; set; }

        public String Comment { get; set; }

        public DateTime Date { get; set; }

        public long UserIdBuyer { get; set; }

        public long UserIdSeller { get; set; }


        [ForeignKey("UserIdBuyer")]
        public User UserBuyer { get; set; }

        [ForeignKey("UserIdSeller")]
        public User UserSeller { get; set; }
    }
}
