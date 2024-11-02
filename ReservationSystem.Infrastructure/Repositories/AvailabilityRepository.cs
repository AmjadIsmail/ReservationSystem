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
        private readonly ICacheService _cacheService;
        private readonly IDBRepository _dbRepository;
        public AvailabilityRepository(IConfiguration _configuration ,  ICacheService cacheService , IDBRepository dBRepository)
        {
            configuration = _configuration;
            _cacheService = cacheService;
            _dbRepository = dBRepository;
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
                      
                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);                           
                            string accessToken = jsonResponse.access_token;
                            returnStr = accessToken;                         
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

        public async Task<AvailabilityModel> GetAvailability(string token, AvailabilityRequest requestModel)
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
                        string ResToSave = JsonConvert.SerializeObject(result, Formatting.Indented);
                        availabilities.data = result.data;
                        if (result.data.Count > 0)
                        {
                            if (flightsDictionary != null && flightsDictionary.FirstOrDefault().apply_markup == true)
                            {
                                result.data = applyMarkup(result.data, flightsDictionary);
                            }
                            if (flightsDictionary != null && flightsDictionary.FirstOrDefault().apply_airline_discount == true)
                            {
                                result.data = applyDiscount(result.data, flightsDictionary);
                            }
                            _cacheService.Set("amadeusRequest" + amadeusRequest, availabilities, TimeSpan.FromMinutes(15));
                            _dbRepository.SaveAvailabilityResult(amadeusRequest, ResToSave.ToString(), result.data.Count);
                        }

                        Console.WriteLine("Response: " + responseContent);
                    }
                    else
                    {
                        availabilities.amadeusError = new AmadeusResponseError();
                        availabilities.amadeusError.error = response.StatusCode.ToString();
                        availabilities.amadeusError.errorCode = 400;
                        if (response.StatusCode.ToString() == "Unauthorized")
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
                if (!String.IsNullOrEmpty(request.cabinClass))
                {
                    res = res + "&travelClass=" + request.cabinClass;
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
                var adultpp = dictionary.FirstOrDefault()?.adult_markup != null ? dictionary.FirstOrDefault()?.adult_markup : 0;
                var childpp = dictionary.FirstOrDefault()?.child_markup != null ? dictionary.FirstOrDefault()?.child_markup : 0;
                var infantpp = dictionary.FirstOrDefault()?.infant_markup != null ? dictionary.FirstOrDefault()?.infant_markup : 0;

                

                #region Apply Markup
                foreach (var item in offers)
                {
                    childpp = item.travelerPricings.Where(e => e.travelerType == "CHILD").Any() ? childpp : 0;
                    infantpp = item.travelerPricings.Where(e => e.travelerType == "HELD_INFANT").Any() ? infantpp : 0;
                    foreach (var item2 in item.travelerPricings)
                    {
                        var travelerType = item2?.travelerType;
                        if (travelerType != null)
                        {
                            switch (travelerType)
                            {
                                case "ADULT":
                                    item.price.markup = adultpp + childpp  + infantpp ;
                                    item.price.total = (Convert.ToDecimal(item?.price?.total) + adultpp + childpp + infantpp ).ToString();
                                    item.price.grandTotal = (Convert.ToDecimal(item?.price?.grandTotal) + adultpp + childpp + infantpp).ToString();
                                    item2.price.markup = adultpp;                                   
                                    item2.price.grandTotal = (Convert.ToDecimal(item2?.price?.total) + adultpp).ToString();
                                    break;
                                case "CHILD":
                                   
                                    item2.price.markup = childpp;
                                    item2.price.grandTotal = (Convert.ToDecimal(item2?.price?.total) + childpp).ToString();
                                    break;
                                case "HELD_INFANT":
                                    item2.price.markup = infantpp;
                                    item2.price.grandTotal = (Convert.ToDecimal(item2?.price?.total) + infantpp).ToString();
                                    break;
                            }
                        }
                    }

                }
                #endregion


            }
            catch( Exception ex)
            {

            }          
            return offers;
        }

        private List<FlightOffer> applyDiscount(List<FlightOffer> offers, List<FlightMarkup> dictionary)
        {
            try
            {            

                #region Apply Airline Discount
                var applyAirlineDis = dictionary.FirstOrDefault().apply_airline_discount;
                if (applyAirlineDis != null && applyAirlineDis == true)
                {
                    var airline = dictionary.FirstOrDefault().airline;
                    var airlineDiscount = dictionary.FirstOrDefault().discount_on_airline;
                    string[] stringArray = airline.Split(',');
                    foreach (var item in stringArray)
                    {
                        var offer = offers.Where(o => o.itineraries.Any(i => i.segments.Any(s => s.marketingCarrierCode == item))).ToList();


                        foreach (var flight in offer)
                        {
                            flight.price.discount = airlineDiscount.Value;
                            flight.price.total = (Convert.ToDecimal(flight?.price?.total) - airlineDiscount.Value).ToString();
                            flight.price.grandTotal = (Convert.ToDecimal(flight?.price?.grandTotal) - airlineDiscount.Value).ToString();
                        }
                    }
                }
                #endregion

                


            }
            catch (Exception ex)
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
