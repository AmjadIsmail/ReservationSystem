using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models.FOP;
using ReservationSystem.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using ReservationSystem.Domain.Models.PricePnr;
using ReservationSystem.Domain.Models.AddPnrMulti;
using ReservationSystem.Domain.Models;
using System.Security.Cryptography.Xml;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging.Abstractions;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class PricePnrRepository : IPricePnrRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        private readonly IHelperRepository _helperRepository;
        public PricePnrRepository(IConfiguration _configuration, IMemoryCache cache , IHelperRepository helperRepository)
        {
            configuration = _configuration;
            _cache = cache;
            _helperRepository = helperRepository;
        }
        public async Task<PricePnrResponse> CreatePricePNRWithBookingClass(PricePnrRequest requestModel)
        {
            PricePnrResponse fopResponse = new PricePnrResponse();

            try
            {

               // var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var logsPath = configuration.GetSection("Logspath");
                var _url = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:ApiUrl"]);
                var _action = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:Fare_PricePNRWithBookingClass"]);
                string Result = string.Empty;
                string Envelope = await CreateSoapRequest(requestModel);
                string ns = "http://xml.amadeus.com/TPCBRR_23_2_1A";
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
                            var errorInfo = xmlDoc.Descendants(fareNS + "applicationError").FirstOrDefault();
                            var warningError = new AmadeusResponseError();
                            if (errorInfo != null)
                            {
                                // Extract error details
                                var errorCode = errorInfo.Descendants(fareNS + "errorOrWarningCodeDetails").Descendants(fareNS + "errorDetails").Descendants(fareNS + "errorCode").FirstOrDefault()?.Value;
                                var errorText = xmlDoc?.Descendants(fareNS + "errorWarningDescription")?.Descendants(fareNS + "freeText").FirstOrDefault()?.Value;
                                fopResponse.amadeusError = new AmadeusResponseError();
                                fopResponse.amadeusError.error = errorText;
                                fopResponse.amadeusError.errorCode = Convert.ToInt16(errorCode);
                                warningError = fopResponse.amadeusError;
                                //return fopResponse;

                            }

                            var res = ConvertXmlToModel(xmlDoc, ns);
                            fopResponse = res;
                            if (warningError.error != null)
                            {
                                fopResponse.warningDetails = warningError.error;
                            }


                        }
                    }
                }
                catch (WebException ex)
                {
                    using (StreamReader rd = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        Result = rd.ReadToEnd();
                        fopResponse.amadeusError = new AmadeusResponseError();
                        fopResponse.amadeusError.error = Result;
                        fopResponse.amadeusError.errorCode = 0;
                        return fopResponse;

                    }
                }
            }
            catch (Exception ex)
            {
                fopResponse.amadeusError = new AmadeusResponseError();
                fopResponse.amadeusError.error = ex.Message.ToString();
                fopResponse.amadeusError.errorCode = 0;
                return fopResponse;
            }
            return fopResponse;
        }
        private string GeneratePricingOptionsGroup(string pricingOptions)
        {
            try
            {
                string result = $@"";
                foreach (var option in pricingOptions?.Split(','))
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
        public async Task<string> CreateSoapRequest(PricePnrRequest requestModel)
        {

           // var amadeusSettings = configuration.GetSection("AmadeusSoap") != null ? configuration.GetSection("AmadeusSoap") : null;
            var action = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:Fare_PricePNRWithBookingClass"]);
            string to = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:ApiUrl"]);
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
    <Fare_PricePNRWithBookingClass>
     { GeneratePricingOptionsGroup(requestModel?.pricingOptionKey)}
      <pricingOptionGroup>
        <pricingOptionKey>
          <pricingOptionKey>VC</pricingOptionKey>
        </pricingOptionKey>
        <carrierInformation>
          <companyIdentification>
            <otherCompany>{requestModel?.carrierCode}</otherCompany>
          </companyIdentification>
        </carrierInformation>
      </pricingOptionGroup>
      <pricingOptionGroup>
        <pricingOptionKey>
          <pricingOptionKey>FCO</pricingOptionKey>
        </pricingOptionKey>
        <currency>
          <firstCurrencyDetails>
            <currencyQualifier>FCO</currencyQualifier>
            <currencyIsoCode>GBP</currencyIsoCode>
          </firstCurrencyDetails>
        </currency>
      </pricingOptionGroup>
    </Fare_PricePNRWithBookingClass>
   </soap:Body>
</soap:Envelope>";

            return Request;
        }

        public PricePnrResponse ConvertXmlToModel(XDocument response, string amadeusns)
        {
            PricePnrResponse ReturnModel = new PricePnrResponse();
            PnrResponseDetails detail = new PnrResponseDetails();
            ReturnModel.responseDetails = detail;
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
            detail.FarePricePNRWithBookingClassReply = new FarePricePNRWithBookingClassReply();
            XNamespace amadeus = amadeusns;

            var fareList = doc.Descendants(amadeus + "fareList")?.ToList();
            if (fareList != null)
            {
                detail.FarePricePNRWithBookingClassReply.FareList = new List<FareList>();
                foreach ( var fare in fareList)
                {
                    FareList flst = new FareList();
                    var tstIndicator = fare.Descendants(amadeus + "pricingInformation")?.Descendants(amadeus + "tstInformation")?.Descendants(amadeus + "tstIndicator")?.FirstOrDefault()?.Value;
                    var fcmi = fare.Descendants(amadeus + "pricingInformation")?.Descendants(amadeus + "tstInformation")?.Descendants(amadeus + "fcmi")?.FirstOrDefault()?.Value;
                    if (tstIndicator != null)
                    {
                        try
                        {
                            flst.PricingInformation = new PricingInformation
                            {
                                TstInformation = new TstInformation { TstIndicator = tstIndicator },
                                Fcmi = fcmi != null ? Convert.ToInt16(fcmi) : 0
                            };
                        }
                        catch{}                       
                    }
                    var fareReference = fare.Descendants(amadeus + "fareReference")?.FirstOrDefault();
                    if(fareReference != null)
                    {
                        var referenceType = fareReference.Descendants(amadeus + "referenceType")?.FirstOrDefault()?.Value;
                        var uniqueReference = fareReference.Descendants(amadeus + "uniqueReference")?.FirstOrDefault()?.Value;
                        flst.FareReference = new FareReference
                        {
                            ReferenceType = referenceType,
                            UniqueReference = uniqueReference
                        };
                    }

                    var lastTktDate = fare.Descendants(amadeus + "lastTktDate")?.FirstOrDefault();
                    if (lastTktDate != null)
                    {
                        var businessSemantic = lastTktDate.Descendants(amadeus + "businessSemantic")?.FirstOrDefault()?.Value;
                        var year = lastTktDate.Descendants(amadeus + "dateTime")?.Descendants(amadeus+ "year")?.FirstOrDefault()?.Value;
                        var month = lastTktDate.Descendants(amadeus + "dateTime")?.Descendants(amadeus + "month")?.FirstOrDefault()?.Value;
                        var day = lastTktDate.Descendants(amadeus + "dateTime")?.Descendants(amadeus + "day")?.FirstOrDefault()?.Value;
                        var hour = lastTktDate.Descendants(amadeus + "dateTime")?.Descendants(amadeus + "hour")?.FirstOrDefault()?.Value;
                        var minutes = lastTktDate.Descendants(amadeus + "dateTime")?.Descendants(amadeus + "minutes")?.FirstOrDefault()?.Value;
                        DateTime fulldate = new DateTime(Convert.ToInt16(year), Convert.ToInt16(month), Convert.ToInt16(day), Convert.ToInt16(hour), Convert.ToInt16(minutes), 0);


                        flst.LastTktDate = new LastTicketDate
                        {
                            BusinessSemantic = businessSemantic,
                            DateTimeT = new DateTimeT { day = day, hour = hour, year = year, month = month, minutes = minutes },
                            FullDate = fulldate
                        };
                    }
                    var validatingCarrier = fare.Descendants(amadeus + "validatingCarrier")?.FirstOrDefault();
                    if (validatingCarrier != null)
                    {
                        var carrierCode = validatingCarrier.Descendants(amadeus + "carrierCode")?.FirstOrDefault()?.Value;
                        flst.ValidatingCarrier = new ValidatingCarrier
                        {
                            CarrierInformation = new CarrierInformation
                            {
                                CarrierCode = carrierCode
                            }
                        };
                    }
                    var paxSegReference = fare.Descendants(amadeus + "paxSegReference")?.FirstOrDefault();
                    if (paxSegReference != null)
                    {
                        var refQualifier = paxSegReference.Descendants(amadeus + "refDetails")?.Descendants(amadeus+ "refQualifier")?.FirstOrDefault()?.Value;
                        var refNumber = paxSegReference.Descendants(amadeus + "refDetails")?.Descendants(amadeus + "refNumber")?.FirstOrDefault()?.Value;
                        flst.PaxSegReference = new PaxSegReference
                        {
                            RefDetails = new RefDetails
                            {
                                RefNumber = refNumber,
                                RefQualifier = refQualifier
                            }
                        };
                    }
                    var fareDataInformation = fare.Descendants(amadeus + "fareDataInformation")?.FirstOrDefault();
                    if( fareDataInformation != null)
                    {
                        flst.FareDataInformation = new FareDataInformation();
                        flst.FareDataInformation.FareDataMainInformation = new FareDataMainInformation();
                        var fareDataMainInformation = fareDataInformation.Descendants(amadeus + "fareDataMainInformation")?.FirstOrDefault();
                        var fareDataQualifier = fareDataMainInformation.Descendants(amadeus + "fareDataQualifier")?.FirstOrDefault()?.Value;
                        flst.FareDataInformation.FareDataMainInformation.FareDataQualifier = fareDataQualifier;
                      


                        var fareDataSupInformation = fareDataInformation.Descendants(amadeus + "fareDataSupInformation")?.ToList();
                        flst.FareDataInformation.FareDataSupInformation = new List<FareDataSupInformation>();
                        foreach (var f in fareDataSupInformation)
                        {
                            FareDataSupInformation info = new FareDataSupInformation();
                            var fq = f.Descendants(amadeus + "fareDataQualifier")?.FirstOrDefault()?.Value;
                            var fareAmount = f.Descendants(amadeus + "fareAmount")?.FirstOrDefault()?.Value;
                            var fareCurrency = f.Descendants(amadeus + "fareCurrency")?.FirstOrDefault()?.Value;
                            info.FareDataQualifier = fq;
                            info.FareAmount = Convert.ToDecimal(fareAmount);
                            info.FareCurrency = fareCurrency;
                            flst.FareDataInformation.FareDataSupInformation.Add(info);
                        }
                       
                    }
                    var taxInformation = fare?.Descendants(amadeus + "taxInformation")?.ToList();
                    flst.TaxInformation = new List<TaxInformation>();
                    foreach( var tax in taxInformation)
                    {
                        var taxQualifier = tax?.Descendants(amadeus + "taxDetails")?.Descendants(amadeus + "taxQualifier")?.FirstOrDefault()?.Value;
                        var taxIdentifier = tax?.Descendants(amadeus + "taxDetails")?.Descendants(amadeus + "taxIdentification")?.Descendants(amadeus+ "taxIdentifier")?.FirstOrDefault()?.Value;
                        var isoCountry = tax?.Descendants(amadeus + "taxDetails")?.Descendants(amadeus + "taxType")?.Descendants(amadeus + "isoCountry")?.FirstOrDefault()?.Value;
                        var taxNature = tax?.Descendants(amadeus + "taxDetails")?.Descendants(amadeus + "taxNature")?.FirstOrDefault()?.Value;
                        TaxInformation taxInformation1 = new TaxInformation();
                        taxInformation1.TaxDetails = new TaxDetails
                        {
                            TaxIdentification = new TaxIdentification { TaxIdentifier = taxIdentifier },
                            TaxNature = taxNature,
                            TaxQualifier = taxQualifier,
                            TaxType = new TaxType { IsoCountry = isoCountry }
                        };
                        var amountDetails = tax?.Descendants(amadeus + "amountDetails")?.FirstOrDefault();
                        if (amountDetails != null)
                        {
                            var fareDataQualifier = amountDetails?.Descendants(amadeus + "fareDataMainInformation")?.Descendants(amadeus + "fareDataQualifier")?.FirstOrDefault()?.Value;
                            var fareAmount = amountDetails?.Descendants(amadeus + "fareDataMainInformation")?.Descendants(amadeus + "fareAmount")?.FirstOrDefault()?.Value;
                            var fareCurrency = amountDetails?.Descendants(amadeus + "fareDataMainInformation")?.Descendants(amadeus + "fareCurrency")?.FirstOrDefault()?.Value;
                            taxInformation1.AmountDetails = new AmountDetails
                            {
                                FareDataMainInformation = new FareDataMainInformation { FareDataQualifier = fareDataQualifier, FareAmount = fareAmount, FareCurrency = fareCurrency }

                            };
                        }
                        flst.TaxInformation.Add(taxInformation1);                     
                    }                   
                    var originDestination = fare?.Descendants(amadeus + "originDestination")?.FirstOrDefault();
                    if (originDestination != null)
                    {
                        flst.OriginDestination = new OriginDestination();
                        
                        var cityCode = originDestination?.Descendants(amadeus + "cityCode").ToList();
                        foreach( var c in cityCode)
                        {
                            var ccode = c.Descendants(amadeus + "citycode")?.FirstOrDefault()?.Value;
                            flst.OriginDestination.CityCode += ccode + " ";
                        }
                    }
                    var segmentInformation = fare?.Descendants(amadeus + "segmentInformation")?.ToList();
                    flst.SegmentInformation = new List<SegmentInformation>();
                    foreach(var seg in segmentInformation)
                    {
                        SegmentInformation info = new SegmentInformation();
                        var connexInformation = seg.Descendants(amadeus + "connexInformation")?.FirstOrDefault();
                        var connexType = connexInformation?.Descendants(amadeus + "connecDetails")?.Descendants(amadeus + "connexType")?.FirstOrDefault()?.Value;
                        info.ConnexInformation = new ConnexInformation { ConnecDetails = new ConnecDetails { ConnexType = connexType } };
                        var segDetails = seg?.Descendants(amadeus + "segDetails")?.FirstOrDefault();
                        if(segDetails != null)
                        {
                            var identification = segDetails.Descendants(amadeus + "identification")?.FirstOrDefault()?.Value;
                            var classOfService = segDetails.Descendants(amadeus + "classOfService.")?.FirstOrDefault()?.Value;
                            info.SegDetails = new SegDetails { SegmentDetail = new SegmentDetail { ClassOfService = classOfService, Identification = identification } };

                        }
                        var fareQualifier = seg?.Descendants(amadeus + "fareQualifier")?.FirstOrDefault();
                        if (fareQualifier != null)
                        {
                            var primaryCode = fareQualifier.Descendants(amadeus + "fareBasisDetails")?.Descendants(amadeus+ "primaryCode")?.FirstOrDefault()?.Value ;
                            var fareBasisCode = fareQualifier.Descendants(amadeus + "fareBasisDetails")?.Descendants(amadeus + "fareBasisCode")?.FirstOrDefault()?.Value ;
                            var ticketDesignator = fareQualifier.Descendants(amadeus + "fareBasisDetails")?.Descendants(amadeus + "ticketDesignator")?.FirstOrDefault()?.Value;
                            var discTktDesignator = fareQualifier.Descendants(amadeus + "fareBasisDetails")?.Descendants(amadeus + "discTktDesignator")?.FirstOrDefault()?.Value;
                            info.FareQualifier = new FareQualifier
                            {
                                FareBasisDetails = new FareBasisDetails { DiscTktDesignator = discTktDesignator, FareBasisCode = fareBasisCode, PrimaryCode = primaryCode , TicketDesignator = ticketDesignator }
                            };
                        }

                        var cabinGroup = seg?.Descendants(amadeus + "cabinGroup")?.FirstOrDefault();
                        if(cabinGroup != null)
                        {
                            info.CabinGroup = new CabinGroup();
                            var designator = cabinGroup.Descendants(amadeus + "cabinSegment")?.Descendants(amadeus + "bookingClassDetails")?.Descendants(amadeus + "designator")?.FirstOrDefault()?.Value;
                            var option = cabinGroup.Descendants(amadeus + "cabinSegment")?.Descendants(amadeus + "bookingClassDetails")?.Descendants(amadeus + "option")?.FirstOrDefault()?.Value;
                            info.CabinGroup.CabinSegment = new CabinSegment
                            {
                                BookingClassDetails = new BookingClassDetails { Designator = designator, Option = option }
                            };
                        }

                        var bagAllowanceInformation = seg?.Descendants(amadeus + "bagAllowanceInformation")?.FirstOrDefault();
                        if (bagAllowanceInformation != null)
                        {
                            info.BagAllowanceInformation  = new BagAllowanceInformation();
                            var baggageQuantity = bagAllowanceInformation.Descendants(amadeus + "bagAllowanceDetails")?.Descendants(amadeus + "baggageQuantity")?.FirstOrDefault()?.Value;
                            var baggageType = bagAllowanceInformation.Descendants(amadeus + "bagAllowanceDetails")?.Descendants(amadeus + "baggageType")?.FirstOrDefault()?.Value;
                            info.BagAllowanceInformation.BagAllowanceDetails = new BagAllowanceDetails
                            {
                                BaggageQuantity = Convert.ToInt16(baggageQuantity),
                                BaggageType = baggageType
                            };
                        }
                        var segmentReference = seg?.Descendants(amadeus + "segmentReference")?.FirstOrDefault();
                        if (segmentReference != null)
                        {
                            info.SegmentReference = new SegmentReference();
                            var refQualifier = segmentReference.Descendants(amadeus + "refDetails")?.Descendants(amadeus + "refQualifier")?.FirstOrDefault()?.Value;
                            var refNumber = segmentReference.Descendants(amadeus + "refDetails")?.Descendants(amadeus + "refNumber")?.FirstOrDefault()?.Value;
                            info.SegmentReference.RefDetails = new RefDetails { RefNumber = refNumber, RefQualifier = refQualifier };
                        }
                        var sequenceInformation = seg?.Descendants(amadeus + "sequenceInformation")?.FirstOrDefault();
                        if(sequenceInformation != null)
                        {
                            info.SequenceInformation = new SequenceInformation();
                            var sequenceNumber = sequenceInformation.Descendants(amadeus + "sequenceSection")?.Descendants(amadeus + "sequenceNumber")?.FirstOrDefault()?.Value;
                            info.SequenceInformation.SequenceSection = new SequenceSection { SequenceNumber =  Convert.ToInt16( sequenceNumber) };
                        }
                        var flightProductInformationType = seg?.Descendants(amadeus + "flightProductInformationType")?.FirstOrDefault();
                        if(flightProductInformationType != null)
                        {
                            var rbd = flightProductInformationType?.Descendants(amadeus + "cabinProduct")?.Descendants(amadeus + "rbd")?.FirstOrDefault()?.Value;
                            var cabin = flightProductInformationType?.Descendants(amadeus + "cabinProduct")?.Descendants(amadeus + "cabin")?.FirstOrDefault()?.Value;
                            var avlStatus = flightProductInformationType?.Descendants(amadeus + "cabinProduct")?.Descendants(amadeus + "avlStatus")?.FirstOrDefault()?.Value;
                            info.FlightProductInformationType = new FlightProductInformationType
                            {
                                CabinProduct = new CabinProduct { AvlStatus = avlStatus, Cabin = cabin, Rbd = rbd }
                            };
                        }
                        flst.SegmentInformation.Add(info);
                    }
                    
                    var otherPricingInfo = fare?.Descendants(amadeus + "otherPricingInfo")?.FirstOrDefault();
                    if(otherPricingInfo != null)
                    {
                        var attributeType = otherPricingInfo?.Descendants(amadeus + "attributeDetails")?.Descendants(amadeus + "attributeType")?.FirstOrDefault()?.Value;
                        var attributeDescription = otherPricingInfo?.Descendants(amadeus + "attributeDetails")?.Descendants(amadeus + "attributeDescription0")?.FirstOrDefault()?.Value;
                        flst.OtherPricingInfo = new OtherPricingInfo
                        {
                            AttributeDetails = new AttributeDetails { AttributeDescription = attributeDescription, AttributeType = attributeType }
                        };
                    }

                    var warningInformation = fare?.Descendants(amadeus + "warningInformation")?.ToList();
                    flst.WarningInformation = new List<WarningInformation>();
                    foreach(var war in warningInformation)
                    {
                        WarningInformation warning = new WarningInformation();
                        var applicationErrorCode = war?.Descendants(amadeus + "warningCode")?.Descendants(amadeus + "applicationErrorDetail")?.Descendants(amadeus + "applicationErrorCode")?.FirstOrDefault()?.Value;
                        var codeListQualifier = war?.Descendants(amadeus + "warningCode")?.Descendants(amadeus + "applicationErrorDetail")?.Descendants(amadeus + "codeListQualifier")?.FirstOrDefault()?.Value;
                        var codeListResponsibleAgency = war?.Descendants(amadeus + "warningCode")?.Descendants(amadeus + "applicationErrorDetail")?.Descendants(amadeus + "codeListResponsibleAgency")?.FirstOrDefault()?.Value;
                        var errorFreeText = war?.Descendants(amadeus + "warningText")?.Descendants(amadeus + "errorFreeText")?.FirstOrDefault()?.Value;
                        warning.WarningCode = new WarningCode { ApplicationErrorDetail = new ApplicationErrorDetail { ApplicationErrorCode = applicationErrorCode, CodeListQualifier = codeListQualifier, CodeListResponsibleAgency = codeListResponsibleAgency } };
                        warning.WarningText = new WarningText { ErrorFreeText = errorFreeText };
                        flst.WarningInformation.Add(warning);
                    }
                    var fareComponentDetailsGroup = fare?.Descendants(amadeus + "fareComponentDetailsGroup")?.ToList();
                    flst.FareComponentDetailsGroup = new List<FareComponentDetailsGroup>();
                    foreach (var far in fareComponentDetailsGroup)
                    {
                        FareComponentDetailsGroup fg = new FareComponentDetailsGroup();
                        var fareComponentID = far.Descendants(amadeus + "fareComponentID")?.FirstOrDefault();
                        if(fareComponentID != null)
                        {
                            var farecompNumber = fareComponentID.Descendants(amadeus + "itemNumberDetails")?.Descendants(amadeus + "number")?.FirstOrDefault()?.Value;
                            var type = fareComponentID.Descendants(amadeus + "itemNumberDetails")?.Descendants(amadeus + "type")?.FirstOrDefault()?.Value;
                            fg.FareComponentID = new FareComponentID { ItemNumberDetails = new ItemNumberDetails { Number = farecompNumber, Type = type } };
                        }

                        var marketFareComponent = far.Descendants(amadeus + "marketFareComponent")?.FirstOrDefault();
                        if(marketFareComponent != null)
                        {
                            var fromLoc = marketFareComponent?.Descendants(amadeus + "boardPointDetails")?.Descendants(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                            var toLoc = marketFareComponent?.Descendants(amadeus + "offpointDetails")?.Descendants(amadeus + "trueLocationId")?.FirstOrDefault()?.Value;
                            fg.MarketFareComponent =
                                new MarketFareComponent
                                {
                                    BoardPointDetails = new BoardPointDetails
                                    {
                                        TrueLocationId = fromLoc
                                    },
                                    OffpointDetails = new OffpointDetails
                                    {
                                        TrueLocationId = toLoc
                                    }
                                };
                        }
                        var monetaryInformation = far.Descendants(amadeus + "monetaryInformation")?.FirstOrDefault();
                        if(monetaryInformation != null)
                        {
                            var typeQualifier = monetaryInformation?.Descendants(amadeus + "monetaryDetails")?.Descendants(amadeus + "typeQualifier")?.FirstOrDefault()?.Value;
                            var amount = monetaryInformation?.Descendants(amadeus + "monetaryDetails")?.Descendants(amadeus + "amount")?.FirstOrDefault()?.Value;
                            var currency = monetaryInformation?.Descendants(amadeus + "monetaryDetails")?.Descendants(amadeus + "currency")?.FirstOrDefault()?.Value;
                            fg.MonetaryInformation = new MonetaryInformation
                            {
                                MonetaryDetails = new MonetaryDetails
                                {
                                    Amount =Convert.ToDecimal(amount),
                                    Currency = currency,
                                    TypeQualifier = typeQualifier

                                }
                            };
                        }

                        var componentClassInfo = far.Descendants(amadeus + "componentClassInfo")?.FirstOrDefault();
                        if (componentClassInfo != null)
                        {
                            var rateTariffClass = componentClassInfo?.Descendants(amadeus + "fareBasisDetails")?.Descendants(amadeus + "rateTariffClass")?.FirstOrDefault()?.Value;
                            var otherRateTariffClass = monetaryInformation?.Descendants(amadeus + "fareBasisDetails")?.Descendants(amadeus + "otherRateTariffClass")?.FirstOrDefault()?.Value;
                            fg.ComponentClassInfo = new ComponentClassInfo
                            {
                                FareBasisDetails = new SegmentFareBasisDetails
                                {
                                    RateTariffClass = rateTariffClass, OtherRateTariffClass = otherRateTariffClass 
                                },
                            };
                        }

                        var fareQualifiersDetail = far.Descendants(amadeus + "fareQualifiersDetail")?.FirstOrDefault();
                        if (fareQualifiersDetail != null)
                        {
                            var fareQualifier = fareQualifiersDetail?.Descendants(amadeus + "discountDetails")?.Descendants(amadeus + "fareQualifier")?.FirstOrDefault()?.Value;
                            fg.FareQualifiersDetail = new FareQualifiersDetail
                            {
                                DiscountDetails = new DiscountDetails { FareQualifier = fareQualifier }
                            };
                        }
                        var fareFamilyDetails = far.Descendants(amadeus + "fareFamilyDetails")?.FirstOrDefault();
                        if (fareFamilyDetails != null)
                        {
                            var fareFamilyname = fareFamilyDetails?.Descendants(amadeus + "fareFamilyname")?.FirstOrDefault()?.Value;
                            var hierarchy = fareFamilyDetails?.Descendants(amadeus + "hierarchy")?.FirstOrDefault()?.Value;
                            fg.FareFamilyDetails = new FareFamilyDetails
                            {
                                FareFamilyName = fareFamilyname,
                                Hierarchy = hierarchy != null ? Convert.ToInt16(hierarchy) : 0
                            };
                        }
                        var couponDetailsGroup = far.Descendants(amadeus + "couponDetailsGroup")?.FirstOrDefault();
                        if(couponDetailsGroup != null)
                        {
                            var productId_type = couponDetailsGroup?.Descendants(amadeus + "productId")?.Descendants(amadeus + "referenceDetails")?.Descendants(amadeus + "type")?.FirstOrDefault()?.Value;
                            var productId_value = couponDetailsGroup?.Descendants(amadeus + "productId")?.Descendants(amadeus + "referenceDetails")?.Descendants(amadeus + "value")?.FirstOrDefault()?.Value;
                            fg.CouponDetailsGroup = new CouponDetailsGroup
                            {
                                ProductId = new ProductId { ReferenceDetails = new ReferenceDetails { Type = productId_type, Value = productId_value } }
                            };
                        }

                        flst.FareComponentDetailsGroup.Add(fg);
                    }

                    detail.FarePricePNRWithBookingClassReply.FareList.Add(flst);
                }              
            }            
            return ReturnModel;
        }
    }
}
