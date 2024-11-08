using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Threading;
using System.Runtime.Intrinsics.Arm;
using ReservationSystem.Domain.Models;
using System.Globalization;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using System.IO;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class Test
    {
        private string GetNonce(byte[] randomBytes)
        {
            
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }

            // Step 2: Encode the bytes using Base64
            string base64String = Convert.ToBase64String(randomBytes);
            return base64String;
        }
        public string GenerateDigestPwd()
        {
           // string NONCE = "secretnonce10111";
            DateTime utcNow = DateTime.UtcNow;
            string TIMESTAMP = utcNow.ToString("yyyy-mm-ddTHH:MM:SSZ");
           string  PASSWORD = "4NRg6gU=Axek";
           // string NONCE = "secretnonce10111";

            // Convert the string to a byte array
          //  byte[] nonceBytes = Encoding.UTF8.GetBytes(NONCE);

            // Encode the byte array to Base64
          //  string nonceBase64 = Convert.ToBase64String(nonceBytes);
            byte[] nonceBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
             {
               rng.GetBytes(nonceBytes);
             }
             string nonceBase64 = Convert.ToBase64String(nonceBytes);
            // byte[] nonceBytes = Encoding.UTF8.GetBytes(NONCE);
            byte[] timestampBytes = Encoding.UTF8.GetBytes(TIMESTAMP);
           //  Compute SHA1 hash of the password using (
            SHA1 sha1 = SHA1.Create();            
            byte[] passwordBytes = Encoding.ASCII.GetBytes(PASSWORD);
            byte[] pwSha1Bytes = sha1.ComputeHash(passwordBytes);
            string PWSHA1 = BitConverter.ToString(pwSha1Bytes);//.Replace("-", "").ToLower();
            byte[] pwSha1BytesArray = Encoding.ASCII.GetBytes(PWSHA1);
            byte[] concatBytes = new byte[nonceBytes.Length + timestampBytes.Length + pwSha1BytesArray.Length]; 
            Buffer.BlockCopy(nonceBytes, 0, concatBytes, 0, nonceBytes.Length); 
            Buffer.BlockCopy(timestampBytes, 0, concatBytes, nonceBytes.Length, timestampBytes.Length);
            Buffer.BlockCopy(pwSha1BytesArray, 0, concatBytes, nonceBytes.Length + timestampBytes.Length, pwSha1BytesArray.Length);
            byte[] concatSha1Bytes = sha1.ComputeHash(concatBytes);
            string PWDIGEST = Convert.ToBase64String(concatSha1Bytes);     
            return PWDIGEST + "|" + nonceBase64 + "|" + TIMESTAMP;
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

        public string GenerateDigestPwd2()
        {
            string password = "4NRg6gU=Axek";
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

                Console.WriteLine("Encoded Nonce: " + nonceBase64);
                Console.WriteLine("Pass SHA: " + passSHA);
            }

            return passSHA + "|" + nonceBase64 + "|" + TIMESTAMP;
        }
        public string CreateSoapEnvelopeSimple()
        {
            #region Region For Password Digest
            //byte[] nonceBytes = new byte[16];
            //using (var rng = new RNGCryptoServiceProvider())
            //{
            //    rng.GetBytes(nonceBytes);
            //}
            //string nonceBase64 = Convert.ToBase64String(nonceBytes);
            //DateTime utcNow = DateTime.UtcNow;
            //string timestamp = utcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            //// string timestamp =   // ((int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
            //string clearPassword = "4NRg6gU=Axek";
            //byte[] clearPasswordBytes = Encoding.UTF8.GetBytes(clearPassword);
            //byte[] clearPasswordSha1;
            //using (SHA1 sha1 = SHA1.Create())
            //{
            //    clearPasswordSha1 = sha1.ComputeHash(clearPasswordBytes);
            //}
            //string clearPasswordSha1Hex = BitConverter.ToString(clearPasswordSha1).Replace("-", "").ToLower();
            //string toHash = nonceBase64 + timestamp + Convert.ToBase64String(clearPasswordSha1);
            //byte[] toHashBytes = Encoding.UTF8.GetBytes(toHash);
            //byte[] digest;
            //using (SHA1 sha1 = SHA1.Create())
            //{
            //    digest = sha1.ComputeHash(toHashBytes);  // Hash the concatenated string
            //}
            //string passwordDigest = Convert.ToBase64String(digest);

            #endregion

            // string iso8601String = utcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");         
            string pwdDigest = GenerateDigestPwd2();// GenerateDigestPwd();

            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:sec=""http://xml.amadeus.com/2010/06/Security_v1"" xmlns:typ=""http://xml.amadeus.com/2010/06/Types_v1"" xmlns:iat=""http://www.iata.org/IATA/2007/00/IATA2010.1"" xmlns:app=""http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3"" xmlns:link=""http://wsdl.amadeus.com/2010/06/ws/Link_v1"" xmlns:ses=""http://xml.amadeus.com/2010/06/Session_v3"">
   <soapenv:Header xmlns:add=""http://www.w3.org/2005/08/addressing"">
      <add:MessageID>{System.Guid.NewGuid()}</add:MessageID>
      <add:Action>http://webservices.amadeus.com/FMPTBQ_24_1_1A</add:Action>
      <add:To>https://nodeD2.test.webservices.amadeus.com/1ASIWJIBJAY</add:To>
      <oas:Security xmlns:oas=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:oas1=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">
         <oas:UsernameToken oas1:Id=""UsernameToken-1"">
            <oas:Username>WSJAYJIB</oas:Username>
            <oas:Nonce EncodingType=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary"">{pwdDigest.Split("|")[1]}</oas:Nonce>
            <oas:Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest"">{pwdDigest.Split("|")[0]}</oas:Password>
            <oas1:Created>{pwdDigest.Split("|")[2]}</oas1:Created>
         </oas:UsernameToken>
      </oas:Security>
      <AMA_SecurityHostedUser xmlns=""http://xml.amadeus.com/2010/06/Security_v1"">
         <UserID AgentDutyCode=""SU"" RequestorType=""U"" PseudoCityCode=""BHXU128JT"" POS_Type=""1""/>
      </AMA_SecurityHostedUser>
   </soapenv:Header>
   <soapenv:Body>
      <Fare_MasterPricerTravelBoardSearch>
         <numberOfUnit>
            <unitNumberDetail>
               <numberOfUnits>2</numberOfUnits>
               <typeOfUnit>PX</typeOfUnit>
            </unitNumberDetail>
            <unitNumberDetail>
               <numberOfUnits>250</numberOfUnits>
               <typeOfUnit>RC</typeOfUnit>
            </unitNumberDetail>
         </numberOfUnit>
         <paxReference>
            <ptc>ADT</ptc>
            <traveller>
               <ref>1</ref>
            </traveller>
            <traveller>
               <ref>2</ref>
            </traveller>
         </paxReference>
         <fareOptions>
            <pricingTickInfo>
               <pricingTicketing>
                  <priceType>ET</priceType>
                  <priceType>RP</priceType>
                  <priceType>RU</priceType>
                  <priceType>TAC</priceType>
                  <priceType>XND</priceType>
                  <priceType>XLA</priceType>
                  <priceType>XLO</priceType>
                  <priceType>XLC</priceType>
                  <priceType>XND</priceType>
               </pricingTicketing>
            </pricingTickInfo>           
         </fareOptions>        
         <itinerary>
            <requestedSegmentRef>
               <segRef>1</segRef>
            </requestedSegmentRef>
            <departureLocalization>
               <departurePoint>
                  <locationId>LON</locationId>
               </departurePoint>
            </departureLocalization>
            <arrivalLocalization>
               <arrivalPointDetails>
                  <locationId>MIA</locationId>
               </arrivalPointDetails>
            </arrivalLocalization>
            <timeDetails>
               <firstDateTimeDetail>
                  <date>260924</date>
               </firstDateTimeDetail>
            </timeDetails>
         </itinerary>
         <itinerary>
            <requestedSegmentRef>
               <segRef>2</segRef>
            </requestedSegmentRef>
            <departureLocalization>
               <departurePoint>
                  <locationId>MIA</locationId>
               </departurePoint>
            </departureLocalization>
            <arrivalLocalization>
               <arrivalPointDetails>
                  <locationId>LON</locationId>
               </arrivalPointDetails>
            </arrivalLocalization>
            <timeDetails>
               <firstDateTimeDetail>
                  <date>011024</date>
               </firstDateTimeDetail>
            </timeDetails>
         </itinerary>
      </Fare_MasterPricerTravelBoardSearch>
   </soapenv:Body>
</soapenv:Envelope>"
             ;
        }

        public AvailabilityModel ConvertXmlToModel(XDocument response)
        {
            AvailabilityModel ReturnModel = new AvailabilityModel();
            ReturnModel.data = new List<FlightOffer>();
            XDocument doc = response;
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace amadeus = "http://xml.amadeus.com/FMPTBR_24_1_1A";
           // FlightOffer offer = new FlightOffer();
            //offer.itineraries = new List<Itinerary>();
            Itinerary itinerary1 = new Itinerary();
            itinerary1.segments = new List<Segment>();
            var currency = doc.Descendants(amadeus + "conversionRate").Descendants(amadeus + "conversionRateDetail")?.Elements(amadeus + "currency")?.FirstOrDefault()?.Value;
            var flightIndexOutBound = doc.Descendants(amadeus +"flightIndex").Where(f => f.Element(amadeus+"requestedSegmentRef").Element(amadeus+"segRef").Value == "1")
                             .FirstOrDefault();            

           // var flightDetailsList = doc.Descendants(amadeus + "flightDetails").ToList();
            if (flightIndexOutBound != null)
            {
                var flightDetails = flightIndexOutBound.Descendants(amadeus + "flightDetails").FirstOrDefault(); 
                //foreach ( var flightDetails in flightIndexOutBound)
                //{
                    var productDateTime = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "productDateTime");
                    var departureDate = productDateTime?.Element(amadeus + "dateOfDeparture")?.Value;
                    var departureTime = productDateTime?.Element(amadeus + "timeOfDeparture")?.Value;
                    var arrivalDate = productDateTime?.Element(amadeus + "dateOfArrival")?.Value;
                    var arrivalTime = productDateTime?.Element(amadeus + "timeOfArrival")?.Value;

                    var departureLocation = flightDetails.Element(amadeus + "flightInformation")?
                        .Elements(amadeus + "location")?.FirstOrDefault()?.Element(amadeus + "locationId")?.Value;
                    var arrivalLocation = flightDetails.Element(amadeus + "flightInformation")?
                        .Elements(amadeus + "location")?.Skip(1).FirstOrDefault()?.Element(amadeus + "locationId")?.Value;

                    var marketingCarrier = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "companyId")?.Element(amadeus + "marketingCarrier")?.Value;
                    var flightNumber = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "flightOrtrainNumber")?.Value;           
                  
                    Segment segment = new Segment();
                    string dateTimeStr = departureDate + departureTime;
                    string format = "ddMMyyHHmm";
                    DateTime departureD = DateTime.ParseExact(dateTimeStr, format, CultureInfo.InvariantCulture);
                    segment.departure = new Departure { at = departureD, iataCode = departureLocation };
                    string arrival = arrivalDate + arrivalTime;
                    DateTime arrivalD = DateTime.ParseExact(arrival, format, CultureInfo.InvariantCulture);
                    segment.arrival = new Arrival { at = arrivalD, iataCode = arrivalLocation };
                    segment.marketingCarrierCode = marketingCarrier;
                    segment.aircraft = new Aircraft { code = flightNumber };
                    itinerary1.segments.Add(segment);
                                     
                //}

               
            }

            var flightIndexInbound = doc.Descendants(amadeus + "flightIndex").Where(f => f.Element(amadeus + "requestedSegmentRef").Element(amadeus + "segRef").Value == "2")
                            .FirstOrDefault();


            if (flightIndexInbound != null)
            {
                var flightDetails = flightIndexInbound.Descendants(amadeus + "flightDetails").FirstOrDefault();

                var productDateTime = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "productDateTime");
                var departureDate = productDateTime?.Element(amadeus + "dateOfDeparture")?.Value;
                var departureTime = productDateTime?.Element(amadeus + "timeOfDeparture")?.Value;
                var arrivalDate = productDateTime?.Element(amadeus + "dateOfArrival")?.Value;
                var arrivalTime = productDateTime?.Element(amadeus + "timeOfArrival")?.Value;

                var departureLocation = flightDetails.Element(amadeus + "flightInformation")?
                    .Elements(amadeus + "location")?.FirstOrDefault()?.Element(amadeus + "locationId")?.Value;
                var arrivalLocation = flightDetails.Element(amadeus + "flightInformation")?
                    .Elements(amadeus + "location")?.Skip(1).FirstOrDefault()?.Element(amadeus + "locationId")?.Value;

                var marketingCarrier = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "companyId")?.Element(amadeus + "marketingCarrier")?.Value;
                var flightNumber = flightDetails.Element(amadeus + "flightInformation")?.Element(amadeus + "flightOrtrainNumber")?.Value;

               
                Segment segment = new Segment();
                string dateTimeStr = departureDate + departureTime;
                string format = "ddMMyyHHmm";
                DateTime departureD = DateTime.ParseExact(dateTimeStr, format, CultureInfo.InvariantCulture);
                segment.departure = new Departure { at = departureD, iataCode = departureLocation };
                string arrival = arrivalDate + arrivalTime;
                DateTime arrivalD = DateTime.ParseExact(arrival, format, CultureInfo.InvariantCulture);
                segment.arrival = new Arrival { at = arrivalD, iataCode = arrivalLocation };
                segment.marketingCarrierCode = marketingCarrier;
                segment.aircraft = new Aircraft { code = flightNumber };
                itinerary1.segments.Add(segment);
                //}


            }
         //   offer.itineraries.Add(itinerary);

            #region Working For Recemondations
            string id = string.Empty, price = string.Empty, refnumenr = string.Empty, totalFareAmount = string.Empty;
            string totalTax = string.Empty; string transportStageQualifier = string.Empty; string transportStageQualifierCompany = string.Empty;
            string pricingTicketingPriceType = string.Empty;
            string isRefundable = string.Empty;
            string LastTicketDate = string.Empty;
            string cabinProduct = string.Empty;
            string farebasis = string.Empty;
            string companyname = string.Empty;
             var recommendationList = doc.Descendants(amadeus + "recommendation").ToList();
            if(recommendationList != null)
            {
                foreach(var item in recommendationList)
                {
                    FlightOffer offer = new FlightOffer();
                    offer.itineraries = new List<Itinerary>();                   
                    offer.itineraries.Add(itinerary1);
                    
                    var itemNumberId = item.Descendants(amadeus + "itemNumber").Elements(amadeus + "itemNumberId")?.FirstOrDefault()?.Value;
                     price = item.Descendants(amadeus + "recPriceInfo").Elements(amadeus + "monetaryDetail").Elements(amadeus + "amount")?.FirstOrDefault()?.Value;
                    var priceinfo2 = item.Descendants(amadeus + "recPriceInfo").Elements(amadeus + "monetaryDetail").Elements(amadeus + "amount").Skip(1)?.FirstOrDefault()?.Value;
                    totalFareAmount = item.Descendants(amadeus + "paxFareProduct").Elements(amadeus + "paxFareDetail").Elements(amadeus + "totalFareAmount")?.FirstOrDefault()?.Value;
                    totalTax = item.Descendants(amadeus + "paxFareProduct").Elements(amadeus + "paxFareDetail").Elements(amadeus + "totalTaxAmount")?.FirstOrDefault()?.Value;
                    var paxReferece = item.Descendants(amadeus + "paxFareProduct").Elements(amadeus + "paxReference").ToList();
                    var companylist = item.Descendants(amadeus + "paxFareProduct").Elements(amadeus + "paxFareDetail").Elements(amadeus + "codeShareDetails").ToList();
                    foreach( var company in companylist)
                    {
                        companyname = companyname + " " + company.Descendants(amadeus + "company")?.FirstOrDefault()?.Value;
                    }
                    var fare = item.Descendants(amadeus + "fare").ToList();
                    string faretype = "";
                    foreach(var fareitem in fare)
                    {
                        var IsREfunableTicket = fareitem.Descendants(amadeus + "pricingMessage").Where(f => f.Element(amadeus + "freeTextQualification").Element(amadeus + "informationType").Value == "70")
                            .FirstOrDefault();
                        if(IsREfunableTicket != null)
                        {
                            isRefundable = fareitem.Descendants(amadeus + "pricingMessage").Elements(amadeus + "description")?.FirstOrDefault().Value;
                        }

                        var LastTkctDate = fareitem.Descendants(amadeus + "pricingMessage").Where(f => f.Element(amadeus + "freeTextQualification").Element(amadeus + "informationType").Value == "40")
                           .FirstOrDefault();
                        if (LastTkctDate != null)
                        {
                            var lstDate = fareitem.Descendants(amadeus + "pricingMessage").Elements(amadeus + "description")?.ToList();
                            foreach(var i in lstDate)
                            {
                                LastTicketDate = LastTicketDate + " "+ i.Value;
                            }
                        }

                    }

                    var fareDetailsGroupOfFare = item.Descendants(amadeus + "fareDetails").Descendants(amadeus+"groupOfFares").ToList();
                    
                    if(fareDetailsGroupOfFare != null)
                    {
                        foreach(var productInfo in fareDetailsGroupOfFare)
                        {
                            var cabinP = productInfo.Descendants(amadeus+ "productInformation").Descendants(amadeus + "cabinProduct").ToList();
                            foreach(var itemcabinp in cabinP)
                            {
                                cabinProduct = cabinProduct + "," + itemcabinp.Descendants(amadeus + "cabin")?.FirstOrDefault()?.Value;
                            }
                           var fbasis = productInfo.Descendants(amadeus + "productInformation").Descendants(amadeus + "fareProductDetail").ToList();
                            foreach(var f in fbasis)
                            {
                                farebasis = farebasis + "," + f.Element(amadeus + "fareBasis")?.Value;
                                faretype = faretype = faretype + "," + f.Element(amadeus + "fareType").Value;
                            }

                        }
                    }
                   
                    
                    var taxes = new List<Taxes>();
                    Taxes t = new Taxes { amount = totalTax, code = "" };
                    taxes.Add(t);
                    offer.price = new Price { baseAmount = totalFareAmount, currency = currency , grandTotal = price
                        , taxes = taxes , total = price , discount = 0 , billingCurrency = currency , markup = 0   };

                    offer.id = itemNumberId;
                    offer.lastTicketingDate = LastTicketDate;
                    offer.oneWay = flightIndexInbound != null ? false : true;
                    offer.pricingOptions = new PriceOption { fareType = faretype.Split(',').ToList<string>() , includedCheckedBagsOnly = false};
                    offer.source = "Amadeus";
                    offer.travelerPricings = new List<TravelerPricing>();
                    foreach (var pfx in paxReferece)
                    {
                        var ptc = pfx.Element(amadeus + "ptc").Value;
                        var traveler = pfx.Descendants(amadeus + "traveller").ToList();
                        foreach( var tr in traveler)
                        {
                            TravelerPricing tp = new TravelerPricing { travelerType = ptc, travelerId = tr.Element(amadeus + "ref").Value , price = offer.price};
                            offer.travelerPricings.Add(tp);
                        }                        
                    }
                    offer.validatingAirlineCodes  = companyname.Split(" ").ToList<string>() ;
                    ReturnModel.data.Add(offer);
                }

            }
            #endregion
          //  ReturnModel.data.Add(offer);

            return ReturnModel;
        }
        public string CreateSoapEnvelopeSimple_Backup()
        {
            #region Region For Password Digest
            byte[] nonceBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(nonceBytes); 
            }
            string nonceBase64 = Convert.ToBase64String(nonceBytes);
            DateTime utcNow = DateTime.UtcNow;
            string timestamp = utcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
           // string timestamp =   // ((int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
            string clearPassword = "4NRg6gU=Axek";
            byte[] clearPasswordBytes = Encoding.UTF8.GetBytes(clearPassword);
            byte[] clearPasswordSha1;
            using (SHA1 sha1 = SHA1.Create())
            {
                clearPasswordSha1 = sha1.ComputeHash(clearPasswordBytes);  
            }
            string clearPasswordSha1Hex = BitConverter.ToString(clearPasswordSha1).Replace("-", "").ToLower();
            string toHash = nonceBase64 + timestamp + Convert.ToBase64String( clearPasswordSha1);
            byte[] toHashBytes = Encoding.UTF8.GetBytes(toHash);
            byte[] digest;
            using (SHA1 sha1 = SHA1.Create())
            {
                digest = sha1.ComputeHash(toHashBytes);  // Hash the concatenated string
            }
            string passwordDigest = Convert.ToBase64String(digest);

            #endregion
            //Random r = new Random();
            //string tokennamespace = "o";
            //DateTime created = DateTime.Now;
            //string createdStr = created.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            //string nonce = Convert.ToBase64String(Encoding.ASCII.GetBytes(SHA1Encrypt(created + r.Next().ToString())));
           // DateTime utcNow = DateTime.UtcNow;
            string iso8601String = utcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            //var _CreateNonce =   CreateNonce();
            //string createnonce = GetNonce(_CreateNonce);// BitConverter.ToString(_CreateNonce);


            // var _PasswordDigest = CreatePasswordDigest(_CreateNonce, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"), "NE5SZzZnVT1BeGVr");
            string pwdDigest = GenerateDigestPwd();

            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:sec=""http://xml.amadeus.com/2010/06/Security_v1"" xmlns:typ=""http://xml.amadeus.com/2010/06/Types_v1"" xmlns:iat=""http://www.iata.org/IATA/2007/00/IATA2010.1"" xmlns:app=""http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3"" xmlns:link=""http://wsdl.amadeus.com/2010/06/ws/Link_v1"" xmlns:ses=""http://xml.amadeus.com/2010/06/Session_v3"">
   <soapenv:Header xmlns:add=""http://www.w3.org/2005/08/addressing"">
      <add:MessageID>{System.Guid.NewGuid()}</add:MessageID>
      <add:Action>http://webservices.amadeus.com/FMPTBQ_24_1_1A</add:Action>
      <add:To>https://nodeD2.test.webservices.amadeus.com/1ASIWJIBJAY</add:To>
      <oas:Security xmlns:oas=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:oas1=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">
         <oas:UsernameToken oas1:Id=""UsernameToken-1"">
            <oas:Username>WSJAYJIB</oas:Username>
            <oas:Nonce EncodingType=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary"">{nonceBase64}</oas:Nonce>
            <oas:Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest"">{passwordDigest}</oas:Password>
            <oas1:Created>{iso8601String}</oas1:Created>
         </oas:UsernameToken>
      </oas:Security>
      <AMA_SecurityHostedUser xmlns=""http://xml.amadeus.com/2010/06/Security_v1"">
         <UserID AgentDutyCode=""SU"" RequestorType=""U"" PseudoCityCode=""BHXU128JT"" POS_Type=""1""/>
      </AMA_SecurityHostedUser>
   </soapenv:Header>
   <soapenv:Body>
      <Fare_MasterPricerTravelBoardSearch>
         <numberOfUnit>
            <unitNumberDetail>
               <numberOfUnits>2</numberOfUnits>
               <typeOfUnit>PX</typeOfUnit>
            </unitNumberDetail>
            <unitNumberDetail>
               <numberOfUnits>100</numberOfUnits>
               <typeOfUnit>RC</typeOfUnit>
            </unitNumberDetail>
         </numberOfUnit>
         <paxReference>
            <ptc>ADT</ptc>
            <traveller>
               <ref>1</ref>
            </traveller>
            <traveller>
               <ref>2</ref>
            </traveller>
         </paxReference>
         <fareOptions>
            <pricingTickInfo>
               <pricingTicketing>
                  <priceType>ET</priceType>
                  <priceType>RP</priceType>
                  <priceType>RU</priceType>
                  <priceType>TAC</priceType>
                  <priceType>XND</priceType>
                  <priceType>XLA</priceType>
                  <priceType>XLO</priceType>
                  <priceType>XLC</priceType>
                  <priceType>XND</priceType>
               </pricingTicketing>
            </pricingTickInfo>
         </fareOptions>
         <travelFlightInfo>
            <cabinId><cabin>M</cabin></cabinId>
            
            <companyIdentity>
               <carrierQualifier>X</carrierQualifier>
               <carrierId>NK</carrierId>
               <carrierId>F9</carrierId>
            </companyIdentity>
            <flightDetail>
               <flightType>N</flightType>
            </flightDetail>
         </travelFlightInfo>
         <itinerary>
            <requestedSegmentRef>
               <segRef>1</segRef>
            </requestedSegmentRef>
            <departureLocalization>
               <departurePoint>
                  <locationId>LON</locationId>
               </departurePoint>
            </departureLocalization>
            <arrivalLocalization>
               <arrivalPointDetails>
                  <locationId>MIA</locationId>
               </arrivalPointDetails>
            </arrivalLocalization>
            <timeDetails>
               <firstDateTimeDetail>
                  <date>260924</date>
               </firstDateTimeDetail>
            </timeDetails>
         </itinerary>
         <itinerary>
            <requestedSegmentRef>
               <segRef>1</segRef>
            </requestedSegmentRef>
            <departureLocalization>
               <departurePoint>
                  <locationId>LON</locationId>
               </departurePoint>
            </departureLocalization>
            <arrivalLocalization>
               <arrivalPointDetails>
                  <locationId>MIA</locationId>
               </arrivalPointDetails>
            </arrivalLocalization>
            <timeDetails>
               <firstDateTimeDetail>
                  <date>011024</date>
               </firstDateTimeDetail>
            </timeDetails>
         </itinerary>
      </Fare_MasterPricerTravelBoardSearch>
   </soapenv:Body>
