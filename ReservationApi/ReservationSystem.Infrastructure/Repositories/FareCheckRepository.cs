using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReservationSystem.Domain.Models.Soap.FlightPrice;
using ReservationSystem.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using ReservationSystem.Domain.Models.Availability;
using System.Security.Cryptography;
using ReservationSystem.Domain.Models.FareCheck;
using System.Xml.Serialization;
using ReservationSystem.Domain.Models;
using System.Globalization;
using System.Runtime.Intrinsics.X86;
using ReservationSystem.Domain.Migrations;
using ReservationSystem.Domain.Models.DBLogs;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class FareCheckRepository : IFareCheckRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        private readonly IHelperRepository _helperRepository;
        private readonly IDBRepository _dbRepository;
        public FareCheckRepository(IConfiguration _configuration, IMemoryCache cache, IHelperRepository helperRepository,IDBRepository dBRepository)
        {
            configuration = _configuration;
            _cache = cache;
            _helperRepository = helperRepository;
            _dbRepository = dBRepository;
        }
        public async Task<FareCheckReturnModel> FareCheckRequest(FareCheckModel fareCheckRequest)
        {
            FareCheckReturnModel fareCheck = new FareCheckReturnModel();
            try
            {

                //var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var _url = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:ApiUrl"]);
                var _action = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:Fare_CheckRules"]);
                string Result = string.Empty;
                string Envelope = await CreateSoapEnvelope(fareCheckRequest);
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
                            await _helperRepository.SaveXmlResponse("FareCheck_Request", Envelope);
                            await _helperRepository.SaveXmlResponse("FareCheck_Response", result2);                            
                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(result2);
                            string jsonText = JsonConvert.SerializeXmlNode(xmlDoc2, Newtonsoft.Json.Formatting.Indented);
                            await _helperRepository.SaveJson(jsonText, "FareCheckResponseJson");
                            XNamespace fareNS = "http://xml.amadeus.com/FARQNR_07_1_1A";
                            SaveReservationLog saveReservationLog = new SaveReservationLog();
                            saveReservationLog.Request = Envelope;
                            saveReservationLog.Response = jsonText;
                            saveReservationLog.RequestName = RequestName.FareCheck.ToString();
                            saveReservationLog.UserId = 0;
                            var errorInfo = xmlDoc.Descendants(fareNS + "errorInfo").FirstOrDefault();
                            if (errorInfo != null)
                            {

                                var errorCode = errorInfo.Descendants(fareNS + "rejectErrorCode").Descendants(fareNS + "errorDetails").Descendants(fareNS + "errorCode").FirstOrDefault()?.Value;
                                var errorText = errorInfo.Descendants(fareNS + "errorFreeText").Descendants(fareNS + "freeText").FirstOrDefault()?.Value;
                                fareCheck.amadeusError = new AmadeusResponseError();
                                fareCheck.amadeusError.error = errorText;
                                fareCheck.amadeusError.errorCode = Convert.ToInt16(errorCode);

                                #region DB Logs
                                try
                                {
                                    saveReservationLog.IsError = true;
                                    saveReservationLog.AmadeusSessionId = fareCheckRequest?.sessionDetails?.SessionId;
                                    await _dbRepository.SaveReservationFlow(saveReservationLog);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error while save db logs {ex.Message.ToString()}");
                                }
                                #endregion
                                return fareCheck;

                            }
                            else
                            {

                                var res = ConvertXmlToModel(xmlDoc, fareNS.NamespaceName);

                                fareCheck.data = res;

                                #region DB Logs
                                try
                                {
                                    saveReservationLog.IsError = false;
                                    saveReservationLog.AmadeusSessionId = res?.session?.SessionId;
                                    await _dbRepository.SaveReservationFlow(saveReservationLog);

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error while save db logs {ex.Message.ToString()}");
                                }
                                #endregion
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
                        return fareCheck;

                    }
                }
            }
            catch (Exception ex)
            {
                fareCheck.amadeusError = new AmadeusResponseError();
                fareCheck.amadeusError.error = ex.Message.ToString();
                fareCheck.amadeusError.errorCode = 0;
                return fareCheck;
            }
            return fareCheck;
        }

        public FareCheckRulesReply ConvertXmlToModel(XDocument response, string xmlNameSpace)
        {

            FareCheckRulesReply fareCheck = new FareCheckRulesReply();
            XDocument doc = response;
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace amadeus = xmlNameSpace;

            List<Itinerary> itinerariesList = new List<Itinerary>();
            var messageFunction = doc.Descendants(amadeus + "transactionType")?.Descendants(amadeus + "messageFunctionDetails")?.Descendants(amadeus + "messageFunction")?.FirstOrDefault()?.Value;
            fareCheck.transactionType = new TransactionType();
            fareCheck.transactionType.messageFunctionDetails = new MessageFunctionDetails();
            fareCheck.transactionType.messageFunctionDetails.messageFunction = messageFunction.ToString();
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
                fareCheck.session = new HeaderSession
                {
                    SecurityToken = securityToken,
                    SequenceNumber = SeqNumber,
                    SessionId = sessionId,
                    TransactionStatusCode = TransactionStatusCode
                };
            }
            var flightDetails = doc.Descendants(amadeus + "flightDetails")?.ToList();
            fareCheck.flightDetails = new List<FlightDetailsFareCheck>();
            foreach (var item in flightDetails)
            {
                var nbOfSegments = item.Descendants(amadeus + "nbOfSegments")?.FirstOrDefault()?.Value;
                FlightDetailsFareCheck _flightfare = new FlightDetailsFareCheck();
                _flightfare.nbOfSegments = nbOfSegments;
                var rateClass = item.Descendants(amadeus + "qualificationFareDetails").Descendants(amadeus + "additionalFareDetails")?.Descendants(amadeus + "rateClass")?.FirstOrDefault().Value;
                _flightfare.qualificationFareDetails = new QualificationFareDetails { additionalFareDetails = new AdditionalFareDetails { rateClass = rateClass } };
                var marketingCompany = item.Descendants(amadeus + "transportService")?.Descendants(amadeus + "companyIdentification")?.Descendants(amadeus + "marketingCompany")?.FirstOrDefault()?.Value;
                _flightfare.transportService = new TransportService { companyIdentification = new CompanyIdentification { marketingCompany = marketingCompany } };
                _flightfare.flightErrorCodes = new List<FlightErrorCode>();
                var flightErrorCode = item.Descendants(amadeus + "flightErrorCode")?.ToList();
                foreach (var itemError in flightErrorCode)
                {
                    var textSubjectQualifier = item.Descendants(amadeus + "freeTextQualification")?.Elements(amadeus + "textSubjectQualifier")?.FirstOrDefault()?.Value;
                    var informationType = item.Descendants(amadeus + "freeTextQualification")?.Elements(amadeus + "informationType")?.FirstOrDefault()?.Value;
                    var freeText = item.Descendants(amadeus + "freeText")?.FirstOrDefault()?.Value;
                    FlightErrorCode code = new FlightErrorCode { freeText = freeText, informationType = informationType, textSubjectQualifier = textSubjectQualifier };
                    _flightfare.flightErrorCodes.Add(code);
                }

                var numberOfUnit = item.Descendants(amadeus + "fareDetailInfo")?.Descendants(amadeus + "nbOfUnits")?.Descendants(amadeus + "quantityDetails")?.Descendants(amadeus + "numberOfUnit")?.FirstOrDefault()?.Value;

                var unitQualifier = item.Descendants(amadeus + "fareDetailInfo")?.Descendants(amadeus + "nbOfUnits")?.Descendants(amadeus + "quantityDetails")?.Descendants(amadeus + "unitQualifier")?.FirstOrDefault()?.Value;
                _flightfare.fareDetailInfo = new FareDetailInfo();
                _flightfare.fareDetailInfo.nbOfUnits = new NbOfUnits();
                _flightfare.fareDetailInfo.nbOfUnits.quantityDetails = new List<QuantityDetails>();
                _flightfare.fareDetailInfo.nbOfUnits.quantityDetails.Add(new QuantityDetails { numberOfUnit = numberOfUnit, unitQualifier = unitQualifier });

                var pricingGroup = item.Descendants(amadeus + "fareDetailInfo")?.Descendants(amadeus + "fareDeatilInfo")?.Descendants(amadeus + "fareTypeGrouping")?.Descendants(amadeus + "pricingGroup")?.FirstOrDefault()?.Value;

                var origin = item.Descendants(amadeus + "odiGrp")?.Descendants(amadeus + "originDestination")?.Descendants(amadeus + "origin")?.FirstOrDefault()?.Value;
                var destination = item.Descendants(amadeus + "odiGrp")?.Descendants(amadeus + "originDestination")?.Descendants(amadeus + "destination")?.FirstOrDefault()?.Value;
                _flightfare.odiGrp = new OdiGrp();
                _flightfare.odiGrp.originDestination = new OriginDestination { destination = destination, origin = origin };

                var travellerGrp_type = item.Descendants(amadeus + "travellerGrp")?.Descendants(amadeus + "travellerIdentRef")?.Descendants(amadeus + "referenceDetails")?.Descendants(amadeus + "type")?.FirstOrDefault()?.Value;
                var travellerGrp_value = item.Descendants(amadeus + "travellerGrp")?.Descendants(amadeus + "travellerIdentRef")?.Descendants(amadeus + "referenceDetails")?.Descendants(amadeus + "value")?.FirstOrDefault()?.Value;

                var fareRulesDetails = item.Descendants(amadeus + "travellerGrp")?.Descendants(amadeus + "fareRulesDetails")?.ToList();
                List<string> ruleSectionIds = new List<string>();

                foreach (var Ruleitem in fareRulesDetails)
                {
                    var Ruleid = Ruleitem.Element(amadeus + "ruleSectionId")?.Value;
                    ruleSectionIds.Add(Ruleid);
                }
                _flightfare.travellerGrp = new TravellerGrp();
                _flightfare.travellerGrp.travellerIdentRef = new TravellerIdentRef { referenceDetails = new ReferenceDetails { type = travellerGrp_type, value = travellerGrp_value, fareRulesDetails = ruleSectionIds } };

                var itemGrp_itemNb_itemNumberDetails_number = item.Descendants(amadeus + "itemGrp")?.Descendants(amadeus + "itemNb")?.Descendants(amadeus + "itemNumberDetails")?.Descendants(amadeus + "number")?.FirstOrDefault()?.Value;
                _flightfare.itemGrp = new ItemGrp();
                _flightfare.itemGrp.itemNb = new ItemNb { itemNumberDetails = new ItemNumberDetails { number = itemGrp_itemNb_itemNumberDetails_number } };

                var quantityDetails = item.Descendants(amadeus + "itemGrp")?.Descendants(amadeus + "unitGrp")?.Descendants(amadeus + "nbOfUnits")?.Descendants(amadeus + "quantityDetails")?.ToList();

                List<QuantityDetails> _quantitydetals = new List<QuantityDetails>();
                foreach (var itemq in quantityDetails)
                {
                    var unit = item.Descendants(amadeus + "numberOfUnit")?.FirstOrDefault()?.Value;
                    var qfy = item.Descendants(amadeus + "unitQualifier")?.FirstOrDefault()?.Value;
                    QuantityDetails det = new QuantityDetails { numberOfUnit = unit, unitQualifier = qfy };
                    _quantitydetals.Add(det);
                }
                _flightfare.itemGrp.unitGrp = new UnitGrp();
                _flightfare.itemGrp.unitGrp.nbOfUnits = new NbOfUnits();
                _flightfare.itemGrp.unitGrp.nbOfUnits.quantityDetails = new List<QuantityDetails>();
                _flightfare.itemGrp.unitGrp.nbOfUnits.quantityDetails.AddRange(_quantitydetals);

                var farepricingGroup = item.Descendants(amadeus + "itemGrp")?.Descendants(amadeus + "unitGrp")?.Descendants(amadeus + "unitFareDetails")?.Descendants(amadeus + "fareTypeGrouping")?.Descendants(amadeus + "pricingGroup")?.FirstOrDefault()?.Value;
                _flightfare.itemGrp.unitGrp.unitFareDetails = new UnitFareDetails();
                _flightfare.itemGrp.unitGrp.unitFareDetails.fareTypeGrouping = new FareTypeGrouping { pricingGroup = farepricingGroup };
                fareCheck.flightDetails.Add(_flightfare);
            }
            return fareCheck;
        }
               
        public async Task<string> CreateSoapEnvelope(FareCheckModel requestModel)
        {
            string pwdDigest = await _helperRepository.generatePassword();
           // var amadeusSettings = configuration.GetSection("AmadeusSoap");
            string action = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:Fare_CheckRules"]);
            string to = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:ApiUrl"]);
            string username = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:webUserId"]);
            string dutyCode = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:dutyCode"]);
            string requesterType = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:requestorType"]);
            string PseudoCityCode = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:PseudoCityCode"]?.ToString());
            string pos_type = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:POS_Type"]);

            //string Request = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:fare=""{action}"" xmlns:sec=""http://xml.amadeus.com/2010/06/Security_v1"" xmlns:typ=""http://xml.amadeus.com/2010/06/Types_v1"" xmlns:iat=""http://www.iata.org/IATA/2007/00/IATA2010.1"" xmlns:app=""http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3"" xmlns:link=""http://wsdl.amadeus.com/2010/06/ws/Link_v1"" xmlns:ses=""http://xml.amadeus.com/2010/06/Session_v3"">
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
     <Fare_CheckRules> 
    {messageFunctionDetails(requestModel?.typeQualifier?.ToList())}
     {itemNumber(requestModel?.itemNumber?.ToList(), requestModel?.FcType)}        
      </Fare_CheckRules>        
   </soap:Body>
</soap:Envelope>";

            return Request;
        }
        private string messageFunctionDetails(List<string> msgFunctionList)
        {
            try
            {
                string result = $@"";
                result += " <msgType>";
                for (int i = 0; i < msgFunctionList.Count; i++)
                {
                    result +=
                        "<messageFunctionDetails>" +
                        "<messageFunction>" + msgFunctionList[i] + "</messageFunction>" +
                        "</messageFunctionDetails>";
                }
                result += "</msgType>";
                return result;

            }
            catch (Exception ex)
            {
                Console.Write("Error while generate messageFunctionDetails " + ex.Message.ToString());
                return "";
            }
        }

        private string itemNumber(List<int> itemNumber, string FcType)
        {
            try
            {
                string result = $@"";
                result += " <itemNumber>";
                for (int i = 0; i < itemNumber.Count; i++)
                {
                    result +=
                        "<itemNumberDetails>" +
                        "<number>" + itemNumber[i] + "</number>" +
                        "</itemNumberDetails>" +
                        "<itemNumberDetails>" +
                        "<number>" + itemNumber[i] + "</number>" +
                       " <type>" + FcType + "</type>" +
                         "</itemNumberDetails>";
                }
                result += "</itemNumber>";
                return result;

            }
            catch (Exception ex)
            {
                Console.Write("Error while generate messageFunctionDetails " + ex.Message.ToString());
                return "";
            }
        }
       
        public async Task<string> CreateSingout_Request()
        {
            string pwdDigest = await _helperRepository.generatePassword();
           // var amadeusSettings = configuration.GetSection("AmadeusSoap");
            string action = Environment.GetEnvironmentVariable(configuration["Fare_CheckRules"]);
            string to = Environment.GetEnvironmentVariable(configuration["ApiUrl"]);
            string username = Environment.GetEnvironmentVariable(configuration["webUserId"]);
            string dutyCode = Environment.GetEnvironmentVariable(configuration["dutyCode"]);
            string requesterType = Environment.GetEnvironmentVariable(configuration["requestorType"]);
            string PseudoCityCode = Environment.GetEnvironmentVariable(configuration["PseudoCityCode"]?.ToString());
            string pos_type = Environment.GetEnvironmentVariable(configuration["POS_Type"]);

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
     <Security_SignOut></Security_SignOut>   
   </soapenv:Body>
</soapenv:Envelope>";

            return Request;
        }

        public async Task Security_Signout(HeaderSession header)
        {
            FareCheckReturnModel fareCheck = new FareCheckReturnModel();
            try
            {

                //var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var _url = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:ApiUrl"]);
                var _action = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:Security_SignOut"]);
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
            //string pwdDigest = await generatePassword();
           // var amadeusSettings = configuration.GetSection("AmadeusSoap");
            string action = Environment.GetEnvironmentVariable(configuration["Security_SignOut"]);
            string to = Environment.GetEnvironmentVariable(configuration["ApiUrl"]);                        
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