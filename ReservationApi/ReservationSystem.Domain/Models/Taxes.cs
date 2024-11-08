using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class Taxes
    {
        public string? amount { get; set; }
        public string? code { get; set; }
    }
    public class TextData
    {
        public string? freeText { get; set; } = string.Empty;
        public  freeTextQualification? freeTextQualifications { get; set; }
    }
    public class freeTextQualification
    {
        public string? textSubjectQualifier { get; set; }
        public string? informationType { get; set; }
    }

    public class taxDetails
    {
        public string? rate { get; set; }
        public string? countryCode { get; set; }
        public string? type { get; set; }

    }
    public class taxesAmount
    {
        public List<taxDetails>? taxDetails { get; set; }
    }
}
