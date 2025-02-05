using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("marketing_source")]
    public class MarketingSource
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("source_id")]
        public int SourceId { get; set; }

        [Column("source_name")]
        public string? SourceName { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        public ICollection<MarkupMarketingSource>? MarkupMarketing { get; set; }

    }

}
