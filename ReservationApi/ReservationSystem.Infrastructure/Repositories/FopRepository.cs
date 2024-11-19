using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReservationSystem.Domain.Models.AddPnrMulti;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using ReservationSystem.Domain.Models.FOP;
using ReservationSystem.Domain.Models.FlightPrice;
using ReservationSystem.Domain.Models;
using System.Globalization;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class FopRepository : IFopRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        private readonly IHelperRepository _helperRepository;
        public FopRepository(IConfiguration _configuration, IMemoryCache cache, IHelperRepository helperRepository)
        {
            configuration = _configuration;
            _cache = cache;
            _helperRepository = helperRepository;
        }
        public async Task<FopResponse> CreateFOP(FopRequest requestModel)
        {
            FopResponse fopResponse = new FopResponse();

            try
            {

              //  var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var logsPath = configuration.GetSection("Logspath");
                var _url = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:ApiUrl"]); 
                var _action = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:FOP_CreateFormOfPayment"]);
                string Result = string.Empty;
                string Envelope = await CreateSoapRequest(requestModel);
                string ns = "http://xml.amadeus.com/TFOPCR_19_2_1A";
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
                            await _helperRepository.SaveXmlResponse("Fop_Request", Envelope);
                            await _helperRepository.SaveXmlResponse("Fop_Response", result2);
                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(result2);
                            string jsonText = JsonConvert.SerializeXmlNode(xmlDoc2, Newtonsoft.Json.Formatting.Indented);
                            await _helperRepository.SaveJson(jsonText, "FopResponseJson");
                            XNamespace fareNS = ns;
                            var errorInfo = xmlDoc.Descendants(fareNS + "transmissionError").FirstOrDefault();
                            if (errorInfo != null)
                            {
                                var errorCode = errorInfo.Descendants(fareNS + "errorOrWarningCodeDetails").Descendants(fareNS + "errorDetails").Descendants(fareNS + "errorCode")?.FirstOrDefault()?.Value;
                                var errorText = errorInfo.Descendants(fareNS + "errorWarningDescription").Descendants(fareNS + "freeText").FirstOrDefault()?.Value;
                                var errorFreeText = xmlDoc.Descendants(fareNS + "fopDescription").Descendants(fareNS + "fpElementError")?.Descendants(fareNS + "errorWarningDescription")?.Descendants(fareNS+ "freeText")?.FirstOrDefault()?.Value;
                                fopResponse.amadeusError = new AmadeusResponseError();
                                fopResponse.amadeusError.error = errorText + " " + errorFreeText;
                                fopResponse.amadeusError.errorCode = Convert.ToInt16(errorCode);
                                return fopResponse;

                            }

                            var res = ConvertXmlToModel(xmlDoc, ns);
                             fopResponse  = res;

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

        public async Task<string> CreateSoapRequest(FopRequest requestModel)
        {

          //  var amadeusSettings = configuration.GetSection("AmadeusSoap") != null ? configuration.GetSection("AmadeusSoap") : null;
            string action = Environment.GetEnvironmentVariable(configuration["AmadeusSoap:FOP_CreateFormOfPayment"]);
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
     <FOP_CreateFormOfPayment>
      <transactionContext>
        <transactionDetails>
          <code>"+requestModel.transactionDetailsCode+@"</code>
        </transactionDetails>
      </transactionContext>
      <fopGroup>
        <fopReference></fopReference>
        <mopDescription>
          <fopSequenceNumber>
            <sequenceDetails>
              <number>1</number>
            </sequenceDetails>
          </fopSequenceNumber>
          <mopDetails>
            <fopPNRDetails>
              <fopDetails>
                <fopCode>"+requestModel.fopCode+@"</fopCode>
              </fopDetails>
            </fopPNRDetails>
          </mopDetails>
        </mopDescription>
      </fopGroup>
    </FOP_CreateFormOfPayment>
   </soap:Body>
</soap:Envelope>";

            return Request;
        }
        public FopResponse ConvertXmlToModel(XDocument response,string ns)
        {
            FopResponse res = new FopResponse();
            FopResponseDetails ReturnModel = new FopResponseDetails();
            res.responseDetails = ReturnModel;
            ReturnModel.CreateFormOfPaymentReply = new FOP_CreateFormOfPaymentReply();           
            FopDescription fopdescription = new FopDescription();
            ReturnModel.CreateFormOfPaymentReply.FopDescription = fopdescription;
            XDocument doc = response;
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace amadeus = ns;
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
                res.session = new HeaderSession
                {
                    SecurityToken = securityToken,
                    SequenceNumber = SeqNumber,
                    SessionId = sessionId,
                    TransactionStatusCode = TransactionStatusCode
                };
            }
            var fopDescription = doc.Descendants(amadeus + "fopDescription").FirstOrDefault();
            if (fopDescription != null)
            {
                var refqualifier = fopDescription?.Descendants(amadeus + "fopReference")?.Descendants(amadeus + "reference")?.Descendants(amadeus + "qualifier")?.FirstOrDefault().Value;
                var refnumber = fopDescription?.Descendants(amadeus + "fopReference")?.Descendants(amadeus + "reference")?.Descendants(amadeus + "number")?.FirstOrDefault().Value;

                fopdescription.FopReference = new FopReference
                {
                    Reference = new Domain.Models.FOP.Reference
                    {
                        Number = refnumber,
                        Qualifier = refqualifier
                    }

                };
                var mopDescription = doc?.Descendants(amadeus + "fopDescription")?.Descendants(amadeus+ "mopDescription").FirstOrDefault();        
                if (mopDescription != null)
                {
                    var fopSequenceNumber = mopDescription?.Descendants(amadeus + "fopSequenceNumber")?.Descendants(amadeus + "sequenceDetails")?.Descendants(amadeus + "number")?.FirstOrDefault()?.Value;
                    fopdescription.MopDescription = new MopDescription();
                    fopdescription.MopDescription.FopSequenceNumber = new FopSequenceNumber
                    {
                        SequenceDetails = new SequenceDetails
                        {
                            Number = fopSequenceNumber
                        }
                    };
                    var mopDetails = mopDescription?.Descendants(amadeus + "mopDetails").FirstOrDefault();
                    if(mopDetails != null)
                    {
                        fopdescription.MopDescription.MopDetails = new MopDetails();
                      var fopPNRDetails = mopDetails?.Descendants(amadeus + "fopPNRDetails")?.Descendants(amadeus + "fopDetails")?.FirstOrDefault();
                        if(fopPNRDetails != null)
                        {
                            var fopCode = fopPNRDetails?.Descendants(amadeus + "fopCode")?.FirstOrDefault()?.Value;
                            var fopStatus = fopPNRDetails?.Descendants(amadeus + "fopStatus")?.FirstOrDefault()?.Value;
                            var fopEdiCode = fopPNRDetails?.Descendants(amadeus + "fopEdiCode")?.FirstOrDefault()?.Value;
                            var fopReportingCode = fopPNRDetails?.Descendants(amadeus + "fopReportingCode")?.FirstOrDefault()?.Value;
                            var fopElecTicketingCode = fopPNRDetails?.Descendants(amadeus + "fopElecTicketingCode")?.FirstOrDefault()?.Value;
                            FopPNRDetails pNRDetails = new FopPNRDetails
                            {
                                FopDetails = new FopDetails
                                {
                                    FopCode = fopCode,
                                    FopEdiCode = fopEdiCode,
                                    FopElecTicketingCode = fopElecTicketingCode,
                                    FopReportingCode = fopReportingCode,
                                    FopStatus = fopStatus
                                }
                            };
                            fopdescription.MopDescription.MopDetails.FopPNRDetails = new FopPNRDetails();
                            fopdescription.MopDescription.MopDetails.FopPNRDetails = pNRDetails;
                        }
                        var oldFopFreeflow = mopDetails?.Descendants(amadeus + "oldFopFreeflow")?.FirstOrDefault();
                        if (oldFopFreeflow != null)
                        {
                            fopdescription.MopDescription.MopDetails.OldFopFreeflow = new OldFopFreeflow();
                            var textSubjectQualifier = oldFopFreeflow?.Descendants(amadeus + "freeTextDetails")?.Descendants(amadeus + "textSubjectQualifier")?.FirstOrDefault()?.Value;
                            var source = oldFopFreeflow?.Descendants(amadeus + "freeTextDetails")?.Descendants(amadeus + "source")?.FirstOrDefault()?.Value;
                            var encoding = oldFopFreeflow?.Descendants(amadeus + "freeTextDetails")?.Descendants(amadeus + "encoding")?.FirstOrDefault()?.Value;
                            var freeText = oldFopFreeflow?.Descendants(amadeus + "freeText")?.FirstOrDefault()?.Value;
                            OldFopFreeflow fopFreeflow = new OldFopFreeflow
                            {
                                FreeText = freeText,
                                FreeTextDetails = new FreeTextDetails
                                {
                                    Encoding = encoding,
                                    Source = source,
                                    TextSubjectQualifier = textSubjectQualifier
                                }
                            };
                            fopdescription.MopDescription.MopDetails.OldFopFreeflow = fopFreeflow;

                        }
                        var pnrSupplementaryData = mopDetails?.Descendants(amadeus + "pnrSupplementaryData")?.ToList();
                        List<PnrSupplementaryData> supplementaryData = new List<PnrSupplementaryData>();
                        foreach( var item in pnrSupplementaryData)
                        {
                            PnrSupplementaryData pnrSupplementary = new PnrSupplementaryData();
                            var criteriaSetType = item?.Descendants(amadeus + "dataAndSwitchMap")?.Descendants(amadeus + "criteriaSetType")?.FirstOrDefault()?.Value;
                            var criteriaDetails = item?.Descendants(amadeus + "dataAndSwitchMap")?.Descendants(amadeus + "criteriaDetails")?.ToList();
                            List<CriteriaDetails> lstCriteriaDetails = new List<CriteriaDetails>();
                            foreach ( var cd in criteriaDetails)
                            {
                                CriteriaDetails criteria = new CriteriaDetails();
                                var attributeType = cd?.Descendants(amadeus + "attributeType")?.FirstOrDefault()?.Value;
                                var attributeDescription = cd?.Descendants(amadeus + "attributeDescription")?.FirstOrDefault()?.Value;
                                criteria.AttributeDescription = attributeDescription;
                                criteria.AttributeType = attributeType;
                                lstCriteriaDetails.Add(criteria);
                           
                            }
                            DataAndSwitchMap dataAndSwitchMap = new DataAndSwitchMap
                            {
                                CriteriaDetails = lstCriteriaDetails,
                                CriteriaSetType = criteriaSetType
                            };
                            pnrSupplementary.DataAndSwitchMap = dataAndSwitchMap;
                            supplementaryData.Add(pnrSupplementary);


                        }
                        fopdescription.MopDescription.MopDetails.PnrSupplementaryData = supplementaryData;

                    }
                    
                    var paymentModule = mopDescription?.Descendants(amadeus + "paymentModule").FirstOrDefault();
                    if(paymentModule != null)
                    {
                        fopdescription.MopDescription.PaymentModule = paymentModule;
                    }
                }
                
            }
            return res;
        }

    }
}
