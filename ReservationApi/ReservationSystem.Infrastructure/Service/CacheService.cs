using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Infrastructure.Service
{
    using ClosedXML.Excel;
    using DocumentFormat.OpenXml.Spreadsheet;
    using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _environment;


        public CacheService(IMemoryCache cache, IServiceProvider serviceProvider, IWebHostEnvironment environment)
        {
            _Markupcache = cache;
            _serviceProvider = serviceProvider;
            _environment = environment;

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

        public void SetAirlineDataTable(string filePath)
        {
            DataTable dataTable = new DataTable();
            var data = new Dictionary<string, List<string>>();
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);                   
                    int rows = worksheet.LastRowUsed().RowNumber();
                    var colAirlineID = worksheet.Column("A").Cells(1, rows).Select(cell => cell.GetValue<string>() ?? string.Empty).ToList();
                    var colAirlineName = worksheet.Column("B").Cells(1, rows).Select(cell => cell.GetValue<string>() ?? string.Empty).ToList();
                    var colAirlineCode = worksheet.Column("C").Cells(1, rows).Select(cell => cell.GetValue<string>() ?? string.Empty).ToList();

                    data["AirlineID"] = colAirlineID;
                    data["AirlineName"] = colAirlineName;
                    data["AirlineCode"] = colAirlineCode;

                    dataTable.Columns.Add("AirlineID", typeof(string));
                    dataTable.Columns.Add("AirlineName", typeof(string));
                    dataTable.Columns.Add("AirlineCode", typeof(string));

                    int rowCount = data.Values.FirstOrDefault()?.Count ?? 0;

                    for (int i = 0; i < rowCount; i++)
                    {
                        var row = dataTable.NewRow();
                        row["AirlineId"] = data["AirlineID"][i];
                        row["AirlineName"] = data["AirlineName"][i];
                        row["AirlineCode"] = data["AirlineCode"][i];
                        dataTable.Rows.Add(row);
                    }

                    _AirlineDT = dataTable;
                }
            }
            catch (Exception ex)
            {
                dataTable.Columns.Add("AirlineID", typeof(string));
                dataTable.Columns.Add("AirlineName", typeof(string));
                dataTable.Columns.Add("AirlineCode", typeof(string));
                var airlineID = "1";
                var airlineName = "British airlines";
                var airlineCode = "BA";
                dataTable.Rows.Add(airlineID, airlineName, airlineCode);
                _AirlineDT = dataTable;
                Console.Write($"Error while reading Airline.xlsx file {ex.Message}");
            }
        }
        public async Task<DataTable> SetAirlineDataTableFromExcelToCache()
        {
            try
            {
                var filePath = Path.Combine(_environment.ContentRootPath, "SupportFiles", "Airlines.xlsx");
                SetAirlineDataTable(filePath);
                return _AirlineDT;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error while set airline cache {ex.Message.ToString()}");
                return _AirlineDT;

            }
                     
        }

        public void SetAirPortDataTable(string filePath)
        {
            DataTable dataTable = new DataTable();
            var data = new Dictionary<string, List<string>>();
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    dataTable.Columns.Add("AirportID", typeof(string));
                    dataTable.Columns.Add("AirportCode", typeof(string));
                    dataTable.Columns.Add("AirportName", typeof(string));
                    dataTable.Columns.Add("AirportCity", typeof(string));
                    dataTable.Columns.Add("AirportCountry", typeof(string));
                   
                    var worksheet = workbook.Worksheet(1);
                    int rows = worksheet.LastRowUsed().RowNumber();
                    var colAirportID = worksheet.Column("A").Cells(1, rows).Select(cell => cell.GetValue<string>() ?? string.Empty).ToList();
                    var colAirportCode = worksheet.Column("B").Cells(1, rows).Select(cell => cell.GetValue<string>() ?? string.Empty).ToList();
                    var colAirportName = worksheet.Column("C").Cells(1, rows).Select(cell => cell.GetValue<string>() ?? string.Empty).ToList();
                    var colAirportCity = worksheet.Column("D").Cells(1, rows).Select(cell => cell.GetValue<string>() ?? string.Empty).ToList();
                    var colAirportCountry = worksheet.Column("E").Cells(1, rows).Select(cell => cell.GetValue<string>() ?? string.Empty).ToList();

                    data["AirportID"] = colAirportID;
                    data["AirportCode"] = colAirportCode;
                    data["AirportName"] = colAirportName;
                    data["AirportCity"] = colAirportCity;
                    data["AirportCountry"] = colAirportCountry;

                    int rowCount = data.Values.FirstOrDefault()?.Count ?? 0;

                    for (int i = 0; i < rowCount; i++)
                    {
                        var row = dataTable.NewRow();                       
                        row["AirportID"] = data["AirportID"][i];
                        row["AirportCode"] = data["AirportCode"][i];
                        row["AirportName"] = data["AirportName"][i];
                        row["AirportCity"] = data["AirportCity"][i];
                        row["AirportCountry"] = data["AirportCountry"][i];
                        dataTable.Rows.Add(row);
                    }

                    
                    _AirportDT = dataTable;
                }
            }
            catch (Exception ex)
            {
                dataTable.Columns.Add("AirportID", typeof(string));
                dataTable.Columns.Add("AirportCode", typeof(string));
                dataTable.Columns.Add("AirportName", typeof(string));
                dataTable.Columns.Add("AirportCity", typeof(string));
                dataTable.Columns.Add("AirportCountry", typeof(string));
                var AirportID = "1";
                var AirportCode = "LHR";
                var AirportName = "London Airport";
                var AirportCity = "London";
                var AirportCountry = "United Kingdom";
                dataTable.Rows.Add(AirportID, AirportCode, AirportName, AirportCity, AirportCountry);
                _AirportDT = dataTable;
                Console.Write($"Error while reading Airport.xlsx file {ex.Message}");
            }
        }

        public async Task<DataTable> SetAirportToCache()
        {
            try
            {
                var filePath = Path.Combine(_environment.ContentRootPath, "SupportFiles", "Airports.xlsx");
                SetAirPortDataTable(filePath);
                return _AirportDT;
            }
            catch( Exception ex)
            {
                Console.WriteLine($"Error while set Airport cache {ex.Message.ToString()}");
                return _AirportDT;
            }
         
           
        }

        public async Task<dynamic> CheckAirlines()
        {
            try
            {
                //var filePath = Path.Combine(_environment.ContentRootPath, "SupportFiles", "Airlines.xlsx");
                //DataTable dataTable = new DataTable();
                //using (var workbook = new XLWorkbook(filePath))
                //{
                //    var worksheet = workbook.Worksheet(1);
                //    dataTable.Columns.Add("AirlineID", typeof(string));
                //    dataTable.Columns.Add("AirlineName", typeof(string));
                //    dataTable.Columns.Add("AirlineCode", typeof(string));
                //    for (int row = 2; row <= worksheet.RowCount(); row++)
                //    {
                //        var airlineID = worksheet.Row(row).Cell(1).Value.ToString();
                //        var airlineName = worksheet.Row(row).Cell(2).Value.ToString();
                //        var airlineCode = worksheet.Row(row).Cell(3).Value.ToString();
                //        dataTable.Rows.Add(airlineID, airlineName, airlineCode);
                //    }
                //}
                //_AirlineDT = dataTable;              
               
                return _AirlineDT;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while set airline cache {ex.Message.ToString()}");
                return ex.Message.ToString() + " " +ex.StackTrace.ToString() ;

            }
        }
    }
}
