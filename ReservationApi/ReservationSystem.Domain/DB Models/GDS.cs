using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("gds")]
    public class GDS
    {

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("gds_id")]
        public int GdsId { get; set; }

        [Column("gds_name")]
        public string? GdsName { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        public ICollection<MarkupGDS> MarkupGds { get; set; }

    }
}
