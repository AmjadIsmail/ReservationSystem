using Microsoft.Extensions.Configuration;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models;
using ReservationSystem.Domain.Repositories;
using ReservationSystem.Domain.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Xml;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;
using ReservationSystem.Domain.DB_Models;
using Newtonsoft.Json;


namespace ReservationSystem.Infrastructure.Repositories
{
    public class TravelBoardSearchRepository : ITravelBoardSearchRepository
    {
        private readonly IConfiguration configuration;         
        private readonly ICacheService _cacheService;
        private readonly IDBRepository _dbRepository;
       
        public TravelBoardSearchRepository(IConfiguration _configuration, ICacheService cacheService, IDBRepository dBRepository)
        {
            configuration = _configuration;
            _cacheService = cacheService;
            _dbRepository = dBRepository;
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
            catch(Exception ex)
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

        public async Task<AvailabilityModel> GetAvailability(AvailabilityRequest requestModel)
        {
             var returnModel = new AvailabilityModel();
          
            try
            {
               // string pwdDigest = await generatePassword();
                var amadeusSettings = configuration.GetSection("AmadeusSoap");
                string action = amadeusSettings["travelBoardSearchAction"];                 
                var _url = amadeusSettings["ApiUrl"]; // "https://nodeD2.test.webservices.amadeus.com/1ASIWJIBJAY";
                var _action = amadeusSettings["travelBoardSearchAction"]; // "http://webservices.amadeus.com/FMPTBQ_24_1_1A";
                string Result = string.Empty;
                string Envelope = await CreateSoapEnvelopeSimple(requestModel);
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
                            try
                            {
                                using (XmlWriter writer = XmlWriter.Create("d:\\reservationlogs\\TbSearchResponse" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".xml", settings))
                                {
                                    xmlDoc.Save(writer);
                                }
                            }
                            catch
                            {

                            }
                           
                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(result2);
                            string jsonText = JsonConvert.SerializeXmlNode(xmlDoc2, Newtonsoft.Json.Formatting.Indented);

                            var res = ConvertXmlToModel(xmlDoc);
                            returnModel.data = res.data;

                        }
                    }
                }
                catch (WebException ex)
                {
                    using (StreamReader rd = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        Result = rd.ReadToEnd();
                        returnModel.amadeusError = new AmadeusResponseError();
                        returnModel.amadeusError.error = Result;
                        returnModel.amadeusError.errorCode = 0;
                        return returnModel;

                    }
                }
            }
            catch(Exception ex)
            {
                returnModel.amadeusError = new AmadeusResponseError();
                returnModel.amadeusError.error = ex.Message.ToString();
                returnModel.amadeusError.errorCode = 0;
                return returnModel;
            }
           

