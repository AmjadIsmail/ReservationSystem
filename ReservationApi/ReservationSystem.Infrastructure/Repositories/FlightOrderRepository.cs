using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReservationSystem.Domain.Models;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models.FlightOrder;
using ReservationSystem.Domain.Models.FlightPrice;
using ReservationSystem.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class FlightOrderRepository : IFlightOrderRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        public FlightOrderRepository(IConfiguration _configuration, IMemoryCache cache)
        {
            configuration = _configuration;
            _cache = cache;
        }

        public async Task<FlightCreateOrderResponse> CreateFlightOrder(string token, FlightOrderCreateRequestModel requestOrder)
        {
            
            FlightCreateOrderResponse flightOrder = new FlightCreateOrderResponse();
            FlightOfferForOrder req = new FlightOfferForOrder();
            #region Flight Request
            req.id = requestOrder?.flightOffers[0].id;
            req.type = requestOrder?.flightOffers[0].type;
            req.source = requestOrder.flightOffers[0].source;
           // req.instantTicketingRequired = requestOrder.flightOffers[0].instantTicketingRequired;
            //req.nonHomogeneous = requestOrder.flightOffers[0].nonHomogeneous;
            req.paymentCardRequired = false;
            req.lastTicketingDate = requestOrder.flightOffers[0].lastTicketingDate;
            req.itineraries = requestOrder.flightOffers[0].itineraries;
            req.price = requestOrder.flightOffers[0].price;
            req.pricingOptions = requestOrder.flightOffers[0].pricingOptions;
            req.validatingAirlineCodes = requestOrder.flightOffers[0].validatingAirlineCodes;
            req.travelerPricings = requestOrder.flightOffers[0].travelerPricings;
            #endregion
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            Traveler _traveler = requestOrder.travelers.FirstOrDefault();
            Contact _contact = requestOrder.contacts.FirstOrDefault();
            TicketingAgreement _agreement = requestOrder.ticketingAgreement;
            Remarks _remarks = requestOrder.remarks;
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/vnd.amadeus+json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var amadeusSettings = configuration.GetSection("Amadeus");
            var apiUrl = amadeusSettings["apiUrl"] + "v1/booking/flight-orders";

            var flightOrderRequest = new FlightOrderRequest
            {
                data = new data
                {
                    type = "flight-order",
                    flightOffers = new List<FlightOfferForOrder>
                       {
                       req
                    },
                    travelers = new List<Traveler>
                    {
                        _traveler
                    },
                    remarks = requestOrder.remarks,
                    ticketingAgreement = _agreement,
                    contacts = new List<Contact>
                    {
                        _contact
                    }
                }
            };
            var jsonData = new
            {
                data = new
                {
                    type = "flight-order",
                    flightOffers = new[]
                       {
                       req
                    },
                    travelers = new[]
                    {
                        _traveler
                    },
                    remarks = requestOrder.remarks,
                    ticketingAgreement = requestOrder.ticketingAgreement,
                    contacts = new[]
                    {
                        _contact
                    }

                }
            };
            string test = @"{
  ""data"": {
    ""type"": ""flight-order"",
    ""id"": ""eJzTd9f3dnIOdw8EAAstAmU%3D"",
    ""queuingOfficeId"": ""NCE4D31SB"",
    ""associatedRecords"": [
      {
        ""reference"": ""KBCWGQ"",
        ""creationDate"": ""2024-06-16T07:55:00.000"",
        ""originSystemCode"": ""GDS"",
        ""flightOfferId"": ""1""
      }
    ],
    ""flightOffers"": [
      {
        ""type"": ""flight-offer"",
        ""id"": ""1"",
        ""source"": ""GDS"",
        ""nonHomogeneous"": false,
        ""lastTicketingDate"": ""2024-08-02"",
        ""itineraries"": [
          {
            ""segments"": [
              {
                ""departure"": {
                  ""iataCode"": ""SYD"",
                  ""terminal"": ""1"",
                  ""at"": ""2024-08-02T11:25:00""
                },
                ""arrival"": {
                  ""iataCode"": ""XMN"",
                  ""terminal"": ""3"",
                  ""at"": ""2024-08-02T18:50:00""
                },
                ""carrierCode"": ""MF"",
                ""number"": ""802"",
                ""aircraft"": {
                  ""code"": ""789""
                },
                ""duration"": ""PT9H25M"",
                ""id"": ""3"",
                ""numberOfStops"": 0,
                ""co2Emissions"": [
                  {
                    ""weight"": 388,
                    ""weightUnit"": ""KG"",
                    ""cabin"": ""ECONOMY""
                  }
                ]
              },
              {
                ""departure"": {
                  ""iataCode"": ""XMN"",
                  ""terminal"": ""3"",
                  ""at"": ""2024-08-03T08:40:00""
                },
                ""arrival"": {
                  ""iataCode"": ""BKK"",
                  ""at"": ""2024-08-03T11:15:00""
                },
                ""carrierCode"": ""MF"",
                ""number"": ""853"",
                ""aircraft"": {
                  ""code"": ""738""
                },
                ""duration"": ""PT3H35M"",
                ""id"": ""4"",
                ""numberOfStops"": 0,
                ""co2Emissions"": [
                  {
                    ""weight"": 162,
                    ""weightUnit"": ""KG"",
                    ""cabin"": ""ECONOMY""
                  }
                ]
              }
            ]
          }
        ],
        ""price"": {
          ""currency"": ""GBP"",
          ""total"": ""207.50"",
          ""base"": ""79.00"",
          ""fees"": [
            {
              ""amount"": ""0.00"",
              ""type"": ""TICKETING""
            },
            {
              ""amount"": ""0.00"",
              ""type"": ""SUPPLIER""
            },
            {
              ""amount"": ""0.00"",
              ""type"": ""FORM_OF_PAYMENT""
            }
          ],
          ""grandTotal"": ""207.50"",
          ""billingCurrency"": ""GBP""
        },
        ""pricingOptions"": {
          ""fareType"": [
            ""PUBLISHED""
          ],
          ""includedCheckedBagsOnly"": true
        },
        ""validatingAirlineCodes"": [
          ""MF""
        ],
        ""travelerPricings"": [
          {
            ""travelerId"": ""1"",
            ""fareOption"": ""STANDARD"",
            ""travelerType"": ""ADULT"",
            ""price"": {
              ""currency"": ""GBP"",
              ""total"": ""207.50"",
              ""base"": ""79.00"",
              ""taxes"": [
                {
                  ""amount"": ""31.20"",
                  ""code"": ""AU""
                },
                {
                  ""amount"": ""9.70"",
                  ""code"": ""CN""
                },
                {
                  ""amount"": ""0.80"",
                  ""code"": ""E7""
                },
                {
                  ""amount"": ""0.30"",
                  ""code"": ""G8""
                },
                {
                  ""amount"": ""26.30"",
                  ""code"": ""WY""
                },
                {
                  ""amount"": ""6.20"",
                  ""code"": ""YQ""
                },
                {
                  ""amount"": ""54.00"",
                  ""code"": ""YR""
                }
              ],
              ""refundableTaxes"": ""128.50""
            },
            ""fareDetailsBySegment"": [
              {
                ""segmentId"": ""3"",
                ""cabin"": ""ECONOMY"",
                ""fareBasis"": ""SOW6AAUS"",
                ""brandedFare"": ""YSTANDARD"",
                ""class"": ""S"",
                ""includedCheckedBags"": {
                  ""quantity"": 1
                }
              },
              {
                ""segmentId"": ""4"",
                ""cabin"": ""ECONOMY"",
                ""fareBasis"": ""SOW6AAUS"",
                ""brandedFare"": ""YSTANDARD"",
                ""class"": ""S"",
                ""includedCheckedBags"": {
                  ""quantity"": 1
                }
              }
            ]
          }
        ]
      }
    ],
    ""travelers"": [
      {
        ""id"": ""1"",
        ""dateOfBirth"": ""1982-01-16"",
        ""gender"": ""MALE"",
        ""name"": {
          ""firstName"": ""JORGE"",
          ""lastName"": ""GONZALES""
        },
        ""documents"": [
          {
            ""number"": ""00000000"",
            ""issuanceDate"": ""2023-04-14"",
            ""expiryDate"": ""2025-04-14"",
            ""issuanceCountry"": ""ES"",
            ""issuanceLocation"": ""Madrid"",
            ""nationality"": ""ES"",
            ""birthPlace"": ""Madrid"",
            ""documentType"": ""PASSPORT"",
            ""holder"": true
          }
        ],
        ""contact"": {
          ""purpose"": ""STANDARD"",
          ""phones"": [
            {
              ""deviceType"": ""MOBILE"",
              ""countryCallingCode"": ""34"",
              ""number"": ""480080076""
            }
          ],
          ""emailAddress"": ""amjad.rahimo@live.com""
        }
      }
    ],
    ""remarks"": {
      ""general"": [
        {
          ""subType"": ""GENERAL_MISCELLANEOUS"",
          ""text"": ""ONLINE BOOKING FROM INCREIBLE VIAJES""
        }
      ]
    },
    ""ticketingAgreement"": {
      ""option"": ""DELAY_TO_CANCEL"",
      ""delay"": ""6D""
    },
    ""automatedProcess"": [
      {
        ""code"": ""IMMEDIATE"",
        ""queue"": {
          ""number"": ""0"",
          ""category"": ""0""
        },
        ""officeId"": ""NCE4D31SB""
      }
    ],
    ""contacts"": [
      {
        ""addresseeName"": {
          ""firstName"": ""PABLO RODRIGUEZ""
        },
        ""address"": {
          ""lines"": [
            ""Calle Prado, 16""
          ],
          ""postalCode"": ""28014"",
          ""countryCode"": ""ES"",
          ""cityName"": ""Madrid""
        },
        ""purpose"": ""STANDARD"",
        ""phones"": [
          {
            ""deviceType"": ""LANDLINE"",
            ""countryCallingCode"": ""34"",
            ""number"": ""480080071""
          },
          {
            ""deviceType"": ""MOBILE"",
            ""countryCallingCode"": ""33"",
            ""number"": ""480080072""
          }
        ],
        ""companyName"": ""INCREIBLE VIAJES"",
        ""emailAddress"": ""support@increibleviajes.es""
      }
    ]
  },
  ""dictionaries"": {
    ""locations"": {
      ""BKK"": {
        ""cityCode"": ""BKK"",
        ""countryCode"": ""TH""
      },
      ""XMN"": {
        ""cityCode"": ""XMN"",
        ""countryCode"": ""CN""
      },
      ""SYD"": {
        ""cityCode"": ""SYD"",
        ""countryCode"": ""AU""
      }
    }
  }
}";
            FlightOrderRequest flightOrderRequesttest = JsonConvert.DeserializeObject<FlightOrderRequest>(test);

            var jsonContent = JsonConvert.SerializeObject(flightOrderRequest);
            jsonContent = jsonContent.Replace("\"base_amount\"", "\"base\"");
            var jsonString = System.Text.Json.JsonSerializer.Serialize(jsonData,options);
            jsonString = jsonString.Replace("base_amount", "base");
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/vnd.amadeus+json");
            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = httpContent
            };
           
            request.Headers.Add("Authorization", "Bearer " + token);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.amadeus+json"));
            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using (JsonDocument document = JsonDocument.Parse(responseContent))
                {
                    JsonElement type = document.RootElement.GetProperty("data").GetProperty("type");
                    var typedata = System.Text.Json.JsonSerializer.Deserialize<string>(type.GetRawText());
                    flightOrder.Type = typedata;
                    JsonElement flightOffersElement = document.RootElement.GetProperty("data").GetProperty("flightOffers");
                    var flightOffers = System.Text.Json.JsonSerializer.Deserialize<List<FlightOffer>>(flightOffersElement.GetRawText());
                    flightOrder.FlightOffers = flightOffers;
                    JsonElement travelers = document.RootElement.GetProperty("data").GetProperty("travelers");
                    var travelerdata = System.Text.Json.JsonSerializer.Deserialize<List<Traveler>>(travelers.GetRawText());
                    flightOrder.Travelers = travelerdata;
                    JsonElement remarks = document.RootElement.GetProperty("data").GetProperty("remarks");
                    var remarksdata = System.Text.Json.JsonSerializer.Deserialize<Remarks>(remarks.GetRawText());
                    flightOrder.Remarks = remarksdata;
                    JsonElement tiecketaggrement = document.RootElement.GetProperty("data").GetProperty("ticketingAgreement");
                    var ticketdata = System.Text.Json.JsonSerializer.Deserialize<TicketingAgreement>(remarks.GetRawText());
                    flightOrder.TicketingAgreement = ticketdata;
                    JsonElement automateprocess = document.RootElement.GetProperty("data").GetProperty("automatedProcess");
                    var autodata = System.Text.Json.JsonSerializer.Deserialize<List<AutomatedProcess>>(automateprocess.GetRawText());
                    flightOrder.AutomatedProcess = autodata;
                    JsonElement contacts = document.RootElement.GetProperty("data").GetProperty("contacts");
                    var contactsdata = System.Text.Json.JsonSerializer.Deserialize<List<Contact>>(contacts.GetRawText());
                    flightOrder.Contacts = contactsdata;
                    JsonElement dictionary = document.RootElement.GetProperty("data").GetProperty("dictionaries");
                    var dictiionarydata = System.Text.Json.JsonSerializer.Deserialize<Dictionaries>(dictionary.GetRawText());
                    flightOrder.Dictionaries = dictiionarydata;

                }
                Console.WriteLine("Response received successfully: " + responseContent);
            }
            else
            {
                flightOrder.amadeusError = new AmadeusResponseError();
                flightOrder.amadeusError.error = response.StatusCode.ToString();
                int statusCode = (int)response.StatusCode;
                string reasonPhrase = response.ReasonPhrase;
                flightOrder.amadeusError.errorCode = statusCode;
                if (response.StatusCode.ToString() == "Unauthorized")
                {
                    flightOrder.amadeusError.errorCode = 401;
                }
                  var error =  await response.Content.ReadAsStringAsync();
                ErrorResponseAmadeus errorResponse = JsonConvert.DeserializeObject<ErrorResponseAmadeus>(error);

                flightOrder.amadeusError.error =  response.StatusCode  + " - " + reasonPhrase;
                flightOrder.amadeusError.error_details = errorResponse;
                Console.WriteLine("Error: " + response.StatusCode);
            }

            return flightOrder;
        }
    }
}
