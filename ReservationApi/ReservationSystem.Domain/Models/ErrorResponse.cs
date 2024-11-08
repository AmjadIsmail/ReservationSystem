using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ReservationSystem.Domain.Models
{
    public class ErrorResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("errors")]
        public List<ErrorDetail> Errors { get; set; }
    }
    public class ErrorResponseAmadeus
    {
        [JsonProperty("errors")]
        public List<Error> Errors { get; set; }
    }
    public class Error
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("source")]
        public Source Source { get; set; }
    }

    public class Source
    {
        [JsonProperty("pointer")]
        public string Pointer { get; set; }

        [JsonProperty("example")]
        public string Example { get; set; }
    }
}
