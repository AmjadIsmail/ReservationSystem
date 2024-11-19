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
using ReservationSystem.Domain.Service;
using System.Data;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class FlightPriceRepository : IFlightPriceRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        private readonly IHelperRepository _helperRepository;
        private readonly ICacheService _cacheService;
        public FlightPriceRepository(IConfiguration _configuration, IMemoryCache cache,IHelperRepository helperRepository , ICacheService cacheService)
        {
            configuration = _configuration;
            _cache = cache;
            _helperRepository = helperRepository;
            _cacheService = cacheService;
        }

        public async Task<FlightPriceReturnModel> GetFlightPrice(FlightPriceMoelSoap requestModel)
        {
            FlightPriceReturnModel flightPrice = new FlightPriceReturnModel();
            try
            {

               // var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var _url = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:ApiUrl"]); 
                var _action = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:fareInformativePricingWithoutPNRAction"]);
                string Result = string.Empty;
                string Envelope = await CreateFlightPriceRequest(requestModel);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url);
                request.Headers.Add("SOAPAction", _action);
                request.ContentType = "text/xml;charset=\"utf-8\"";
                request.Accept = "text/xml";
                request.Method = "POST";
                XNamespace fareNS = "http://xml.amadeus.com/TIPNRR_23_1_1A"; // price
                XDocument xmlDoc = new XDocument();
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
                            xmlDoc = XDocument.Parse(result2);
                            await _helperRepository.SaveXmlResponse("Fare_InformativePricingWithoutPNR_Request", Envelope);
                            await _helperRepository.SaveXmlResponse("Fare_InformativePricingWithoutPNR_Response", result2);
                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(result2);
                            string jsonText = JsonConvert.SerializeXmlNode(xmlDoc2, Newtonsoft.Json.Formatting.Indented);
                            await _helperRepository.SaveJson(jsonText, "Fare_InformativePricingWithoutPNRJson");
                            var errorInfo = xmlDoc.Descendants(fareNS + "errorMessage").FirstOrDefault();
                            if (errorInfo != null)
                            {
                                var errorText = xmlDoc.Descendants(fareNS + "errorMessage").Descendants(fareNS + "errorMessageText").Descendants(fareNS + "description")?.FirstOrDefault()?.Value;
                                var errorCode = xmlDoc.Descendants(fareNS + "errorMessage").Descendants(fareNS + "applicationError").Descendants(fareNS + "applicationErrorDetail").Descendants(fareNS + "error")?.FirstOrDefault()?.Value;
                                flightPrice.amadeusError = new AmadeusResponseError();
                                flightPrice.amadeusError.error = errorText;
                                flightPrice.amadeusError.errorCode = Convert.ToInt16(errorCode);
                                return flightPrice;

                            }
                            
                            var res = ConvertXmlToModel(xmlDoc, fareNS);
                            flightPrice = res;
                           

                        }
                    }
                }
                catch (WebException ex)
                {
                    using (StreamReader rd = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        Result = rd.ReadToEnd();
                        var errorText = xmlDoc.Descendants(fareNS + "errorMessage").Descendants(fareNS + "errorMessageText").Descendants(fareNS + "description")?.FirstOrDefault()?.Value;
                        var errorCode = xmlDoc.Descendants(fareNS + "errorMessage").Descendants(fareNS + "applicationError").Descendants(fareNS + "applicationErrorDetail").Descendants(fareNS + "error")?.FirstOrDefault()?.Value;

                        flightPrice.amadeusError = new AmadeusResponseError();
                        flightPrice.amadeusError.error = errorText;
                        flightPrice.amadeusError.errorCode = errorCode != null ? Convert.ToInt16(errorCode) : 0;
                        return flightPrice;

                    }
                }
            }
            catch (Exception ex)
            {
                flightPrice.amadeusError = new AmadeusResponseError();
                flightPrice.amadeusError.error = ex.Message.ToString(); ;
                flightPrice.amadeusError.errorCode = 500;
                return flightPrice;
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
                        <measurementValue>{adt + i}</measurementValue>
                        </travellerDetails>";
                    }
                    childPassenger += " </travellersID>";

                    passengerGroupChild = $@" <passengersGroup>
                        <segmentRepetitionControl>
                        <segmentControlDetails>
                        <quantity>2</quantity>
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
                    InfPassenger = "<travellersID>";
                    for (int i = 1; i <= inf; i++)
                    {
                        InfPassenger += $@" <travellerDetails>
                  <measurementValue>{i}</measurementValue>
               </travellerDetails>";
                    }
                    InfPassenger += " </travellersID>";

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
                int ob = 1;
                foreach(var modelo in model.outbound)
                {
                    group = group + $@"<segmentGroup>
                    <segmentInformation>
                       <flightDate>
                          <departureDate>{modelo.departure_date.Replace("-", "")}</departureDate>
                          <departureTime>{modelo.departure_time.Replace(":", "")}</departureTime>
                       </flightDate>
                       <boardPointDetails>
                          <trueLocationId>{modelo.airport_from.Replace("-", "")}</trueLocationId>
                       </boardPointDetails>
                       <offpointDetails>
                          <trueLocationId>{modelo.airport_to.Replace("-", "")}</trueLocationId>
                       </offpointDetails>
                       <companyDetails>
                          <marketingCompany>{modelo.marketing_company}</marketingCompany>
                       </companyDetails>
                       <flightIdentification>
                          <flightNumber>{modelo.flight_number}</flightNumber>
                          <bookingClass>{modelo.booking_class}</bookingClass>
                       </flightIdentification>
                       <flightTypeDetails>
                          <flightIndicator>1</flightIndicator>
                       </flightTypeDetails>
                       <itemNumber>{ob}</itemNumber>
                             </segmentInformation>
                            </segmentGroup>";
                    ob++;
                }
                int ib = model.outbound.Count() +1;
                foreach (var modeli in model.inbound)
                {
                    group = group + $@"<segmentGroup>
                    <segmentInformation>
                       <flightDate>
                          <departureDate>{modeli.departure_date.Replace("-", "")}</departureDate>
                          <departureTime>{modeli.departure_time.Replace(":", "")}</departureTime>
                       </flightDate>
                       <boardPointDetails>
                          <trueLocationId>{modeli.airport_from.Replace("-", "")}</trueLocationId>
                       </boardPointDetails>
                       <offpointDetails>
                          <trueLocationId>{modeli.airport_to.Replace("-", "")}</trueLocationId>
                       </offpointDetails>
                       <companyDetails>
                          <marketingCompany>{modeli.marketing_company}</marketingCompany>
                       </companyDetails>
                       <flightIdentification>
                          <flightNumber>{modeli.flight_number}</flightNumber>
                          <bookingClass>{modeli.booking_class}</bookingClass>
                       </flightIdentification>
                       <flightTypeDetails>
                          <flightIndicator>2</flightIndicator>
                       </flightTypeDetails>
                       <itemNumber>{ib}</itemNumber>
                             </segmentInformation>
                            </segmentGroup>";
                    ib++;
                }




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
            string pwdDigest = await _helperRepository.generatePassword();
           // var amadeusSettings = configuration.GetSection("AmadeusSoap") != null ? configuration.GetSection("AmadeusSoap") : null;
            string action = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:fareInformativePricingWithoutPNRAction"]);
            string to = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:ApiUrl"]);
            string username = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:webUserId"]);
            string dutyCode = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:dutyCode"]);
            string requesterType = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:requestorType"]);
            string PseudoCityCode = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:PseudoCityCode"]?.ToString());
            string pos_type = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:POS_Type"]);
            requestModel.child = requestModel?.child != null ? requestModel.child : 0;
            requestModel.infant = requestModel?.infant != null ? requestModel.infant : 0;

            string Request = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:sec=""http://xml.amadeus.com/2010/06/Security_v1"" xmlns:typ=""http://xml.amadeus.com/2010/06/Types_v1"" xmlns:iat=""http://www.iata.org/IATA/2007/00/IATA2010.1"" xmlns:app=""http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3"" xmlns:link=""http://wsdl.amadeus.com/2010/06/ws/Link_v1"" xmlns:ses=""http://xml.amadeus.com/2010/06/Session_v3"">
        <soapenv:Header xmlns:add=""http://www.w3.org/2005/08/addressing"">
        <ses:Session TransactionStatusCode=""Start""/>
        <add:MessageID>{System.Guid.NewGuid()}</add:MessageID>
        <add:Action>{action}</add:Action>
      <add:To>{to}</add:To>
      <link:TransactionFlowLink xmlns:link=""http://wsdl.amadeus.com/2010/06/ws/Link_v1""/>
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
       {GeneratePassengerGroup(requestModel.adults.Value, requestModel.child.Value, requestModel.infant.Value)}
        {GenerateSegmentGroup(requestModel)}
         {GeneratePricingOptionsGroup(requestModel.pricingOptionKey)}
      </Fare_InformativePricingWithoutPNR>
   </soapenv:Body>

