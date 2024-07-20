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
    using ReservationSystem.Domain.DB_Models;
    using ReservationSystem.Domain.DBContext;
    using ReservationSystem.Domain.Repositories;
    using ReservationSystem.Domain.Service;
    using System;
    using System.Collections.Concurrent;
    using System.Xml;

    public class CacheService : ICacheService
    {


        private readonly ConcurrentDictionary<string, (object Value, DateTime Expiry)> _cache
            = new ConcurrentDictionary<string, (object, DateTime)>();

        private readonly IMemoryCache _Markupcache;
        private const string FlightMarkupKey = "FlightsMarkup";
        private static List<FlightMarkup> _markup;
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
                var data = dbContext.flightMarkups.ToDictionary(e => e.markup_id, e => e);
                _markup = await dbContext.flightMarkups.ToListAsync();
                _Markupcache.Set(FlightMarkupKey, data);
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
    }
}
