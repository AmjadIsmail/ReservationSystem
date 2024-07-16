using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("availibility_results")]
    public class SearchAvailabilityResults
    {
        [Key, Required]
        public long result_id { get; set; }
        [Column(TypeName = "jsonb")]
        public string? request { get; set; }
        [Column(TypeName = "jsonb")]
        public string? response { get; set; }
        public int? user_id { get; set; }
        public int? total_results { get; set; }
        public DateTime? created_on { get; set; } = DateTime.Now;
        
    }
}
