using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReservationSystem.Domain.Models.AirSellFromRecommendation;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using ReservationSystem.Domain.Models.AddPnrMulti;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;
using ReservationSystem.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Metadata;
using ReservationSystem.Domain.Models.FareCheck;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class AddPnrMultiRepository : IAddPnrMultiRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        private readonly IHelperRepository _helperRepository;
        public AddPnrMultiRepository(IConfiguration _configuration, IMemoryCache cache,IHelperRepository helperRepository)
        {
            configuration = _configuration;
            _cache = cache;
            _helperRepository = helperRepository;
        }
        public async Task<PnrCommitResponse?> CommitPNR(PnrCommitRequest requestModel)
        {
            PnrCommitResponse pnrCommit = new PnrCommitResponse();
            try
            {

                var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var _url = amadeusSettings["ApiUrl"]; 
                var _action = amadeusSettings["PNR_AddMultiElements"];
                string Result = string.Empty;
                string Envelope = await CreatePnrCommitRequest(requestModel);
                string ns = "http://xml.amadeus.com/PNRACC_21_1_1A";
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
                            await _helperRepository.SaveXmlResponse("CommitPNR_Request", Envelope);
                            await _helperRepository.SaveXmlResponse("CommitPNR_Response", result2);   
                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(result2);
                            string jsonText = JsonConvert.SerializeXmlNode(xmlDoc2, Newtonsoft.Json.Formatting.Indented);
                            await _helperRepository.SaveJson(jsonText, "CommitPNRResponseJson");
                            XNamespace fareNS = ns;
                            var errorInfo = xmlDoc.Descendants(fareNS + "generalErrorInfo").FirstOrDefault();
                            if (errorInfo != null)
                            {                                
                                var errorCode = errorInfo.Descendants(fareNS + "errorOrWarningCodeDetails").Descendants(fareNS + "errorDetails").Descendants(fareNS + "errorCode").FirstOrDefault()?.Value;
                                var errorTextList = errorInfo.Descendants(fareNS + "errorWarningDescription").Descendants(fareNS + "freeText")?.ToList();
                                string errorText = string.Empty;
                                foreach(var error in errorTextList)
                                {
                                    errorText += error + " ";
                                }
                                pnrCommit.amadeusError = new AmadeusResponseError();
                                pnrCommit.amadeusError.error = errorText;
                                pnrCommit.amadeusError.errorCode = Convert.ToInt16(errorCode);
                                return pnrCommit;

                            }

                           var res = ConvertXmlToModelCommitPnr(xmlDoc, ns);
                            pnrCommit = res;

                        }
                    }
                }
                catch (WebException ex)
                {
                    using (StreamReader rd = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        Result = rd.ReadToEnd();
                        pnrCommit.amadeusError = new AmadeusResponseError();
                        pnrCommit.amadeusError.error = Result;
                        pnrCommit.amadeusError.errorCode = 0;
                        return pnrCommit;

                    }
                }
            }
            catch (Exception ex)
            {
                pnrCommit.amadeusError = new AmadeusResponseError();
                pnrCommit.amadeusError.error = ex.Message.ToString();
                pnrCommit.amadeusError.errorCode = 0;
                return pnrCommit;
            }
            return pnrCommit;
        }
        public async Task<AddPnrMultiResponse> AddPnrMulti(AddPnrMultiRequset requestModel)
        {
            AddPnrMultiResponse AirSell = new AddPnrMultiResponse();

            try
            {

                var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var _url = amadeusSettings["ApiUrl"]; // "https://nodeD2.test.webservices.amadeus.com/1ASIWJIBJAY";
                var _action = amadeusSettings["PNR_AddMultiElements"];
                string Result = string.Empty;
                string Envelope = await CreateAddPnrMultiRequest(requestModel);
                string  ns = "http://xml.amadeus.com/PNRACC_21_1_1A";
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
                            await _helperRepository.SaveXmlResponse("PNRMulti_Request", Envelope);
                            await _helperRepository.SaveXmlResponse("PNRMulti_Response", result2); 
                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(result2);
                            string jsonText = JsonConvert.SerializeXmlNode(xmlDoc2, Newtonsoft.Json.Formatting.Indented);
                            await _helperRepository.SaveJson(jsonText, "PNRMultiResponseJson");
                            XNamespace fareNS = ns;
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
                            
                           var res = ConvertXmlToModel(xmlDoc, ns);
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
        public AddPnrMultiResponse ConvertXmlToModel(XDocument response,string amadeusns)
        {
            AddPnrMultiResponse ReturnModel = new AddPnrMultiResponse();
            ReturnModel.addPnrMultiDetails = new AddPnrMultiDetails();
            XDocument doc = response;

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
                ReturnModel.session = new HeaderSession
                {
                    SecurityToken = securityToken,
                    SequenceNumber = SeqNumber,
                    SessionId = sessionId,
                    TransactionStatusCode = TransactionStatusCode
                };
            }
            AddPnrMultiDetails addPnrMulti = new AddPnrMultiDetails();
            XNamespace amadeus = amadeusns;

            var securityInformation = doc.Descendants(amadeus + "securityInformation")?.FirstOrDefault();
            if(securityInformation != null)
            {
                var typeOfPnrElement = securityInformation.Descendants(amadeus + "responsibilityInformation")?.Descendants(amadeus + "typeOfPnrElement")?.FirstOrDefault()?.Value;
                var officeId = securityInformation.Descendants(amadeus + "responsibilityInformation")?.Descendants(amadeus + "officeId")?.FirstOrDefault()?.Value;
                var  iataCode = securityInformation.Descendants(amadeus + "responsibilityInformation")?.Descendants(amadeus + "iataCode")?.FirstOrDefault()?.Value;
               var cityCode = securityInformation.Descendants(amadeus + "cityCode")?.FirstOrDefault()?.Value;
                addPnrMulti.securityInformation = new SecurityInformation();
                addPnrMulti.securityInformation.typeOfPnr = typeOfPnrElement;
                addPnrMulti.securityInformation.OfficeId = officeId;
                addPnrMulti.securityInformation.iataCode = iataCode;
                addPnrMulti.securityInformation.cityCode = cityCode;
            }
            var pnrHeader = doc.Descendants(amadeus + "pnrHeaderTag")?.FirstOrDefault();
            if (pnrHeader != null)
            {
                var indicator = pnrHeader.Descendants(amadeus + "statusInformation")?.Descendants(amadeus + "indicator").FirstOrDefault()?.Value;
                addPnrMulti.pnrHeader = new PnrHeaderTag { indicator = indicator };
            }
            var sbrUpdatorPosDetails = doc.Descendants(amadeus + "sbrUpdatorPosDetails")?.FirstOrDefault();
            if(sbrUpdatorPosDetails != null)
            {
                var originatorId = sbrUpdatorPosDetails.Descendants(amadeus + "sbrUserIdentificationOwn")?.Descendants(amadeus + "originIdentification")?.Descendants(amadeus + "originatorId")?.FirstOrDefault()?.Value;
                var inHouseIdentification1 = sbrUpdatorPosDetails.Descendants(amadeus + "sbrUserIdentificationOwn")?.Descendants(amadeus + "originIdentification")?.Descendants(amadeus + "inHouseIdentification1")?.FirstOrDefault()?.Value;
                var originatorTypeCode = sbrUpdatorPosDetails.Descendants(amadeus + "sbrUserIdentificationOwn")?.Descendants(amadeus + "originatorTypeCode")?.FirstOrDefault()?.Value;
                var companyId = sbrUpdatorPosDetails.Descendants(amadeus + "sbrSystemDetails")?.Descendants(amadeus + "deliveringSystem")?.Descendants(amadeus + "companyId")?.FirstOrDefault()?.Value;
                var locationId = sbrUpdatorPosDetails.Descendants(amadeus + "sbrSystemDetails")?.Descendants(amadeus + "deliveringSystem")?.Descendants(amadeus + "locationId")?.FirstOrDefault()?.Value;
                var codedCountry = sbrUpdatorPosDetails.Descendants(amadeus + "sbrPreferences")?.Descendants(amadeus + "userPreferences")?.Descendants(amadeus + "codedCountry")?.FirstOrDefault()?.Value;
                addPnrMulti.SbrUpdatorPosDetails = new SbrUpdatorPosDetails
                {
                 inHouseIdentification1 = inHouseIdentification1,
                 originatorId= originatorId,
                 originatorTypeCode = originatorTypeCode,
                 sbrPreferences = new SbrPreferences {  codedCountry = codedCountry},
                 sbrSystemDetails = new SbrSystemDetails { companyId = companyId , locationId = locationId}
                };
            }
            var travellerInfo = doc.Descendants(amadeus + "travellerInfo")?.ToList();
            if(travellerInfo != null)
            {
                addPnrMulti.travellerInfo = new List<TravellerInfo>();
                foreach (var info in travellerInfo)
                {
                    var elementManagementPassenger = info.Descendants(amadeus + "elementManagementPassenger")?.FirstOrDefault();
                    var segmentName = elementManagementPassenger?.Descendants(amadeus + "segmentName")?.FirstOrDefault()?.Value;
                    var lineNumber = elementManagementPassenger?.Descendants(amadeus + "lineNumber")?.FirstOrDefault().Value;
                    var refQuilifier = elementManagementPassenger?.Descendants(amadeus + "reference")?.Descendants(amadeus + "qualifier")?.FirstOrDefault()?.Value;
                    var refNumber = elementManagementPassenger?.Descendants(amadeus + "reference")?.Descendants(amadeus + "number")?.FirstOrDefault()?.Value;
                    TravellerInfo tinfo = new TravellerInfo();
                    tinfo.passengerElement = new ElementManagementPassenger
                    {
                        lineNumber = lineNumber,
                        reference = new Reference
                        {
                            number = refNumber,
                            qualifier = refQuilifier
                        },
                        segmentName = segmentName
                    };
                    var passengerData = info.Descendants(amadeus + "passengerData")?.FirstOrDefault();
                    if(passengerData != null)
                    {
                        var travellerSurname = passengerData.Descendants(amadeus + "travellerInformation")?.Descendants(amadeus+ "traveller")?.Descendants(amadeus+ "surname")?. FirstOrDefault()?.Value;
                        var travellerquantity = passengerData.Descendants(amadeus + "travellerInformation")?.Descendants(amadeus + "traveller")?.Descendants(amadeus + "quantity")?.FirstOrDefault()?.Value;
                        var passengerFirstName = passengerData.Descendants(amadeus + "travellerInformation")?.Descendants(amadeus + "passenger")?.Descendants(amadeus + "firstName")?.FirstOrDefault()?.Value;
                        var passengertype = passengerData.Descendants(amadeus + "travellerInformation")?.Descendants(amadeus + "passenger")?.Descendants(amadeus + "type")?.FirstOrDefault()?.Value;
                        var infantIndicator = passengerData.Descendants(amadeus + "travellerInformation")?.Descendants(amadeus + "passenger")?.Descendants(amadeus + "infantIndicator")?.FirstOrDefault()?.Value;
                        var passengerDob = passengerData.Descendants(amadeus + "dateOfBirth")?.Descendants(amadeus + "dateAndTimeDetails")?.Descendants(amadeus + "date")?.FirstOrDefault()?.Value;

                        tinfo.passengerData = new PassengerData
                        {
                            passenger = new Passenger
                            {
                                infantIndicator = infantIndicator,
                                firstName = passengerFirstName,
                                type = passengertype,
                                dob = passengerDob
                            },
                            traveler = new Traveller
                            {
                                surname = travellerSurname,
                                quantity = travellerquantity
                            }

                        };
                    }
                    var enhancedPassengerData = info.Descendants(amadeus + "enhancedPassengerData")?.ToList();
                    if(enhancedPassengerData != null)
                    {
                        tinfo.enhancedPassengerData = new List<EnhancedPassengerData>();
                        foreach (var item in enhancedPassengerData)
                        {
                            EnhancedPassengerData enhancedPassengerData1 = new EnhancedPassengerData();
                            var travelerInfoname_quantity = item?.Descendants(amadeus + "enhancedTravellerInformation")?.Descendants(amadeus + "travellerNameInfo")?.Descendants(amadeus + "quantity")?.FirstOrDefault()?.Value;
                            var travelerInfoname_type = item?.Descendants(amadeus + "enhancedTravellerInformation")?.Descendants(amadeus + "travellerNameInfo")?.Descendants(amadeus + "type")?.FirstOrDefault()?.Value;
                            var infantIndicator = item?.Descendants(amadeus + "enhancedTravellerInformation")?.Descendants(amadeus + "travellerNameInfo")?.Descendants(amadeus + "infantIndicator")?.FirstOrDefault()?.Value;
                            enhancedPassengerData1.enhancedTravellerInformation = new EnhancedTravellerInformation();
                            enhancedPassengerData1.enhancedTravellerInformation.travellerNameInfo = new TravellerNameInfo
                            {
                                 quantity = travelerInfoname_quantity,
                                  type =travelerInfoname_type ,
                                  infantIndicator = infantIndicator
                                  
                            };
                          var otherPaxNamesDetails =item?.Descendants(amadeus + "enhancedTravellerInformation")?.Descendants(amadeus + "otherPaxNamesDetails")?.FirstOrDefault();
                          if (otherPaxNamesDetails != null)
                            {
                                var nameType = otherPaxNamesDetails?.Descendants(amadeus + "nameType")?.FirstOrDefault()?.Value;
                                var referenceName = otherPaxNamesDetails?.Descendants(amadeus + "referenceName")?.FirstOrDefault()?.Value;
                                var displayedName = otherPaxNamesDetails?.Descendants(amadeus + "displayedName")?.FirstOrDefault()?.Value;
                                var surname = otherPaxNamesDetails?.Descendants(amadeus + "surname")?.FirstOrDefault()?.Value;
                                var givenName = otherPaxNamesDetails?.Descendants(amadeus + "givenName")?.FirstOrDefault()?.Value;
                                enhancedPassengerData1.enhancedTravellerInformation.otherPaxNamesDetails = new OtherPaxNamesDetails
                                {
                                    displayedName = displayedName,
                                    givenName = givenName,
                                    nameType = nameType,
                                    referenceName = referenceName,
                                    surname = surname
                                };
                            }

                            var dateOfBirthInEnhancedPaxData = item?.Descendants(amadeus + "dateOfBirthInEnhancedPaxData")?.FirstOrDefault();
                            if (dateOfBirthInEnhancedPaxData != null)
                            {
                                var date = dateOfBirthInEnhancedPaxData?.Descendants(amadeus + "dateAndTimeDetails")?.Descendants(amadeus+ "date")?.FirstOrDefault()?.Value;
                                var qualifier = dateOfBirthInEnhancedPaxData?.Descendants(amadeus + "dateAndTimeDetails")?.Descendants(amadeus + "qualifier")?.FirstOrDefault()?.Value;
                                enhancedPassengerData1.enhancedTravellerInformation.dateOfBirthInEnhancedPaxData = new DateOfBirthInEnhancedPaxData
                                {
                                    date = date,
                                    qualifier = qualifier
                                };
                            }
                            tinfo.enhancedPassengerData.Add(enhancedPassengerData1);
                        }

                    }
                    addPnrMulti.travellerInfo.Add(tinfo);
                }
            }
            addPnrMulti.destinationDetails = new OriginDestinationDetails();
            addPnrMulti.destinationDetails.itineraryInfos = new List<ItineraryInfo>();

            var itineraryInfo = doc.Descendants(amadeus + "originDestinationDetails")?.Descendants(amadeus+ "itineraryInfo").ToList();
            foreach( var itiinfo in itineraryInfo)
            {
                ItineraryInfo info = new ItineraryInfo();
                var elementManagementItinerary = itiinfo.Descendants(amadeus + "elementManagementItinerary")?.FirstOrDefault();
                var segmentName = elementManagementItinerary?.Descendants(amadeus + "segmentName")?.FirstOrDefault()?.Value;
                var lineNumber = elementManagementItinerary?.Descendants(amadeus + "lineNumber")?.FirstOrDefault().Value;
                var refQuilifier = elementManagementItinerary?.Descendants(amadeus + "reference")?.Descendants(amadeus + "qualifier")?.FirstOrDefault()?.Value;
                var refNumber = elementManagementItinerary?.Descendants(amadeus + "reference")?.Descendants(amadeus + "number")?.FirstOrDefault()?.Value;
                info.elementManagementItinerary = new ElementManagementItinerary
                {
                    lineNumber = lineNumber,
                    Reference = new Reference
                    {
                        number = lineNumber,
                        qualifier = refQuilifier
                    },
                    segmentName = segmentName
                };
                info.travelProduct = new TravelProduct();
                var depDate = itiinfo.Descendants(amadeus + "travelProduct")?.Descendants(amadeus + "product")?.Descendants(amadeus + "depDate")?.FirstOrDefault()?.Value;
                var depTime = itiinfo.Descendants(amadeus + "travelProduct")?.Descendants(amadeus + "product")?.Descendants(amadeus + "depTime")?.FirstOrDefault()?.Value;
                var arrDate = itiinfo.Descendants(amadeus + "travelProduct")?.Descendants(amadeus + "product")?.Descendants(amadeus + "arrDate")?.FirstOrDefault()?.Value;
                var arrTime = itiinfo.Descendants(amadeus + "travelProduct")?.Descendants(amadeus + "product")?.Descendants(amadeus + "arrTime")?.FirstOrDefault()?.Value;
                var dayChangeIndicator = itiinfo.Descendants(amadeus + "travelProduct")?.Descendants(amadeus + "product")?.Descendants(amadeus + "dayChangeIndicator")?.FirstOrDefault()?.Value;
                var fromCity = itiinfo.Descendants(amadeus + "travelProduct")?.Descendants(amadeus + "boardpointDetail")?.Descendants(amadeus + "cityCode")?.FirstOrDefault()?.Value;
                var toCity = itiinfo.Descendants(amadeus + "travelProduct")?.Descendants(amadeus + "offpointDetail")?.Descendants(amadeus + "cityCode")?.FirstOrDefault()?.Value;
                var company = itiinfo.Descendants(amadeus + "travelProduct")?.Descendants(amadeus + "companyDetail")?.Descendants(amadeus + "identification")?.FirstOrDefault()?.Value;
                var flightNumber = itiinfo.Descendants(amadeus + "travelProduct")?.Descendants(amadeus + "productDetails")?.Descendants(amadeus + "identification")?.FirstOrDefault()?.Value;
                var cabinClass = itiinfo.Descendants(amadeus + "travelProduct")?.Descendants(amadeus + "productDetails")?.Descendants(amadeus + "classOfService")?.FirstOrDefault()?.Value;
                var typeDetails = itiinfo.Descendants(amadeus + "travelProduct")?.Descendants(amadeus + "typeDetail")?.Descendants(amadeus + "detail")?.FirstOrDefault()?.Value;
                info.travelProduct.depDate = depDate;
                info.travelProduct.depTime = depTime;
                info.travelProduct.arrDate = arrDate;
                info.travelProduct.arrTime = arrTime;
                info.travelProduct.dayChangeIndicator = dayChangeIndicator;
                info.travelProduct.fromCity = fromCity;
                info.travelProduct.toCity = toCity;
                info.travelProduct.company = company;
                info.travelProduct.flightNumber = flightNumber;
                info.travelProduct.cabinClass = cabinClass;
                info.travelProduct.typeDetails = typeDetails;

                var flightDetail = itiinfo.Descendants(amadeus + "flightDetail")?.FirstOrDefault();
                var equipment = itiinfo.Descendants(amadeus + "productDetails")?.Descendants(amadeus+ "equipment")?.FirstOrDefault()?.Value;
                var numOfStops = itiinfo.Descendants(amadeus + "productDetails")?.Descendants(amadeus + "numOfStops")?.FirstOrDefault()?.Value;
                var duration = itiinfo.Descendants(amadeus + "productDetails")?.Descendants(amadeus + "duration")?.FirstOrDefault()?.Value;
                var weekDay = itiinfo.Descendants(amadeus + "productDetails")?.Descendants(amadeus + "weekDay")?.FirstOrDefault()?.Value;
                var departTerminal = itiinfo.Descendants(amadeus + "departureInformation")?.Descendants(amadeus + "departTerminal")?.FirstOrDefault()?.Value;
                var  arrivalTerminal = itiinfo.Descendants(amadeus + "arrivalStationInfo")?.Descendants(amadeus + "terminal")?.FirstOrDefault()?.Value;
                var flightLegMileage = itiinfo.Descendants(amadeus + "mileageTimeDetails")?.Descendants(amadeus + "flightLegMileage")?.FirstOrDefault()?.Value;
                FlightDetails details = new FlightDetails
                {
                    arrivalTerminal = arrivalTerminal,
                    departTerminal = departTerminal,
                    duration = duration,
                    equipment = equipment,
                    flightLegMileage = flightLegMileage,
                    numOfStops = numOfStops,
                    weekDay = weekDay
                };
                info.flightDetails = details;
                var classDesignator = itiinfo.Descendants(amadeus + "cabinDetails")?.Descendants(amadeus + "cabinDetails")?.Descendants(amadeus+ "classDesignator")?.FirstOrDefault()?.Value;
                info.cabinDetails = new CabinDetails { classDesignator = classDesignator };
                var section = itiinfo.Descendants(amadeus + "selectionDetails")?.Descendants(amadeus + "selection")?.Descendants(amadeus + "option")?.FirstOrDefault()?.Value;
                info.sectiondetails = new Sectiondetails { section = section };
                var departureDate = itiinfo.Descendants(amadeus + "legInfo")?.Descendants(amadeus + "legTravelProduct")?.Descendants(amadeus + "flightDate")?.Descendants(amadeus+ "departureDate")?.FirstOrDefault()?.Value;
                var departureTime = itiinfo.Descendants(amadeus + "legInfo")?.Descendants(amadeus + "legTravelProduct")?.Descendants(amadeus + "flightDate")?.Descendants(amadeus + "departureTime")?.FirstOrDefault()?.Value;
                var arrivalDate = itiinfo.Descendants(amadeus + "legInfo")?.Descendants(amadeus + "legTravelProduct")?.Descendants(amadeus + "flightDate")?.Descendants(amadeus + "arrivalDate")?.FirstOrDefault()?.Value;
                var arrivalTime = itiinfo.Descendants(amadeus + "legInfo")?.Descendants(amadeus + "legTravelProduct")?.Descendants(amadeus + "flightDate")?.Descendants(amadeus + "arrivalTime")?.FirstOrDefault()?.Value;
                var fromAirport = itiinfo.Descendants(amadeus + "legInfo")?.Descendants(amadeus + "legTravelProduct")?.Descendants(amadeus + "boardPointDetails")?.Descendants(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                var toAirport = itiinfo.Descendants(amadeus + "legInfo")?.Descendants(amadeus + "legTravelProduct")?.Descendants(amadeus + "offpointDetails")?.Descendants(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                LegInfo linfo = new LegInfo
                {
                    departureDate = departureDate,
                    departureTime = departureTime,
                    arrivalDate = arrivalDate,
                    arrivalTime = arrivalTime,
                    fromAirport = fromAirport,
                    toAirport = toAirport
                };
                info.leginfo = linfo;               
                addPnrMulti.destinationDetails.itineraryInfos.Add(info);

            }
                    
            var dataElementsIndiv = doc.Descendants(amadeus + "dataElementsMaster")?.Descendants(amadeus + "dataElementsIndiv")?.ToList();
            addPnrMulti.elementsMaster = dataElementsIndiv;
            ReturnModel.addPnrMultiDetails = addPnrMulti;
            return ReturnModel;
        }
        public async Task<string> CreateAddPnrMultiRequest(AddPnrMultiRequset requestModel)
        {
           
            var amadeusSettings = configuration.GetSection("AmadeusSoap") != null ? configuration.GetSection("AmadeusSoap") : null;
            string action = amadeusSettings["PNR_AddMultiElements"];
            string to = amadeusSettings["ApiUrl"];
            string ApMobile = amadeusSettings["APMobile"];
            string ApPhone = amadeusSettings["APOfficePhone"];
            string ApEmail = amadeusSettings["APEmail"];
            var LeadPassenger = requestModel.passengerDetails.Where(e => e.isLeadPassenger == true).FirstOrDefault();
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
      <PNR_AddMultiElements>
        <pnrActions>
         <optionCode>0</optionCode>
        </pnrActions>
          {GetTravellerInfo(requestModel)}
        <dataElementsMaster>
    <marker1 />
    <dataElementsIndiv>
          <elementManagementData>
            <segmentName>AP</segmentName>
          </elementManagementData>
          <freetextData>
            <freetextDetail>
              <subjectQualifier>3</subjectQualifier>
              <type>7</type>
            </freetextDetail>
            <longFreetext>"+ApMobile+@"</longFreetext>
          </freetextData>
        </dataElementsIndiv>
  <dataElementsIndiv>
      <elementManagementData>
        <segmentName>AP</segmentName>
      </elementManagementData>
      <freetextData>
        <freetextDetail>
          <subjectQualifier>3</subjectQualifier>
          <type>P21</type>
        </freetextDetail>
        <longFreetext>"+ApPhone+@"</longFreetext>
      </freetextData>
    </dataElementsIndiv>
     <dataElementsIndiv>
          <elementManagementData>
            <segmentName>AP</segmentName>
          </elementManagementData>
          <freetextData>
            <freetextDetail>
              <subjectQualifier>3</subjectQualifier>
              <type>P02</type>
            </freetextDetail>
            <longFreetext>"+ApEmail+@"</longFreetext>
          </freetextData>
     </dataElementsIndiv>
     <dataElementsIndiv>
          <elementManagementData>
            <segmentName>SSR</segmentName>
          </elementManagementData>
          <serviceRequest>
            <ssr>
              <type>CTCE</type>
              <status>HK</status>
              <companyId>YY</companyId>
              <freetext>"+LeadPassenger?.email+@"</freetext>
            </ssr>
          </serviceRequest>
     </dataElementsIndiv>
     <dataElementsIndiv>
          <elementManagementData>
            <segmentName>SSR</segmentName>
          </elementManagementData>
          <serviceRequest>
            <ssr>
              <type>CTCM</type>
              <status>HK</status>
              <companyId>YY</companyId>
              <freetext>"+LeadPassenger.phone+@"/AR</freetext>
            </ssr>
          </serviceRequest>
     </dataElementsIndiv>
     <dataElementsIndiv>
          <elementManagementData>
            <reference>
              <qualifier>OT</qualifier>
              <number>1</number>
            </reference>
            <segmentName>FM</segmentName>
          </elementManagementData>
          <commission>
            <commissionInfo>
              <percentage>1</percentage>
            </commissionInfo>
          </commission>
     </dataElementsIndiv>
    
<dataElementsIndiv>
      <elementManagementData>
        <segmentName>TK</segmentName>
      </elementManagementData>
      <ticketElement>
        <ticket>
          <indicator>OK</indicator>
        </ticket>
      </ticketElement>
    </dataElementsIndiv>
    <dataElementsIndiv>
      <elementManagementData>
        <segmentName>RF</segmentName>
      </elementManagementData>
      <freetextData>
        <freetextDetail>
          <subjectQualifier>3</subjectQualifier>
          <type>P22</type>
        </freetextDetail>
        <longFreetext>WEB</longFreetext>
      </freetextData>
    </dataElementsIndiv>
  </dataElementsMaster>
      </PNR_AddMultiElements>
   </soap:Body>
</soap:Envelope>";

            return Request;
        }
        private string GetTravellerInfo(AddPnrMultiRequset requestModel)
        {
            try
            {
                int infantCount = 0, AdtCount = 0, ChildCount = 0;
                infantCount = requestModel.passengerDetails.Where(e => e.type == "INF").ToList().Count();
                AdtCount = requestModel.passengerDetails.Where(e => e.type == "ADT").ToList().Count();
                ChildCount = requestModel.passengerDetails.Where(e => e.type == "CHD").ToList().Count();
                string travllerInfo =  $@"";
                requestModel.passengerDetails = requestModel.passengerDetails.OrderBy(e => e.isLeadPassenger).OrderBy(e => e.number).ToList();
                foreach ( var passenger in requestModel.passengerDetails.Where(e=>e.type == "ADT").ToList())
                {
                    if(passenger.isLeadPassenger == true && infantCount > 0)
                    {
                        var infant1 = requestModel.passengerDetails.OrderBy(e => e.number).Where(e => e.type == "INF").FirstOrDefault();
                        travllerInfo += "<travellerInfo>\r\n " +
                            "<elementManagementPassenger>\r\n " +
                            "<reference>\r\n" +
                            "<qualifier>PR</qualifier>\r\n " +
                            "<number>"+passenger.number+"</number>\r\n " +
                            "</reference>\r\n " +
                            "<segmentName>NM</segmentName>\r\n " +
                            "</elementManagementPassenger>\r\n" +
                            "<passengerData>\r\n" +
                            "<travellerInformation>\r\n " +
                            "<traveller>\r\n  " +
                            "<surname>"+passenger.surName+"</surname>\r\n " +
                            "<quantity>2</quantity>\r\n " +
                            "</traveller>\r\n " +
                            "<passenger>\r\n  " +
                            "<firstName>"+passenger.firstName+"</firstName>\r\n  " +
                            "<type>"+passenger.type+"</type>\r\n          " +
                            "<infantIndicator>"+(AdtCount +1)+"</infantIndicator>\r\n      " +
                            "</passenger>\r\n      " +
                            "</travellerInformation>\r\n   " +
                            "<dateOfBirth>\r\n      " +
                            "<dateAndTimeDetails>\r\n      " +
                            "<date>" + ConvertDob(passenger?.dob) + "</date>\r\n    " +
                            "</dateAndTimeDetails>\r\n   " +
                            "</dateOfBirth>\r\n  " +
                            "</passengerData>\r\n   " +
                            "<passengerData>\r\n " +
                            "<travellerInformation>\r\n    " +
                            "<traveller>\r\n     " +
                            "<surname>"+infant1?.surName+"</surname>\r\n    " +
                            "</traveller>\r\n      " +
                            "<passenger>\r\n       " +
                            "<firstName>"+infant1?.firstName+"</firstName>\r\n " +
                            "<type>INF</type>\r\n   " +
                            "</passenger>\r\n   " +
                            "</travellerInformation>\r\n  " +
                            "<dateOfBirth>\r\n      " +
                            "<dateAndTimeDetails>\r\n      " +
                            "<date>"+ConvertDob(infant1?.dob)+"</date>\r\n    " +
                            "</dateAndTimeDetails>\r\n   " +
                            "</dateOfBirth>\r\n  " +
                            "</passengerData>\r\n   " +
                            "</travellerInfo>\r\n";
                    }
                    else
                    {
                        int qty = 1;
                        string infData = @""; 
                        if(infantCount > 1) // for two infants
                        {
                            qty = 2;
                            var infant2 = requestModel.passengerDetails.OrderBy(e => e.number).Where(e => e.type == "INF").Skip(1).FirstOrDefault();
                            infData = @"<passengerData>\r\n " +
                            "<travellerInformation>\r\n    " +
                            "<traveller>\r\n     " +
                            "<surname>"+infant2?.surName+"</surname>\r\n    " +
                            "</traveller>\r\n" +
                            "<passenger>\r\n" +
                            "<firstName>"+infant2?.firstName+"</firstName>\r\n " +
                            "<type>INF</type>\r\n   " +
                            "</passenger>\r\n   " +
                            "</travellerInformation>\r\n  " +
                            "<dateOfBirth>\r\n      " +
                            "<dateAndTimeDetails>\r\n      " +
                            "<date>"+ConvertDob(infant2?.dob)+"</date>\r\n    " +
                            "</dateAndTimeDetails>\r\n   " +
                            "</dateOfBirth>\r\n" +
                            "</passengerData>\r\n";
                        }
                        travllerInfo += "<travellerInfo>\r\n " +
                           "<elementManagementPassenger>\r\n " +
                           "<reference>\r\n" +
                           "<qualifier>PR</qualifier>\r\n " +
                           "<number>" + passenger.number + "</number>\r\n " +
                           "</reference>\r\n " +
                           "<segmentName>NM</segmentName>\r\n " +
                           "</elementManagementPassenger>\r\n" +
                           "<passengerData>\r\n" +
                           "<travellerInformation>\r\n " +
                           "<traveller>\r\n  " +
                           "<surname>" + passenger.surName + "</surname>\r\n " +
                           "<quantity>"+qty+"</quantity>\r\n " +
                           "</traveller>\r\n " +
                           "<passenger>\r\n  " +
                           "<firstName>" + passenger.firstName + "</firstName>\r\n  " +
                           "<type>" + passenger.type + "</type>\r\n          " +                          
                           "</passenger>\r\n      " +
                           "</travellerInformation>\r\n   " +
                            "<dateOfBirth>\r\n      " +
                            "<dateAndTimeDetails>\r\n      " +
                            "<date>" + ConvertDob(passenger?.dob) + "</date>\r\n    " +
                            "</dateAndTimeDetails>\r\n   " +
                            "</dateOfBirth>\r\n  " +
                           "</passengerData>\r\n   " +
                           infData +
                           "</travellerInfo>\r\n";
                    }
                   
                }
                if (ChildCount > 0)
                {
                    foreach (var passenger in requestModel.passengerDetails.Where(e => e.type == "CHD").ToList())
                    {
                        travllerInfo += "<travellerInfo>\r\n " +
                           "<elementManagementPassenger>\r\n " +
                           "<reference>\r\n" +
                           "<qualifier>PR</qualifier>\r\n " +
                           "<number>" + passenger.number + "</number>\r\n " +
                           "</reference>\r\n " +
                           "<segmentName>NM</segmentName>\r\n " +
                           "</elementManagementPassenger>\r\n" +
                           "<passengerData>\r\n" +
                           "<travellerInformation>\r\n " +
                           "<traveller>\r\n  " +
                           "<surname>" + passenger.surName + "</surname>\r\n " +
                           "<quantity>1</quantity>\r\n " +
                           "</traveller>\r\n " +
                           "<passenger>\r\n  " +
                           "<firstName>" + passenger.firstName + "</firstName>\r\n  " +
                           "<type>" + passenger.type + "</type>\r\n          " +
                           "</passenger>\r\n      " +
                           "</travellerInformation>\r\n   " +
                           "<dateOfBirth>\r\n      " +
                            "<dateAndTimeDetails>\r\n      " +
                            "<date>" + ConvertDob(passenger?.dob) + "</date>\r\n    " +
                            "</dateAndTimeDetails>\r\n   " +
                            "</dateOfBirth>\r\n" +
                           "</passengerData>\r\n   " +                           
                           "</travellerInfo>\r\n";
                    }
                }


                return travllerInfo;
            }
            catch
            {
                return "";
            }
        }
        private string ConvertDob(string dob)
        {
            try
            {
                DateTime date = DateTime.ParseExact(dob, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                return date.ToString("ddMMMyy").ToUpper();
            }
            catch
            {
                return dob;
            }
        }
        public async Task<string> CreatePnrCommitRequest(PnrCommitRequest requestModel)
        {

            var amadeusSettings = configuration.GetSection("AmadeusSoap") != null ? configuration.GetSection("AmadeusSoap") : null;
            string action = amadeusSettings["PNR_AddMultiElements"];
            string to = amadeusSettings["ApiUrl"];
           string Request = $@"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:ses=""http://xml.amadeus.com/2010/06/Session_v3"">
      <soap:Header xmlns:add=""http://www.w3.org/2005/08/addressing"">
      <ses:Session TransactionStatusCode=""{requestModel.sessionDetails.TransactionStatusCode}"">
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
     <PNR_AddMultiElements>
          <pnrActions>
            <optionCode>{requestModel.optionCode1}</optionCode>
            <optionCode>{requestModel.optionCode2}</optionCode>
          </pnrActions>
    </PNR_AddMultiElements>
   </soap:Body>
</soap:Envelope>";

            return Request;
        }

        public PnrCommitResponse ConvertXmlToModelCommitPnr(XDocument response, string amadeusns)
        {
            PnrCommitResponse ReturnModel = new PnrCommitResponse();
           
            XDocument doc = response;

            XNamespace awsse = "http://xml.amadeus.com/2010/06/Session_v3";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";
            XNamespace amadeus = amadeusns;
            var sessionElement = doc.Descendants(awsse + "Session").FirstOrDefault();
            if (sessionElement != null)
            {
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
            ReturnModel.PNRHeader = new PNRHeader();
            var pnrHeader = doc?.Descendants(amadeus + "pnrHeader")?.FirstOrDefault();
            if (pnrHeader != null)
            {
                var companyId = pnrHeader?.Descendants(amadeus + "reservationInfo")?.Descendants(amadeus + "reservation")?.Descendants(amadeus + "companyId")?.FirstOrDefault()?.Value;
                var controlNumber = pnrHeader?.Descendants(amadeus + "reservationInfo")?.Descendants(amadeus + "reservation")?.Descendants(amadeus + "controlNumber")?.FirstOrDefault()?.Value;
                ReturnModel.PNRHeader.Reservation = new Reservation { companyId = companyId, controlNumber = controlNumber, PNR = controlNumber };
            }
            var securityInformation = doc?.Descendants(amadeus + "securityInformation")?.FirstOrDefault();
            if(securityInformation != null)
            {
                var typeOfPnrElement = doc?.Descendants(amadeus + "responsibilityInformation")?.Descendants(amadeus + "typeOfPnrElement")?.FirstOrDefault()?.Value;
                ReturnModel.PNRHeader.pnrSecurityInformation = new PnrSecurityInformation
                {
                    responsibilityInformation = new ResponsibilityInformation
                    {
                        typeOfPnrElement = typeOfPnrElement
                    }
                };
            }
            var sbrPOSDetails = doc?.Descendants(amadeus + "sbrPOSDetails")?.FirstOrDefault();
            if(sbrPOSDetails != null)
            {
                ReturnModel.PNRHeader.sbrPOSDetails = sbrPOSDetails;
            }
            var sbrCreationPosDetails = doc?.Descendants(amadeus + "sbrCreationPosDetails")?.FirstOrDefault();
            if (sbrCreationPosDetails != null)
            {
                ReturnModel.PNRHeader.sbrCreationPosDetails = sbrCreationPosDetails;
            }
            var sbrUpdatorPosDetails = doc?.Descendants(amadeus + "sbrUpdatorPosDetails")?.FirstOrDefault();
            if (sbrUpdatorPosDetails != null)
            {
                ReturnModel.PNRHeader.sbrCreationPosDetails = sbrUpdatorPosDetails;
            }
            var originDestinationDetails = doc?.Descendants(amadeus + "originDestinationDetails")?.FirstOrDefault();
            if (originDestinationDetails != null)
            {
                ReturnModel.PNRHeader.sbrCreationPosDetails = originDestinationDetails;
            }


            return ReturnModel;
        }

        public async Task Security_Signout(HeaderSession header)
        {
            FareCheckReturnModel fareCheck = new FareCheckReturnModel();
            try
            {

                var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var _url = amadeusSettings["ApiUrl"];
                var _action = amadeusSettings["Security_SignOut"];
                string Result = string.Empty;
                string Envelope = await Signout_Request(header);
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
                            await _helperRepository.SaveXmlResponse("securitySignout", result2);
                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(result2);
                            string jsonText = JsonConvert.SerializeXmlNode(xmlDoc2, Newtonsoft.Json.Formatting.Indented);
                            await _helperRepository.SaveJson(jsonText, "securitySignoutJson");
                            XNamespace fareNS = "http://xml.amadeus.com/FARQNR_07_1_1A";
                            var errorInfo = xmlDoc.Descendants(fareNS + "errorInfo").FirstOrDefault();
                            if (errorInfo != null)
                            {
                                var errorCode = errorInfo.Descendants(fareNS + "rejectErrorCode").Descendants(fareNS + "errorDetails").Descendants(fareNS + "errorCode").FirstOrDefault()?.Value;
                                var errorText = errorInfo.Descendants(fareNS + "errorFreeText").Descendants(fareNS + "freeText").FirstOrDefault()?.Value;
                                fareCheck.amadeusError = new AmadeusResponseError();
                                fareCheck.amadeusError.error = errorText;
                                fareCheck.amadeusError.errorCode = Convert.ToInt16(errorCode);

                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    using (StreamReader rd = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        Result = rd.ReadToEnd();
                        fareCheck.amadeusError = new AmadeusResponseError();
                        fareCheck.amadeusError.error = Result;
                        fareCheck.amadeusError.errorCode = 0;

                    }
                }
            }
            catch (Exception ex)
            {
                fareCheck.amadeusError = new AmadeusResponseError();
                fareCheck.amadeusError.error = ex.Message.ToString();
                fareCheck.amadeusError.errorCode = 0;
                // return fareCheck;
            }
            // return fareCheck;
        }
        public async Task<string> Signout_Request(HeaderSession requestModel)
        {
           
            var amadeusSettings = configuration.GetSection("AmadeusSoap");
            string action = amadeusSettings["Security_SignOut"];
            string to = amadeusSettings["ApiUrl"];
            string Request = $@"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:ses=""http://xml.amadeus.com/2010/06/Session_v3"">
   <soap:Header xmlns:add=""http://www.w3.org/2005/08/addressing"">
      <ses:Session TransactionStatusCode=""End"">
      <ses:SessionId>{requestModel.SessionId}</ses:SessionId>
      <ses:SequenceNumber>{requestModel.SequenceNumber + 1}</ses:SequenceNumber>
      <ses:SecurityToken>{requestModel.SecurityToken}</ses:SecurityToken>
    </ses:Session>
    <add:MessageID>{System.Guid.NewGuid()}</add:MessageID>
    <add:Action>{action}</add:Action>
    <add:To>{to}</add:To>  
    <link:TransactionFlowLink xmlns:link=""http://wsdl.amadeus.com/2010/06/ws/Link_v1""/>
   </soap:Header>
   <soap:Body>
     <Security_SignOut></Security_SignOut>  
   </soap:Body>
</soap:Envelope>";

            return Request;
        }
       
    }
}
