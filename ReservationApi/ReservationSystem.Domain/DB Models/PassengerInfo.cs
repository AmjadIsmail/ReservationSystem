using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("passenger_info")]
    public   class PassengerInfo
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PassengerId { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("dob")]
        public string? DOB { get; set; }

        public int FlightId { get; set; }
       
        public FlightInfo? Flight { get; set; }

        [Column("session_id")]
        public string? SessionId { get; set; }

        [Column("passenger_type")]
        public string? PassengerType { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public bool? IsLead { get; set; }
    }
}
