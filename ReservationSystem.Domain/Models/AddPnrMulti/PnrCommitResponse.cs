using ReservationSystem.Domain.Models.Availability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.AddPnrMulti
{
    public class PnrCommitResponse
    {
        public HeaderSession? session { get; set; }
        public PNRHeader? PNRHeader { get; set; }
        public AmadeusResponseError? amadeusError { get; set; }
       
    }
    public class PNRHeader
    {
        public Reservation? Reservation { get; set; }
        public PnrSecurityInformation? pnrSecurityInformation { get; set; }
        public object? sbrPOSDetails { get; set; }
        public object? sbrCreationPosDetails { get; set;}
        public object? sbrUpdatorPosDetails { get; set; }
        public object? originDestinationDetails { get; set; }
        public object? dataElementsMaster { get; set; }

    }
    public class Reservation
    {
        public string? companyId { get; set; }
        public string? controlNumber { get; set; }
        public string? PNR { get; set; }
    }

    public class PnrSecurityInformation
    {
        public ResponsibilityInformation  responsibilityInformation { get; set; }
    }
    public class ResponsibilityInformation
    {
        public string? typeOfPnrElement { get; set; }
    }
}
