using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.DBLogs
{
    public class SaveReservationLog
    {
        public string? AmadeusSessionId { get; set; }
        public string? RequestName { get; set; }     
        public string? Request { get; set; }       
        public string? Response { get; set; }       
        public int? UserId { get; set; }        
        public bool? IsError { get; set; }

    }
    public enum RequestName
    {
        Availability,
        AirSell,
        DocIssue,
        FareCheck,
        FlightPrice,
        FlightPriceBest,
        Fop,
        FarePricePNR,
        CreateTst,
        PNRMulti,
        GeneratePNR
    }
}
