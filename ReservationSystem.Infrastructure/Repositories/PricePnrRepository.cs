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

                var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var logsPath = configuration.GetSection("Logspath");
                var _url = amadeusSettings["ApiUrl"];
                var _action = amadeusSettings["Fare_PricePNRWithBookingClass"];
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
                            if (errorInfo != null)
                            {
                                // Extract error details
                                var errorCode = errorInfo.Descendants(fareNS + "errorOrWarningCodeDetails").Descendants(fareNS + "errorDetails").Descendants(fareNS + "errorCode").FirstOrDefault()?.Value;
                                var errorText = xmlDoc?.Descendants(fareNS + "errorWarningDescription")?.Descendants(fareNS + "freeText").FirstOrDefault()?.Value;
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
        private string GeneratePricingOptionsGroup(string pricingOptions)
        {
            try
            {
                string result = $@"";
                foreach (var option in pricingOptions.Split(','))
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

            var amadeusSettings = configuration.GetSection("AmadeusSoap") != null ? configuration.GetSection("AmadeusSoap") : null;
            var action = amadeusSettings["Fare_PricePNRWithBookingClass"];
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
    <Fare_PricePNRWithBookingClass>
     { GeneratePricingOptionsGroup(requestModel.pricingOptionKey)}
      <pricingOptionGroup>
        <pricingOptionKey>
          <pricingOptionKey>VC</pricingOptionKey>
        </pricingOptionKey>
        <carrierInformation>
          <companyIdentification>
            <otherCompany>{requestModel.carrierCode}</otherCompany>
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
    }
}
