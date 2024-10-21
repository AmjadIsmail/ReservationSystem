using ReservationSystem.Domain.Models.Soap.FlightPrice;
using ReservationSystem.Domain.Models;
using ReservationSystem.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReservationSystem.Domain.Models.AirSellFromRecommendation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Xml.Linq;
using System.Xml;
using System.Security.Cryptography;
using ReservationSystem.Domain.Models.Availability;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class AirsellRepository : IAirSellRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        public AirsellRepository(IConfiguration _configuration, IMemoryCache cache)
        {
            configuration = _configuration;
            _cache = cache;
        }

        public async Task<AirSellFromRecResponseModel> GetAirSellRecommendation(AirSellFromRecommendationRequest requestModel)
        {
            AirSellFromRecResponseModel AirSell = new AirSellFromRecResponseModel();
           
            try
            {

                var amadeusSettings = configuration.GetSection("AmadeusSoap");
                var _url = amadeusSettings["ApiUrl"]; // "https://nodeD2.test.webservices.amadeus.com/1ASIWJIBJAY";
                var _action = amadeusSettings["fareInformativePricingWithoutPNRAction"];
                string Result = string.Empty;
                string Envelope = await CreateAirSellRecommendationRequest(requestModel);
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
                                using (XmlWriter writer = XmlWriter.Create("d:\\reservationlogs\\AirSellResponse" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".xml", settings))
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
                           // var res = ConvertXmlToModel(xmlDoc);
                           // flightPrice.data = res.data;

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

        public async Task<string> CreateAirSellRecommendationRequest(AirSellFromRecommendationRequest requestModel)
        {
            string pwdDigest = await generatePassword();
            var amadeusSettings = configuration.GetSection("AmadeusSoap") != null ? configuration.GetSection("AmadeusSoap") : null;
            string action = amadeusSettings["Air_SellFromRecommendation"];
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
      <Air_SellFromRecommendation>
         <messageActionDetails>
            <messageFunctionDetails>
               <messageFunction>{requestModel.messageFunction}</messageFunction>
               <additionalMessageFunction>{requestModel.additionalMessageFunction}</additionalMessageFunction>
            </messageFunctionDetails>
         </messageActionDetails>
         <itineraryDetails>
            <originDestinationDetails>
               <origin>{requestModel.outBound.origin}</origin>
               <destination>{requestModel.outBound.destination }</destination>
            </originDestinationDetails>
            <message>
               <messageFunctionDetails>
                  <messageFunction>{requestModel.messageFunction}</messageFunction>
               </messageFunctionDetails>
            </message>
            <segmentInformation>
               <travelProductInformation>
                  <flightDate>
                     <departureDate>{requestModel.outBound.segmentInformation.travelProductInformation.departureDate}</departureDate>
                  </flightDate>
                  <boardPointDetails>
                     <trueLocationId>{requestModel.outBound.segmentInformation.travelProductInformation.fromAirport}</trueLocationId>
                  </boardPointDetails>
                  <offpointDetails>
                     <trueLocationId>{requestModel.outBound.segmentInformation.travelProductInformation.toAirport}</trueLocationId>
                  </offpointDetails>
                  <companyDetails>
                     <marketingCompany>{requestModel.outBound.segmentInformation.travelProductInformation.marketingCompany}</marketingCompany>
                  </companyDetails>
                  <flightIdentification>
                     <flightNumber>{requestModel.outBound.segmentInformation.travelProductInformation.flightNumber}</flightNumber>
                     <bookingClass>{requestModel.outBound.segmentInformation.travelProductInformation.bookingClass}</bookingClass>
                  </flightIdentification>
               </travelProductInformation>
               <relatedproductInformation>
                  <quantity>{requestModel.outBound.segmentInformation.travelProductInformation.relatedproductInformation.quantity}</quantity>
                  <statusCode>{requestModel.outBound.segmentInformation.travelProductInformation.relatedproductInformation.statusCode}</statusCode>
               </relatedproductInformation>
            </segmentInformation>
         </itineraryDetails>
         <itineraryDetails>
            <originDestinationDetails>
               <origin>{requestModel.inBound.origin}</origin>
               <destination>{requestModel.inBound.destination}</destination>
            </originDestinationDetails>
            <message>
               <messageFunctionDetails>
                  <messageFunction>{requestModel.messageFunction}</messageFunction>
               </messageFunctionDetails>
            </message>
            <segmentInformation>
               <travelProductInformation>
                  <flightDate>
                     <departureDate>{requestModel.inBound.segmentInformation.travelProductInformation.departureDate}</departureDate>
                  </flightDate>
                  <boardPointDetails>
                     <trueLocationId>{requestModel.inBound.segmentInformation.travelProductInformation.fromAirport}</trueLocationId>
                  </boardPointDetails>
                  <offpointDetails>
                     <trueLocationId>{requestModel.inBound.segmentInformation.travelProductInformation.toAirport}</trueLocationId>
                  </offpointDetails>
                  <companyDetails>
                     <marketingCompany>{requestModel.inBound.segmentInformation.travelProductInformation.marketingCompany}</marketingCompany>
                  </companyDetails>
                  <flightIdentification>
                     <flightNumber>{requestModel.inBound.segmentInformation.travelProductInformation.flightNumber}</flightNumber>
                     <bookingClass>{requestModel.inBound.segmentInformation.travelProductInformation.bookingClass}</bookingClass>
                  </flightIdentification>
               </travelProductInformation>
               <relatedproductInformation>
                  <quantity>{requestModel.inBound.segmentInformation.travelProductInformation.relatedproductInformation.quantity}</quantity>
                  <statusCode>{requestModel.inBound.segmentInformation.travelProductInformation.relatedproductInformation.statusCode}</statusCode>
               </relatedproductInformation>
            </segmentInformation>
         </itineraryDetails>
      </Air_SellFromRecommendation>
   </soapenv:Body>

</soapenv:Envelope>";

            return Request;
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
    }
}
