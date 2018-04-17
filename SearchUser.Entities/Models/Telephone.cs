using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SearchUser.Entities.Models
{
    [Table("Telephones")]
    public class Telephone
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Phone]
        public string Number { get; set; }

        [Required]
        public int ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}
