using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Infrastructure.Service
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using OfficeOpenXml;
    using ReservationSystem.Domain.DB_Models;
    using ReservationSystem.Domain.DBContext;
    using ReservationSystem.Domain.Repositories;
    using ReservationSystem.Domain.Service;
    using System;
    using System.Collections.Concurrent;
    using System.Data;
    using System.Xml;

    public class CacheService : ICacheService
    {


        private readonly ConcurrentDictionary<string, (object Value, DateTime Expiry)> _cache
            = new ConcurrentDictionary<string, (object, DateTime)>();

        private readonly IMemoryCache _Markupcache;
        private const string FlightMarkupKey = "FlightsMarkup";
        private static List<FlightMarkup> _markup;
        private static DataTable _AirlineDT;
        private static DataTable _AirportDT;
        private readonly IServiceProvider _serviceProvider;


        public CacheService(IMemoryCache cache, IServiceProvider serviceProvider)
        {
            _Markupcache = cache;
            _serviceProvider = serviceProvider;

        }

        public async void LoadDataIntoCache()
        {
           
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DB_Context>();
               // var data = dbContext.flightMarkups.ToDictionary(e => e.markup_id, e => e);
               //// _markup = await dbContext.flightMarkups.ToListAsync();
             //   _Markupcache.Set(FlightMarkupKey, data);
            }
        }
        public void ResetCacheData()
        {
            _Markupcache.Remove(FlightMarkupKey);
            LoadDataIntoCache();
        }
        public Dictionary<int, FlightMarkup> GetFlightsMarkupCachedData()
        {

             _Markupcache.TryGetValue(FlightMarkupKey, out Dictionary<int, FlightMarkup> cachedData);
            return cachedData;
        }

        public List<FlightMarkup> GetFlightsMarkup()
        {
            return _markup;
        }
        public void Set<T>(string key, T value, TimeSpan duration)
        {
            var expiry = DateTime.Now.Add(duration);
            _cache[key] = (value, expiry);
        }

        public T Get<T>(string key)
        {
            if (_cache.TryGetValue(key, out var cacheEntry))
            {
                if (cacheEntry.Expiry > DateTime.Now)
                {
                    return (T)cacheEntry.Value;
                }
                else
                {
                    // Expired entry, remove it
                    _cache.TryRemove(key, out _);
                }
            }
            return default;
        }

        public void Remove(string key)
        {
            _cache.TryRemove(key, out _);
        }
        public void RemoveAll()
        {
            _cache.Clear();
        }
        public DataTable GetAirlines()
        {
            if(_AirlineDT == null)
            {
                SetAirlineDataTableFromExcelToCache();
            }
            return _AirlineDT;
        }
        public DataTable GetAirports()
        {
            if (_AirportDT == null)
            {
                SetAirportToCache();
            }
            return _AirportDT;
        }

        public async Task<DataTable> SetAirlineDataTableFromExcelToCache()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory.Replace("Debug\\net8.0\\",""), "Airlines.xlsx");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            DataTable dataTable = new DataTable();
            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0]; 
                    
                    dataTable.Columns.Add("AirlineID", typeof(int));
                    dataTable.Columns.Add("AirlineName", typeof(string));
                    dataTable.Columns.Add("AirlineCode", typeof(string));
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++) 
                    {
                        var airlineID = Convert.ToInt32(worksheet.Cells[row, 1].Value);
                        var airlineName = worksheet.Cells[row, 2].Value.ToString();
                        var airlineCode = worksheet.Cells[row, 3].Value.ToString();
                        dataTable.Rows.Add(airlineID, airlineName, airlineCode);
                    }
                    _AirlineDT = dataTable;
                }
            }
            catch{ }  
            return dataTable;
        }

        public async Task<DataTable> SetAirportToCache()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory.Replace("Debug\\net8.0\\", ""), "Airports.xlsx");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            DataTable dataTable = new DataTable();
            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                    dataTable.Columns.Add("AirportID", typeof(string));
                    dataTable.Columns.Add("AirportCode", typeof(string));
                    dataTable.Columns.Add("AirportName", typeof(string));
                    dataTable.Columns.Add("AirportCity", typeof(string));
                    dataTable.Columns.Add("AirportCountry", typeof(string));
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var AirportID = worksheet.Cells[row, 1].Value != null ? worksheet.Cells[row, 1].Value : "";
                        var AirportCode = worksheet.Cells[row, 2].Value != null ? worksheet.Cells[row, 2].Value.ToString() : "";
                        var AirportName = worksheet.Cells[row, 3].Value != null ? worksheet.Cells[row, 3].Value.ToString() : "";
                        var AirportCity = worksheet.Cells[row, 4].Value != null ? worksheet.Cells[row, 4].Value.ToString() : "";
                        var AirportCountry = worksheet.Cells[row, 5].Value != null ? worksheet.Cells[row, 5].Value.ToString():"";
                        dataTable.Rows.Add(AirportID, AirportCode, AirportName,AirportCity,AirportCountry);
                    }
                    _AirportDT = dataTable;
                }
            }
            catch { }
            return dataTable;
        }
    }
}
