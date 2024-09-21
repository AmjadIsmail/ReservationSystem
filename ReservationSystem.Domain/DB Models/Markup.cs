using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("flight_markup")]
    public class FlightMarkup
    {
        [Key, Required]
        public long markup_id { get; set; }
        public decimal? adult_markup {get;set;}
        public decimal? child_markup { get; set; }
        public decimal? infant_markup { get; set; }
        public bool? apply_markup { get; set; }
        public string? airline { get; set; }
        public decimal? discount_on_airline { get; set; }
        public bool? apply_airline_discount { get; set; }
        public string? meta { get; set; }
        public decimal? discount_on_meta { get; set; }
        public DateTime created_on { get; set; } = DateTime.Now;
       
    }
}
