using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using ReservationSystem.Domain.Repositories;
using System.Net.Http.Headers;
using ReservationSystem.Domain.Models.FlightPrice;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class FlightPriceRepository : IFlightPriceRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        public FlightPriceRepository(IConfiguration _configuration, IMemoryCache cache)
        {
            configuration = _configuration;
            _cache = cache;
        }
        public async Task<FlightPriceModelReturn> GetFlightPrice(string token, FlightPriceModel requestModel)
        {
            FlightPriceModelReturn flightPrice = new FlightPriceModelReturn();
            flightPrice.data = new FligthPriceData();
            try
            {
                string amadeusRequest = "";
                List<FlightPriceModel> flightPriceModels = new List<FlightPriceModel>();
             
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/vnd.amadeus+json");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                flightPriceModels.Add(requestModel);
                var jsonData = new
                {
                    data = new
                    {
                        type = "flight-offers-pricing",
                        flightOffers = new[]
                        {
                       requestModel.flightOffers
                    }

                    }
                };
                var amadeusSettings = configuration.GetSection("Amadeus");
                var apiUrl = amadeusSettings["apiUrl"] + "v1/shopping/flight-offers/pricing?forceClass=false";
                var jsonContent = JsonConvert.SerializeObject(jsonData);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/vnd.amadeus+json");
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = httpContent
                };
                request.Headers.Add("Authorization", "Bearer " + token);               
                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    var responseContent = await response.Content.ReadAsStringAsync();
                    flightPrice = JsonConvert.DeserializeObject<FlightPriceModelReturn>(responseContent);

                    using (JsonDocument document = JsonDocument.Parse(responseContent))
                    {
                     //   JsonElement flightOffersElement = document.RootElement.GetProperty("data").GetProperty("flightOffers");
                     //   var flightOffers = System.Text.Json.JsonSerializer.Deserialize<List<FlightOffer>>(flightOffersElement.GetRawText());
                     //   flightPrice.data.flightOffers = flightOffers;
                    //    JsonElement bookingRequirementsElement = document.RootElement.GetProperty("data").GetProperty("bookingRequirements");
                    //    var bookingRequirements = System.Text.Json.JsonSerializer.Deserialize<BookingRequirements>(bookingRequirementsElement.GetRawText());
                    //    flightPrice.data.BookingRequirements = bookingRequirements;
                    //    JsonElement type = document.RootElement.GetProperty("data").GetProperty("type");
                    //    var typedata = System.Text.Json.JsonSerializer.Deserialize<string>(type.GetRawText());
                    //    flightPrice.data.Type = typedata;

                    }
                    Console.WriteLine("Response received successfully: " + responseContent);
                }
                else
                {
                 
                  var error =  await response.Content.ReadAsStringAsync();

                    flightPrice.amadeusError = new AmadeusResponseError();
                    flightPrice.amadeusError.error = response.StatusCode.ToString();
                    int statusCode = (int)response.StatusCode;
                    string reasonPhrase = response.ReasonPhrase;
                    flightPrice.amadeusError.errorCode = statusCode;
                    if (response.StatusCode.ToString() == "Unauthorized")
                    {
                        flightPrice.amadeusError.errorCode = 401;
                    }
                    flightPrice.amadeusError.error = statusCode.ToString() + " - " + reasonPhrase;
                    ErrorResponseAmadeus errorResponse = JsonConvert.DeserializeObject<ErrorResponseAmadeus>(error);
                    flightPrice.amadeusError.error = response.StatusCode.ToString();
                    flightPrice.amadeusError.error_details = errorResponse;
                    Console.WriteLine("Error: " + response.StatusCode);
                }

                return flightPrice;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                return flightPrice;
            }
          

           
           
        }

    }
}
