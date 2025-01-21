using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.DB_Models
{
    [Table("flight_markup")]
    public class FlightMarkup
    {
        [Column("markup_id")]
        [Key, Required]
        public long MarkupId { get; set; }
        [Column("adult_markup")]
        public decimal? AdultMarkup {get;set;}
        [Column("child_markup")]
        public decimal? ChildMarkup { get; set; }
        [Column("infant_markup")]
        public decimal? InfantMarkup { get; set; }
        [Column("apply_markup")]
        public bool? ApplyMarkup { get; set; }

        [Column("is_percentage")]
        public bool? IsPercentage { get; set; }

        [Column("airline")]
        public string? Airline { get; set; }

        [Column("airline_markup")]
        public decimal? AirlineMarkup { get; set; }

        [Column("discount_on_airline")]
        public decimal? DiscountOnAirline { get; set; }
        [Column("apply_airline_discount")]
        public bool? ApplyAirlineDiscount { get; set; }

        [Column("meta")]
        public string? Meta { get; set; }

        [Column("meta_markup")]
        public decimal? MetaMarkup { get; set; }

        [Column("discount_on_meta")]
        public decimal? DiscountOnMeta { get; set; }
        [Column("created_on")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        [Column("gds")]
        public string? Gds { get; set; }
        [Column("gds_markup")]
        public decimal? GdsMarkup { get; set; }

        [Column("marketing_source")]
        public string? MarketingSource { get; set; }
        [Column("marketing_source_markup")]
        public decimal? MarketingSourceMarkup { get; set; }

        [Column("fare_type")]
        public string? FareType { get; set; }

        [Column("fare_type_markup")]
        public decimal? FareTypeMarkup { get; set; }

        [Column("journy_type")]
        public string? JournyType { get; set; }

        [Column("journy_type_markup")]
        public decimal? JournyTypeMarkup { get; set; }

        [Column("between_hours")]
        public string? BetweenHours { get; set; }

        [Column("between_hours_markup")]
        public decimal? BetweenHoursMarkup { get; set; }

        [Column("start_airport")]
        public string? StartAirport { get; set; }

        [Column("start_airport_markup")]
        public decimal? StartAirportMarkup { get; set; }

        [Column("end_airport")]
        public string? EndAirport { get; set; }

        [Column("end_airport_markup")]
        public decimal? EndAirportMarkup { get; set; }

        [Column("from_date")]
        public DateOnly? FromDate { get; set; }

        [Column("to_date")]
        public DateOnly? ToDate { get; set; }

        [Column("date_markup")]
        public decimal? DateMarkup { get; set; }

    }
}
