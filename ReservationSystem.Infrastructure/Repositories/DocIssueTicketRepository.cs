using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models.PNRRetrive;
using ReservationSystem.Domain.Models;
using ReservationSystem.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using ReservationSystem.Domain.Models.DocIssueTicket;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class DocIssueTicketRepository : IDocIssueTicketRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        private readonly IHelperRepository _helperRepository;
        public DocIssueTicketRepository(IConfiguration _configuration, IMemoryCache cache, IHelperRepository helperRepository)
        {
            configuration = _configuration;
            _cache = cache;
            _helperRepository = helperRepository;
        }
        public async Task<DocIssueTicketResponse> DocIssueTicket(DocIssueTicketRequest requestModel)
        {
            DocIssueTicketResponse pnrResponse = new DocIssueTicketResponse();
            try
            {

                var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var _url = amadeusSettings["ApiUrl"];
                var _action = amadeusSettings["DocIssuance_IssueTicket"];
                string Result = string.Empty;
                string Envelope = await CreateSoapRequest(requestModel);
                string ns = "http://xml.amadeus.com/TTKTIR_15_1_1A";
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
                            await _helperRepository.SaveXmlResponse("DocIssuanceTicket_Request", Envelope);
                            await _helperRepository.SaveXmlResponse("DocIssuanceTicket_Response", result2);
                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(result2);
                            string jsonText = JsonConvert.SerializeXmlNode(xmlDoc2, Newtonsoft.Json.Formatting.Indented);
                            await _helperRepository.SaveJson(jsonText, "DocIssuanceTicketResponseJson");
                            XNamespace fareNS = ns;
                            var errorInfo = xmlDoc.Descendants(fareNS + "generalErrorInfo").FirstOrDefault();
                            if (errorInfo != null)
                            {
                                var errorCode = errorInfo.Descendants(fareNS + "errorOrWarningCodeDetails").Descendants(fareNS + "errorDetails").Descendants(fareNS + "errorCode").FirstOrDefault()?.Value;
                                var errorTextList = errorInfo.Descendants(fareNS + "errorWarningDescription").Descendants(fareNS + "freeText")?.ToList();
                                string errorText = string.Empty;
                                foreach (var error in errorTextList)
                                {
                                    errorText += error + " ";
                                }
                                pnrResponse.amadeusError = new AmadeusResponseError();
                                pnrResponse.amadeusError.error = errorText;
                                pnrResponse.amadeusError.errorCode = Convert.ToInt16(errorCode);
                                return pnrResponse;

                            }

                            var res = ConvertXmlToModel(xmlDoc, ns);
                            pnrResponse = res;

                        }
                    }
                }
                catch (WebException ex)
                {
                    using (StreamReader rd = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        Result = rd.ReadToEnd();
                        pnrResponse.amadeusError = new AmadeusResponseError();
                        pnrResponse.amadeusError.error = Result;
                        pnrResponse.amadeusError.errorCode = 0;
                        return pnrResponse;

                    }
                }
            }
            catch (Exception ex)
            {
                pnrResponse.amadeusError = new AmadeusResponseError();
                pnrResponse.amadeusError.error = ex.Message.ToString();
                pnrResponse.amadeusError.errorCode = 0;
                return pnrResponse;
            }
            return pnrResponse;
        }

        public async Task<string> CreateSoapRequest(DocIssueTicketRequest requestModel)
        {

            var amadeusSettings = configuration.GetSection("AmadeusSoap") != null ? configuration.GetSection("AmadeusSoap") : null;
            var action = amadeusSettings["DocIssuance_IssueTicket"];
            string to = amadeusSettings["ApiUrl"];
            string pwdDigest = await _helperRepository.generatePassword();
            string username = amadeusSettings["webUserId"];
            string dutyCode = amadeusSettings["dutyCode"];
            string requesterType = amadeusSettings["requestorType"];
            string PseudoCityCode = amadeusSettings["PseudoCityCode"]?.ToString();
            string pos_type = amadeusSettings["POS_Type"];

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
   <DocIssuance_IssueTicket>
         <optionGroup>
            <switches>
               <statusDetails>
                  <indicator>" +requestModel.indicator+@"</indicator>
               </statusDetails>
            </switches>
         </optionGroup>
      </DocIssuance_IssueTicket>
   </soap:Body>
</soap:Envelope>";

            return Request;
        }

        public DocIssueTicketResponse ConvertXmlToModel(XDocument response, string amadeusns)
        {
            DocIssueTicketResponse ReturnModel = new DocIssueTicketResponse();

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

            var pnrinfo = doc.Descendants(amadeus + "DocIssuance_IssueTicketReply").FirstOrDefault();
            ReturnModel.responseDetails = pnrinfo;

            return ReturnModel;
        }

       
     
    }
}