</soapenv:Envelope>"
             ;
        }

        public string CreateTravelBoardSearch()
        {
            DateTime utcNow = DateTime.UtcNow;
            string iso8601String = utcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:sec=""http://xml.amadeus.com/2010/06/Security_v1"" xmlns:typ=""http://xml.amadeus.com/2010/06/Types_v1"" xmlns:iat=""http://www.iata.org/IATA/2007/00/IATA2010.1"" xmlns:app=""http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3"" xmlns:link=""http://wsdl.amadeus.com/2010/06/ws/Link_v1"" xmlns:ses=""http://xml.amadeus.com/2010/06/Session_v3"">
   <soapenv:Header xmlns:add=""http://www.w3.org/2005/08/addressing"">
      <add:MessageID>WbsConsu-REATjljnxhc8TkzwqGv9CnoRXEPv8su-RiqWQxQRk</add:MessageID>
      <add:Action>http://webservices.amadeus.com/FMPTBQ_24_1_1A</add:Action>
      <add:To>https://nodeD1.test.webservices.amadeus.com/1ASIWJIBJAY</add:To>
      <oas:Security xmlns:oas=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:oas1=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">
         <oas:UsernameToken oas1:Id=""UsernameToken-1"">
            <oas:Username>WSJAYJIB</oas:Username>
            <oas:Nonce EncodingType=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary"">noncetext</oas:Nonce>
            <oas:Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest"">NE5SZzZnVT1BeGVr</oas:Password>
            <oas1:Created>{iso8601String}</oas1:Created>
         </oas:UsernameToken>
      </oas:Security>
      <AMA_SecurityHostedUser xmlns=""http://xml.amadeus.com/2010/06/Security_v1"">
         <UserID AgentDutyCode=""SU"" RequestorType=""U"" PseudoCityCode=""BHXU128JT"" POS_Type=""1""/>
      </AMA_SecurityHostedUser>
   </soapenv:Header>
   <soapenv:Body>
      <Fare_MasterPricerTravelBoardSearch>
         <numberOfUnit>
            <unitNumberDetail>
               <numberOfUnits>2</numberOfUnits>
               <typeOfUnit>PX</typeOfUnit>
            </unitNumberDetail>
            <unitNumberDetail>
               <numberOfUnits>100</numberOfUnits>
               <typeOfUnit>RC</typeOfUnit>
            </unitNumberDetail>
         </numberOfUnit>
         <paxReference>
            <ptc>ADT</ptc>
            <traveller>
               <ref>1</ref>
            </traveller>
            <traveller>
               <ref>2</ref>
            </traveller>
         </paxReference>
         <fareOptions>
            <pricingTickInfo>
               <pricingTicketing>
                  <priceType>ET</priceType>
                  <priceType>RP</priceType>
                  <priceType>RU</priceType>
                  <priceType>TAC</priceType>
                  <priceType>XND</priceType>
                  <priceType>XLA</priceType>
                  <priceType>XLO</priceType>
                  <priceType>XLC</priceType>
                  <priceType>XND</priceType>
               </pricingTicketing>
            </pricingTickInfo>
         </fareOptions>
         <travelFlightInfo>
            <cabinId><cabin>M</cabin></cabinId>
            
            <companyIdentity>
               <carrierQualifier>X</carrierQualifier>
               <carrierId>NK</carrierId>
               <carrierId>F9</carrierId>
            </companyIdentity>
            <flightDetail>
               <flightType>N</flightType>
            </flightDetail>
         </travelFlightInfo>
         <itinerary>
            <requestedSegmentRef>
               <segRef>1</segRef>
            </requestedSegmentRef>
            <departureLocalization>
               <departurePoint>
                  <locationId>LON</locationId>
               </departurePoint>
            </departureLocalization>
            <arrivalLocalization>
               <arrivalPointDetails>
                  <locationId>MIA</locationId>
               </arrivalPointDetails>
            </arrivalLocalization>
            <timeDetails>
               <firstDateTimeDetail>
                  <date>260924</date>
               </firstDateTimeDetail>
            </timeDetails>
         </itinerary>
         <itinerary>
            <requestedSegmentRef>
               <segRef>1</segRef>
            </requestedSegmentRef>
            <departureLocalization>
               <departurePoint>
                  <locationId>LON</locationId>
               </departurePoint>
            </departureLocalization>
            <arrivalLocalization>
               <arrivalPointDetails>
                  <locationId>MIA</locationId>
               </arrivalPointDetails>
            </arrivalLocalization>
            <timeDetails>
               <firstDateTimeDetail>
                  <date>011024</date>
               </firstDateTimeDetail>
            </timeDetails>
         </itinerary>
      </Fare_MasterPricerTravelBoardSearch>
   </soapenv:Body>
</soapenv:Envelope>";
        }

        public HttpWebRequest CreateWebRequest(string url, string action)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Credentials = CredentialCache.DefaultNetworkCredentials;
            webRequest.Headers.Add("SOAPAction", action);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        public XmlDocument InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
            return soapEnvelopeXml;
        }
        public XmlDocument CreateSoapEnvelope()
        {
            DateTime utcNow = DateTime.UtcNow;
            string iso8601String = utcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            XmlDocument soapEnvelopeDocument = new XmlDocument();
            soapEnvelopeDocument.LoadXml(
            $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:sec=""http://xml.amadeus.com/2010/06/Security_v1"" xmlns:typ=""http://xml.amadeus.com/2010/06/Types_v1"" xmlns:iat=""http://www.iata.org/IATA/2007/00/IATA2010.1"" xmlns:app=""http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3"" xmlns:link=""http://wsdl.amadeus.com/2010/06/ws/Link_v1"" xmlns:ses=""http://xml.amadeus.com/2010/06/Session_v3"">
   <soapenv:Header xmlns:add=""http://www.w3.org/2005/08/addressing"">
      <add:MessageID>WbsConsu-REATjljnxhc8TkzwqGv9CnoRXEPv8su-RiqWQxQRk</add:MessageID>
      <add:Action>http://webservices.amadeus.com/FMPTBQ_24_1_1A</add:Action>
      <add:To>https://noded1.test.webservices.amadeus.com/1ASIWJIBJAY</add:To>
      <oas:Security xmlns:oas=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:oas1=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">
         <oas:UsernameToken oas1:Id=""UsernameToken-1"">
            <oas:Username>WSJAYJIB</oas:Username>
            <oas:Nonce EncodingType=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary"">noncetext</oas:Nonce>
            <oas:Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest"">NE5SZzZnVT1BeGVr</oas:Password>
            <oas1:Created>{iso8601String}</oas1:Created>
         </oas:UsernameToken>
      </oas:Security>
      <AMA_SecurityHostedUser xmlns=""http://xml.amadeus.com/2010/06/Security_v1"">
         <UserID AgentDutyCode=""SU"" RequestorType=""U"" PseudoCityCode=""BHXU128JT"" POS_Type=""1""/>
      </AMA_SecurityHostedUser>
   </soapenv:Header>
   <soapenv:Body>
      <Fare_MasterPricerTravelBoardSearch>
         <numberOfUnit>
            <unitNumberDetail>
               <numberOfUnits>2</numberOfUnits>
               <typeOfUnit>PX</typeOfUnit>
            </unitNumberDetail>
            <unitNumberDetail>
               <numberOfUnits>100</numberOfUnits>
               <typeOfUnit>RC</typeOfUnit>
            </unitNumberDetail>
         </numberOfUnit>
         <paxReference>
            <ptc>ADT</ptc>
            <traveller>
               <ref>1</ref>
            </traveller>
            <traveller>
               <ref>2</ref>
            </traveller>
         </paxReference>
         <fareOptions>
            <pricingTickInfo>
               <pricingTicketing>
                  <priceType>ET</priceType>
                  <priceType>RP</priceType>
                  <priceType>RU</priceType>
                  <priceType>TAC</priceType>
                  <priceType>XND</priceType>
                  <priceType>XLA</priceType>
                  <priceType>XLO</priceType>
                  <priceType>XLC</priceType>
                  <priceType>XND</priceType>
               </pricingTicketing>
            </pricingTickInfo>
         </fareOptions>
         <travelFlightInfo>
            <cabinId><cabin>M</cabin></cabinId>
            
            <companyIdentity>
               <carrierQualifier>X</carrierQualifier>
               <carrierId>NK</carrierId>
               <carrierId>F9</carrierId>
            </companyIdentity>
            <flightDetail>
               <flightType>N</flightType>
            </flightDetail>
         </travelFlightInfo>
         <itinerary>
            <requestedSegmentRef>
               <segRef>1</segRef>
            </requestedSegmentRef>
            <departureLocalization>
               <departurePoint>
                  <locationId>LON</locationId>
               </departurePoint>
            </departureLocalization>
            <arrivalLocalization>
               <arrivalPointDetails>
                  <locationId>MIA</locationId>
               </arrivalPointDetails>
            </arrivalLocalization>
            <timeDetails>
               <firstDateTimeDetail>
                  <date>260924</date>
               </firstDateTimeDetail>
            </timeDetails>
         </itinerary>
         <itinerary>
            <requestedSegmentRef>
               <segRef>1</segRef>
            </requestedSegmentRef>
            <departureLocalization>
               <departurePoint>
                  <locationId>LON</locationId>
               </departurePoint>
            </departureLocalization>
            <arrivalLocalization>
               <arrivalPointDetails>
                  <locationId>MIA</locationId>
               </arrivalPointDetails>
            </arrivalLocalization>
            <timeDetails>
               <firstDateTimeDetail>
                  <date>011024</date>
               </firstDateTimeDetail>
            </timeDetails>
         </itinerary>
      </Fare_MasterPricerTravelBoardSearch>
   </soapenv:Body>
</soapenv:Envelope>");
            return soapEnvelopeDocument;
        }

      
        public T DeserializeSoapResponse<T>(string soapResponse)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(soapResponse))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

       

        public string CreatePasswordDigest(byte[] nonce, string createdTime, string password)
        {
            // combine three byte arrays into one
            byte[] time = Encoding.UTF8.GetBytes(createdTime);
            byte[] pwd = Encoding.UTF8.GetBytes(password);
            byte[] operand = new byte[nonce.Length + time.Length + pwd.Length];
            Array.Copy(nonce, operand, nonce.Length);
            Array.Copy(time, 0, operand, nonce.Length, time.Length);
            Array.Copy(pwd, 0, operand, nonce.Length + time.Length, pwd.Length);

            // create the hash
            var sha1Hasher = new SHA1CryptoServiceProvider();
            byte[] hashedDataBytes = sha1Hasher.ComputeHash(operand);
            return Convert.ToBase64String(hashedDataBytes);
        }
        private byte[] CreateNonce()
        {
            var Rand = new RNGCryptoServiceProvider();
            //make random octets
            byte[] buf = new byte[0x10];
            Rand.GetBytes(buf);
            return buf;
        }
        protected String ByteArrayToString(byte[] inputArray)
        {
            StringBuilder output = new StringBuilder("");
            for (int i = 0; i < inputArray.Length; i++)
            {
                output.Append(inputArray[i].ToString("X2"));
            }
            return output.ToString();
        }
        protected String SHA1Encrypt(String phrase)
        {
            UTF8Encoding encoder = new UTF8Encoding();
            SHA1CryptoServiceProvider sha1Hasher = new SHA1CryptoServiceProvider();
            byte[] hashedDataBytes = sha1Hasher.ComputeHash(encoder.GetBytes(phrase));
            return ByteArrayToString(hashedDataBytes);
        }
       
    }
     

}
