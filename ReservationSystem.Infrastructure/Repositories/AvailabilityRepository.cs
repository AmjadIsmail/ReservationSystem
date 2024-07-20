using Microsoft.Extensions.Configuration;
using ReservationSystem.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReservationSystem.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using ReservationSystem.Domain.Models.Availability;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using ReservationSystem.Domain.Service;
using ReservationSystem.Domain.DB_Models;


namespace ReservationSystem.Infrastructure.Repositories
{
    public class AvailabilityRepository  : IAvailabilityRepository
    {
        private readonly IConfiguration configuration;
       // private readonly IMemoryCache _cache;
        private readonly ICacheService _cacheService;
        public AvailabilityRepository(IConfiguration _configuration ,  ICacheService cacheService)
        {
            configuration = _configuration;
            _cacheService = cacheService;
        }
        public async Task<string> getToken()
        {
            try
            {
                string token;
                token =  _cacheService.Get<string>("amadeusToken");
                if (token == null)
                {
                    string returnStr = "";
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                        var amadeusSettings = configuration.GetSection("Amadeus");

                        var grantType = amadeusSettings["grant_type"];
                        var clientId = amadeusSettings["client_id"];
                        var clientSecret = amadeusSettings["client_secret"];
                        var tokenurl = amadeusSettings["tokenUrl"];
                        var formData = new Dictionary<string, string>
            {
                { "grant_type", grantType },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            };

                        var request = new HttpRequestMessage(HttpMethod.Post, tokenurl);

                        request.Content = new FormUrlEncodedContent(formData);

                        var response = await httpClient.SendAsync(request);

                        // 6. Handle the response
                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);

                            // 7. Extract the access_token
                            string accessToken = jsonResponse.access_token;
                            returnStr = accessToken;
                            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15));
                           // _cache.Set("amadeusToken", accessToken, cacheEntryOptions);
                            _cacheService.Set("amadeusToken" , accessToken, TimeSpan.FromMinutes(15));
                            Console.WriteLine("Response: " + responseContent);
                        }
                        else
                        {
                            Console.WriteLine("Error: " + response.StatusCode);
                        }
                    }
                    token = returnStr;
                }
               
                return token;
            }
            catch( Exception ex)
            {
                _cacheService.Remove("amadeusToken");
                return "";
            }
          
        }

        public async Task<AvailabilityModel> GetAvailability(string token , AvailabilityRequest requestModel)
        {
            string amadeusRequest = generateRequest(requestModel);
            AvailabilityModel availabilities = new AvailabilityModel();
            List<FlightMarkup> flightsDictionary = _cacheService.GetFlightsMarkup();
            var availabilityModel = _cacheService.Get<AvailabilityModel>("amadeusRequest" + amadeusRequest);
            if (availabilityModel == null)
            {
                availabilities = new AvailabilityModel();
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                    var request = new HttpRequestMessage(HttpMethod.Get, amadeusRequest);
                    request.Headers.Add("Authorization", "Bearer " + token);

                    var response = await httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                     
                        AvailabilityResult result = JsonConvert.DeserializeObject<AvailabilityResult>(responseContent);
                        availabilities.data = result.data;
                        if (result.data.Count > 0)
                        {
                           
                            result.data = applyMarkup(result.data, flightsDictionary);
                            _cacheService.Set("amadeusRequest" + amadeusRequest, availabilities, TimeSpan.FromMinutes(15));
                        }
                        
                        Console.WriteLine("Response: " + responseContent);
                    }
                    else
                    {
                        availabilities.amadeusError = new AmadeusResponseError();
                        availabilities.amadeusError.error = response.StatusCode.ToString();
                        availabilities.amadeusError.errorCode = 400;
                        if(response.StatusCode.ToString() == "Unauthorized")
                        {
                            _cacheService.Remove("amadeusToken");
                            availabilities.amadeusError.errorCode = 401;
                        }                     
                        var error = await response.Content.ReadAsStringAsync();
                        ErrorResponseAmadeus errorResponse = JsonConvert.DeserializeObject<ErrorResponseAmadeus>(error);

                        availabilities.amadeusError.error = response.StatusCode.ToString();
                        availabilities.amadeusError.error_details = errorResponse;
                        Console.WriteLine("Error: " + response.StatusCode);
                    }
                }
            }
            else
            {
                availabilities = availabilityModel;
            }
               
            return availabilities;
        }
        private string generateRequest(AvailabilityRequest request)
        {
            string res = string.Empty;
            try
            {
                // https://test.api.amadeus.com/v2/shopping/flight-offers?originLocationCode=SYD&destinationLocationCode=BKK&departureDate=2024-07-02&returnDate=2024-07-22&adults=2&children=1&infants=1&travelClass=ECONOMY&nonStop=false&currencyCode=GBP&maxPrice=500&max=25
                // https://test.api.amadeus.com/v2/shopping/flight-offers?originLocationCode=SYD&destinationLocationCode=BKK&departureDate=2024-07-02&returnDate=2024-07-22&adults=2&children=1&infants=1&travelClass=ECONOMY&nonStop=False&currencyCode=GBP&maxPrice=500&max=20
                var amadeusSettings = configuration.GetSection("Amadeus");
                var apiUrl = amadeusSettings["apiUrl"]+ "v2/shopping/flight-offers?";
                res = apiUrl + "originLocationCode=" + request.origin + "&destinationLocationCode=" + request.destination;
                res = res + "&departureDate=" + request.departureDate;
                if(!String.IsNullOrEmpty(request.returnDate))
                {
                    res = res + "&returnDate=" + request.returnDate;
                }
                res = res + "&adults=" + request.adults;
                if(request.children != null && request.children != 0)
                {
                    res = res + "&children=" + request.children;
                }
                if (request.infant != null && request.infant != 0)
                {
                    res = res + "&infants=" + request.infant;
                }
                if (!String.IsNullOrEmpty(request.travelClass))
                {
                    res = res + "&travelClass=" + request.travelClass;
                }
                if(request.nonStop != null)
                {
                    res = res + "&nonStop=" + request.nonStop.ToString().ToLower();
                }
                res = res + "&currencyCode=GBP";
                if(request.maxPrice != null)
                {
                    res = res + "&maxPrice=" + request.maxPrice;
                }
                if (request.maxFlights  != null)
                {
                    res = res + "&max=" + request.maxFlights;
                }

            }
            catch (Exception ex)
            {

            }
            return res;
        }        

        private List<FlightOffer> applyMarkup (List<FlightOffer> offers , List<FlightMarkup> dictionary)
        {
            try
            {
                var adultpp = dictionary.FirstOrDefault().adult_markup;
                var childpp = dictionary.FirstOrDefault().child_markup;
                var infantpp = dictionary.FirstOrDefault().infant_markup;

                foreach (var item in offers)
                {
                    foreach( var item2 in item.travelerPricings)
                    {
                        var travelerType = item2?.travelerType;
                        if (travelerType != null)
                        {
                            switch (travelerType)
                            {
                                case "ADULT":
                                    item.price.markup = adultpp.Value;
                                    item.price.total = (Convert.ToDecimal(item?.price?.total) + adultpp.Value).ToString();
                                    item.price.grandTotal = (Convert.ToDecimal(item?.price?.grandTotal) + adultpp.Value).ToString();
                                    item2.price.markup = adultpp.Value;
                                    item2.price.total = (Convert.ToDecimal(item2?.price?.total) + adultpp.Value).ToString();
                                    item2.price.grandTotal = (Convert.ToDecimal(item?.price?.grandTotal)).ToString();
                                    break;
                                case "CHILD":
                                    item.price.markup = childpp.Value;
                                    item.price.total = (Convert.ToDecimal(item?.price?.total) + childpp.Value).ToString();
                                    item.price.grandTotal = (Convert.ToDecimal(item?.price?.grandTotal) + childpp.Value).ToString();
                                    item2.price.markup = childpp.Value;
                                    item2.price.total = (Convert.ToDecimal(item2?.price?.total) + childpp.Value).ToString();
                                    item2.price.grandTotal = (Convert.ToDecimal(item?.price?.grandTotal)).ToString();
                                    break;
                                case "INFANT":
                                    item.price.markup = infantpp.Value;
                                    item.price.total = (Convert.ToDecimal(item?.price?.total) + infantpp.Value).ToString();
                                    item.price.grandTotal = (Convert.ToDecimal(item?.price?.grandTotal) + infantpp.Value).ToString();
                                    item2.price.markup = infantpp.Value;
                                    item2.price.total = (Convert.ToDecimal(item2?.price?.total) + infantpp.Value).ToString();
                                    item2.price.grandTotal = (Convert.ToDecimal(item?.price?.grandTotal)).ToString();
                                    break;
                            }
                        }
                    }
                   
                }
            }
            catch( Exception ex)
            {

            }          
            return offers;
        }

        public async Task ClearCache()
        {
             _cacheService.RemoveAll();
            _cacheService.ResetCacheData();
        }
    }

}