</soapenv:Envelope>";

            return Request;
        }

        public async Task<string> CreateFlightPriceRequestWithBestPrice(FlightPriceMoelSoap requestModel , string _action)
        {
            string pwdDigest = await _helperRepository.generatePassword();
            //var amadeusSettings = configuration.GetSection("AmadeusSoap") != null ? configuration.GetSection("AmadeusSoap") : null;
            string action = _action;
            string to = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:ApiUrl"]);
            string username = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:webUserId"]);
            string dutyCode = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:dutyCode"]);
            string requesterType = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:requestorType"]);
            string PseudoCityCode = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:PseudoCityCode"]?.ToString());
            string pos_type = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:POS_Type"]);
            requestModel.child = requestModel?.child != null ? requestModel.child : 0;
            requestModel.infant = requestModel?.infant != null ? requestModel.infant : 0;

            string Request = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:sec=""http://xml.amadeus.com/2010/06/Security_v1"" xmlns:typ=""http://xml.amadeus.com/2010/06/Types_v1"" xmlns:iat=""http://www.iata.org/IATA/2007/00/IATA2010.1"" xmlns:app=""http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3"" xmlns:link=""http://wsdl.amadeus.com/2010/06/ws/Link_v1"" xmlns:ses=""http://xml.amadeus.com/2010/06/Session_v3"">
        <soapenv:Header xmlns:add=""http://www.w3.org/2005/08/addressing"">
        <ses:Session TransactionStatusCode=""Start""/>
        <add:MessageID>{System.Guid.NewGuid()}</add:MessageID>
        <add:Action>{action}</add:Action>
      <add:To>{to}</add:To>
      <link:TransactionFlowLink xmlns:link=""http://wsdl.amadeus.com/2010/06/ws/Link_v1""/>
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
      <Fare_InformativeBestPricingWithoutPNR>
       {GeneratePassengerGroup(requestModel.adults.Value, requestModel.child.Value, requestModel.infant.Value)}
        {GenerateSegmentGroup(requestModel)}
         {GeneratePricingOptionsGroup(requestModel.pricingOptionKey)}
      </Fare_InformativeBestPricingWithoutPNR>
   </soapenv:Body>

</soapenv:Envelope>";

            return Request;
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

        public FlightPriceReturnModel ConvertXmlToModel(XDocument response, XNamespace ns)
        {
            FlightPriceReturnModel ReturnModel = new FlightPriceReturnModel();
            ReturnModel.flightPrice = new List<FlightOfferForFlightPrice>();
            XDocument doc = response;
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace amadeus = ns;
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
            decimal totalAmount = 0;
            decimal totalBase = 0;
            string? otherMonetaryDetails_currency;
            List<TextData> textData = new List<TextData>();
            string uniqueOfferReference = string.Empty;
            string? textData_freeTextQualification_textSubjectQualifier;
            string? textData_freeTextQualification_informationType;

            XNamespace awsse = "http://xml.amadeus.com/2010/06/Session_v3";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            var sessionElement = doc.Descendants(awsse + "Session").FirstOrDefault();
            if (sessionElement != null)
            {
                string sessionId = sessionElement.Element(awsse + "SessionId")?.Value;
                string sequenceNumber = sessionElement.Element(awsse + "SequenceNumber")?.Value;
                string securityToken = sessionElement.Element(awsse + "SecurityToken")?.Value;
                string TransactionStatusCode = sessionElement.Attribute("TransactionStatusCode")?.Value;
                int SeqNumber = 0;
                if (sequenceNumber != null) { SeqNumber = Convert.ToInt32(sequenceNumber); }
                ReturnModel.Session = new HeaderSession
                {
                    SecurityToken = securityToken,
                    SequenceNumber = SeqNumber,
                    SessionId = sessionId,
                    TransactionStatusCode = TransactionStatusCode
                };
            }
            var messegeFunction = doc.Descendants(amadeus + "messageDetails").FirstOrDefault();
            string strMsgFunction = string.Empty;
            if(messegeFunction != null)
            {
                var messegDetail = messegeFunction?.Descendants(amadeus + "messageFunctionDetails")?.Descendants(amadeus + "messageFunction")?.FirstOrDefault().Value;
                strMsgFunction = messegDetail;
            }
            var pricingGroupLevelGroup = doc.Descendants(amadeus + "pricingGroupLevelGroup").ToList();
            if (pricingGroupLevelGroup != null)
            {

                var AirlineCache = _cacheService.GetAirlines();
                var AirportCache = _cacheService.GetAirports();
                foreach (var item in pricingGroupLevelGroup)
                {
                    FlightOfferForFlightPrice offer = new FlightOfferForFlightPrice();
                    offer.messageFunction = strMsgFunction;
                    offer.itineraries = new List<Itinerary>();
                    Itinerary itinerary = new Itinerary();
                    itinerary.segments = new List<Segment>();
                    List<taxDetails> lstTaxdetails = new List<taxDetails>();
                    numberOfPax = item?.Descendants(amadeus + "numberOfPax")?.Descendants(amadeus + "segmentControlDetails")?.Elements(amadeus + "numberOfUnits")?.FirstOrDefault()?.Value;
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
                            lstTaxdetails = new List<taxDetails>();
                            foreach (var sgroup in surchargesGroup)
                            {
                               
                                var taxDetails = sgroup.Descendants(amadeus + "taxDetails");
                                var rate = sgroup.Element(amadeus + "rate")?.Value;
                                var countryCode = sgroup.Element(amadeus + "countryCode")?.Value;
                                var type = sgroup.Element(amadeus + "type")?.Value;
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
                    var segmentInfo = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "segmentLevelGroup")?.Descendants(amadeus + "segmentInformation")?.ToList();
                    int midSegment = segmentInfo.Count() / 2;
                    var segmentInformation = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "segmentLevelGroup")?.Descendants(amadeus + "segmentInformation")?.ToList().Take(midSegment);
                    var segmentInformation_Return = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "segmentLevelGroup")?.Descendants(amadeus + "segmentInformation")?.ToList().Skip(midSegment);
                    if (segmentInformation != null)
                    {
                       
                        foreach (var seg in segmentInformation)
                        {
                           
                            Segment journy = new Segment();
                            journy.departure = new Departure();
                            var dept = seg?.Descendants(amadeus + "flightDate")?.Elements(amadeus + "departureDate")?.FirstOrDefault()?.Value;
                            if (dept != null)
                            {
                                journy.departure.at = DateTime.ParseExact(dept, "ddMMyy", CultureInfo.InvariantCulture);

                            }
                            var itemNumber = seg?.Descendants(amadeus + "itemNumber")?.FirstOrDefault()?.Value;
                            var fromlocation = seg?.Descendants(amadeus + "boardPointDetails")?.Elements(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                            journy.departure.iataCode = fromlocation != null ? fromlocation : "";
                            DataRow depatureAirport = AirportCache != null ?  AirportCache.AsEnumerable().FirstOrDefault(r => r.Field<string>("AirportCode") == fromlocation) : null;
                            var depAirportName = depatureAirport != null ? depatureAirport[2].ToString() + " , " + depatureAirport[4].ToString() : "";
                            journy.departure.iataName = depAirportName;
                            var toLocation = seg?.Descendants(amadeus + "offpointDetails")?.Elements(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                            journy.arrival = new Arrival();
                            journy.arrival.iataCode = toLocation;
                            DataRow arrivalAirport = AirportCache != null ?  AirportCache.AsEnumerable().FirstOrDefault(r => r.Field<string>("AirportCode") == toLocation) : null;
                            var arrAirportName = arrivalAirport != null ? arrivalAirport[2].ToString() + " , " + arrivalAirport[4].ToString() : "";
                            journy.arrival.iataName = arrAirportName;
                            var Addinfo = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "segmentLevelGroup")?.Descendants(amadeus + "additionalInformation")?.FirstOrDefault();
                            var arrivalDate = Addinfo?.Descendants(amadeus + "productDateTimeDetails")?.Descendants(amadeus + "arrivalDate")?.FirstOrDefault()?.Value;
                            if (arrivalDate != null)
                            {
                                journy.arrival.at = DateTime.ParseExact(arrivalDate, "ddMMyy", CultureInfo.InvariantCulture);
                            }
                            var marketingCompany = seg?.Descendants(amadeus + "companyDetails")?.Elements(amadeus + "marketingCompany")?.FirstOrDefault()?.Value;
                            var flightNumber = seg?.Descendants(amadeus + "flightIdentification")?.Elements(amadeus + "flightNumber")?.FirstOrDefault()?.Value;
                            var bookingClass = seg?.Descendants(amadeus + "flightIdentification")?.Elements(amadeus + "bookingClass")?.FirstOrDefault()?.Value;
                            var itemnumber = seg?.Descendants(amadeus + "itemNumber")?.FirstOrDefault()?.Value;
                            journy.marketingCarrierCode = marketingCompany;
                            DataRow carrier = AirlineCache != null ? AirlineCache.AsEnumerable().FirstOrDefault(r => r.Field<string>("AirlineCode") == marketingCompany) : null;
                            var marketingcarriername = carrier != null ? carrier[1].ToString() : "";
                            journy.marketingCarrierName = marketingcarriername;
                            journy.number = flightNumber;
                            journy.aircraft = new Aircraft { code = flightNumber };
                            var numberOfStops = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "segmentLevelGroup")?.Descendants(amadeus + "segmentInformation")?.Where(e => e.Element(amadeus + "itemNumber")?.Value == "1")?.ToList().Count() - 1;
                            journy.numberOfStops = numberOfStops;                           
                            var farebasis_rateclass = item?.Descendants(amadeus + "fareBasis")?.Descendants(amadeus + "additionalFareDetails")?.Elements(amadeus + "rateClass")?.FirstOrDefault()?.Value;
                            var farebasis_secondRateClass = item?.Descendants(amadeus + "fareBasis")?.Descendants(amadeus + "additionalFareDetails")?.Elements(amadeus + "secondRateClass")?.FirstOrDefault()?.Value;
                            var cabin_group = item?.Descendants(amadeus + "cabinGroup")?.Descendants(amadeus + "cabinSegment")?.Descendants(amadeus + "bookingClassDetails")?.Elements(amadeus + "designator")?.FirstOrDefault()?.Value;
                            var baggage_freeAllowence = item?.Descendants(amadeus + "baggageAllowance")?.Descendants(amadeus + "baggageDetails")?.Elements(amadeus + "freeAllowance")?.FirstOrDefault()?.Value;
                            var baggage_quantityCode = item?.Descendants(amadeus + "baggageAllowance")?.Descendants(amadeus + "baggageDetails")?.Elements(amadeus + "quantityCode")?.FirstOrDefault()?.Value;
                            var cabin = item?.Descendants(amadeus + "flightProductInformationType")?.Descendants(amadeus + "cabinProduct")?.Elements(amadeus + "cabin")?.FirstOrDefault()?.Value;
                            var cabin_avlStatus = item?.Descendants(amadeus + "flightProductInformationType")?.Descendants(amadeus + "cabinProduct")?.Elements(amadeus + "avlStatus")?.FirstOrDefault()?.Value;
                            var bookingClassRbd = item?.Descendants(amadeus + "flightProductInformationType")?.Descendants(amadeus + "cabinProduct")?.Elements(amadeus + "rbd")?.FirstOrDefault()?.Value;
                            journy.baggageAllowence = new BaggageAllowance
                            {
                                free_allowance = baggage_freeAllowence != null ? baggage_freeAllowence : "",
                                quantity_code = baggage_quantityCode != null ? baggage_quantityCode : ""
                            };
                            journy.cabinClass = cabin;
                            journy.cabinStatus = cabin_avlStatus;
                            journy.rateClass = farebasis_rateclass;
                            journy.bookingClass = bookingClassRbd;
                            journy.fareBasis = farebasis_rateclass;
                            journy.avlStatus = cabin_avlStatus;
                            journy.id = Convert.ToInt16( itemnumber);
                            itinerary.segments.Add(journy);
                            itinerary.segment_type = "OutBound";                           
                                                     
                         }
                    }
                    offer.itineraries.Add(itinerary);
                    itinerary = new Itinerary();
                    itinerary.segments = new List<Segment>();
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
                            DataRow depatureAirport = AirportCache != null ? AirportCache.AsEnumerable().FirstOrDefault(r => r.Field<string>("AirportCode") == fromlocation) : null;
                            var depAirportName = depatureAirport != null ? depatureAirport[2].ToString() + " , " + depatureAirport[4].ToString() : "";
                            inbound.departure.iataName = depAirportName;

                            var toLocation = arrival?.Descendants(amadeus + "offpointDetails")?.Elements(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                            inbound.arrival = new Arrival();
                            inbound.arrival.iataCode = toLocation;
                            DataRow arrivalAirport = AirportCache != null ? AirportCache.AsEnumerable().FirstOrDefault(r => r.Field<string>("AirportCode") == toLocation) : null;
                            var arrAirportName = arrivalAirport != null ? arrivalAirport[2].ToString() + " , " + arrivalAirport[4].ToString() : "";
                            inbound.arrival.iataName = arrAirportName;

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
                            inbound.marketingCarrierCode = marketingCompany;
                            DataRow carrier = AirlineCache != null ? AirlineCache.AsEnumerable().FirstOrDefault(r => r.Field<string>("AirlineCode") == marketingCompany) : null;
                            var marketingcarriername = carrier != null ? carrier[1].ToString() : "";
                            inbound.marketingCarrierName = marketingcarriername;
                            inbound.number = flightNumber;
                            inbound.aircraft = new Aircraft { code = flightNumber };
                           // inbound.operating = new Operating { carrierCode = marketingCompany };
                            var numberOfStops = item?.Descendants(amadeus + "fareInfoGroup")?.Descendants(amadeus + "segmentLevelGroup")?.Descendants(amadeus + "segmentInformation")?.Where(e => e.Element(amadeus + "itemNumber")?.Value == "2")?.ToList().Count() - 1;
                            inbound.numberOfStops = numberOfStops;
                            // working from here to fareBasis
                            var farebasis_rateclass = item?.Descendants(amadeus + "fareBasis")?.Descendants(amadeus + "additionalFareDetails")?.Elements(amadeus + "rateClass")?.FirstOrDefault()?.Value;
                            var farebasis_secondRateClass = item?.Descendants(amadeus + "fareBasis")?.Descendants(amadeus + "additionalFareDetails")?.Elements(amadeus + "secondRateClass")?.FirstOrDefault()?.Value;
                            var cabin_group = item?.Descendants(amadeus + "cabinGroup")?.Descendants(amadeus + "cabinSegment")?.Descendants(amadeus + "bookingClassDetails")?.Elements(amadeus + "designator")?.FirstOrDefault()?.Value;
                            var baggage_freeAllowence = item?.Descendants(amadeus + "baggageAllowance")?.Descendants(amadeus + "baggageDetails")?.Elements(amadeus + "freeAllowance")?.FirstOrDefault()?.Value;
                            var baggage_quantityCode = item?.Descendants(amadeus + "baggageAllowance")?.Descendants(amadeus + "baggageDetails")?.Elements(amadeus + "quantityCode")?.FirstOrDefault()?.Value;
                            var cabin = item?.Descendants(amadeus + "flightProductInformationType")?.Descendants(amadeus + "cabinProduct")?.Elements(amadeus + "cabin")?.FirstOrDefault()?.Value;
                            var cabin_avlStatus = arrival?.Descendants(amadeus + "flightProductInformationType")?.Descendants(amadeus + "cabinProduct")?.Elements(amadeus + "avlStatus")?.FirstOrDefault()?.Value;
                            var bookingClassRbd = item?.Descendants(amadeus + "flightProductInformationType")?.Descendants(amadeus + "cabinProduct")?.Elements(amadeus + "rbd")?.FirstOrDefault()?.Value;
                            inbound.baggageAllowence = new BaggageAllowance
                            {
                                free_allowance = baggage_freeAllowence != null ? baggage_freeAllowence : "",
                                quantity_code = baggage_quantityCode != null ? baggage_quantityCode : ""
                            };
                            inbound.cabinClass = cabin;
                            inbound.cabinStatus = cabin_avlStatus;
                            inbound.rateClass = farebasis_rateclass;
                            inbound.bookingClass = bookingClassRbd;
                            inbound.fareBasis = farebasis_rateclass;
                            inbound.avlStatus = cabin_avlStatus;
                            itinerary.segments.Add(inbound);
                            itinerary.segment_type = "InBound";
                        }
                    }
                    offer.itineraries.Add(itinerary);
                    var price = new Price();
                    price.baseAmount = "";
                    price.total = otherMonetaryDetails_amount;
                    price.grandTotal = otherMonetaryDetails_amount;
                    price.billingCurrency = fareAmount_currency;
                    price.currency = fareAmount_currency;
                    var taxes = lstTaxdetails;
                    price.taxDetails = new List<taxDetails>();
                    price.taxDetails = lstTaxdetails;                    
                    offer.price = price;

                    ReturnModel.flightPrice.Add(offer);
                }

            }
            return ReturnModel;
        }

        public async Task<FlightPriceReturnModel> GetFlightPriceWithBestPrice(FlightPriceMoelSoap requestModel)
        {
            FlightPriceReturnModel flightPrice = new FlightPriceReturnModel();
            try
            {

              //  var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var _url = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:ApiUrl"]);
                var _action = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:fareInformativeBestPricingWithoutPNRAction"]);
                string Result = string.Empty;
                string Envelope = await CreateFlightPriceRequestWithBestPrice(requestModel,_action);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url);
                request.Headers.Add("SOAPAction", _action);
                request.ContentType = "text/xml;charset=\"utf-8\"";
                request.Accept = "text/xml";
                request.Method = "POST";
                XNamespace fareNS = "http://xml.amadeus.com/TIBNRR_23_1_1A"; // price
                XDocument xmlDoc = new XDocument();
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
                            xmlDoc = XDocument.Parse(result2);
                            await _helperRepository.SaveXmlResponse("Fare_InformativeBestPricingWithoutPNR_Request", Envelope);
                            await _helperRepository.SaveXmlResponse("Fare_InformativeBestPricingWithoutPNR_Response", result2);
                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(result2);
                            string jsonText = JsonConvert.SerializeXmlNode(xmlDoc2, Newtonsoft.Json.Formatting.Indented);
                            await _helperRepository.SaveJson(jsonText, "Fare_InformativeBestPricingWithoutPNRJson");
                            var errorInfo = xmlDoc.Descendants(fareNS + "errorMessage").FirstOrDefault();
                            if (errorInfo != null)
                            {
                                var errorText = xmlDoc.Descendants(fareNS + "errorMessage").Descendants(fareNS + "errorMessageText").Descendants(fareNS + "description")?.FirstOrDefault()?.Value;
                                var errorCode = xmlDoc.Descendants(fareNS + "errorMessage").Descendants(fareNS + "applicationError").Descendants(fareNS + "applicationErrorDetail").Descendants(fareNS + "error")?.FirstOrDefault()?.Value;
                                flightPrice.amadeusError = new AmadeusResponseError();
                                flightPrice.amadeusError.error = errorText;
                                flightPrice.amadeusError.errorCode = Convert.ToInt16(errorCode);
                                return flightPrice;

                            }

                            var res = ConvertXmlToModel(xmlDoc, fareNS);
                            flightPrice = res;


                        }
                    }
                }
                catch (WebException ex)
                {
                    using (StreamReader rd = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        Result = rd.ReadToEnd();
                        var errorText = xmlDoc.Descendants(fareNS + "errorMessage").Descendants(fareNS + "errorMessageText").Descendants(fareNS + "description")?.FirstOrDefault()?.Value;
                        var errorCode = xmlDoc.Descendants(fareNS + "errorMessage").Descendants(fareNS + "applicationError").Descendants(fareNS + "applicationErrorDetail").Descendants(fareNS + "error")?.FirstOrDefault()?.Value;

                        flightPrice.amadeusError = new AmadeusResponseError();
                        flightPrice.amadeusError.error = errorText;
                        flightPrice.amadeusError.errorCode = errorCode != null ? Convert.ToInt16(errorCode) : 0;
                        return flightPrice;

                    }
                }
            }
            catch (Exception ex)
            {
                flightPrice.amadeusError = new AmadeusResponseError();
                flightPrice.amadeusError.error = ex.Message.ToString(); ;
                flightPrice.amadeusError.errorCode = 500;
                return flightPrice;
            }
            return flightPrice;
        }
    }
}