            return returnModel;
        }


        private string GetPaxReference(int adults , int child , int infant)
        {
           
            StringBuilder sb = new StringBuilder();
            sb.Append("<paxReference>");
            for(int i = 1; i<= adults; i++)
            {
                sb.Append("<ptc>ADT</ptc>\r\n    <traveller>\r\n      <ref>").Append(i.ToString()).Append("</ref>\r\n    </traveller>\r\n");
            }
            sb.Append("</paxReference>");
            if(child > 0)
            {
                sb.Append("<paxReference>");
                for (int c = 1; c <= child; c++)
                {
                    sb.Append("<ptc>CNN</ptc>\r\n    <traveller>\r\n      <ref>").Append(c.ToString()).Append("</ref>\r\n    </traveller>\r\n");
                }
                sb.Append("</paxReference>");
            }
            if(infant > 0)
            {
                sb.Append("<paxReference>");
                for (int d = 1; d <= infant; d++)
                {
                    sb.Append("<ptc>INF</ptc>\r\n    <traveller>\r\n      <ref>").Append(d.ToString()).Append("</ref>\r\n    </traveller>\r\n");
                }
                sb.Append("</paxReference>");
            }
          
            
            return sb.ToString();
        }

      
        public async  Task<string> CreateSoapEnvelopeSimple(AvailabilityRequest requestModel)
        {                      
            string pwdDigest =  await generatePassword();
            var amadeusSettings = configuration.GetSection("AmadeusSoap");
            string action = amadeusSettings["travelBoardSearchAction"];
            string to = amadeusSettings["ApiUrl"];
            string username = amadeusSettings["webUserId"];
            string dutyCode = amadeusSettings["dutyCode"];
            string requesterType = amadeusSettings["requestorType"];
            string PseudoCityCode = amadeusSettings["PseudoCityCode"]?.ToString();
            string pos_type = amadeusSettings["POS_Type"];
            requestModel.children = requestModel.children != null ? requestModel.children : 0;
            requestModel.infant = requestModel.infant != null ? requestModel.infant : 0;
            if(requestModel.departureDate != null && requestModel.departureDate != "")
            {
                string inputDate = requestModel.departureDate;                
                DateTime deptdate = DateTime.Parse(inputDate);
                requestModel.departureDate = deptdate.ToString("ddMMyy");
            }
            if (requestModel.returnDate != null && requestModel.returnDate != "")
            {
                string inputDate = requestModel.returnDate;
                DateTime retdate = DateTime.Parse(inputDate);
                requestModel.returnDate = retdate.ToString("ddMMyy");
            }
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
      <Fare_MasterPricerTravelBoardSearch>
         <numberOfUnit>
            <unitNumberDetail>
               <numberOfUnits>{requestModel.adults+requestModel.children+requestModel.infant}</numberOfUnits>
               <typeOfUnit>PX</typeOfUnit>
            </unitNumberDetail>
            <unitNumberDetail>
               <numberOfUnits>{requestModel.maxFlights}</numberOfUnits>
               <typeOfUnit>RC</typeOfUnit>
            </unitNumberDetail>
         </numberOfUnit>"
        + GetPaxReference(requestModel.adults.Value,requestModel.children.Value,requestModel.infant.Value)+
         @"<fareOptions>
            <pricingTickInfo>
               <pricingTicketing>
                  <priceType>ET</priceType>
                  <priceType>RP</priceType>
                  <priceType>RU</priceType>
                  <priceType>TAC</priceType>
                  <priceType>XND</priceType>
                  <priceType>XLA</priceType>
                  <priceType>XLO</priceType>
                  <priceType>XLC</priceType>
                  <priceType>XND</priceType>
               </pricingTicketing>
            </pricingTickInfo>           
         </fareOptions> 
<travelFlightInfo>
            <companyIdentity>
               <carrierQualifier>X</carrierQualifier>
               <carrierId>NK</carrierId>
               <carrierId>F9</carrierId>
            </companyIdentity>
            <flightDetail>
               <flightType>N</flightType>
            </flightDetail>
</travelFlightInfo>
         <itinerary>
            <requestedSegmentRef>
               <segRef>1</segRef>
            </requestedSegmentRef>
            <departureLocalization>
               <departurePoint>
                  <locationId>" + requestModel.origin +@"</locationId>
               </departurePoint>
            </departureLocalization>
            <arrivalLocalization>
               <arrivalPointDetails>
                  <locationId>" + requestModel.destination + @"</locationId>
               </arrivalPointDetails>
            </arrivalLocalization>
            <timeDetails>
               <firstDateTimeDetail>
                  <date>" + requestModel.departureDate + @"</date>
               </firstDateTimeDetail>
            </timeDetails>
         </itinerary>
         <itinerary>
            <requestedSegmentRef>
               <segRef>2</segRef>
            </requestedSegmentRef>
            <departureLocalization>
               <departurePoint>
                  <locationId>" + requestModel.destination + @"</locationId>
               </departurePoint>
            </departureLocalization>
            <arrivalLocalization>
               <arrivalPointDetails>
                  <locationId>" + requestModel.origin + @"</locationId>
               </arrivalPointDetails>
            </arrivalLocalization>
            <timeDetails>
               <firstDateTimeDetail>
                  <date>" + requestModel.returnDate+ @"</date>
               </firstDateTimeDetail>
            </timeDetails>
         </itinerary>
      </Fare_MasterPricerTravelBoardSearch>
   </soapenv:Body>

</soapenv:Envelope>";

            return Request;
        }
        public AvailabilityModel ConvertXmlToModel(XDocument response)
        {
            AvailabilityModel ReturnModel = new AvailabilityModel();
            ReturnModel.data = new List<FlightOffer>();
            XDocument doc = response;
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace amadeus = "http://xml.amadeus.com/FMPTBR_24_1_1A";

            List<Itinerary>  itinerariesList = new List<Itinerary>();
            
            var currency = doc.Descendants(amadeus + "conversionRate").Descendants(amadeus + "conversionRateDetail")?.Elements(amadeus + "currency")?.FirstOrDefault()?.Value;
            var flightIndexOutBound = doc.Descendants(amadeus + "flightIndex").Where(f => f.Element(amadeus + "requestedSegmentRef").Element(amadeus + "segRef").Value == "1")
                             .ToList();

            // var flightDetailsList = doc.Descendants(amadeus + "flightDetails").ToList();
            if (flightIndexOutBound != null)
            {
                
                var flightDetails1 = flightIndexOutBound.Descendants(amadeus + "groupOfFlights").ToList();
               
               
                foreach ( var groupOfFlights in flightDetails1)
                {
                    Itinerary itinerary = new Itinerary();
                    itinerary.segments = new List<Segment>();
                    var FlightProposal = groupOfFlights.Element(amadeus + "propFlightGrDetail")?.Element(amadeus + "flightProposal").Element(amadeus + "ref").Value;
                    var numberOfStops = groupOfFlights.Descendants(amadeus + "flightDetails").ToList().Count();
                    numberOfStops = numberOfStops - 1;
                    foreach (var flightDetails in (groupOfFlights.Descendants(amadeus + "flightDetails").ToList()))
                    {
                        var productDateTime = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "productDateTime");
                        var departureDate = productDateTime?.Element(amadeus + "dateOfDeparture")?.Value;
                        var departureTime = productDateTime?.Element(amadeus + "timeOfDeparture")?.Value;
                        var arrivalDate = productDateTime?.Element(amadeus + "dateOfArrival")?.Value;
                        var arrivalTime = productDateTime?.Element(amadeus + "timeOfArrival")?.Value;
                        var FlightNumber = flightDetails?.Element(amadeus + "flightInformation")?.Element(amadeus + "flightOrtrainNumber")?.Value;
                        string FlightDuration = string.Empty;  
                        var duration = flightDetails.Descendants(amadeus + "attributeDetails").Where(e => e.Element(amadeus + "attributeType")?.Value == "EFT" );
                        if (duration != null)
                        {
                            FlightDuration = duration?.Descendants(amadeus + "attributeDescription")?.FirstOrDefault().Value;
                        }
                        var departureLocation = flightDetails.Element(amadeus + "flightInformation")?
                            .Elements(amadeus + "location")?.FirstOrDefault()?.Element(amadeus + "locationId")?.Value;
                        var departureTerminal = flightDetails.Element(amadeus + "flightInformation")?
                            .Elements(amadeus + "location")?.FirstOrDefault()?.Element(amadeus + "terminal")?.Value;
                        var arrivalLocation = flightDetails.Element(amadeus + "flightInformation")?
                            .Elements(amadeus + "location")?.Skip(1).FirstOrDefault()?.Element(amadeus + "locationId")?.Value;
                        var arrivalTerminal = flightDetails.Element(amadeus + "flightInformation")?
                            .Elements(amadeus + "location")?.Skip(1).FirstOrDefault()?.Element(amadeus + "terminal")?.Value;

                        var marketingCarrier = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "companyId")?.Element(amadeus + "marketingCarrier")?.Value;
                        var flightNumber = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "flightOrtrainNumber")?.Value;
                        Segment segment = new Segment();
                        string dateTimeStr = departureDate + departureTime;
                        string format = "ddMMyyHHmm";
                        DateTime departureD = DateTime.ParseExact(dateTimeStr, format, CultureInfo.InvariantCulture);
                        segment.departure = new Departure { at = departureD, iataCode = departureLocation , terminal = departureTerminal  };
                        string arrival = arrivalDate + arrivalTime;
                        DateTime arrivalD = DateTime.ParseExact(arrival, format, CultureInfo.InvariantCulture);
                        segment.arrival = new Arrival { at = arrivalD, iataCode = arrivalLocation , terminal = arrivalTerminal };
                        segment.carrierCode = marketingCarrier;
                        segment.aircraft = new Aircraft { code = flightNumber };
                        segment.duration = FlightDuration;
                        segment.number = FlightNumber;
                        segment.id = FlightProposal;
                        segment.operating = new Operating {  carrierCode = marketingCarrier };
                        segment.numberOfStops = numberOfStops;
                        
                        itinerary.segments.Add(segment);
                        itinerary.flightProposal_ref = FlightProposal;
                        itinerary.segment_type = "OutBound";

                    }

                    itinerariesList.Add(itinerary);


                }


            }

            var flightIndexInbound = doc.Descendants(amadeus + "flightIndex").Where(f => f.Element(amadeus + "requestedSegmentRef").Element(amadeus + "segRef").Value == "2")
                            .ToList();

            if (flightIndexInbound != null)
            {

                var flightDetails1 = flightIndexInbound.Descendants(amadeus + "groupOfFlights").ToList();


                foreach (var groupOfFlights in flightDetails1)
                {
                    Itinerary itinerary = new Itinerary();
                    itinerary.segments = new List<Segment>();
                    var FlightProposal = groupOfFlights.Element(amadeus + "propFlightGrDetail")?.Element(amadeus + "flightProposal").Element(amadeus + "ref").Value;
                    var numberOfStops = groupOfFlights.Descendants(amadeus + "flightDetails").ToList().Count();
                    numberOfStops = numberOfStops - 1;
                    foreach (var flightDetails in (groupOfFlights.Descendants(amadeus + "flightDetails").ToList()))
                    {
                        var productDateTime = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "productDateTime");
                        var departureDate = productDateTime?.Element(amadeus + "dateOfDeparture")?.Value;
                        var departureTime = productDateTime?.Element(amadeus + "timeOfDeparture")?.Value;
                        var arrivalDate = productDateTime?.Element(amadeus + "dateOfArrival")?.Value;
                        var arrivalTime = productDateTime?.Element(amadeus + "timeOfArrival")?.Value;
                        var FlightNumber = flightDetails?.Element(amadeus + "flightInformation")?.Element(amadeus + "flightOrtrainNumber")?.Value;
                        string FlightDuration = string.Empty;
                        var duration = flightDetails.Descendants(amadeus + "attributeDetails").Where(e => e.Element(amadeus + "attributeType")?.Value == "EFT");
                        if (duration != null)
                        {
                            FlightDuration = duration?.Descendants(amadeus + "attributeDescription")?.FirstOrDefault().Value;
                        }
                        var departureLocation = flightDetails.Element(amadeus + "flightInformation")?
                            .Elements(amadeus + "location")?.FirstOrDefault()?.Element(amadeus + "locationId")?.Value;
                        var departureTerminal = flightDetails.Element(amadeus + "flightInformation")?
                            .Elements(amadeus + "location")?.FirstOrDefault()?.Element(amadeus + "terminal")?.Value;
                        var arrivalLocation = flightDetails.Element(amadeus + "flightInformation")?
                            .Elements(amadeus + "location")?.Skip(1).FirstOrDefault()?.Element(amadeus + "locationId")?.Value;
                        var arrivalTerminal = flightDetails.Element(amadeus + "flightInformation")?
                            .Elements(amadeus + "location")?.Skip(1).FirstOrDefault()?.Element(amadeus + "terminal")?.Value;

                        var marketingCarrier = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "companyId")?.Element(amadeus + "marketingCarrier")?.Value;
                        var flightNumber = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "flightOrtrainNumber")?.Value;
                        Segment segment = new Segment();
                        string dateTimeStr = departureDate + departureTime;
                        string format = "ddMMyyHHmm";
                        DateTime departureD = DateTime.ParseExact(dateTimeStr, format, CultureInfo.InvariantCulture);
                        segment.departure = new Departure { at = departureD, iataCode = departureLocation, terminal = departureTerminal };
                        string arrival = arrivalDate + arrivalTime;
                        DateTime arrivalD = DateTime.ParseExact(arrival, format, CultureInfo.InvariantCulture);
                        segment.arrival = new Arrival { at = arrivalD, iataCode = arrivalLocation, terminal = arrivalTerminal };
                        segment.carrierCode = marketingCarrier;
                        segment.aircraft = new Aircraft { code = flightNumber };
                        segment.duration = FlightDuration;
                        segment.number = FlightNumber;
                        segment.id = FlightProposal;
                        segment.operating = new Operating { carrierCode = marketingCarrier };
                        segment.numberOfStops = numberOfStops;

                        itinerary.segments.Add(segment);
                        itinerary.flightProposal_ref = FlightProposal;
                        itinerary.segment_type = "InBound";

                    }

                    itinerariesList.Add(itinerary);


                }


            }
               
            //if (flightIndexInbound != null)
            //{
            //    var flightDetails2 = flightIndexInbound.Descendants(amadeus + "flightDetails").ToList();
            //    foreach (var flightDetails in flightDetails2)
            //    { 
            //    var productDateTime = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "productDateTime");
            //    var departureDate = productDateTime?.Element(amadeus + "dateOfDeparture")?.Value;
            //    var departureTime = productDateTime?.Element(amadeus + "timeOfDeparture")?.Value;
            //    var arrivalDate = productDateTime?.Element(amadeus + "dateOfArrival")?.Value;
            //    var arrivalTime = productDateTime?.Element(amadeus + "timeOfArrival")?.Value;

            //    var departureLocation = flightDetails.Element(amadeus + "flightInformation")?
            //        .Elements(amadeus + "location")?.FirstOrDefault()?.Element(amadeus + "locationId")?.Value;
            //    var arrivalLocation = flightDetails.Element(amadeus + "flightInformation")?
            //        .Elements(amadeus + "location")?.Skip(1).FirstOrDefault()?.Element(amadeus + "locationId")?.Value;

            //    var marketingCarrier = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "companyId")?.Element(amadeus + "marketingCarrier")?.Value;
            //    var flightNumber = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "flightOrtrainNumber")?.Value;


            //    Segment segment = new Segment();
            //    string dateTimeStr = departureDate + departureTime;
            //    string format = "ddMMyyHHmm";
            //    DateTime departureD = DateTime.ParseExact(dateTimeStr, format, CultureInfo.InvariantCulture);
            //    segment.departure = new Departure { at = departureD, iataCode = departureLocation };
            //    string arrival = arrivalDate + arrivalTime;
            //    DateTime arrivalD = DateTime.ParseExact(arrival, format, CultureInfo.InvariantCulture);
            //    segment.arrival = new Arrival { at = arrivalD, iataCode = arrivalLocation };
            //    segment.carrierCode = marketingCarrier;
            //    segment.aircraft = new Aircraft { code = flightNumber };
            //    itinerary1.segments.Add(segment);
            //    }


            //}
            ////   offer.itineraries.Add(itinerary);

            #region Working For Recemondations
            string id = string.Empty, price = string.Empty, refnumenr = string.Empty, totalFareAmount = string.Empty;
            string totalTax = string.Empty; string transportStageQualifier = string.Empty; string transportStageQualifierCompany = string.Empty;
            string pricingTicketingPriceType = string.Empty;
            string isRefundable = string.Empty;
            string LastTicketDate = string.Empty;
            string cabinProduct = string.Empty;
            string farebasis = string.Empty;
            string companyname = string.Empty;
            var recommendationList = doc.Descendants(amadeus + "recommendation").ToList();
            if (recommendationList != null)
            {
                foreach (var item in recommendationList)
                {
                    FlightOffer offer = new FlightOffer();
                    offer.itineraries = new List<Itinerary>();
                    // offer.itineraries.Add(itinerary1);
                    LastTicketDate = string.Empty;

                    var itemNumberId = item.Descendants(amadeus + "itemNumber").Elements(amadeus + "itemNumberId")?.FirstOrDefault()?.Value;
                    price = item.Descendants(amadeus + "recPriceInfo").Elements(amadeus + "monetaryDetail").Elements(amadeus + "amount")?.FirstOrDefault()?.Value;
                    var priceinfo2 = item.Descendants(amadeus + "recPriceInfo").Elements(amadeus + "monetaryDetail").Elements(amadeus + "amount").Skip(1)?.FirstOrDefault()?.Value;
                    totalFareAmount = item.Descendants(amadeus + "paxFareProduct").Elements(amadeus + "paxFareDetail").Elements(amadeus + "totalFareAmount")?.FirstOrDefault()?.Value;
                    totalTax = item.Descendants(amadeus + "paxFareProduct").Elements(amadeus + "paxFareDetail").Elements(amadeus + "totalTaxAmount")?.FirstOrDefault()?.Value;
                    var paxReferece = item.Descendants(amadeus + "paxFareProduct").Elements(amadeus + "paxReference").ToList();
                    var companylist = item.Descendants(amadeus + "paxFareProduct").Elements(amadeus + "paxFareDetail").Elements(amadeus + "codeShareDetails").ToList();
                    foreach (var company in companylist)
                    {
                        companyname = companyname + " " + company.Descendants(amadeus + "company")?.FirstOrDefault()?.Value;
                    }
                    var fare = item.Descendants(amadeus + "fare").ToList();
                    string faretype = "";
                    foreach (var fareitem in fare)
                    {
                        var IsREfunableTicket = fareitem.Descendants(amadeus + "pricingMessage").Where(f => f.Element(amadeus + "freeTextQualification").Element(amadeus + "informationType").Value == "70")
                            .FirstOrDefault();
                        if (IsREfunableTicket != null)
                        {
                            isRefundable = fareitem.Descendants(amadeus + "pricingMessage").Elements(amadeus + "description")?.FirstOrDefault().Value;
                        }

                        var LastTkctDate = fareitem.Descendants(amadeus + "pricingMessage").Where(f => f.Element(amadeus + "freeTextQualification").Element(amadeus + "informationType").Value == "40")
                           .FirstOrDefault();
                        if (LastTkctDate != null)
                        {
                            var lstDate = fareitem.Descendants(amadeus + "pricingMessage").Elements(amadeus + "description")?.ToList();
                            foreach (var i in lstDate?.Skip(1)?.Take(1))
                            {
                                LastTicketDate = LastTicketDate + " " + i.Value;
                            }
                        }

                    }

                    var fareDetailsGroupOfFare = item.Descendants(amadeus + "fareDetails").Descendants(amadeus + "groupOfFares").ToList();

                    if (fareDetailsGroupOfFare != null)
                    {
                        foreach (var productInfo in fareDetailsGroupOfFare)
                        {
                            var cabinP = productInfo.Descendants(amadeus + "productInformation").Descendants(amadeus + "cabinProduct").ToList();
                            foreach (var itemcabinp in cabinP)
                            {
                                cabinProduct = cabinProduct + "," + itemcabinp.Descendants(amadeus + "cabin")?.FirstOrDefault()?.Value;
                            }
                            var fbasis = productInfo.Descendants(amadeus + "productInformation").Descendants(amadeus + "fareProductDetail").ToList();
                            foreach (var f in fbasis)
                            {
                                farebasis = farebasis + "," + f.Element(amadeus + "fareBasis")?.Value;
                                faretype = faretype = faretype + "," + f.Element(amadeus + "fareType").Value;
                            }

                        }
                    }


                    var taxes = new List<Taxes>();
                    Taxes t = new Taxes { amount = totalTax, code = "" };
                    taxes.Add(t);
                    offer.price = new Price
                    {
                        base_amount = totalFareAmount,
                        currency = currency,
                        grandTotal = price
                        ,
                        taxes = taxes,
                        total = price,
                        discount = 0,
                        billingCurrency = currency,
                        markup = 0
                    };

                    offer.id = itemNumberId;
                    offer.lastTicketingDate = LastTicketDate;
                    offer.lastTicketingDateTime = "0:00";
                    offer.oneWay = flightIndexInbound != null ? false : true;
                    offer.pricingOptions = new PriceOption { fareType = faretype.Split(',').ToList<string>(), includedCheckedBagsOnly = false };
                    offer.source = "Amadeus";
                    offer.travelerPricings = new List<TravelerPricing>();
                    foreach (var pfx in paxReferece)
                    {
                        var ptc = pfx.Element(amadeus + "ptc").Value;
                        var traveler = pfx.Descendants(amadeus + "traveller").ToList();
                        foreach (var tr in traveler)
                        {
                            TravelerPricing tp = new TravelerPricing { travelerType = ptc, travelerId = tr.Element(amadeus + "ref").Value, price = offer.price };
                            offer.travelerPricings.Add(tp);
                        }
                    }
                    offer.bookingClass = cabinProduct;
                    offer.validatingAirlineCodes = companyname.Split(" ").ToList<string>();
                    #region Get Itineraries from outbound
                    List<Itinerary> _outbounItineraries = new List<Itinerary>(); List<Itinerary> _inbounItineraries = new List<Itinerary>();
                    _outbounItineraries = itinerariesList.Where(e => e.segment_type == "OutBound" && e.flightProposal_ref == itemNumberId).ToList();
                    _inbounItineraries = itinerariesList.Where(e => e.segment_type == "InBound" && e.flightProposal_ref == itemNumberId).ToList();
                    offer.itineraries.AddRange(_outbounItineraries);
                    offer.itineraries.AddRange(_inbounItineraries);
                    #endregion
                    ReturnModel.data.Add(offer);
                }

            }
            #endregion
             

            return ReturnModel;
        }

        private List<FlightOffer> applyMarkup(List<FlightOffer> offers, List<FlightMarkup> dictionary)
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
                                    item.price.markup = adultpp + childpp + infantpp;
                                    item.price.total = (Convert.ToDecimal(item?.price?.total) + adultpp + childpp + infantpp).ToString();
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
            catch (Exception ex)
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
                        var offer = offers.Where(o => o.itineraries.Any(i => i.segments.Any(s => s.carrierCode == item))).ToList();


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
