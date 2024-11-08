using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class Document
    {
        public string? documentType { get; set; }
        public string? birthPlace { get; set; }
        public string? issuanceLocation { get; set; }
        public string? issuanceDate { get; set; }
        public string? number { get; set; }
        public string? expiryDate { get; set; }
        public string? issuanceCountry { get; set; }
        public string? validityCountry { get; set; }
        public string? nationality { get; set; }
        public bool? holder { get; set; }
    }
}
