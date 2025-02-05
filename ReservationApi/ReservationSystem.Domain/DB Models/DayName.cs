using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("day_name")]
    public class DayName
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("day_id")]
        public int DayId { get; set; }

        [Column("day_name")]
        public string? Day_Name { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        public ICollection<MarkupDay> MarkupDay { get; set; }

    }
}
