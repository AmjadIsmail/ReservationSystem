using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class Segment
    {
        public Departure? departure { get; set; }
        public Arrival? arrival { get; set; }
        public string? carrierCode { get; set; }
        public string? number { get; set; }
        public Aircraft? aircraft { get; set; }
        public Operating? operating { get; set; }
        public string? duration { get; set; }
        public string? id { get; set; }
        public int? numberOfStops { get; set; }
        public List<Co2Emissions>? co2Emissions { get; set; }
        public bool? blacklistedInEU { get; set; }
        public BaggageAllowance? baggage_allowence { get; set; }
        public string? cabin_class { get; set; }
        public string? cabin_status { get; set; }
    }

    public class BaggageAllowance
    {
        public string? free_allowance { get; set; }
        public string? quantity_code { get; set; }
    }
}
