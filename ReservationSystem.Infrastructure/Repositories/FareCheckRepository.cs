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

namespace ReservationSystem.Infrastructure.Repositories
{
    public class FareCheckRepository : IFareCheckRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        private readonly IHelperRepository _helperRepository;
        public FareCheckRepository(IConfiguration _configuration, IMemoryCache cache, IHelperRepository helperRepository)
        {
            configuration = _configuration;
            _cache = cache;
            _helperRepository = helperRepository;
        }
        public async Task<FareCheckReturnModel> FareCheckRequest(FareCheckModel fareCheckRequest)
        {
            FareCheckReturnModel fareCheck = new FareCheckReturnModel();
            try
            {

                var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var _url = amadeusSettings["ApiUrl"];
                var _action = amadeusSettings["Fare_CheckRules"];
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
                            await _helperRepository.SaveXmlResponse("FareCheckResponse", result2);                            
                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(result2);
                            string jsonText = JsonConvert.SerializeXmlNode(xmlDoc2, Newtonsoft.Json.Formatting.Indented);
                            await _helperRepository.SaveJson(jsonText, "FareCheckResponseJson");
                            XNamespace fareNS = "http://xml.amadeus.com/FARQNR_07_1_1A";
                            var errorInfo = xmlDoc.Descendants(fareNS + "errorInfo").FirstOrDefault();
                            if (errorInfo != null)
                            {

                                var errorCode = errorInfo.Descendants(fareNS + "rejectErrorCode").Descendants(fareNS + "errorDetails").Descendants(fareNS + "errorCode").FirstOrDefault()?.Value;
                                var errorText = errorInfo.Descendants(fareNS + "errorFreeText").Descendants(fareNS + "freeText").FirstOrDefault()?.Value;
                                fareCheck.amadeusError = new AmadeusResponseError();
                                fareCheck.amadeusError.error = errorText;
                                fareCheck.amadeusError.errorCode = Convert.ToInt16(errorCode);
                                return fareCheck;

                            }
                            else
                            {

                                var res = ConvertXmlToModel(xmlDoc, fareNS.NamespaceName);

                                fareCheck.data = res;
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
            fareCheck.TransactionType = new TransactionType();
            fareCheck.TransactionType.MessageFunctionDetails = new MessageFunctionDetails();
            fareCheck.TransactionType.MessageFunctionDetails.MessageFunction = messageFunction.ToString();
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
                fareCheck.Session = new HeaderSession
                {
                    SecurityToken = securityToken,
                    SequenceNumber = SeqNumber,
                    SessionId = sessionId,
                    TransactionStatusCode = TransactionStatusCode
                };
            }
            var flightDetails = doc.Descendants(amadeus + "flightDetails")?.ToList();
            fareCheck.FlightDetails = new List<FlightDetailsFareCheck>();
            foreach (var item in flightDetails)
            {
                var nbOfSegments = item.Descendants(amadeus + "nbOfSegments")?.FirstOrDefault()?.Value;
                FlightDetailsFareCheck _flightfare = new FlightDetailsFareCheck();
                _flightfare.NbOfSegments = nbOfSegments;
                var rateClass = item.Descendants(amadeus + "qualificationFareDetails").Descendants(amadeus + "additionalFareDetails")?.Descendants(amadeus + "rateClass")?.FirstOrDefault().Value;
                _flightfare.QualificationFareDetails = new QualificationFareDetails { AdditionalFareDetails = new AdditionalFareDetails { RateClass = rateClass } };
                var marketingCompany = item.Descendants(amadeus + "transportService")?.Descendants(amadeus + "companyIdentification")?.Descendants(amadeus + "marketingCompany")?.FirstOrDefault()?.Value;
                _flightfare.TransportService = new TransportService { CompanyIdentification = new CompanyIdentification { MarketingCompany = marketingCompany } };
                _flightfare.FlightErrorCodes = new List<FlightErrorCode>();
                var flightErrorCode = item.Descendants(amadeus + "flightErrorCode")?.ToList();
                foreach (var itemError in flightErrorCode)
                {
                    var textSubjectQualifier = item.Descendants(amadeus + "freeTextQualification")?.Elements(amadeus + "textSubjectQualifier")?.FirstOrDefault()?.Value;
                    var informationType = item.Descendants(amadeus + "freeTextQualification")?.Elements(amadeus + "informationType")?.FirstOrDefault()?.Value;
                    var freeText = item.Descendants(amadeus + "freeText")?.FirstOrDefault()?.Value;
                    FlightErrorCode code = new FlightErrorCode { freeText = freeText, informationType = informationType, textSubjectQualifier = textSubjectQualifier };
                    _flightfare.FlightErrorCodes.Add(code);
                }

                var numberOfUnit = item.Descendants(amadeus + "fareDetailInfo")?.Descendants(amadeus + "nbOfUnits")?.Descendants(amadeus + "quantityDetails")?.Descendants(amadeus + "numberOfUnit")?.FirstOrDefault()?.Value;

                var unitQualifier = item.Descendants(amadeus + "fareDetailInfo")?.Descendants(amadeus + "nbOfUnits")?.Descendants(amadeus + "quantityDetails")?.Descendants(amadeus + "unitQualifier")?.FirstOrDefault()?.Value;
                _flightfare.FareDetailInfo = new FareDetailInfo();
                _flightfare.FareDetailInfo.NbOfUnits = new NbOfUnits();
                _flightfare.FareDetailInfo.NbOfUnits.QuantityDetails = new List<QuantityDetails>();
                _flightfare.FareDetailInfo.NbOfUnits.QuantityDetails.Add(new QuantityDetails { NumberOfUnit = numberOfUnit, UnitQualifier = unitQualifier });

                var pricingGroup = item.Descendants(amadeus + "fareDetailInfo")?.Descendants(amadeus + "fareDeatilInfo")?.Descendants(amadeus + "fareTypeGrouping")?.Descendants(amadeus + "pricingGroup")?.FirstOrDefault()?.Value;

                var origin = item.Descendants(amadeus + "odiGrp")?.Descendants(amadeus + "originDestination")?.Descendants(amadeus + "origin")?.FirstOrDefault()?.Value;
                var destination = item.Descendants(amadeus + "odiGrp")?.Descendants(amadeus + "originDestination")?.Descendants(amadeus + "destination")?.FirstOrDefault()?.Value;
                _flightfare.OdiGrp = new OdiGrp();
                _flightfare.OdiGrp.OriginDestination = new OriginDestination { Destination = destination, Origin = origin };

                var travellerGrp_type = item.Descendants(amadeus + "travellerGrp")?.Descendants(amadeus + "travellerIdentRef")?.Descendants(amadeus + "referenceDetails")?.Descendants(amadeus + "type")?.FirstOrDefault()?.Value;
                var travellerGrp_value = item.Descendants(amadeus + "travellerGrp")?.Descendants(amadeus + "travellerIdentRef")?.Descendants(amadeus + "referenceDetails")?.Descendants(amadeus + "value")?.FirstOrDefault()?.Value;

                var fareRulesDetails = item.Descendants(amadeus + "travellerGrp")?.Descendants(amadeus + "fareRulesDetails")?.ToList();
                List<string> ruleSectionIds = new List<string>();

                foreach (var Ruleitem in fareRulesDetails)
                {
                    var Ruleid = Ruleitem.Element(amadeus + "ruleSectionId")?.Value;
                    ruleSectionIds.Add(Ruleid);
                }
                _flightfare.TravellerGrp = new TravellerGrp();
                _flightfare.TravellerGrp.TravellerIdentRef = new TravellerIdentRef { ReferenceDetails = new ReferenceDetails { Type = travellerGrp_type, Value = travellerGrp_value, fareRulesDetails = ruleSectionIds } };

                var itemGrp_itemNb_itemNumberDetails_number = item.Descendants(amadeus + "itemGrp")?.Descendants(amadeus + "itemNb")?.Descendants(amadeus + "itemNumberDetails")?.Descendants(amadeus + "number")?.FirstOrDefault()?.Value;
                _flightfare.ItemGrp = new ItemGrp();
                _flightfare.ItemGrp.ItemNb = new ItemNb { ItemNumberDetails = new ItemNumberDetails { Number = itemGrp_itemNb_itemNumberDetails_number } };

                var quantityDetails = item.Descendants(amadeus + "itemGrp")?.Descendants(amadeus + "unitGrp")?.Descendants(amadeus + "nbOfUnits")?.Descendants(amadeus + "quantityDetails")?.ToList();

                List<QuantityDetails> _quantitydetals = new List<QuantityDetails>();
                foreach (var itemq in quantityDetails)
                {
                    var unit = item.Descendants(amadeus + "numberOfUnit")?.FirstOrDefault()?.Value;
                    var qfy = item.Descendants(amadeus + "unitQualifier")?.FirstOrDefault()?.Value;
                    QuantityDetails det = new QuantityDetails { NumberOfUnit = unit, UnitQualifier = qfy };
                    _quantitydetals.Add(det);
                }
                _flightfare.ItemGrp.UnitGrp = new UnitGrp();
                _flightfare.ItemGrp.UnitGrp.NbOfUnits = new NbOfUnits();
                _flightfare.ItemGrp.UnitGrp.NbOfUnits.QuantityDetails = new List<QuantityDetails>();
                _flightfare.ItemGrp.UnitGrp.NbOfUnits.QuantityDetails.AddRange(_quantitydetals);

                var farepricingGroup = item.Descendants(amadeus + "itemGrp")?.Descendants(amadeus + "unitGrp")?.Descendants(amadeus + "unitFareDetails")?.Descendants(amadeus + "fareTypeGrouping")?.Descendants(amadeus + "pricingGroup")?.FirstOrDefault()?.Value;
                _flightfare.ItemGrp.UnitGrp.UnitFareDetails = new UnitFareDetails();
                _flightfare.ItemGrp.UnitGrp.UnitFareDetails.FareTypeGrouping = new FareTypeGrouping { PricingGroup = farepricingGroup };
                fareCheck.FlightDetails.Add(_flightfare);
            }
            return fareCheck;
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
        public async Task<string> CreateSoapEnvelope(FareCheckModel requestModel)
        {
            string pwdDigest = await generatePassword();
            var amadeusSettings = configuration.GetSection("AmadeusSoap");
            string action = amadeusSettings["Fare_CheckRules"];
            string to = amadeusSettings["ApiUrl"];
            string username = amadeusSettings["webUserId"];
            string dutyCode = amadeusSettings["dutyCode"];
            string requesterType = amadeusSettings["requestorType"];
            string PseudoCityCode = amadeusSettings["PseudoCityCode"]?.ToString();
            string pos_type = amadeusSettings["POS_Type"];

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
        public async Task<bool> Security_Signout()
        {

            try
            {

                var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var _url = amadeusSettings["ApiUrl"];
                var _action = amadeusSettings["Security_SignOut"];
                string Result = string.Empty;
                string Envelope = await CreateSingout_Request();
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
                                using (XmlWriter writer = XmlWriter.Create("d:\\reservationlogs\\Security_Signout_Response" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".xml", settings))
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
                            XNamespace fareNS = "http://xml.amadeus.com/FARQNR_07_1_1A";
                            var errorInfo = xmlDoc.Descendants(fareNS + "errorInfo").FirstOrDefault();
                            if (errorInfo != null)
                            {
                                // Extract error details
                                var errorCode = errorInfo.Descendants(fareNS + "rejectErrorCode").Descendants(fareNS + "errorDetails").Descendants(fareNS + "errorCode").FirstOrDefault()?.Value;
                                var errorText = errorInfo.Descendants(fareNS + "errorFreeText").Descendants(fareNS + "freeText").FirstOrDefault()?.Value;

                                return false;

                            }

                        }
                    }
                }
                catch (WebException ex)
                {
                    using (StreamReader rd = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        Result = rd.ReadToEnd();

                        return false;

                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public async Task<string> CreateSingout_Request()
        {
            string pwdDigest = await generatePassword();
            var amadeusSettings = configuration.GetSection("AmadeusSoap");
            string action = amadeusSettings["Fare_CheckRules"];
            string to = amadeusSettings["ApiUrl"];
            string username = amadeusSettings["webUserId"];
            string dutyCode = amadeusSettings["dutyCode"];
            string requesterType = amadeusSettings["requestorType"];
            string PseudoCityCode = amadeusSettings["PseudoCityCode"]?.ToString();
            string pos_type = amadeusSettings["POS_Type"];

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
    }

}