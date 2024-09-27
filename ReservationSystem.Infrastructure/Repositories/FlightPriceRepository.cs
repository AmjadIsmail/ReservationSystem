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
using ReservationSystem.Domain.Models.Soap.FlightPrice;
using System.Net;
using System.Xml.Linq;
using System.Xml;
using System.Security.Cryptography;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Http;

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

        public async Task<AvailabilityModel> GetFlightPrice(FlightPriceMoelSoap requestModel)
        {
            AvailabilityModel flightPrice = new AvailabilityModel();
          //  flightPrice.data = new FligthPriceData();
            try
            {

                var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var _url = amadeusSettings["ApiUrl"]; // "https://nodeD2.test.webservices.amadeus.com/1ASIWJIBJAY";
                var _action = amadeusSettings["fareInformativePricingWithoutPNRAction"];
                string Result = string.Empty;
                string Envelope = await CreateFlightPriceRequest(requestModel);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url);
                request.Headers.Add("SOAPAction", _action);
                request.ContentType = "text/xml;charset=\"utf-8\"";
                request.Accept = "text/xml";
                request.Method = "POST";

                using (Stream stream = request.GetRequestStream())
                {
                    byte[] content = Encoding.UTF8.GetBytes(Envelope);
                    stream.Write(content, 0, content.Length);
                }

                try
                {
                    using (WebResponse response = request.GetResponse())
                    {
                        using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                        {
                            var result2 = rd.ReadToEnd();
                            XDocument xmlDoc = XDocument.Parse(result2);
                            XmlWriterSettings settings = new XmlWriterSettings
                            {
                                Indent = true,
                                OmitXmlDeclaration = false,
                                Encoding = System.Text.Encoding.UTF8
                            };
                            using (XmlWriter writer = XmlWriter.Create("d:\\reservationlogs\\FlightPriceResponse" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".xml", settings))
                            {
                                xmlDoc.Save(writer);
                            }
                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(result2);
                            string jsonText = JsonConvert.SerializeXmlNode(xmlDoc2, Newtonsoft.Json.Formatting.Indented);
                            var res = ConvertXmlToModel(xmlDoc);
                            flightPrice.data = res.data;

                        }
                    }
                }
                catch (WebException ex)
                {
                    using (StreamReader rd = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        Result = rd.ReadToEnd();
                        //  returnModel.amadeusError = new AmadeusResponseError();
                        //returnModel.amadeusError.error = Result;
                        //returnModel.amadeusError.errorCode = 0;
                        //return returnModel;

                    }
                }
            }
            catch (Exception ex)
            {
                //  returnModel.amadeusError = new AmadeusResponseError();
                //  returnModel.amadeusError.error = ex.Message.ToString();
                //   returnModel.amadeusError.errorCode = 0;
                //   return returnModel;
            }
            return flightPrice;
        }
        private string GeneratePricingOptionsGroup(string pricingOptions)
        {
            try
            {
                string result = $@"";
                foreach (var option in pricingOptions.Split(','))
                {
                    result += " <pricingOptionGroup>" +
                        "<pricingOptionKey>" +
                        "<pricingOptionKey>" + option + "</pricingOptionKey>" +
                        "</pricingOptionKey>" +
                        "</pricingOptionGroup>";
                }
                return result;

            }
            catch (Exception ex)
            {
                Console.Write("Error while generate Pricing options " + ex.Message.ToString());
                return "";
            }
        }
        private string GeneratePassengerGroup(int adt, int child, int inf)
        {
            string passengerGroupAdt = string.Empty;
            string passengerGroupChild = string.Empty;
            string passengerGroupInf = string.Empty;
            try
            {
                string adtPassenger = "", childPassenger = "", InfPassenger = "";
                if (adt != 0)
                {
                    adtPassenger = " <travellersID>";
                    for (int i = 1; i <= adt; i++)
                    {
                        adtPassenger += $@" <travellerDetails>
                                           <measurementValue>{i}</measurementValue>
                                         </travellerDetails>";
                    }
                    adtPassenger += " </travellersID>";
                    passengerGroupAdt = $@" <passengersGroup>
                        <segmentRepetitionControl>
                        <segmentControlDetails>
                        <quantity>1</quantity>
                         <numberOfUnits>{adt}</numberOfUnits>
                         </segmentControlDetails>
                         </segmentRepetitionControl>
                            {adtPassenger}
                        <discountPtc>
                           <valueQualifier>ADT</valueQualifier>
                        </discountPtc>
                     </passengersGroup>";
                }

                if (child != 0)
                {
                    childPassenger = " <travellersID>";
                    for (int i = 1; i <= child; i++)
                    {
                        childPassenger += $@" <travellerDetails>
                        <measurementValue>{i}</measurementValue>
                        </travellerDetails>";
                    }
                    childPassenger = " </travellersID>";

                    passengerGroupChild = $@" <passengersGroup>
                        <segmentRepetitionControl>
                        <segmentControlDetails>
                        <quantity>1</quantity>
                         <numberOfUnits>{child}</numberOfUnits>
                         </segmentControlDetails>
                         </segmentRepetitionControl>
                            {childPassenger}
                        <discountPtc>
                           <valueQualifier>CNN</valueQualifier>
                        </discountPtc>
                     </passengersGroup>";
                }

                if (inf != 0)
                {
                    InfPassenger = " $<travellersID>";
                    for (int i = 1; i <= inf; i++)
                    {
                        InfPassenger += $@" <travellerDetails>
                  <measurementValue>{i}</measurementValue>
               </travellerDetails>";
                    }
                    InfPassenger = " </travellersID>";

                    passengerGroupInf = $@" <passengersGroup>
                        <segmentRepetitionControl>
                        <segmentControlDetails>
                        <quantity>1</quantity>
                         <numberOfUnits>{inf}</numberOfUnits>
                         </segmentControlDetails>
                         </segmentRepetitionControl>
                            {InfPassenger}
                        <discountPtc>
                           <valueQualifier>INF</valueQualifier>
                        </discountPtc>
                     </passengersGroup>";
                }



                return passengerGroupAdt + passengerGroupChild + passengerGroupInf;

            }
            catch (Exception ex)
            {
                Console.Write($"Error while generate passenger group {ex.Message.ToString()}");
                return passengerGroupAdt;
            }
        }
        private string GenerateSegmentGroup(FlightPriceMoelSoap model)
        {
            try
            {
                string group = $@"";

                group = $@"<segmentGroup>
            <segmentInformation>
               <flightDate>
                  <departureDate>{model.Outbound.departure_date.Replace("-", "")}</departureDate>
                  <departureTime>{model.Outbound.departure_time.Replace(":", "")}</departureTime>
               </flightDate>
               <boardPointDetails>
                  <trueLocationId>{model.Outbound.airport_from.Replace("-", "")}</trueLocationId>
               </boardPointDetails>
               <offpointDetails>
                  <trueLocationId>{model.Outbound.airport_to.Replace("-", "")}</trueLocationId>
               </offpointDetails>
               <companyDetails>
                  <marketingCompany>{model.Outbound.marketing_company}</marketingCompany>
               </companyDetails>
               <flightIdentification>
                  <flightNumber>{model.Outbound.flight_number}</flightNumber>
                  <bookingClass>{model.Outbound.booking_class}</bookingClass>
               </flightIdentification>
               <flightTypeDetails>
                  <flightIndicator>1</flightIndicator>
               </flightTypeDetails>
               <itemNumber>1</itemNumber>
            </segmentInformation>
         </segmentGroup>
            <segmentGroup>
            <segmentInformation>
               <flightDate>
                  <departureDate>{model.Inbound.departure_date.Replace("-", "")}</departureDate>
                  <departureTime>{model.Inbound.departure_time.Replace(":", "")}</departureTime>
               </flightDate>
               <boardPointDetails>
                  <trueLocationId>{model.Inbound.airport_from}</trueLocationId>
               </boardPointDetails>
               <offpointDetails>
                  <trueLocationId>{model.Inbound.airport_to}</trueLocationId>
               </offpointDetails>
               <companyDetails>
                  <marketingCompany>{model.Inbound.marketing_company}</marketingCompany>
               </companyDetails>
               <flightIdentification>
                  <flightNumber>{model.Inbound.flight_number}</flightNumber>
                  <bookingClass>{model.Inbound.booking_class}</bookingClass>
               </flightIdentification>
               <flightTypeDetails>
                  <flightIndicator>2</flightIndicator>
               </flightTypeDetails>
               <itemNumber>2</itemNumber>
            </segmentInformation>
         </segmentGroup>
        ";

                return group;
            }
            catch (Exception ex)
            {
                Console.Write("Error while generate Segment group " + ex.Message.ToString());
                return "";
            }
        }
        public async Task<string> CreateFlightPriceRequest(FlightPriceMoelSoap requestModel)
        {
            string pwdDigest = await generatePassword();
            var amadeusSettings = configuration.GetSection("AmadeusSoap") != null ? configuration.GetSection("AmadeusSoap") : null;
            string action = amadeusSettings["fareInformativePricingWithoutPNRAction"];
            string to = amadeusSettings["ApiUrl"];
            string username = amadeusSettings["webUserId"];
            string dutyCode = amadeusSettings["dutyCode"];
            string requesterType = amadeusSettings["requestorType"];
            string PseudoCityCode = amadeusSettings["PseudoCityCode"]?.ToString();
            string pos_type = amadeusSettings["POS_Type"];
            requestModel.Child = requestModel?.Child != null ? requestModel.Child : 0;
            requestModel.Infant = requestModel?.Infant != null ? requestModel.Infant : 0;

            string Request = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:sec=""http://xml.amadeus.com/2010/06/Security_v1"" xmlns:typ=""http://xml.amadeus.com/2010/06/Types_v1"" xmlns:iat=""http://www.iata.org/IATA/2007/00/IATA2010.1"" xmlns:app=""http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3"" xmlns:link=""http://wsdl.amadeus.com/2010/06/ws/Link_v1"" xmlns:ses=""http://xml.amadeus.com/2010/06/Session_v3"">
   <soapenv:Header xmlns:add=""http://www.w3.org/2005/08/addressing"">
      <add:MessageID>{System.Guid.NewGuid()}</add:MessageID>
      <add:Action>{action}</add:Action>
      <add:To>{to}</add:To>
      <oas:Security xmlns:oas=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:oas1=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">
         <oas:UsernameToken oas1:Id=""UsernameToken-1"">
            <oas:Username>{username}</oas:Username>
            <oas:Nonce EncodingType=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary"">{pwdDigest.Split("|")[1]}</oas:Nonce>
            <oas:Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest"">{pwdDigest.Split("|")[0]}</oas:Password>
            <oas1:Created>{pwdDigest.Split("|")[2]}</oas1:Created>
         </oas:UsernameToken>
      </oas:Security>
      <AMA_SecurityHostedUser xmlns=""http://xml.amadeus.com/2010/06/Security_v1"">
         <UserID AgentDutyCode=""{dutyCode}"" RequestorType=""{requesterType}"" PseudoCityCode=""{PseudoCityCode}"" POS_Type=""{pos_type}""/>
      </AMA_SecurityHostedUser>
   </soapenv:Header>
   <soapenv:Body>
      <Fare_InformativePricingWithoutPNR>
       {GeneratePassengerGroup(requestModel.Adults.Value, requestModel.Child.Value, requestModel.Infant.Value)}
        {GenerateSegmentGroup(requestModel)}
         {GeneratePricingOptionsGroup(requestModel.pricingOptionKey)}
      </Fare_InformativePricingWithoutPNR>
   </soapenv:Body>

</soapenv:Envelope>";

            return Request;
        }

        public async Task<string> generatePassword()
        {
            try
            {
                var amadeusSettings = configuration.GetSection("AmadeusSoap");
                string password = amadeusSettings["clearPassword"];
                string passSHA;
                byte[] nonce = new byte[32];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(nonce);
                }
                DateTime utcNow = DateTime.UtcNow;
                string TIMESTAMP = utcNow.ToString("o");
                string nonceBase64 = Convert.ToBase64String(nonce);
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] passwordSha = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                    byte[] combined = Combine(nonce, Encoding.UTF8.GetBytes(TIMESTAMP), passwordSha);
                    byte[] passSha = sha1.ComputeHash(combined);
                    passSHA = Convert.ToBase64String(passSha);
                }
                return passSHA + "|" + nonceBase64 + "|" + TIMESTAMP;

            }
            catch (Exception ex)
            {
                return "Error while generate pwd " + ex.Message.ToString();
            }
        }

        static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
        public async Task<FlightPriceModelReturn> GetFlightPrice_Rest(string token, FlightPriceModel requestModel)
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

                    var error = await response.Content.ReadAsStringAsync();

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

        public AvailabilityModel ConvertXmlToModel(XDocument response)
        {
            AvailabilityModel ReturnModel = new AvailabilityModel();
            ReturnModel.data = new List<FlightOffer>();
            XDocument doc = response;
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace amadeus = "http://xml.amadeus.com/TIPNRR_23_1_1A";
            List<Itinerary> itinerariesList = new List<Itinerary>();
            string? numberOfPax;
            string? passengerid;
            string? pricingIndicators;
            string? priceTariffType;
            string? productDateTimeDetails_departureDate;
            string? productDateTimeDetails_departureTime;
            string? companyDetails;
            string? fareAmount_typeQualifier;
            string? fareAmount_amount;
            string? fareAmount_currency;
            string? otherMonetaryDetails_typeQualifier;
            string? otherMonetaryDetails_amount;
            string? otherMonetaryDetails_currency;
            List<TextData> textData = new List<TextData>();
            string uniqueOfferReference = string.Empty;
            string? textData_freeTextQualification_textSubjectQualifier;
            string? textData_freeTextQualification_informationType;
         
            var pricingGroupLevelGroup = doc.Descendants(amadeus + "pricingGroupLevelGroup").ToList();
            if (pricingGroupLevelGroup != null)
            {
               

                foreach (var item in pricingGroupLevelGroup)
                {
                    FlightOffer offer = new FlightOffer();
                    offer.itineraries = new List<Itinerary>();
                    Itinerary itinerary = new Itinerary();
                    itinerary.segments = new List<Segment>();

                    numberOfPax = item?.Descendants(amadeus + "numberOfPax")?.Descendants(amadeus + "segmentControlDetails")?.Elements(amadeus + "quantity")?.FirstOrDefault()?.Value;
                    passengerid = item?.Descendants(amadeus + "passengersID")?.Descendants(amadeus + "travellerDetails")?.Elements(amadeus + "measurementValue")?.FirstOrDefault()?.Value;
                    pricingIndicators = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "pricingIndicators")?.Descendants(amadeus + "priceTicketDetails")?.Elements(amadeus + "indicators").FirstOrDefault()?.Value;
                    priceTariffType = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "pricingIndicators")?.Elements(amadeus + "priceTariffType")?.FirstOrDefault()?.Value;
                    productDateTimeDetails_departureDate = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "pricingIndicators")?.Descendants(amadeus + "productDateTimeDetails")?.Elements(amadeus + "departureDate")?.FirstOrDefault()?.Value;
                    productDateTimeDetails_departureTime = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "pricingIndicators")?.Descendants(amadeus + "productDateTimeDetails")?.Elements(amadeus + "departureTime")?.FirstOrDefault()?.Value;
                    companyDetails = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "pricingIndicators")?.Descendants(amadeus + "companyDetails")?.Elements(amadeus + "otherCompany")?.FirstOrDefault()?.Value;
                    fareAmount_typeQualifier = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "fareAmount")?.Descendants(amadeus + "monetaryDetails")?.Elements(amadeus + "typeQualifier")?.FirstOrDefault()?.Value;
                    fareAmount_amount = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "fareAmount")?.Descendants(amadeus + "monetaryDetails")?.Elements(amadeus + "amount")?.FirstOrDefault()?.Value;
                    fareAmount_currency = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "fareAmount")?.Descendants(amadeus + "monetaryDetails")?.Elements(amadeus + "currency")?.FirstOrDefault()?.Value;
                    otherMonetaryDetails_typeQualifier = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "fareAmount")?.Descendants(amadeus + "otherMonetaryDetails")?.Elements(amadeus + "typeQualifier")?.FirstOrDefault()?.Value;
                    otherMonetaryDetails_amount = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "fareAmount")?.Descendants(amadeus + "otherMonetaryDetails")?.Elements(amadeus + "amount")?.FirstOrDefault()?.Value;
                    otherMonetaryDetails_currency = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "fareAmount")?.Descendants(amadeus + "otherMonetaryDetails")?.Elements(amadeus + "currency")?.FirstOrDefault()?.Value;
                    var textdata = item?.Descendants(amadeus + "fareInfoGroup").Descendants(amadeus + "textData").ToList();
                    List<TextData> lstTextData = new List<TextData>();
                    foreach (var titem in textdata)
                    {
                        var subjectQuilifier = titem?.Descendants(amadeus + "freeTextQualification")?.Elements(amadeus + "textSubjectQualifier")?.FirstOrDefault().Value;
                        var infotype = titem?.Descendants(amadeus + "freeTextQualification")?.Elements(amadeus + "informationType")?.FirstOrDefault().Value;
                        var freetext = titem?.Elements(amadeus + "freeText")?.FirstOrDefault()?.Value;
                        lstTextData.Add(new TextData
                        {
                            freeText = freetext,
                            freeTextQualifications = new freeTextQualification { informationType = infotype, textSubjectQualifier = subjectQuilifier }
                        });
                        var surchargesGroup = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "surchargesGroup")?.Descendants(amadeus + "taxesAmount")?.Descendants(amadeus + "taxDetails").ToList();
                        if (surchargesGroup != null)
                        {
                            List<taxDetails> lstTaxdetails = new List<taxDetails>();
                            foreach (var sgroup in surchargesGroup)
                            {
                                var rate = titem?.Elements(amadeus + "rate")?.FirstOrDefault()?.Value;
                                var countryCode = titem?.Elements(amadeus + "countryCode")?.FirstOrDefault()?.Value;
                                var type = titem?.Elements(amadeus + "type")?.FirstOrDefault()?.Value;
                                lstTaxdetails.Add(new taxDetails
                                {
                                    countryCode = countryCode,
                                    rate = rate,
                                    type = type
                                });

                            }
                        }

                    }
                    var offerReferences = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "offerReferences")?.ToList();
                    if (offerReferences != null)
                    {
                        uniqueOfferReference = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "offerReferences")?.Descendants(amadeus + "offerIdentifier")?.Elements(amadeus + "uniqueOfferReference")?.FirstOrDefault()?.Value;
                    }

                    // segmentLevelGroup start working from here
                    var segmentInformation_OutBound = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "segmentLevelGroup")?.Descendants(amadeus + "segmentInformation")?.Where(e => e.Element(amadeus + "itemNumber")?.Value == "1")?.ToList();
                    var segmentInformation_Return = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "segmentLevelGroup")?.Descendants(amadeus + "segmentInformation")?.Where(e => e.Element(amadeus + "itemNumber")?.Value == "2").ToList();
                    if (segmentInformation_OutBound != null)
                    {
                        foreach(var departure in segmentInformation_OutBound)
                        {
                            Segment outbound = new Segment();
                            outbound.departure = new Departure();
                            var dept = departure?.Descendants(amadeus + "flightDate")?.Elements(amadeus + "departureDate")?.FirstOrDefault()?.Value;
                            if (dept != null)
                            {
                                outbound.departure.at = DateTime.ParseExact(dept, "ddMMyy", CultureInfo.InvariantCulture);
                            }
                            var fromlocation = departure?.Descendants(amadeus + "boardPointDetails")?.Elements(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                            outbound.departure.iataCode = fromlocation != null ? fromlocation : "";
                            var toLocation = departure?.Descendants(amadeus + "offpointDetails")?.Elements(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                            outbound.arrival = new Arrival();
                            outbound.arrival.iataCode = toLocation;
                            var Addinfo = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "segmentLevelGroup")?.Descendants(amadeus + "additionalInformation")?.FirstOrDefault();
                            var arrivalDate = Addinfo?.Descendants(amadeus + "productDateTimeDetails")?.Descendants(amadeus + "arrivalDate")?.FirstOrDefault()?.Value;
                            if ( arrivalDate != null)
                            {
                                outbound.arrival.at = DateTime.ParseExact(arrivalDate, "ddMMyy", CultureInfo.InvariantCulture);
                            }                          
                            var marketingCompany = departure?.Descendants(amadeus + "companyDetails")?.Elements(amadeus + "marketingCompany")?.FirstOrDefault()?.Value;
                            var flightNumber = departure?.Descendants(amadeus + "flightIdentification")?.Elements(amadeus + "flightNumber")?.FirstOrDefault()?.Value;
                            var bookingClass = departure?.Descendants(amadeus + "flightIdentification")?.Elements(amadeus + "bookingClass")?.FirstOrDefault()?.Value;
                            var itemnumber = departure?.Descendants(amadeus + "itemNumber")?.FirstOrDefault()?.Value;
                            outbound.carrierCode = marketingCompany;
                            outbound.number = flightNumber;
                            outbound.aircraft = new Aircraft { code = flightNumber };
                            outbound.operating = new Operating { carrierCode = marketingCompany };
                            var numberOfStops = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "segmentLevelGroup") ?.Descendants(amadeus + "segmentInformation")?.Where(e => e.Element(amadeus + "itemNumber")?.Value == "1")?.ToList().Count() - 1;
                            outbound.numberOfStops = numberOfStops;
                            // working from here to fareBasis
                            var farebasis_rateclass = item?.Descendants(amadeus + "fareBasis")?.Descendants(amadeus + "additionalFareDetails")?.Elements(amadeus + "rateClass")?.FirstOrDefault()?.Value;
                            var farebasis_secondRateClass = item?.Descendants(amadeus + "fareBasis")?.Descendants(amadeus + "additionalFareDetails")?.Elements(amadeus + "secondRateClass")?.FirstOrDefault()?.Value;
                            var cabin_group = item?.Descendants(amadeus + "cabinGroup")?.Descendants(amadeus + "cabinSegment")?.Descendants(amadeus + "bookingClassDetails")?.Elements(amadeus + "designator")?.FirstOrDefault()?.Value;
                            var baggage_freeAllowence = item?.Descendants(amadeus + "baggageAllowance")?.Descendants(amadeus + "baggageDetails")?.Elements(amadeus + "freeAllowance")?.FirstOrDefault()?.Value;
                            var baggage_quantityCode = item?.Descendants(amadeus + "baggageAllowance")?.Descendants(amadeus + "baggageDetails")?.Elements(amadeus + "quantityCode")?.FirstOrDefault()?.Value;
                            var cabin = item?.Descendants(amadeus + "flightProductInformationType")?.Descendants(amadeus + "cabinProduct")?.Elements(amadeus + "cabin")?.FirstOrDefault()?.Value;
                            var cabin_avlStatus = item?.Descendants(amadeus + "flightProductInformationType")?.Descendants(amadeus + "cabinProduct")?.Elements(amadeus + "avlStatus")?.FirstOrDefault()?.Value;
                            outbound.baggage_allowence = new BaggageAllowance
                            {
                                free_allowance = baggage_freeAllowence != null ? baggage_freeAllowence : "",
                                 quantity_code = baggage_quantityCode != null ? baggage_quantityCode : ""
                            };
                            outbound.cabin_class = cabin;
                            outbound.cabin_status = cabin_avlStatus;
                            itinerary.segments.Add(outbound);
                            itinerary.segment_type = "OutBound";
                          
                        }
                       

                    }
                    if (segmentInformation_Return != null)
                    {
                        foreach (var arrival in segmentInformation_Return)
                        {
                            Segment inbound = new Segment();
                            inbound.departure = new Departure();
                            var dept = arrival?.Descendants(amadeus + "flightDate")?.Elements(amadeus + "departureDate")?.FirstOrDefault()?.Value;
                            if (dept != null)
                            {
                                inbound.departure.at = DateTime.ParseExact(dept, "ddMMyy", CultureInfo.InvariantCulture);
                            }
                            var fromlocation = arrival?.Descendants(amadeus + "boardPointDetails")?.Elements(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                            inbound.departure.iataCode = fromlocation != null ? fromlocation : "";
                            var toLocation = arrival?.Descendants(amadeus + "offpointDetails")?.Elements(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                            inbound.arrival = new Arrival();
                            inbound.arrival.iataCode = toLocation;
                            var Addinfo = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "segmentLevelGroup")?.Descendants(amadeus + "additionalInformation")?.FirstOrDefault();
                            var arrivalDate = Addinfo?.Descendants(amadeus + "productDateTimeDetails")?.Descendants(amadeus + "arrivalDate")?.FirstOrDefault()?.Value;
                            if (arrivalDate != null)
                            {
                                inbound.arrival.at = DateTime.ParseExact(arrivalDate, "ddMMyy", CultureInfo.InvariantCulture);
                            }
                            var marketingCompany = arrival?.Descendants(amadeus + "companyDetails")?.Elements(amadeus + "marketingCompany")?.FirstOrDefault()?.Value;
                            var flightNumber = arrival?.Descendants(amadeus + "flightIdentification")?.Elements(amadeus + "flightNumber")?.FirstOrDefault()?.Value;
                            var bookingClass = arrival?.Descendants(amadeus + "flightIdentification")?.Elements(amadeus + "bookingClass")?.FirstOrDefault()?.Value;
                            var itemnumber = arrival?.Descendants(amadeus + "itemNumber")?.FirstOrDefault()?.Value;
                            inbound.carrierCode = marketingCompany;
                            inbound.number = flightNumber;
                            inbound.aircraft = new Aircraft { code = flightNumber };
                            inbound.operating = new Operating { carrierCode = marketingCompany };
                            var numberOfStops = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "segmentLevelGroup") ?.Descendants(amadeus + "segmentInformation")?.Where(e => e.Element(amadeus + "itemNumber")?.Value == "2")?.ToList().Count() - 1;
                            inbound.numberOfStops = numberOfStops;
                            // working from here to fareBasis
                            var farebasis_rateclass = item?.Descendants(amadeus + "fareBasis")?.Descendants(amadeus + "additionalFareDetails")?.Elements(amadeus + "rateClass")?.FirstOrDefault()?.Value;
                            var farebasis_secondRateClass = item?.Descendants(amadeus + "fareBasis")?.Descendants(amadeus + "additionalFareDetails")?.Elements(amadeus + "secondRateClass")?.FirstOrDefault()?.Value;
                            var cabin_group = item?.Descendants(amadeus + "cabinGroup")?.Descendants(amadeus + "cabinSegment")?.Descendants(amadeus + "bookingClassDetails")?.Elements(amadeus + "designator")?.FirstOrDefault()?.Value;
                            var baggage_freeAllowence = item?.Descendants(amadeus + "baggageAllowance")?.Descendants(amadeus + "baggageDetails")?.Elements(amadeus + "freeAllowance")?.FirstOrDefault()?.Value;
                            var baggage_quantityCode = item?.Descendants(amadeus + "baggageAllowance")?.Descendants(amadeus + "baggageDetails")?.Elements(amadeus + "quantityCode")?.FirstOrDefault()?.Value;
                            var cabin = item?.Descendants(amadeus + "flightProductInformationType")?.Descendants(amadeus + "cabinProduct")?.Elements(amadeus + "cabin")?.FirstOrDefault()?.Value;
                            var cabin_avlStatus = arrival?.Descendants(amadeus + "flightProductInformationType")?.Descendants(amadeus + "cabinProduct")?.Elements(amadeus + "avlStatus")?.FirstOrDefault()?.Value;
                            inbound.baggage_allowence = new BaggageAllowance
                            {
                                free_allowance = baggage_freeAllowence != null ? baggage_freeAllowence : "",
                                quantity_code = baggage_quantityCode != null ? baggage_quantityCode : ""
                            };
                            inbound.cabin_class = cabin;
                            inbound.cabin_status = cabin_avlStatus;
                            itinerary.segments.Add(inbound);
                            itinerary.segment_type = "InBound";
                        }
                    }
                    var price = new Price();
                    price.base_amount = fareAmount_amount;
                    price.total = fareAmount_amount;
                    price.currency = fareAmount_currency;
                    price.taxes = new List<Taxes>();
                    offer.itineraries.Add(itinerary);
                    offer.price = price;

                    // fareComponentDetailsGroup
                    //var fareComponentDetailsGroup_Outbound = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "fareComponentDetailsGroup")?.Descendants(amadeus + "fareComponentID")?.Descendants(amadeus + "itemNumberDetails")?.Where(e => e.Element(amadeus + "itemNumber")?.Value == "1")?.FirstOrDefault();
                    //if (fareComponentDetailsGroup_Outbound != null)
                    //{
                    //    var from = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "marketFareComponent")?.Descendants(amadeus + "boardPointDetails")?.Elements(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                    //    var to = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "marketFareComponent")?.Descendants(amadeus + "offpointDetails")?.Elements(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                    //    var pricetype = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "monetaryInformation")?.Descendants(amadeus + "monetaryDetails")?.Elements(amadeus + "typeQualifier")?.FirstOrDefault()?.Value;
                    //    var priceAmount = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "monetaryInformation")?.Descendants(amadeus + "monetaryDetails")?.Elements(amadeus + "amount")?.FirstOrDefault()?.Value;
                    //    var priceCurrency = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "monetaryInformation")?.Descendants(amadeus + "monetaryDetails")?.Elements(amadeus + "currency")?.FirstOrDefault()?.Value;
                    //    var rateTariffClass = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "componentClassInfo")?.Descendants(amadeus + "fareBasisDetails")?.Elements(amadeus + "rateTariffClass")?.FirstOrDefault()?.Value;
                    //    var discountDetails = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "fareQualifiersDetail")?.Descendants(amadeus + "discountDetails")?.Elements(amadeus + "fareQualifier")?.FirstOrDefault()?.Value;
                    //    var fareFamilyname = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "fareFamilyDetails")?.Elements(amadeus + "fareFamilyname")?.FirstOrDefault()?.Value;
                    //    var otherCompany = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "fareFamilyOwner")?.Descendants(amadeus + "companyIdentification")?.Elements(amadeus + "otherCompany")?.FirstOrDefault()?.Value;


                    //}
                    //var fareComponentDetailsGroup_Inbound = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "fareComponentDetailsGroup")?.Descendants(amadeus + "fareComponentID")?.Descendants(amadeus + "itemNumberDetails")?.Where(e => e.Element(amadeus + "itemNumber")?.Value == "2")?.FirstOrDefault();
                    //if (fareComponentDetailsGroup_Inbound != null)
                    //{
                    //    var from = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "marketFareComponent")?.Descendants(amadeus + "boardPointDetails")?.Elements(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                    //    var to = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "marketFareComponent")?.Descendants(amadeus + "offpointDetails")?.Elements(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                    //    var pricetype = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "monetaryInformation")?.Descendants(amadeus + "monetaryDetails")?.Elements(amadeus + "typeQualifier")?.FirstOrDefault()?.Value;
                    //    var priceAmount = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "monetaryInformation")?.Descendants(amadeus + "monetaryDetails")?.Elements(amadeus + "amount")?.FirstOrDefault()?.Value;
                    //    var priceCurrency = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "monetaryInformation")?.Descendants(amadeus + "monetaryDetails")?.Elements(amadeus + "currency")?.FirstOrDefault()?.Value;
                    //    var rateTariffClass = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "componentClassInfo")?.Descendants(amadeus + "fareBasisDetails")?.Elements(amadeus + "rateTariffClass")?.FirstOrDefault()?.Value;
                    //    var discountDetails = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "fareQualifiersDetail")?.Descendants(amadeus + "discountDetails")?.Elements(amadeus + "fareQualifier")?.FirstOrDefault()?.Value;
                    //    var fareFamilyname = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "fareFamilyDetails")?.Elements(amadeus + "fareFamilyname")?.FirstOrDefault()?.Value;
                    //    var otherCompany = fareComponentDetailsGroup_Outbound?.Descendants(amadeus + "fareFamilyOwner")?.Descendants(amadeus + "companyIdentification")?.Elements(amadeus + "otherCompany")?.FirstOrDefault()?.Value;

                    //}

                    ReturnModel.data.Add(offer);
                }


               




            }
            return ReturnModel;
        }
    }
}
