using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models.PricePnr;
using ReservationSystem.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using ReservationSystem.Domain.Models.TicketTst;
using ReservationSystem.Domain.Models;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class TicketTstRepository : ITicketTstRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        private readonly IHelperRepository _helperRepository;
        public TicketTstRepository(IConfiguration _configuration, IMemoryCache cache, IHelperRepository helperRepository)
        {
            configuration = _configuration;
            _cache = cache;
            _helperRepository = helperRepository;
        }
        public async Task<TicketTstResponse> CreateTicketTst(TicketTstRequest requestModel)
        {
            TicketTstResponse fopResponse = new TicketTstResponse();

            try
            {

                var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var logsPath = configuration.GetSection("Logspath");
                var _url = amadeusSettings["ApiUrl"];
                var _action = amadeusSettings["Ticket_CreateTSTFromPricing"];
                string Result = string.Empty;
                string Envelope = await CreateSoapRequest(requestModel);
                string ns = "http://xml.amadeus.com/TAUTCR_04_1_1A";
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
                            await _helperRepository.SaveXmlResponse("Tst_Request", Envelope);
                            await _helperRepository.SaveXmlResponse("Tst_Response", result2);
                            XDocument xmlDoc = XDocument.Parse(result2);
                            XmlWriterSettings settings = new XmlWriterSettings
                            {
                                Indent = true,
                                OmitXmlDeclaration = false,
                                Encoding = System.Text.Encoding.UTF8
                            };
                            try
                            {
                                using (XmlWriter writer = XmlWriter.Create(logsPath + "PNRMultiResponse" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".xml", settings))
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
                            XNamespace fareNS = ns;
                            var errorInfo = xmlDoc.Descendants(fareNS + "applicationError").FirstOrDefault();
                            if (errorInfo != null)
                            {
                             
                                var errorCode = errorInfo.Descendants(fareNS + "applicationErrorInfo").Descendants(fareNS + "applicationErrorDetail").Descendants(fareNS + "applicationErrorCode").FirstOrDefault()?.Value;
                                var errorText = errorInfo.Descendants(fareNS + "errorText").Descendants(fareNS + "errorFreeText").FirstOrDefault()?.Value;
                                fopResponse.amadeusError = new AmadeusResponseError();
                                fopResponse.amadeusError.error = errorText;
                                fopResponse.amadeusError.errorCode = Convert.ToInt16(errorCode);
                                return fopResponse;

                            }

                            var res = ConvertXmlToModel(xmlDoc, ns);
                            fopResponse = res;

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
        private string GeneratePsaList(TicketTstRequest ticketTst )
        {
            try
            {
                string result = $@"";
                if (ticketTst.adults > 0)
                {
                    result += "<psaList>\r\n" +
                   "<itemReference>\r\n " +
                   "<referenceType>TST</referenceType>\r\n" +
                   "<uniqueReference>1</uniqueReference>\r\n" +
                   "</itemReference>\r\n " +
                   " </psaList>\r\n";
                }
                if(ticketTst.children > 0)
                {
                    result += "<psaList>\r\n" +
                        "<itemReference>\r\n" +
                        "<referenceType>TST</referenceType>\r\n" +
                        "<uniqueReference>2</uniqueReference>\r\n" +
                        "</itemReference>\r\n  " +
                        "</psaList>\r\n";
                }
                if (ticketTst.infants > 0 && ticketTst.children > 0)
                {
                    result += "<psaList>\r\n" +
                        "<itemReference>\r\n" +
                        "<referenceType>TST</referenceType>\r\n" +
                        "<uniqueReference>3</uniqueReference>\r\n" +
                        "</itemReference>\r\n  " +
                        "</psaList>\r\n";
                }
                else if(ticketTst.infants > 0) // when no child in booking
                {
                    result += "<psaList>\r\n" +
                      "<itemReference>\r\n" +
                      "<referenceType>TST</referenceType>\r\n" +
                      "<uniqueReference>2</uniqueReference>\r\n" +
                      "</itemReference>\r\n  " +
                      "</psaList>\r\n";
                }
              
                return result;

            }
            catch (Exception ex)
            {
                Console.Write("Error while Generate PSA List " + ex.Message.ToString());
                return "";
            }
        }
        public async Task<string> CreateSoapRequest(TicketTstRequest requestModel)
        {

            var amadeusSettings = configuration.GetSection("AmadeusSoap") != null ? configuration.GetSection("AmadeusSoap") : null;
            string action = amadeusSettings["Ticket_CreateTSTFromPricing"];
            string to = amadeusSettings["ApiUrl"] ?? "https://nodeD2.test.webservices.amadeus.com/1ASIWJIBJAY";
            string Request = $@"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:ses=""http://xml.amadeus.com/2010/06/Session_v3"">
      <soap:Header xmlns:add=""http://www.w3.org/2005/08/addressing"">
      <ses:Session TransactionStatusCode=""InSeries"">
      <ses:SessionId>{requestModel?.sessionDetails?.SessionId}</ses:SessionId>
      <ses:SequenceNumber>{requestModel?.sessionDetails?.SequenceNumber + 1}</ses:SequenceNumber>
      <ses:SecurityToken>{requestModel?.sessionDetails?.SecurityToken}</ses:SecurityToken>
    </ses:Session>
    <add:MessageID>{System.Guid.NewGuid()}</add:MessageID>
    <add:Action>{action}</add:Action>
    <add:To>{to}</add:To>  
    <link:TransactionFlowLink xmlns:link=""http://wsdl.amadeus.com/2010/06/ws/Link_v1""/>
   </soap:Header>
   <soap:Body>
    <Ticket_CreateTSTFromPricing>
    {GeneratePsaList(requestModel)}
    </Ticket_CreateTSTFromPricing>
   </soap:Body>
</soap:Envelope>";

            return Request;
        }

        public TicketTstResponse ConvertXmlToModel(XDocument response, string amadeusns)
        {
            TicketTstResponse ReturnModel = new TicketTstResponse();
            TicketTstResponseDetails detail = new TicketTstResponseDetails();
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
         
            XNamespace amadeus = amadeusns;

            var tstList = doc.Descendants(amadeus + "tstList")?.ToList();
           
            if (tstList != null)
            {
                detail.tstList = new List<TstList>();
             foreach(var lst in tstList)
                {
                    TstList tst = new TstList();
                    var tstReference = lst.Descendants(amadeus + "tstReference")?.FirstOrDefault();
                    if(tstReference != null)
                    {
                        var referenceType = tstReference.Descendants(amadeus + "referenceType")?.FirstOrDefault()?.Value;
                        var uniqueReference = tstReference.Descendants(amadeus + "uniqueReference")?.FirstOrDefault()?.Value;
                        var iDSequenceNumber = tstReference.Descendants(amadeus + "iDDescription")?.Descendants(amadeus + "iDSequenceNumber")?.FirstOrDefault()?.Value;
                        tst.TstReference = new TstReference
                        {
                            IDDescription = new IDDescription { IDSequenceNumber = iDSequenceNumber != null ? Convert.ToInt16(iDSequenceNumber) : 0 },
                            UniqueReference = uniqueReference != null ? Convert.ToInt16 (uniqueReference) : 0,
                            ReferenceType = referenceType
                        };
                    }
                    var paxInformation = lst.Descendants(amadeus + "paxInformation")?.FirstOrDefault();
                    if( paxInformation != null)
                    {
                        var refQualifier = paxInformation.Descendants(amadeus + "refDetails")?.Descendants(amadeus+ "refQualifier")?.FirstOrDefault()?.Value;
                        var refNumber = paxInformation.Descendants(amadeus + "refDetails")?.Descendants(amadeus + "refNumber")?.FirstOrDefault()?.Value;
                        tst.PaxInformation = new PaxInformation
                        {
                            RefDetails = new Domain.Models.TicketTst.RefDetails
                            {
                                RefNumber = refNumber != null ? Convert.ToInt16(refNumber): 0,
                                RefQualifier = refQualifier
                            }
                        };
                    }
                    detail.tstList.Add(tst);
                }
            }
            return ReturnModel;
        }
    }
}
