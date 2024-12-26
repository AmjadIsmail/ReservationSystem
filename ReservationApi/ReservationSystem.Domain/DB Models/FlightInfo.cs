using ReservationSystem.Domain.Models.AddPnrMulti;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("flight_info")]
    public class FlightInfo
    {
        [Key] 
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FlightId { get; set; }

        [Column("flight_number")]
        public string? FlightNumber { get; set; }

        [Column("departure")]
        public string? Departure { get; set; }

        [Column("destination")]
        public string? Destination { get; set; }

        [Column("departure_time")]
        public DateTime? DepartureTime { get; set; }

        [Column("arrival_time")]
        public DateTime? ArrivalTime { get; set; }

        [Column("cabin_class")]
        public string? CabinClass { get; set; }

        [Column("flight_offer", TypeName = "jsonb")]
        public string? FlightOffer { get; set; }

        [Column("amadeus_session_id")]
        public string? AmadeusSessionId { get; set; }

        [Column("created_on")]
        public DateTime? CreatedOn { get; set; } = DateTime.Now;
    }
}
