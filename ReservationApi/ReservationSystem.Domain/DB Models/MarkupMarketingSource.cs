using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("MarkupMarketingSources")]
    public class MarkupMarketingSource
    {
        [Column("markup_source_id")]
        [Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MarkupSourceId { get; set; }
       // public int MarkupId { get; set; }
        public ApplyMarkup Markup { get; set; }

     //   public int SourceId { get; set; }
        public MarketingSource Source { get; set; }
    }
}
