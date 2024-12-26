using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("reservation_flow")]
    public class ReservationFlow
    {
        [Key, Required]
        [Column("auto_id")]
        public long AutoId { get; set; }

        [Column("amadeus_session_id")]
        public string? AmadeusSessionId { get; set; }

        [Column("request_name")]
        public string? RequestName { get; set; }

        [Column("request" , TypeName = "jsonb")]
        public string? Request { get; set; }

        [Column("response" , TypeName = "jsonb")]
        public string? Response { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("is_error")]
        public bool? IsError { get; set; }

        [Column("created_on")]
        public DateTime? CreatedOn { get; set; } = DateTime.Now;
    }
}
