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

        public List<ApplyMarkup> GetMarkup();

        public List<FareType> GetFareType();
       
        public List<MarkupFareType> GetMarkupFareTypes();

        public List<GDS> GetGds();

        public List<MarkupGDS> GetMarkupGds();

        public List<JourneyType> GetJournyType();

        public List<MarkupJournyType> GetMarkupJournyType();

        public List<MarketingSource> GetMarketingSource();
        public List<MarkupMarketingSource> GetMarkupMarketingSource();

        public List<DayName> GetDayName();

        public List<MarkupDay> GetMarkupDayName();       

        public DataTable GetAirlines();
        public Task<DataTable> SetAirlineDataTableFromExcelToCache();

        public Task<DataTable> SetAirportToCache();

        public DataTable GetAirports();

        public Task<dynamic> CheckAirlines();
    }
}
