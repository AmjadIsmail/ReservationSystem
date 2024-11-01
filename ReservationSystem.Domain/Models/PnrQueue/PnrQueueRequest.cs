using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.PnrQueue
{
    public class PnrQueueRequest
    {
        public string? selectionDetailsOption { get; set; } 
        public string? sourceQualifier1 { get; set; }
        public string? queueDetailNumber { get; set; }
        public string? identificationType { get; set; }
        public string? identificationNumber { get; set; }
        public string? pnr { get; set; }
    }
}
