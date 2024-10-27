using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ReservationSystem.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class HelperRepository : IHelperRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        public HelperRepository(IConfiguration _configuration, IMemoryCache cache)
        {
            configuration = _configuration;
            _cache = cache;
        }
        public async Task SaveJson( string jsonText , string filename)
        {
            try
            {
                var logsPath = configuration.GetSection("Logspath").Value;
                await System.IO.File.WriteAllTextAsync(logsPath + filename + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".json", jsonText);                
            }
            catch (Exception ex)
            {
               
            }
        }

        public async Task SaveXmlResponse(string filename, string response)
        {
            XDocument xmlDoc = XDocument.Parse(response);
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = false,
                Encoding = System.Text.Encoding.UTF8
            };
            try
            {
                var logsPath = configuration.GetSection("Logspath").Value;
                using (XmlWriter writer = XmlWriter.Create(logsPath + filename + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".xml", settings))
                {
                    xmlDoc.Save(writer);
                }
            }
            catch
            {

            }
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
    }
}
