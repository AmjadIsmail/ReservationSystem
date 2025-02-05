using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("journy_type")]
    public class JourneyType
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("journytype_id")]
        public int JournyTypeId { get; set; }

        [Column("journytype")]
        public string? JournyType { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        public ICollection<MarkupJournyType>? MarkupJournyType { get; set; }
    }
}
