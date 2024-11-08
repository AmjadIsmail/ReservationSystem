using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.Availability
{
    public class AmadeusResponseError
    {
        public int? errorCode { get; set; }
        public string? error { get; set; }
        public ErrorResponseAmadeus? error_details { get; set; }
    }
}
