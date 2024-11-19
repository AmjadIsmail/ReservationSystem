using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models.PricePnr;
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
using ReservationSystem.Domain.Models.PnrQueue;
using ReservationSystem.Domain.Models.PNRRetrive;
using System.Security.Cryptography;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class PnrQueueRepository : IPnrQueueRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        private readonly IHelperRepository _helperRepository;
        public PnrQueueRepository(IConfiguration _configuration, IMemoryCache cache, IHelperRepository helperRepository)
        {
            configuration = _configuration;
            _cache = cache;
            _helperRepository = helperRepository;
        }

        public async Task<PnrQueueResponse> CreatePnrQueue(PnrQueueRequest requestModel)
        {
            PnrQueueResponse pnrResponse = new PnrQueueResponse();
            try
            {

               // var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var _url = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:ApiUrl"]);
                var _action = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:Queue_PlacePNR"]);
                string Result = string.Empty;
                string Envelope = await CreateSoapRequest(requestModel);
                string ns = "http://xml.amadeus.com/QUQPCR_03_1_1A";
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
                            await _helperRepository.SaveXmlResponse("QueuePlacePNR_Request", Envelope);
                            await _helperRepository.SaveXmlResponse("QueuePlacePNR_Response", result2);
                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(result2);
                            string jsonText = JsonConvert.SerializeXmlNode(xmlDoc2, Newtonsoft.Json.Formatting.Indented);
                            await _helperRepository.SaveJson(jsonText, "QueuePlacePNResponseJson");
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
     
      
        public async Task<string> CreateSoapRequest(PnrQueueRequest requestModel)
        {

           // var amadeusSettings = configuration.GetSection("AmadeusSoap") != null ? configuration.GetSection("AmadeusSoap") : null;
            var action = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:Queue_PlacePNR"]);
            string to = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:ApiUrl"]);
            string pwdDigest = await _helperRepository.generatePassword();
            string username = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:webUserId"]);
            string dutyCode = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:dutyCode"]);
            string requesterType = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:requestorType"]);
            string PseudoCityCode = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:PseudoCityCode"]?.ToString());
            string pos_type = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:POS_Type"]);

            string Request = $@"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:ses=""http://xml.amadeus.com/2010/06/Session_v3"">
     <soap:Header xmlns:add=""http://www.w3.org/2005/08/addressing"">
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
  </soap:Header>
   <soap:Body><Queue_PlacePNR>
      <placementOption>
        <selectionDetails>
          <option>{requestModel?.selectionDetailsOption}</option>
        </selectionDetails>
      </placementOption>
      <targetDetails>
        <targetOffice>
          <sourceType>
            <sourceQualifier1>{requestModel?.sourceQualifier1}</sourceQualifier1>
          </sourceType>
          <originatorDetails>
            <inHouseIdentification1>{PseudoCityCode}</inHouseIdentification1>
          </originatorDetails>
        </targetOffice>
        <queueNumber>
          <queueDetails>
            <number>{requestModel?.queueDetailNumber}</number>
          </queueDetails>
        </queueNumber>
        <categoryDetails>
          <subQueueInfoDetails>
            <identificationType>{requestModel?.identificationType}</identificationType>
            <itemNumber>{requestModel?.identificationNumber}</itemNumber> 
          </subQueueInfoDetails>
        </categoryDetails>
      </targetDetails>
      <recordLocator>
        <reservation>
          <controlNumber>{requestModel?.pnr}</controlNumber>
        </reservation>
      </recordLocator>
    </Queue_PlacePNR>
   </soap:Body>
</soap:Envelope>";

            return Request;
        }

        public PnrQueueResponse ConvertXmlToModel(XDocument response, string amadeusns)
        {
            PnrQueueResponse ReturnModel = new PnrQueueResponse();

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

            var pnrinfo = doc.Descendants(amadeus + "Queue_PlacePNRReply").FirstOrDefault();
            ReturnModel.responseDetails = pnrinfo;

            return ReturnModel;
        }
    }
}
