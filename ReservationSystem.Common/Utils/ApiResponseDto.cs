using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Common.Utils
{
    public class ApiResponseDto
    {
        public dynamic Response { get; set; }
        public bool IsSuccessful { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public dynamic Data { get; set; }
    }
}
