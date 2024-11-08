using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class AutomatedProcess
    {
        public string? Code { get; set; }
        public Queue? Queue { get; set; }
        public string? OfficeId { get; set; }
    }
}
