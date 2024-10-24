using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.AddPnrMulti
{
    public class AddPnrMultiRequset
    {
        public HeaderSession sessionDetails { get; set; }
        public List<PassengerDetails> passengerDetails { get; set; }
    }
    public class PassengerDetails
    {
        public string? firstName { get; set; }
        public string? surName { get; set; }
        public string? type { get; set; }
        public string? dob { get; set; }
        public bool? isLeadPassenger { get; set; }
        public int? number { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }

    }
}
