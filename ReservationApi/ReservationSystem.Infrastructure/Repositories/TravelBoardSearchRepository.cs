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
using System.Data;
using ReservationSystem.Domain.Models.FlightPrice;
using DocumentFormat.OpenXml.Office.CustomUI;



namespace ReservationSystem.Infrastructure.Repositories
{
    public class TravelBoardSearchRepository : ITravelBoardSearchRepository
    {
        private readonly IConfiguration configuration;         
        private readonly ICacheService _cacheService;
        private readonly IDBRepository _dbRepository;
        private readonly IHelperRepository _helperRepository;
       
        public TravelBoardSearchRepository(IConfiguration _configuration, ICacheService cacheService, IDBRepository dBRepository, IHelperRepository helperRepository)
        {
            configuration = _configuration;
            _cacheService = cacheService;
            _dbRepository = dBRepository;
            _helperRepository = helperRepository;
        }        
        

        public async Task<AvailabilityModel> GetAvailability(AvailabilityRequest requestModel)
        {
             var returnModel = new AvailabilityModel();
          
            try
            {
                #region Checking Results from Error
                //var availabilityModel = _cacheService.Get<AvailabilityModel>("amadeusRequest" + requestModel.ToString());
                //if(availabilityModel != null)
                //{
                //    return availabilityModel;
                //}
                #endregion
                var amadeusSettings = configuration; //.GetSection("AmadeusSoap");
                string action = Environment.GetEnvironmentVariable(amadeusSettings["AmadeusSoap:travelBoardSearchAction"]);                 
                var _url = Environment.GetEnvironmentVariable(amadeusSettings["AmadeusSoap:ApiUrl"]); 
                var _action = Environment.GetEnvironmentVariable(amadeusSettings["AmadeusSoap:travelBoardSearchAction"]);
                string Result = string.Empty;
                string Envelope = await CreateSoapEnvelopeSimple(requestModel);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url);
                request.Headers.Add("SOAPAction", _action);
                request.ContentType = "text/xml;charset=\"utf-8\"";
                request.Accept = "text/xml";
                request.Method = "POST";
                XNamespace fareNS = "http://xml.amadeus.com/FMPTBR_24_1_1A";

                #region Temp Region for read response from xml file
                //var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SupportFiles", "TbSearch_Response.xml");
                //var xDocumentTemp = XDocument.Load(filePath);
                //var restemp = ConvertXmlToModel(xDocumentTemp);
                //returnModel.data = restemp.data;
                //return returnModel;
                #endregion
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
                             _helperRepository.SaveXmlResponse("TbSearch_Request", Envelope);
                             _helperRepository.SaveXmlResponse("TbSearch_Response", result2);
                            
                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(result2);
                            string jsonText = JsonConvert.SerializeXmlNode(xmlDoc2, Newtonsoft.Json.Formatting.Indented);
                             _helperRepository.SaveJson(jsonText, "TbSearchResponseJson");
                            var errorInfo = xmlDoc.Descendants(fareNS + "errorMessage").FirstOrDefault();
                            if ( errorInfo != null)
                            {
                                var errorText = xmlDoc.Descendants(fareNS + "errorMessage").Descendants(fareNS + "errorMessageText").Descendants(fareNS + "description")?.FirstOrDefault()?.Value;
                                var errorCode = xmlDoc.Descendants(fareNS + "errorMessage").Descendants(fareNS + "applicationError").Descendants(fareNS + "applicationErrorDetail").Descendants(fareNS + "error")?.FirstOrDefault()?.Value;
                                returnModel.amadeusError = new AmadeusResponseError();
                                returnModel.amadeusError.error = errorText;
                                returnModel.amadeusError.errorCode = Convert.ToInt16(errorCode);
                                #region Save Results To dB
                                try
                                {
                                    await _dbRepository.SaveAvailabilityResult(System.Text.Json.JsonSerializer.Serialize(requestModel), System.Text.Json.JsonSerializer.Serialize(returnModel), 0);
                                }
                                catch (Exception ex)
                                {
                                    Console.Write($"Error while saving Availibilty log{ex.Message.ToString()}");
                                }

                                #endregion
                                return returnModel;

                            }
                            var res = ConvertXmlToModel(xmlDoc);                             
                            returnModel.data = res.data;
                            if(res?.data.Count > 0)
                            {
                                #region Save Results To dB
                                try
                                {
                                    await _dbRepository.SaveAvailabilityResult(System.Text.Json.JsonSerializer.Serialize(requestModel), System.Text.Json.JsonSerializer.Serialize(returnModel), res.data.Count());
                                }
                                catch (Exception ex)
                                {
                                    Console.Write($"Error while saving Availibilty log{ex.Message.ToString()}");
                                }
                                #endregion
                               // _cacheService.Set("amadeusRequest" + requestModel.ToString(), returnModel, TimeSpan.FromMinutes(15));
                            }
                           

                        }
                    }
                }
                catch (WebException ex)
                {
                    using (StreamReader rd = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        Result = rd.ReadToEnd();
                        XDocument Errordoc = XDocument.Parse(Result);
                        XNamespace soapenvNs = "http://schemas.xmlsoap.org/soap/envelope/";
                        var errorInfo = Errordoc.Descendants(soapenvNs + "Fault")?.FirstOrDefault ().Value;
                        returnModel.amadeusError = new AmadeusResponseError();
                        returnModel.amadeusError.error = errorInfo;
                        returnModel.amadeusError.errorCode = 0;
                        try
                        {
                            await _dbRepository.SaveAvailabilityResult(System.Text.Json.JsonSerializer.Serialize(requestModel), System.Text.Json.JsonSerializer.Serialize(returnModel), 0);
                        }
                        catch (Exception ex2)
                        {
                            Console.Write($"Error while saving Availibilty log{ex2.Message.ToString()}");
                        }
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
            sb.Append("<ptc>ADT</ptc>");
            for (int i = 1; i<= adults; i++)
            {
                sb.Append("\r\n    <traveller>\r\n      <ref>").Append(i.ToString()).Append("</ref>\r\n    </traveller>\r\n");
            }
            sb.Append("</paxReference>");
            if(child > 0)
            {
                sb.Append("<paxReference>");
                sb.Append("<ptc>CNN</ptc>");
                for (int c = 1; c <= child; c++)
                {
                    sb.Append("\r\n    <traveller>\r\n      <ref>").Append((adults+c).ToString()).Append("</ref>\r\n    </traveller>\r\n");
                }
                sb.Append("</paxReference>");
            }
            if(infant > 0)
            {
                sb.Append("<paxReference>");
                sb.Append("<ptc>INF</ptc>");
                for (int d = 1; d <= infant; d++)
                {
                    sb.Append("\r\n    <traveller>\r\n      <ref>").Append(d.ToString()).Append("</ref>\r\n");
                    sb.Append("\r\n <infantIndicator>1</infantIndicator>");
                    sb.Append("\r\n    </traveller>\r\n");
                }
                sb.Append("</paxReference>");
            }
          
            
            return sb.ToString();
        }

      
        public async  Task<string> CreateSoapEnvelopeSimple(AvailabilityRequest requestModel)
        {                      
            string pwdDigest =  await _helperRepository.generatePassword();
            var amadeusSettings = configuration;//.GetSection("AmadeusSoap");
            string action = Environment.GetEnvironmentVariable(amadeusSettings["AmadeusSoap:travelBoardSearchAction"]);
            string to = Environment.GetEnvironmentVariable(amadeusSettings["AmadeusSoap:ApiUrl"]);
            string username = Environment.GetEnvironmentVariable(amadeusSettings["AmadeusSoap:webUserId"]);
            string dutyCode = Environment.GetEnvironmentVariable(amadeusSettings["AmadeusSoap:dutyCode"]);
            string requesterType = Environment.GetEnvironmentVariable(amadeusSettings["AmadeusSoap:requestorType"]);
            string PseudoCityCode = Environment.GetEnvironmentVariable(amadeusSettings["AmadeusSoap:PseudoCityCode"]?.ToString());
            string pos_type = Environment.GetEnvironmentVariable(amadeusSettings["AmadeusSoap:POS_Type"]);
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
            requestModel.maxFlights = requestModel.maxFlights != null ? requestModel.maxFlights : 250;
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
               <numberOfUnits>{requestModel.adults + requestModel.children}</numberOfUnits>
               <typeOfUnit>PX</typeOfUnit>
            </unitNumberDetail>
            <unitNumberDetail>
               <numberOfUnits>{requestModel.maxFlights }</numberOfUnits>
               <typeOfUnit>RC</typeOfUnit>
            </unitNumberDetail>
         </numberOfUnit>"
        + GetPaxReference(requestModel.adults.Value, requestModel.children.Value, requestModel.infant.Value) +
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
           " + getCabinClass(requestModel.cabinClass) + @"
           " + getFlightType(requestModel.flightType) + @"
</travelFlightInfo>
         <itinerary>
            <requestedSegmentRef>
               <segRef>1</segRef>
            </requestedSegmentRef>
            <departureLocalization>
               <departurePoint>
                  <locationId>" + requestModel.origin + @"</locationId>
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
           "+ getReturnDate(requestModel.returnDate)+@"
         </itinerary>
      </Fare_MasterPricerTravelBoardSearch>
   </soapenv:Body>

</soapenv:Envelope>";

            return Request;
        }
        private string getReturnDate(string? returnDate)
        {
            string retDate = $@"";
            try
            {
                if (!string.IsNullOrEmpty(returnDate)){
                    retDate = $@" <timeDetails>
               <firstDateTimeDetail>
                  <date>" + returnDate + @"</date>
               </firstDateTimeDetail>
            </timeDetails>";
                }
            }
            catch{

            }
            return retDate;
        }
        private string getCabinClass (string cabinclass)
        {
            string returncabinClass = @"";
            try
            {
                if (!string.IsNullOrEmpty(cabinclass))
                {
                    returncabinClass = returncabinClass + @"  
            <cabinId>
             <cabin>"+cabinclass+@"</cabin>
            </cabinId>";
                }
            }
            catch
            {

            }
            return returncabinClass;
        }
        private string getFlightType(string flightType)
        {
            string returnflightType = @"";
            try
            {
                if (!string.IsNullOrEmpty(flightType))
                {
                    returnflightType = returnflightType + @"  
                    <flightDetail>
                    <flightType>"+ flightType + @"</flightType>
                   </flightDetail>";
                }
            }
            catch{}
            return returnflightType;
        }
        public AvailabilityModel ConvertXmlToModel(XDocument response)
        {
            AvailabilityModel ReturnModel = new AvailabilityModel();
            ReturnModel.data = new List<FlightOffer>();
            XDocument doc = response;
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace amadeus = "http://xml.amadeus.com/FMPTBR_24_1_1A";
            var AirlineCache = _cacheService.GetAirlines();
            var AirportCache = _cacheService.GetAirports();

            List<Itinerary>  itinerariesList = new List<Itinerary>();
            List<string> timeduration = new List<string>();

            var currency = doc.Descendants(amadeus + "conversionRate").Descendants(amadeus + "conversionRateDetail")?.Elements(amadeus + "currency")?.FirstOrDefault()?.Value;
            var flightIndexOutBound = doc.Descendants(amadeus + "flightIndex").Where(f => f.Element(amadeus + "requestedSegmentRef").Element(amadeus + "segRef").Value == "1")
                             .ToList();           
            if (flightIndexOutBound != null)
            {
                
                var flightDetails1 = flightIndexOutBound.Descendants(amadeus + "groupOfFlights").ToList();
                var segRef = "1";

                foreach ( var groupOfFlights in flightDetails1)
                {
                    Itinerary itinerary = new Itinerary();
                    itinerary.segments = new List<Segment>();
                    var FlightProposal = groupOfFlights.Element(amadeus + "propFlightGrDetail")?.Element(amadeus + "flightProposal").Element(amadeus + "ref").Value;
                    var numberOfStops = groupOfFlights.Descendants(amadeus + "flightDetails").ToList().Count();
                    numberOfStops = numberOfStops - 1;
                    int segmentRef = 1;
                    timeduration = new List<string>();
                    string airport_city = string.Empty;
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
                            timeduration.Add(FlightDuration);
                        }
                        var departureLocation = flightDetails.Element(amadeus + "flightInformation")?.Elements(amadeus + "location")?.FirstOrDefault()?.Element(amadeus + "locationId")?.Value;
                        DataRow depatureAirport = AirportCache!= null ? AirportCache.AsEnumerable().FirstOrDefault(r => r.Field<string>("AirportCode") == departureLocation) : null;
                        var depAirportName = depatureAirport != null ? depatureAirport[2]?.ToString() + " , " + depatureAirport[4].ToString() : "";
                        airport_city = airport_city == string.Empty ?  depatureAirport != null ? depatureAirport[3]?.ToString() : "" : airport_city;
                        var departureTerminal = flightDetails.Element(amadeus + "flightInformation")?
                            .Elements(amadeus + "location")?.FirstOrDefault()?.Element(amadeus + "terminal")?.Value;
                        var arrivalLocation = flightDetails.Element(amadeus + "flightInformation")?
                            .Elements(amadeus + "location")?.Skip(1).FirstOrDefault()?.Element(amadeus + "locationId")?.Value;
                        DataRow arrivalAirport = AirportCache != null ? AirportCache.AsEnumerable().FirstOrDefault(r => r.Field<string>("AirportCode") == arrivalLocation) : null;
                        var arrAirportName = arrivalAirport != null ? arrivalAirport[2].ToString() + " , " + arrivalAirport[4].ToString() : "";

                        var arrivalTerminal = flightDetails.Element(amadeus + "flightInformation")?
                            .Elements(amadeus + "location")?.Skip(1).FirstOrDefault()?.Element(amadeus + "terminal")?.Value;

                        var marketingCarrier = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "companyId")?.Element(amadeus + "marketingCarrier")?.Value;
                        var operatingCarrier = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "companyId")?.Element(amadeus + "operatingCarrier")?.Value;
                        DataRow carrier = AirlineCache != null ? AirlineCache.AsEnumerable().FirstOrDefault(r => r.Field<string>("AirlineCode") == marketingCarrier) : null;
                        var marketingcarriername =  carrier != null && carrier[1] != null ? carrier[1].ToString() : "";
                        var flightNumber = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "flightOrtrainNumber")?.Value;
                        Segment segment = new Segment();
                        segment.segmentRef = segRef;
                        string dateTimeStr = departureDate + departureTime;
                        string format = "ddMMyyHHmm";
                        DateTime departureD = DateTime.ParseExact(dateTimeStr, format, CultureInfo.InvariantCulture);
                        segment.departure = new Departure { at = departureD, iataCode = departureLocation , terminal = departureTerminal , iataName = depAirportName  };
                        string arrival = arrivalDate + arrivalTime;
                        DateTime arrivalD = DateTime.ParseExact(arrival, format, CultureInfo.InvariantCulture);
                        segment.arrival = new Arrival { at = arrivalD, iataCode = arrivalLocation , terminal = arrivalTerminal ,  iataName=arrAirportName };
                        segment.marketingCarrierCode = marketingCarrier;
                        segment.marketingCarrierName = marketingcarriername;
                        segment.aircraft = new Aircraft { code = flightNumber };
                        segment.duration = FlightDuration;
                        segment.number = FlightNumber;
                        segment.id = FlightProposal != null ? Convert.ToInt16(FlightProposal) : 0;
                        var tempRecommend = doc.Descendants(amadeus + "recommendation").Where(e => e.Element(amadeus + "itemNumber")?.Elements(amadeus + "itemNumberId")?.Elements(amadeus + "number")?.FirstOrDefault().Value == FlightProposal).FirstOrDefault();
                        if(tempRecommend != null)
                        {
                            var paxFareProduct = tempRecommend?.Descendants(amadeus + "paxFareProduct")?.FirstOrDefault();
                            var fareDetails = paxFareProduct?.Descendants(amadeus + "fareDetails").Where(e => e.Elements(amadeus + "segmentRef")?.Elements(amadeus+ "segRef")?.FirstOrDefault().Value == segmentRef.ToString()).FirstOrDefault();
                            var rbd = fareDetails?.Descendants(amadeus + "groupOfFares")?.Descendants(amadeus + "productInformation")?.Descendants(amadeus + "cabinProduct")?.Descendants(amadeus + "rbd")?.FirstOrDefault()?.Value;
                            var cabin = fareDetails?.Descendants(amadeus + "groupOfFares")?.Descendants(amadeus + "productInformation")?.Descendants(amadeus + "cabinProduct")?.Descendants(amadeus + "cabin")?.FirstOrDefault()?.Value;
                            var availStatus = fareDetails?.Descendants(amadeus + "groupOfFares")?.Descendants(amadeus + "productInformation")?.Descendants(amadeus + "cabinProduct")?.Descendants(amadeus + "avlStatus")?.FirstOrDefault()?.Value;
                            var fareBasis = fareDetails?.Descendants(amadeus + "groupOfFares")?.Descendants(amadeus + "productInformation")?.Descendants(amadeus + "fareProductDetail")?.Descendants(amadeus + "fareBasis")?.FirstOrDefault()?.Value;
                            var breakpoint = fareDetails?.Descendants(amadeus + "groupOfFares")?.Descendants(amadeus + "productInformation")?.Descendants(amadeus + "breakPoint")?.FirstOrDefault()?.Value;
                            segment.avlStatus = availStatus;
                            segment.bookingClass = rbd;
                            segment.cabinClass = cabin;
                            segment.fareBasis = fareBasis;
                            segment.breakPoint = breakpoint;
                            segment.rateClass = fareBasis;
                            segment.cabinStatus = availStatus;
                        }
                        DataRow droperatingCarrier = AirlineCache != null ?  AirlineCache.AsEnumerable().FirstOrDefault(r => r.Field<string>("AirlineCode") == operatingCarrier): null;
                        var operatingCarrierName = droperatingCarrier != null ? droperatingCarrier[1].ToString() : "";
                        segment.operating = new Operating { operatingCarrierCode = operatingCarrier, operatingCarrierName = operatingCarrierName };
                        segment.numberOfStops = numberOfStops;
                        itinerary.segments.Add(segment);
                        itinerary.flightProposal_ref = FlightProposal;
                        itinerary.duration = CalculateDuration(timeduration); 
                        itinerary.segment_type = "OutBound";
                        itinerary.airport_city = airport_city;
                      
                    }
                    itinerariesList.Add(itinerary);
                }
            }

            

            var flightIndexInbound = doc.Descendants(amadeus + "flightIndex").Where(f => f.Element(amadeus + "requestedSegmentRef").Element(amadeus + "segRef").Value == "2")
                            .ToList();

            if (flightIndexInbound != null)
            {

                var flightDetails1 = flightIndexInbound.Descendants(amadeus + "groupOfFlights").ToList();

                var segRef = "2";
                string airport_city = string.Empty;
                foreach (var groupOfFlights in flightDetails1)
                {
                    Itinerary itinerary = new Itinerary();
                    itinerary.segments = new List<Segment>();
                    var FlightProposal = groupOfFlights.Element(amadeus + "propFlightGrDetail")?.Element(amadeus + "flightProposal").Element(amadeus + "ref").Value;
                    var numberOfStops = groupOfFlights.Descendants(amadeus + "flightDetails").ToList().Count();
                    numberOfStops = numberOfStops - 1;
                    int segmentRef =2;
                    timeduration = new List<string>();
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
                            timeduration.Add(FlightDuration);
                        }
                        var departureLocation = flightDetails.Element(amadeus + "flightInformation")?.Elements(amadeus + "location")?.FirstOrDefault()?.Element(amadeus + "locationId")?.Value;
                         DataRow depatureAirport = AirportCache != null ? AirportCache.AsEnumerable().FirstOrDefault(r => r.Field<string>("AirportCode") == departureLocation) : null;
                         var depAirportName = depatureAirport != null ? depatureAirport[2]?.ToString() + " , " + depatureAirport[4]?.ToString() : "";
                        airport_city = airport_city == string.Empty ? depatureAirport != null ? depatureAirport[3]?.ToString() : "" : airport_city;

                       var departureTerminal = flightDetails.Element(amadeus + "flightInformation")?
                            .Elements(amadeus + "location")?.FirstOrDefault()?.Element(amadeus + "terminal")?.Value;
                        var arrivalLocation = flightDetails.Element(amadeus + "flightInformation")?
                            .Elements(amadeus + "location")?.Skip(1).FirstOrDefault()?.Element(amadeus + "locationId")?.Value;
                        DataRow arrivalAirport = AirportCache != null ? AirportCache.AsEnumerable().FirstOrDefault(r => r.Field<string>("AirportCode") == arrivalLocation): null;
                        var arrAirportName = arrivalAirport != null ? arrivalAirport[2].ToString() + " , " + arrivalAirport[4].ToString() : "";


                        var arrivalTerminal = flightDetails.Element(amadeus + "flightInformation")?
                            .Elements(amadeus + "location")?.Skip(1).FirstOrDefault()?.Element(amadeus + "terminal")?.Value;

                        var marketingCarrier = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "companyId")?.Element(amadeus + "marketingCarrier")?.Value;
                        var operatingCarrier = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "companyId")?.Element(amadeus + "operatingCarrier")?.Value;
                        DataRow drmarketingcarrier = AirlineCache != null ? AirlineCache.AsEnumerable().FirstOrDefault(r => r.Field<string>("AirlineCode") == marketingCarrier): null;
                        var marketingcarriername = drmarketingcarrier != null ? drmarketingcarrier[1].ToString() : "";


                        var flightNumber = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "flightOrtrainNumber")?.Value;
                        Segment segment = new Segment();
                        string dateTimeStr = departureDate + departureTime;
                        string format = "ddMMyyHHmm";
                        DateTime departureD = DateTime.ParseExact(dateTimeStr, format, CultureInfo.InvariantCulture);
                        segment.departure = new Departure { at = departureD, iataCode = departureLocation, iataName = depAirportName, terminal = departureTerminal };
                        string arrival = arrivalDate + arrivalTime;
                        DateTime arrivalD = DateTime.ParseExact(arrival, format, CultureInfo.InvariantCulture);
                        segment.arrival = new Arrival { at = arrivalD, iataCode = arrivalLocation, iataName = arrAirportName, terminal = arrivalTerminal };
                        segment.marketingCarrierCode = marketingCarrier;
                        segment.marketingCarrierName = marketingcarriername;
                        segment.aircraft = new Aircraft { code = flightNumber };
                        segment.duration = FlightDuration;
                        segment.number = FlightNumber;
                        segment.id = FlightProposal != null ? Convert.ToInt16(FlightProposal) : 0;
                        var tempRecommend = doc.Descendants(amadeus + "recommendation").Where(e => e.Element(amadeus + "itemNumber")?.Elements(amadeus + "itemNumberId")?.Elements(amadeus + "number")?.FirstOrDefault().Value == FlightProposal).FirstOrDefault();
                        if (tempRecommend != null)
                        {
                            var paxFareProduct = tempRecommend?.Descendants(amadeus + "paxFareProduct")?.FirstOrDefault();
                            var fareDetails = paxFareProduct?.Descendants(amadeus + "fareDetails").Where(e => e.Elements(amadeus + "segmentRef")?.Elements(amadeus + "segRef")?.FirstOrDefault().Value == segmentRef.ToString()).FirstOrDefault();
                            var rbd = fareDetails?.Descendants(amadeus + "groupOfFares")?.Descendants(amadeus + "productInformation")?.Descendants(amadeus + "cabinProduct")?.Descendants(amadeus + "rbd")?.FirstOrDefault()?.Value;
                            var cabin = fareDetails?.Descendants(amadeus + "groupOfFares")?.Descendants(amadeus + "productInformation")?.Descendants(amadeus + "cabinProduct")?.Descendants(amadeus + "cabin")?.FirstOrDefault()?.Value;
                            var availStatus = fareDetails?.Descendants(amadeus + "groupOfFares")?.Descendants(amadeus + "productInformation")?.Descendants(amadeus + "cabinProduct")?.Descendants(amadeus + "avlStatus")?.FirstOrDefault()?.Value;
                            var fareBasis = fareDetails?.Descendants(amadeus + "groupOfFares")?.Descendants(amadeus + "productInformation")?.Descendants(amadeus + "fareProductDetail")?.Descendants(amadeus + "fareBasis")?.FirstOrDefault()?.Value;
                            var breakpoint = fareDetails?.Descendants(amadeus + "groupOfFares")?.Descendants(amadeus + "productInformation")?.Descendants(amadeus + "breakPoint")?.FirstOrDefault()?.Value;
                            segment.avlStatus = availStatus;
                            segment.bookingClass = rbd;
                            segment.cabinClass = cabin;
                            segment.fareBasis = fareBasis;
                            segment.breakPoint = breakpoint;
                            segment.rateClass = fareBasis;
                            segment.cabinStatus = availStatus;
                        }
                        DataRow droperatingCarrier = AirlineCache != null ? AirlineCache.AsEnumerable().FirstOrDefault(r => r.Field<string>("AirlineCode") == operatingCarrier) : null;
                        var operatingCarrierName = droperatingCarrier != null ? droperatingCarrier[1].ToString() : "";
                        segment.operating = new Operating { operatingCarrierCode = operatingCarrier, operatingCarrierName = operatingCarrierName };
                        segment.numberOfStops = numberOfStops;                         
                        itinerary.segments.Add(segment);
                        itinerary.flightProposal_ref = FlightProposal;
                        itinerary.duration = CalculateDuration(timeduration); 
                        itinerary.segment_type = "InBound";
                        itinerary.airport_city = airport_city;
                        
                    }

                    itinerariesList.Add(itinerary);


                }


            }
       
            List<BaggageDetails> baggageDetails = new List<BaggageDetails>();
            #region Working For Baggagel Allowence
            try
            {
                var baggageList = doc.Descendants(amadeus + "serviceFeesGrp")?.Descendants(amadeus + "freeBagAllowanceGrp")?.ToList();
               
                foreach (var bitem in baggageList)
                {
                    var itemNumber = bitem.Descendants(amadeus + "itemNumberInfo")?.Descendants(amadeus + "itemNumberDetails")?.Descendants(amadeus + "number")?.FirstOrDefault()?.Value;
                    var freeAllowence = bitem.Descendants(amadeus + "freeBagAllownceInfo")?.Descendants(amadeus + "baggageDetails")?.Descendants(amadeus + "freeAllowance")?.FirstOrDefault()?.Value;
                    var quantityCode = bitem.Descendants(amadeus + "freeBagAllownceInfo")?.Descendants(amadeus + "baggageDetails")?.Descendants(amadeus + "quantityCode")?.FirstOrDefault()?.Value;
                    var unitQualifier = bitem.Descendants(amadeus + "freeBagAllownceInfo")?.Descendants(amadeus + "baggageDetails")?.Descendants(amadeus + "unitQualifier")?.FirstOrDefault()?.Value;
                    baggageDetails.Add(new BaggageDetails { itemNumber = itemNumber, freeAllowance = freeAllowence, quantityCode = quantityCode, unitQualifier = unitQualifier });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while Generate baggage details {ex.Message.ToString()}");
            }
            #endregion

            #region Working For Recemondations
            string id = string.Empty, price = string.Empty, refnumenr = string.Empty, totalFareAmount = string.Empty;
            string totalTax = string.Empty; string transportStageQualifier = string.Empty; string transportStageQualifierCompany = string.Empty;
            string pricingTicketingPriceType = string.Empty;
            string isRefundable = string.Empty;
            string LastTicketDate = string.Empty;
            string cabinClass = string.Empty;
            string bookingClass = string.Empty;
            string avlStatus = string.Empty;
            string farebasis = string.Empty;
            string passengerType = string.Empty;
            string fareType = string.Empty;
            string breakPoint = string.Empty;   
            string companyname = string.Empty;
            string adultpp = string.Empty;
            string adulttax = string.Empty;
            string childpp = string.Empty;
            string childtax = string.Empty;
            string infantpp = string.Empty;
            string infanttax = string.Empty;
            decimal  totalPrice = 0;
            int totalAdult = 0;
            int totalChild = 0;
            int totalInfant = 0;
            var recommendationList = doc.Descendants(amadeus + "recommendation").ToList();
            if (recommendationList != null)
            {
                foreach (var item in recommendationList)
                {
                    FlightOffer offer = new FlightOffer();
                    offer.itineraries = new List<Itinerary>();                   
                    LastTicketDate = string.Empty;                   
                    var adultFare = item.Descendants(amadeus + "paxFareProduct").Where(e => e.Element(amadeus + "paxReference")?.Elements(amadeus + "ptc")?.FirstOrDefault().Value == "ADT").FirstOrDefault();
                    var childFare = item.Descendants(amadeus + "paxFareProduct").Where(e => e.Element(amadeus + "paxReference")?.Elements(amadeus + "ptc")?.FirstOrDefault().Value == "CNN").FirstOrDefault();
                    var infFare = item.Descendants(amadeus + "paxFareProduct").Where(e => e.Element(amadeus + "paxReference")?.Elements(amadeus + "ptc")?.FirstOrDefault().Value == "INF").FirstOrDefault();
                    if (adultFare != null)
                    {
                        totalAdult = adultFare.Descendants(amadeus + "paxReference").Elements(amadeus+"traveller").ToList().Count();
                        adultpp = adultFare?.Descendants(amadeus + "paxFareDetail")?.Elements(amadeus + "totalFareAmount")?.FirstOrDefault().Value;
                        adulttax = adultFare?.Descendants(amadeus + "paxFareDetail")?.Elements(amadeus + "totalTaxAmount")?.FirstOrDefault().Value;
                        decimal _totAdt = (Convert.ToDecimal ( adultpp) + Convert.ToDecimal( adulttax)) * totalAdult;
                        totalPrice = _totAdt;

                    }
                    if (childFare != null)
                    {
                        totalChild = childFare.Descendants(amadeus + "paxReference").Elements(amadeus + "traveller").ToList().Count();
                        childpp = childFare?.Descendants(amadeus + "paxFareDetail")?.Elements(amadeus + "totalFareAmount")?.FirstOrDefault().Value;
                        childtax = childFare?.Descendants(amadeus + "paxFareDetail")?.Elements(amadeus + "totalTaxAmount")?.FirstOrDefault().Value;
                        decimal _totChd = (Convert.ToDecimal(childpp) + Convert.ToDecimal(childtax)) * totalChild;
                        totalPrice = totalPrice + _totChd;
                    }
                    if (infFare != null)
                    {
                        totalInfant = infFare.Descendants(amadeus + "paxReference").Elements(amadeus + "traveller").ToList().Count();
                        infantpp = infFare?.Descendants(amadeus + "paxFareDetail")?.Elements(amadeus + "totalFareAmount")?.FirstOrDefault().Value;
                        infanttax = infFare?.Descendants(amadeus + "paxFareDetail")?.Elements(amadeus + "totalTaxAmount")?.FirstOrDefault().Value;
                        decimal _totInf = (Convert.ToDecimal(infantpp) + Convert.ToDecimal(infanttax)) * totalInfant;
                        totalPrice = totalPrice + _totInf;
                    }

                    var itemNumberId = item.Descendants(amadeus + "itemNumber").Elements(amadeus + "itemNumberId")?.FirstOrDefault()?.Value;
                    price = item.Descendants(amadeus + "recPriceInfo").Elements(amadeus + "monetaryDetail").Elements(amadeus + "amount")?.FirstOrDefault()?.Value;
                    var priceinfo2 = item.Descendants(amadeus + "recPriceInfo").Elements(amadeus + "monetaryDetail").Elements(amadeus + "amount").Skip(1)?.FirstOrDefault()?.Value;
                    totalFareAmount = item.Descendants(amadeus + "paxFareProduct").Elements(amadeus + "paxFareDetail").Elements(amadeus + "totalFareAmount")?.FirstOrDefault()?.Value;
                    totalTax = item.Descendants(amadeus + "paxFareProduct").Elements(amadeus + "paxFareDetail").Elements(amadeus + "totalTaxAmount")?.FirstOrDefault()?.Value;
                    var paxReferece = item.Descendants(amadeus + "paxFareProduct").Elements(amadeus + "paxReference").ToList();
                    var companylist = item.Descendants(amadeus + "paxFareProduct").Elements(amadeus + "paxFareDetail").Elements(amadeus + "codeShareDetails").ToList();
                    companyname = string.Empty;
                    foreach (var company in companylist)
                    {
                        if (companyname == string.Empty)
                        {
                            companyname =  company.Descendants(amadeus + "company")?.FirstOrDefault()?.Value;
                        }
                        else
                        {
                            companyname = companyname + " " + company.Descendants(amadeus + "company")?.FirstOrDefault()?.Value;
                        }
                        
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
                                LastTicketDate = i.Value;
                            }
                        }

                    }
                    cabinClass = string.Empty;bookingClass = string.Empty;avlStatus = string.Empty;farebasis = string.Empty;
                    passengerType = string.Empty;faretype = string.Empty;avlStatus = string.Empty;
                    var fareDetailsGroupOfFare = item.Descendants(amadeus + "fareDetails").Descendants(amadeus + "groupOfFares").ToList();
                   
                    if (fareDetailsGroupOfFare != null)
                    {
                      
                        foreach (var productInfo in fareDetailsGroupOfFare)
                        {
                            // read segment ref
                            var cabinclass = productInfo.Descendants(amadeus + "productInformation").Descendants(amadeus + "cabinProduct")?.Descendants(amadeus + "cabin")?.FirstOrDefault()?.Value;
                            var bookingclass = productInfo.Descendants(amadeus + "productInformation").Descendants(amadeus + "cabinProduct")?.Descendants(amadeus + "rbd")?.FirstOrDefault()?.Value;
                            var availlStatus = productInfo.Descendants(amadeus + "productInformation").Descendants(amadeus + "cabinProduct")?.Descendants(amadeus + "avlStatus")?.FirstOrDefault()?.Value;
                            cabinClass = cabinclass;
                            bookingClass = bookingclass;
                            avlStatus = availlStatus;

                            var fbasis = productInfo.Descendants(amadeus + "productInformation").Descendants(amadeus + "fareProductDetail")?.Descendants(amadeus + "fareBasis")?.FirstOrDefault()?.Value;
                            var pasType = productInfo.Descendants(amadeus + "productInformation").Descendants(amadeus + "fareProductDetail")?.Descendants(amadeus + "passengerType")?.FirstOrDefault()?.Value;
                            var ftype = productInfo.Descendants(amadeus + "productInformation").Descendants(amadeus + "fareProductDetail")?.Descendants(amadeus + "fareType")?.FirstOrDefault()?.Value;
                            var breakpoint = productInfo.Descendants(amadeus + "productInformation").Descendants(amadeus + "breakPoint")?.FirstOrDefault()?.Value;
                            farebasis = fbasis;
                            passengerType = pasType;
                            fareType = ftype;
                            breakPoint = breakpoint;
                        }
                    }
                    var itineary = itinerariesList.Where(e => e.flightProposal_ref == itemNumberId)?.FirstOrDefault();
                    if(itineary != null)
                    {
                        itineary.segments.ForEach(e => e.cabinClass = cabinClass);
                    }

                    var taxes = new List<Taxes>();
                    if (adultpp != string.Empty)
                    {
                        Taxes t = new Taxes { amount = adulttax, code = "ADT" };
                        taxes.Add(t);
                    }
                    if (childpp != string.Empty)
                    {
                        Taxes t = new Taxes { amount = childtax, code = "CNN" };
                        taxes.Add(t);
                    }
                    if (infantpp != string.Empty)
                    {
                        Taxes t = new Taxes { amount = infanttax, code = "INF" };
                        taxes.Add(t);
                    }
                    offer.price = new Price
                    {
                        adultPP = adultpp,
                        adultTax = adulttax ,
                        childPp = childpp,
                        childTax = childtax,
                        infantPp = infantpp,
                        infantTax = infanttax,
                        baseAmount = "",
                        currency = currency,
                        grandTotal = totalPrice.ToString(),
                        taxes = taxes,
                        total = totalPrice.ToString(),
                        discount = 0,
                        billingCurrency = currency,
                        markup = 0
                    };

                    offer.id = itemNumberId;
                    offer.type = "Availability";
                    offer.lastTicketingDate = LastTicketDate;
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
                            var tempPrice = new Price();
                            if(ptc == "ADT")
                            {
                             
                                tempPrice.currency = offer.price.currency;
                                tempPrice.adultPP = offer.price.adultPP;
                                tempPrice.adultTax = offer.price.adultTax;
                                tempPrice.billingCurrency = offer.price.currency;
                                tempPrice.baseAmount = offer.price.baseAmount;
                              
                            }
                            else if (ptc == "CNN")
                            {
                                tempPrice.childPp = offer.price.childPp;
                                tempPrice.childTax = offer.price.childTax;
                                tempPrice.currency = offer.price.currency;
                                tempPrice.baseAmount = offer.price.baseAmount;
                                tempPrice.billingCurrency = offer.price.currency;
                            }
                            else if (ptc == "INF")
                            {
                                tempPrice.infantPp = offer.price.infantPp;
                                tempPrice.infantTax = offer.price.infantTax;
                                tempPrice.currency = offer.price.currency;
                                tempPrice.baseAmount = offer.price.baseAmount;
                                tempPrice.billingCurrency = offer.price.currency;
                            }

                            TravelerPricing tp = new TravelerPricing { travelerType = ptc, travelerId = tr.Element(amadeus + "ref").Value, price = tempPrice};
                            offer.travelerPricings.Add(tp);
                        }
                    }
                    offer.bookingClass = bookingClass;
                    offer.cabinClass = cabinClass;
                    offer.avlStatus = avlStatus;
                    offer.fareBasis = farebasis;
                    offer.passengerType = passengerType;
                    offer.fareType = faretype;
                    offer.breakPoint = breakPoint;                   
                    offer.validatingAirlineCodes = companyname.Split(" ").ToList<string>();
                    #region Get Itineraries from outbound
                    List<Itinerary> _outbounItineraries = new List<Itinerary>(); List<Itinerary> _inbounItineraries = new List<Itinerary>();
                    _outbounItineraries = itinerariesList.Where(e => e.segment_type == "OutBound" && e.flightProposal_ref == itemNumberId).ToList();
                    _inbounItineraries = itinerariesList.Where(e => e.segment_type == "InBound" && e.flightProposal_ref == itemNumberId).ToList();
                    offer.itineraries.AddRange(_outbounItineraries);
                    offer.itineraries.AddRange(_inbounItineraries);
                    #endregion
                    offer.baggageDetails = baggageDetails.Where(e => e.itemNumber == offer.id).FirstOrDefault();
                    if (itineary != null)
                    {
                        ReturnModel.data.Add(offer);
                    }
                        
                }

            }
            #endregion

            ReturnModel.data = ReturnModel?.data?.Where(e => e?.itineraries?.Count > 1).ToList();
            return ReturnModel;
        }
        private string CalculateDuration(List<string> timestring)
        {
            string _duration = timestring[0];
            try
            {
                TimeSpan totalDuration = new TimeSpan();
                foreach(var item in timestring)
                {
                    TimeSpan span1 = TimeSpan.ParseExact(item, "hhmm", null);
                    totalDuration += span1;
                }
                return totalDuration.ToString(@"hh\:mm");
            }
            catch
            {
                return _duration;
            }
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
