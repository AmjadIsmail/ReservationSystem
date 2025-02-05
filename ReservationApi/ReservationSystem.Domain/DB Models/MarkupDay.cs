using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("markup_day")]
    public class MarkupDay
    {
        [Column("markup_day_id")]
        [Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MarkupDayId { get; set; }
       // public int MarkupId { get; set; }
        public ApplyMarkup? Markup { get; set; }

       // public int DayId { get; set; }
        public DayName? Day { get; set; }

        public bool? IsActive { get; set; }
    }
}
