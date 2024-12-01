using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.PricePnr
{
    public class PricePnrRequest
    {
        [Required]
        public HeaderSession? sessionDetails { get; set; }
        [Required]
        public string pricingOptionKey { get; set; }
        [Required]
        public string carrierCode { get; set; }
    }
}
