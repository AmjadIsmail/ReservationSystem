using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("MarkupFareTypes")]
    public class MarkupFareType
    {
        [Column("markup_fare_id")]
        [Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MarkupFareId { get; set; }
       // public int MarkupId { get; set; }
        public ApplyMarkup Markup { get; set; }

       // public int FareTypeId { get; set; }
        public FareType FareType { get; set; }
    }
}
