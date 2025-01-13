using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.Enquiry
{
    public class Enquiry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("first_name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("last_name")]
        public string LastName { get; set; }

        [Required]
        [Phone]
        [Column("phone_no")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [Column("email_address")]
        public string EmailAddress { get; set; }

        [Required]
        [MaxLength(500)]
        [Column("message")]
        public string Message { get; set; }
        [Column("source")]
        public string Source { get; set; } = "Contact-Us";
        [Column("created_on")]
        public DateTime CreatedOn { get; set; }
    }
}
