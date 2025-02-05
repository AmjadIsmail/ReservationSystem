using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReservationSystem.Domain.Models
{
    public class Price
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? currency { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? adultPP { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? adultTax { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? adulMarkup { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? childPp { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? childTax{ get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? childMarkup { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? infantPp { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? infantTax { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? infantMarkup { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? total { get; set; }
        [JsonProperty("base",NullValueHandling = NullValueHandling.Ignore)]
        public string? baseAmount { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Taxes>? taxes { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<taxDetails>? taxDetails { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Fee>? fees { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? grandTotal { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? billingCurrency { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? refundableTaxes { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? markup { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? discount { get; set; }
        public int? MarkupID { get; set; }
        }
    }

