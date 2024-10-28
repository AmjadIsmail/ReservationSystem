using ReservationSystem.Domain.Models.Soap.FlightPrice;
using ReservationSystem.Domain.Models;
using ReservationSystem.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReservationSystem.Domain.Models.AirSellFromRecommendation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Xml.Linq;
using System.Xml;
using System.Security.Cryptography;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models.FlightPrice;
using System.Globalization;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class AirsellRepository : IAirSellRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        private readonly IHelperRepository _helperRepository;
        public AirsellRepository(IConfiguration _configuration, IMemoryCache cache, IHelperRepository helperRepository)
        {
            configuration = _configuration;
            _cache = cache;
            _helperRepository = helperRepository;
        }

        public async Task<AirSellFromRecResponseModel> GetAirSellRecommendation(AirSellFromRecommendationRequest requestModel)
        {
            AirSellFromRecResponseModel AirSell = new AirSellFromRecResponseModel();

            try
            {

                var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var _url = amadeusSettings["ApiUrl"]; // "https://nodeD2.test.webservices.amadeus.com/1ASIWJIBJAY";
                var _action = amadeusSettings["Air_SellFromRecommendation"];
                string Result = string.Empty;
                string Envelope = await CreateAirSellRecommendationRequest(requestModel);
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
                            await _helperRepository.SaveXmlResponse("AirSellResponse", result2);
                          

                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(result2);
                            string jsonText = JsonConvert.SerializeXmlNode(xmlDoc2, Newtonsoft.Json.Formatting.Indented);
                            await _helperRepository.SaveJson(jsonText, "AirSellResponseJson");
                            XNamespace fareNS = "http://xml.amadeus.com/ITARES_05_2_IA";
                            var errorInfo = xmlDoc.Descendants(fareNS + "errorInfo").FirstOrDefault();
                            if (errorInfo != null)
                            {
                                // Extract error details
                                var errorCode = errorInfo.Descendants(fareNS + "rejectErrorCode").Descendants(fareNS + "errorDetails").Descendants(fareNS + "errorCode").FirstOrDefault()?.Value;
                                var errorText = errorInfo.Descendants(fareNS + "errorFreeText").Descendants(fareNS + "freeText").FirstOrDefault()?.Value;
                                AirSell.amadeusError = new AmadeusResponseError();
                                AirSell.amadeusError.error = errorText;
                                AirSell.amadeusError.errorCode = Convert.ToInt16(errorCode);
                                return AirSell;

                            }

                            var res = ConvertXmlToModel(xmlDoc);
                            AirSell = res;

                        }
                    }
                }
                catch (WebException ex)
                {
                    using (StreamReader rd = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        Result = rd.ReadToEnd();
                        AirSell.amadeusError = new AmadeusResponseError();
                        AirSell.amadeusError.error = Result;
                        AirSell.amadeusError.errorCode = 0;
                        return AirSell;

                    }
                }
            }
            catch (Exception ex)
            {
                AirSell.amadeusError = new AmadeusResponseError();
                AirSell.amadeusError.error = ex.Message.ToString();
                AirSell.amadeusError.errorCode = 0;
                return AirSell;
            }
            return AirSell;
        }
        public AirSellFromRecResponseModel ConvertXmlToModel(XDocument response)
        {
            AirSellFromRecResponseModel ReturnModel = new AirSellFromRecResponseModel();
            ReturnModel.airSellResponse = new List<AirSellItineraryDetails>();
            XDocument doc = response;

            XNamespace awsse = "http://xml.amadeus.com/2010/06/Session_v3";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            var sessionElement = doc.Descendants(awsse + "Session").FirstOrDefault();
            if (sessionElement != null)
            {
                // Extract SessionId, SequenceNumber, and SecurityToken
                string sessionId = sessionElement.Element(awsse + "SessionId")?.Value;
                string sequenceNumber = sessionElement.Element(awsse + "SequenceNumber")?.Value;
                string securityToken = sessionElement.Element(awsse + "SecurityToken")?.Value;
                string TransactionStatusCode = sessionElement.Attribute("TransactionStatusCode")?.Value;
                int SeqNumber = 0;
                if (sequenceNumber != null) { SeqNumber = Convert.ToInt32(sequenceNumber); }
                ReturnModel.session = new HeaderSession
                {
                    SecurityToken = securityToken,
                    SequenceNumber = SeqNumber,
                    SessionId = sessionId,
                    TransactionStatusCode = TransactionStatusCode
                };
            }

            XNamespace amadeus = "http://xml.amadeus.com/ITARES_05_2_IA";
            var messegeFunction = doc.Descendants(amadeus + "message")?.Descendants(amadeus + "messageFunctionDetails")?.Descendants(amadeus + "messageFunction")?.FirstOrDefault().Value;
            var itineraryDetails = doc.Descendants(amadeus + "itineraryDetails").ToList();
            if (itineraryDetails != null)
            {
                foreach (var item in itineraryDetails)
                {
                    AirSellItineraryDetails airSellItinerary = new AirSellItineraryDetails();
                    airSellItinerary.messageFunction = messegeFunction;
                    var origin = item.Descendants(amadeus + "originDestination").Elements(amadeus + "origin")?.FirstOrDefault().Value;
                    var destination = item.Descendants(amadeus + "originDestination").Elements(amadeus + "destination")?.FirstOrDefault().Value;
                    airSellItinerary.originDestination = new OriginDestination { origin = origin, destination = destination };
                    AirSellFlightDetails flightDetails = new AirSellFlightDetails();
                    var departureDate = item.Descendants(amadeus + "segmentInformation")?.Descendants(amadeus + "flightDetails")?.Descendants(amadeus + "flightDate")?.Elements(amadeus + "departureDate").FirstOrDefault().Value;
                    if (departureDate != null)
                    {
                        DateTime deptdate = DateTime.ParseExact(departureDate, "ddMMyy", System.Globalization.CultureInfo.InvariantCulture);
                        flightDetails.departureDate = DateOnly.FromDateTime(deptdate);
                    }
                    var departureTime = item.Descendants(amadeus + "segmentInformation")?.Descendants(amadeus + "flightDetails")?.Descendants(amadeus + "flightDate")?.Elements(amadeus + "departureTime").FirstOrDefault().Value;
                    if (departureTime != null)
                    {
                        TimeOnly deptTime = TimeOnly.ParseExact(departureTime, "HHmm");
                        flightDetails.departureTime = deptTime;
                    }
                    var arrivalDate = item.Descendants(amadeus + "segmentInformation")?.Descendants(amadeus + "flightDetails")?.Descendants(amadeus + "flightDate")?.Elements(amadeus + "arrivalDate").FirstOrDefault().Value;
                    if (arrivalDate != null)
                    {
                        DateTime date = DateTime.ParseExact(arrivalDate, "ddMMyy", System.Globalization.CultureInfo.InvariantCulture);
                        flightDetails.arrivalDate = DateOnly.FromDateTime(date);
                    }
                    var arrivalTime = item.Descendants(amadeus + "segmentInformation")?.Descendants(amadeus + "flightDetails")?.Descendants(amadeus + "flightDate")?.Elements(amadeus + "departureTime").FirstOrDefault().Value;
                    if (arrivalTime != null)
                    {
                        TimeOnly arrTime = TimeOnly.ParseExact(arrivalTime, "HHmm");
                        flightDetails.arrivalTime = arrTime;
                    }
                    var fromAirport = item.Descendants(amadeus + "segmentInformation")?.Descendants(amadeus + "flightDetails")?.Descendants(amadeus + "boardPointDetails")?.Elements(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                    var toAirport = item.Descendants(amadeus + "segmentInformation")?.Descendants(amadeus + "flightDetails")?.Descendants(amadeus + "offpointDetails")?.Elements(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                    flightDetails.fromAirport = fromAirport;
                    flightDetails.toAirport = toAirport;
                    var marketingCompany = item.Descendants(amadeus + "segmentInformation")?.Descendants(amadeus + "flightDetails")?.Descendants(amadeus + "companyDetails")?.Elements(amadeus + "marketingCompany")?.FirstOrDefault()?.Value;
                    flightDetails.marketingCompany = marketingCompany;
                    var flightNumber = item.Descendants(amadeus + "segmentInformation")?.Descendants(amadeus + "flightDetails")?.Descendants(amadeus + "flightIdentification")?.Elements(amadeus + "flightNumber")?.FirstOrDefault()?.Value;
                    var bookingClass = item.Descendants(amadeus + "segmentInformation")?.Descendants(amadeus + "flightDetails")?.Descendants(amadeus + "flightIdentification")?.Elements(amadeus + "bookingClass")?.FirstOrDefault()?.Value;
                    flightDetails.flightNumber = flightNumber;
                    flightDetails.marketingCompany = marketingCompany;
                    var flightIndicator = item.Descendants(amadeus + "segmentInformation")?.Descendants(amadeus + "flightDetails")?.Descendants(amadeus + "flightTypeDetails")?.Elements(amadeus + "flightIndicator")?.FirstOrDefault()?.Value;
                    flightDetails.flightIndicator = flightIndicator;
                    var specialSegment = item.Descendants(amadeus + "segmentInformation")?.Descendants(amadeus + "flightDetails")?.Descendants(amadeus + "specialSegment")?.FirstOrDefault()?.Value;
                    flightDetails.specialSegment = specialSegment;
                    var equipment = item.Descendants(amadeus + "segmentInformation")?.Descendants(amadeus + "apdSegment")?.Descendants(amadeus + "legDetails")?.Elements(amadeus + "equipment")?.FirstOrDefault()?.Value;
                    flightDetails.legdetails = new LegDetails { equipment = equipment };
                    var deptTerminal = item.Descendants(amadeus + "segmentInformation")?.Descendants(amadeus + "apdSegment")?.Descendants(amadeus + "departureStationInfo")?.Elements(amadeus + "terminal")?.FirstOrDefault()?.Value;
                    var arrivalTerminal = item.Descendants(amadeus + "segmentInformation")?.Descendants(amadeus + "apdSegment")?.Descendants(amadeus + "arrivalStationInfo")?.Elements(amadeus + "terminal")?.FirstOrDefault()?.Value;
                    flightDetails.departureTerminal = deptTerminal;
                    flightDetails.arrivalTerminal = arrivalTerminal;
                    var statusCode = item.Descendants(amadeus + "segmentInformation")?.Descendants(amadeus + "actionDetails")?.Elements(amadeus + "statusCode")?.FirstOrDefault()?.Value;
                    flightDetails.statusCode = statusCode;
                    airSellItinerary.flightDetails = flightDetails;
                    ReturnModel.airSellResponse.Add(airSellItinerary);

                }
            }
            return ReturnModel;
        }
        public async Task<string> CreateAirSellRecommendationRequest(AirSellFromRecommendationRequest requestModel)
        {
            string pwdDigest = await generatePassword();
            var amadeusSettings = configuration.GetSection("AmadeusSoap") != null ? configuration.GetSection("AmadeusSoap") : null;
            string action = amadeusSettings["Air_SellFromRecommendation"];
            string to = amadeusSettings["ApiUrl"];
            string username = amadeusSettings["webUserId"];
            string dutyCode = amadeusSettings["dutyCode"];
            string requesterType = amadeusSettings["requestorType"];
            string PseudoCityCode = amadeusSettings["PseudoCityCode"]?.ToString();
            string pos_type = amadeusSettings["POS_Type"];

            // string Request = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:sec=""http://xml.amadeus.com/2010/06/Security_v1"" xmlns:typ=""http://xml.amadeus.com/2010/06/Types_v1"" xmlns:iat=""http://www.iata.org/IATA/2007/00/IATA2010.1"" xmlns:app=""http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3"" xmlns:link=""http://wsdl.amadeus.com/2010/06/ws/Link_v1"" xmlns:ses=""http://xml.amadeus.com/2010/06/Session_v3"">
            string Request = $@"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:ses=""http://xml.amadeus.com/2010/06/Session_v3"">
      <soap:Header xmlns:add=""http://www.w3.org/2005/08/addressing"">
      <ses:Session TransactionStatusCode=""InSeries"">
      <ses:SessionId>{requestModel.sessionDetails.SessionId}</ses:SessionId>
      <ses:SequenceNumber>{requestModel.sessionDetails.SequenceNumber + 1}</ses:SequenceNumber>
      <ses:SecurityToken>{requestModel.sessionDetails.SecurityToken}</ses:SecurityToken>
    </ses:Session>
    <add:MessageID>{System.Guid.NewGuid()}</add:MessageID>
    <add:Action>{action}</add:Action>
    <add:To>{to}</add:To>  
    <link:TransactionFlowLink xmlns:link=""http://wsdl.amadeus.com/2010/06/ws/Link_v1""/>
   </soap:Header>
   <soap:Body>
      <Air_SellFromRecommendation>
         <messageActionDetails>
            <messageFunctionDetails>
               <messageFunction>{requestModel.messageFunction}</messageFunction>
               <additionalMessageFunction>{requestModel.additionalMessageFunction}</additionalMessageFunction>
            </messageFunctionDetails>
         </messageActionDetails>
         <itineraryDetails>
            <originDestinationDetails>
               <origin>{requestModel.outBound.origin}</origin>
               <destination>{requestModel.outBound.destination}</destination>
            </originDestinationDetails>
            <message>
               <messageFunctionDetails>
                  <messageFunction>{requestModel.messageFunction}</messageFunction>
               </messageFunctionDetails>
            </message>
            <segmentInformation>
               <travelProductInformation>
                  <flightDate>
                     <departureDate>{requestModel.outBound.segmentInformation.travelProductInformation.departureDate}</departureDate>
                  </flightDate>
                  <boardPointDetails>
                     <trueLocationId>{requestModel.outBound.segmentInformation.travelProductInformation.fromAirport}</trueLocationId>
                  </boardPointDetails>
                  <offpointDetails>
                     <trueLocationId>{requestModel.outBound.segmentInformation.travelProductInformation.toAirport}</trueLocationId>
                  </offpointDetails>
                  <companyDetails>
                     <marketingCompany>{requestModel.outBound.segmentInformation.travelProductInformation.marketingCompany}</marketingCompany>
                  </companyDetails>
                  <flightIdentification>
                     <flightNumber>{requestModel.outBound.segmentInformation.travelProductInformation.flightNumber}</flightNumber>
                     <bookingClass>{requestModel.outBound.segmentInformation.travelProductInformation.bookingClass}</bookingClass>
                  </flightIdentification>
               </travelProductInformation>
               <relatedproductInformation>
                  <quantity>{requestModel.outBound.segmentInformation.travelProductInformation.relatedproductInformation.quantity}</quantity>
                  <statusCode>{requestModel.outBound.segmentInformation.travelProductInformation.relatedproductInformation.statusCode}</statusCode>
               </relatedproductInformation>
            </segmentInformation>
         </itineraryDetails>
         <itineraryDetails>
            <originDestinationDetails>
               <origin>{requestModel.inBound.origin}</origin>
               <destination>{requestModel.inBound.destination}</destination>
            </originDestinationDetails>
            <message>
               <messageFunctionDetails>
                  <messageFunction>{requestModel.messageFunction}</messageFunction>
               </messageFunctionDetails>
            </message>
            <segmentInformation>
               <travelProductInformation>
                  <flightDate>
                     <departureDate>{requestModel.inBound.segmentInformation.travelProductInformation.departureDate}</departureDate>
                  </flightDate>
                  <boardPointDetails>
                     <trueLocationId>{requestModel.inBound.segmentInformation.travelProductInformation.fromAirport}</trueLocationId>
                  </boardPointDetails>
                  <offpointDetails>
                     <trueLocationId>{requestModel.inBound.segmentInformation.travelProductInformation.toAirport}</trueLocationId>
                  </offpointDetails>
                  <companyDetails>
                     <marketingCompany>{requestModel.inBound.segmentInformation.travelProductInformation.marketingCompany}</marketingCompany>
                  </companyDetails>
                  <flightIdentification>
                     <flightNumber>{requestModel.inBound.segmentInformation.travelProductInformation.flightNumber}</flightNumber>
                     <bookingClass>{requestModel.inBound.segmentInformation.travelProductInformation.bookingClass}</bookingClass>
                  </flightIdentification>
               </travelProductInformation>
               <relatedproductInformation>
                  <quantity>{requestModel.inBound.segmentInformation.travelProductInformation.relatedproductInformation.quantity}</quantity>
                  <statusCode>{requestModel.inBound.segmentInformation.travelProductInformation.relatedproductInformation.statusCode}</statusCode>
               </relatedproductInformation>
            </segmentInformation>
         </itineraryDetails>
      </Air_SellFromRecommendation>
   </soap:Body>

</soap:Envelope>";

            return Request;
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
    }
}