using Microsoft.AspNetCore.Mvc;
using System.Net;
using System;
using System.Net.Http;
using System.Text;
using System.Xml.Serialization;
using ReservationSystem.Infrastructure.Repositories;
using Newtonsoft.Json;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models;
using AmadeusServiceReference;
using System.ServiceModel;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/TestTravelboardSearch")]
    [ApiController]
    public class tesTravelboardSearchController : ControllerBase
    {

        // GET: api/<ValuesController>
        [HttpGet("TravelBoardSearch")]
        public async Task<IActionResult> Get()
        {
          
            Test t = new Test();
          
            


            var _url = "https://nodeD2.test.webservices.amadeus.com/1ASIWJIBJAY";
            var _action = "http://webservices.amadeus.com/FMPTBQ_24_1_1A";
            string Result = string.Empty;
            string Envelope = t.CreateSoapEnvelopeSimple();
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
                            Indent = true,  // Pretty print with indentation
                            OmitXmlDeclaration = false,  // Include the XML declaration (<?xml version="1.0"?>)
                            Encoding = System.Text.Encoding.UTF8
                        };
                        using (XmlWriter writer = XmlWriter.Create("d:\\reservationlogs\\TbSearchResponse" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") +".xml", settings))
                        {
                            xmlDoc.Save(writer);  // Write the XDocument content to the XmlWriter
                        }
                        
                        var test = t.ConvertXmlToModel(xmlDoc);
                        return Ok(xmlDoc);
                         
                    }
                }
            }
            catch (WebException ex)
            {
                using (StreamReader rd = new StreamReader(ex.Response.GetResponseStream()))
                {
                    Result = rd.ReadToEnd();

                    
                }
            }          
            return Ok(Result);
        }

        // GET: api/<ValuesController>
        [HttpGet("Test")]
        public async Task<IActionResult> GetTest()
        {
            int byteLength = 32; // You can change this value as needed

            // Generate random bytes
            byte[] randomBytes = new byte[byteLength];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            // Encode the byte array to Base64
            string base64String = Convert.ToBase64String(randomBytes);
            Test t = new Test();
            var domain = "https://nodeD1.test.webservices.amadeus.com";
            var config = new AmadeusWebServicesPTClient.EndpointConfiguration();

            #region Soap Client work
            AmadeusWebServicesPTClient client = new AmadeusWebServicesPTClient();
            var endpointAddress = new EndpointAddress("https://nodeD1.test.webservices.amadeus.com");

            client.ClientCredentials.UserName.UserName = "1ASIWJIBJAY";
            client.ClientCredentials.UserName.Password = "4NRg6gU=Axek";
            AmadeusServiceReference.Session session = new AmadeusServiceReference.Session();
            AmadeusServiceReference.AMA_SecurityHostedUser securityHostedUser = new AmadeusServiceReference.AMA_SecurityHostedUser()
            {
                UserID = new AmadeusServiceReference.AMA_SecurityHostedUserUserID()
                {
                    AgentDutyCode = "SU",
                    PseudoCityCode = "BHXU128JT",
                    POS_Type = "1"
                },
                WorkstationID = "1ASIWJIBJAY"
            };
            AmadeusServiceReference.TransactionFlowLinkType transactionFlowLinkType = new AmadeusServiceReference.TransactionFlowLinkType();
            AmadeusServiceReference.Fare_MasterPricerTravelBoardSearch TBSearch = new AmadeusServiceReference.Fare_MasterPricerTravelBoardSearch();
            //_req.AMA_SecurityHostedUser = new AMA_SecurityHostedUser();
            //_req.AMA_SecurityHostedUser.UserID = new AMA_SecurityHostedUserUserID();
            //_req.AMA_SecurityHostedUser.UserID.ERSP_UserID = "WSJAYJIB";
            //_req.AMA_SecurityHostedUser.UserID.AgentDutyCode = "SU";
            //_req.AMA_SecurityHostedUser.UserID.RequestorType = "U";
            //_req.AMA_SecurityHostedUser.UserID.PseudoCityCode = "BHXU128JT";
            //_req.AMA_SecurityHostedUser.UserID.POS_Type = "1";
            //_req.Fare_MasterPricerTravelBoardSearch = new Fare_MasterPricerTravelBoardSearch();

            TBSearch.numberOfUnit = new NumberOfUnitDetailsType_312621C[2];
            TBSearch.numberOfUnit[0] = new NumberOfUnitDetailsType_312621C();
            TBSearch.numberOfUnit[0].numberOfUnits = "250";
            TBSearch.numberOfUnit[0].typeOfUnit = "RC";
            TBSearch.numberOfUnit[1] = new NumberOfUnitDetailsType_312621C();
            TBSearch.numberOfUnit[1].numberOfUnits = "2";
            TBSearch.numberOfUnit[1].typeOfUnit = "PX";

            TBSearch.paxReference = new TravellerReferenceInformationType_230874S[2];
            TBSearch.paxReference[0] = new TravellerReferenceInformationType_230874S();
            TBSearch.paxReference[0].ptc = new string[1];
            TBSearch.paxReference[0].ptc[0] = "ADT";
            TBSearch.paxReference[0].traveller = new TravellerDetailsType4[1];
            TBSearch.paxReference[0].traveller[0] = new TravellerDetailsType4 { @ref = "1" };

            TBSearch.fareOptions = new Fare_MasterPricerTravelBoardSearchFareOptions();
            TBSearch.fareOptions.pricingTickInfo = new PricingTicketingDetailsType2();
            TBSearch.fareOptions.pricingTickInfo.pricingTicketing = "ET,RP,RU,TAC,XND,XLA,XLO,XLC,XND".Split(",");



            TBSearch.travelFlightInfo = new TravelFlightInformationType_231130S();
            TBSearch.travelFlightInfo.companyIdentity = new CompanyIdentificationType_315715C[1];
            TBSearch.travelFlightInfo.companyIdentity[0] = new CompanyIdentificationType_315715C();
            TBSearch.travelFlightInfo.companyIdentity[0].carrierId = "NK,F9".Split(",");
            TBSearch.travelFlightInfo.companyIdentity[0].carrierQualifier = "X";

            TBSearch.itinerary = new Fare_MasterPricerTravelBoardSearchItinerary[2];
            TBSearch.itinerary[0] = new Fare_MasterPricerTravelBoardSearchItinerary();
            TBSearch.itinerary[0].requestedSegmentRef = new OriginAndDestinationRequestType2 { segRef = "1" };
            TBSearch.itinerary[0].departureLocalization = new DepartureLocationType1
            {
                departurePoint = new ArrivalLocationDetailsType_120834C1
                {
                    locationId = "LON"
                }
            };
            TBSearch.itinerary[0].arrivalLocalization = new ArrivalLocalizationType1
            {
                arrivalPointDetails = new ArrivalLocationDetailsType1
                {
                    locationId = "MIA"
                }
            };
            TBSearch.itinerary[0].timeDetails = new DateAndTimeInformationType_181295S1
            {
                firstDateTimeDetail = new DateAndTimeDetailsTypeI1 { date = "200924" }
            };

            TBSearch.itinerary[1] = new Fare_MasterPricerTravelBoardSearchItinerary();
            TBSearch.itinerary[1].requestedSegmentRef = new OriginAndDestinationRequestType2 { segRef = "2" };
            TBSearch.itinerary[1].departureLocalization = new DepartureLocationType1
            {
                departurePoint = new ArrivalLocationDetailsType_120834C1
                {
                    locationId = "MIA"
                }
            };
            TBSearch.itinerary[1].arrivalLocalization = new ArrivalLocalizationType1
            {
                arrivalPointDetails = new ArrivalLocationDetailsType1
                {
                    locationId = "LON"
                }
            };
            TBSearch.itinerary[1].timeDetails = new DateAndTimeInformationType_181295S1
            {
                firstDateTimeDetail = new DateAndTimeDetailsTypeI1
                {
                    date = "011024"
                }
            }; try
            {
                var result = client.Fare_MasterPricerTravelBoardSearchAsync(session, transactionFlowLinkType, securityHostedUser, TBSearch);

            }
            catch (Exception ex)
            {

            }





            #endregion


            var _url = "https://nodeD2.test.webservices.amadeus.com/1ASIWJIBJAY";
            var _action = "http://webservices.amadeus.com/FMPTBQ_24_1_1A";

            //  XmlDocument soapEnvelopeXml = t.CreateSoapEnvelope();
            //  soapEnvelopeXml.InnerXml  = soapEnvelopeXml.InnerXml.ToString().Replace("noncetext", base64String.ToString());
            HttpWebRequest webRequest = t.CreateWebRequest(_url, _action);
            //  soapEnvelopeXml = t.InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);
            string Envelope = t.CreateSoapEnvelopeSimple();

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
                        Console.WriteLine(result2);
                    }
                }
            }
            catch (WebException ex)
            {
                using (StreamReader rd = new StreamReader(ex.Response.GetResponseStream()))
                {
                    var result2 = rd.ReadToEnd();
                    Console.WriteLine($"Error: {result2}");
                }
            }

            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

            // suspend this thread until call is complete. You might want to
            // do something usefull here like update your UI.
            asyncResult.AsyncWaitHandle.WaitOne();

            // get the response from the completed web request.
            string soapResult;
            using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
            {
                using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                {
                    soapResult = rd.ReadToEnd();
                }
                Console.Write(soapResult);
            }
            var soapEnvelope = t.CreateTravelBoardSearch();
            try
            {

                soapEnvelope = soapEnvelope.ToString().Replace("noncetext", base64String.ToString());
                using var client2 = new HttpClient();
                using var content = new MultipartFormDataContent();
                var soapXml = @"<?xml version=""1.0"" encoding=""utf-8""?>";
                soapXml = soapEnvelope;
                var soapContent = new StringContent(soapXml, Encoding.UTF8, "text/xml");
                content.Add(soapContent, "file", "soap.xml");
                var response = await client2.PostAsync("https://nodeD1.test.webservices.amadeus.com", content);
                var res = response.Content.ReadAsStringAsync().Result;
                if (response.IsSuccessStatusCode)
                {
                    // Process the success response
                    return Ok("SOAP request sent successfully!");
                }
                else
                {
                    // Handle the error response
                    return BadRequest("Error sending SOAP request.");
                }

            }
            catch (WebException ex)
            {
                Console.Write(ex.Message.ToString());
            }
            return Ok("test");
        }

       
    }
}
