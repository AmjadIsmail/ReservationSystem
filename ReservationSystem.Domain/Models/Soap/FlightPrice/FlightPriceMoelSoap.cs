﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.Soap.FlightPrice
{
    public class FlightPriceMoelSoap
    {
        [Required(ErrorMessage = "The Number of passengers are required.")]
        public int? Adults { get; set; }
        public int? Child { get; set; }  
        public int? Infant { get; set; }
        public string? pricingOptionKey { get; set; }

        public FlightSegmentSoap Outbound { get; set; }
        public FlightSegmentSoap Inbound { get; set; }
    }
    public class FlightSegmentSoap
    {
        [Required(ErrorMessage = "The Departure Date is required.")]
        public string? departure_date { get; set; }
        [Required(ErrorMessage = "The Departure Time is required.")]
        public string? departure_time { get; set; }
        [Required(ErrorMessage = "The Airport From is required.")]
        public string? airport_from { get; set; }
        [Required(ErrorMessage = "The Airport To is required.")]
        public string? airport_to { get; set; }
        [Required(ErrorMessage = "The Marketing Company is required.")]
        public string? marketing_company { get; set; }
        public string? flight_number { get; set; }
        public string? booking_class { get; set; }
    }
}