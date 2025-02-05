using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("fare_type")]
    public class FareType
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("faretype_id")]
        public int FareTypeId { get; set; }

        [Column("fare_type_name")]
        public string? Fare_Type { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        public ICollection<MarkupFareType> MarkupFareTypes { get; set; }
    }
}
