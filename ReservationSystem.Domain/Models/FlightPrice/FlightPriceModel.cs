using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ReservationSystem.Domain.Models.Availability;

namespace ReservationSystem.Domain.Models.FlightPrice
{
    public class FlightPriceModel
    {
        public FlightOffer flightOffers { get; set; }
    }
    
    public class FlightPriceModelReturn
    {
        [JsonPropertyName("data")]
        public FligthPriceData data { get; set; }       
        public AmadeusResponseError? amadeusError { get; set; }
    }

    public class FligthPriceData
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("flightOffers")]
        public List<FlightOffer> flightOffers { get; set; }

        [JsonPropertyName("bookingRequirements")]
        public BookingRequirements BookingRequirements { get; set; }
    }
   
    public class FlightPriceReturnModel
    {
<<<<<<< HEAD
        public List<FlightOfferForFlightPrice> data { get; set; }
        public AmadeusResponseError? amadeusError { get; set; }
=======
        public HeaderSession? Session { get; set; }
        public List<FlightOfferForFlightPrice> flightPrice { get; set; }
        public AmadeusResponseError? amadeusError { get; set; }
        
>>>>>>> 327b9c02008c178a3d19c315f50d1a405d46bcb1
    }
}
