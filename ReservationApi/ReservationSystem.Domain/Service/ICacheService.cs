using ReservationSystem.Domain.DB_Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Service
{
    public interface ICacheService
    {
        public Dictionary<int, FlightMarkup> GetFlightsMarkupCachedData();
        public void LoadDataIntoCache();
        public void ResetCacheData();
        public void Set<T>(string key, T value, TimeSpan duration);

        public T Get<T>(string key);

        public void Remove(string key);

        public void RemoveAll();

        public List<FlightMarkup> GetFlightsMarkup();

        public DataTable GetAirlines();
        public Task<DataTable> SetAirlineDataTableFromExcelToCache();

        public Task<DataTable> SetAirportToCache();

        public DataTable GetAirports();

        public Task<dynamic> CheckAirlines();
    }
}
