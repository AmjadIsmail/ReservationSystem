using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("apply_markup")]
    public class ApplyMarkup
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("markup_id")]
        public int MarkupId { get; set; }

        [Column("adult_markup")]
        public decimal? AdultMarkup { get; set; }
        [Column("child_markup")]
        public decimal? ChildMarkup { get; set; }
        [Column("infant_markup")]
        public decimal? InfantMarkup { get; set; }
      
        [Column("is_percentage")]
        public bool? IsPercentage { get; set; }

        [Column("airline")]
        public string? Airline { get; set; }  

        [Column("between_hours_from")]
        public string? BetweenHoursFrom { get; set; }

        [Column("between_hours_to")]
        public decimal? BetweenHoursTo { get; set; }

        [Column("start_airport")]
        public string? StartAirport { get; set; }

        [Column("end_airport")]
        public string? EndAirport { get; set; }

        [Column("from_date")]
        public DateOnly? FromDate { get; set; }

        [Column("to_date")]
        public DateOnly? ToDate { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        [Column("created_on")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public ICollection<MarkupDay>? MarkupDay { get; set; }

        public ICollection<MarkupGDS>? MarkupGds { get; set; }

        public ICollection<MarkupFareType>? MarkupFareTypes { get; set; }

        public ICollection<MarkupMarketingSource>? MarkupMarketing { get; set; }

        public ICollection<MarkupJournyType>? MarkupJournyType { get; set; }
    }
}
