using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("MarkupGds")]
    public class MarkupGDS
    {
        [Column("markup_gds_id")]
        [Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MarkupGdsId { get; set; }
       // public int MarkupId { get; set; }
        public ApplyMarkup Markup { get; set; }

     //   public int GdsId { get; set; }
        public GDS gds { get; set; }

        public bool? IsActive { get; set; }
    }
}
