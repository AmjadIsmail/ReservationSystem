using ReservationSystem.Domain.Models.Validators;
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
        public string? selectedFlightOffer { get; set; }
    }
    public class PassengerDetails
    {
        [CheckName]
        public string? firstName { get; set; }
        [CheckName]
        public string? surName { get; set; }
        public string? type { get; set; }
        [CheckDOB]
        public string? dob { get; set; }
        public bool? isLeadPassenger { get; set; }
        public int? number { get; set; }
        [CheckEmail]
        public string? email { get; set; }
        
        public string? phone { get; set; }

    }
}
