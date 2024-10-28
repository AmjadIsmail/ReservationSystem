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

namespace ReservationSystem.Infrastructure.Repositories
{
    public class TicketTstRepository : ITicketTstRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        public TicketTstRepository(IConfiguration _configuration, IMemoryCache cache)
        {
            configuration = _configuration;
            _cache = cache;
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
                            var errorInfo = xmlDoc.Descendants(fareNS + "errorInfo").FirstOrDefault();
                            if (errorInfo != null)
                            {
                                // Extract error details
                                var errorCode = errorInfo.Descendants(fareNS + "rejectErrorCode").Descendants(fareNS + "errorDetails").Descendants(fareNS + "errorCode").FirstOrDefault()?.Value;
                                var errorText = errorInfo.Descendants(fareNS + "errorFreeText").Descendants(fareNS + "freeText").FirstOrDefault()?.Value;
                                fopResponse.amadeusError = new AmadeusResponseError();
                                fopResponse.amadeusError.error = errorText;
                                fopResponse.amadeusError.errorCode = Convert.ToInt16(errorCode);
                                return fopResponse;

                            }

                            // var res = ConvertXmlToModel(xmlDoc, ns);
                            //  fopResponse = res;

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
            string to = amadeusSettings["ApiUrl"];
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
    <Ticket_CreateTSTFromPricing>
    {GeneratePsaList(requestModel)}
    </Ticket_CreateTSTFromPricing>
   </soap:Body>
</soap:Envelope>";

            return Request;
        }
    }
}
