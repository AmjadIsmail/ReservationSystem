using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("MarkupJournyTypes")]
    public class MarkupJournyType
    {
        [Column("markup_journytype_id")]
        [Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MarkupJournyTypeId { get; set; }
       // public int MarkupId { get; set; }
        public ApplyMarkup Markup { get; set; }

       // public int JournyTypeId { get; set; }
        public JourneyType Journy { get; set; }


    }
}
