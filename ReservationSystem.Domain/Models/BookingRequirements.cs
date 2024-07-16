using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class BookingRequirements
    {
        [JsonPropertyName("emailAddressRequired")]
        public bool EmailAddressRequired { get; set; }

        [JsonPropertyName("mobilePhoneNumberRequired")]
        public bool MobilePhoneNumberRequired { get; set; }
    }
}
