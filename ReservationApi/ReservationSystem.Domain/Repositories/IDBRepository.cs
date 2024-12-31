using ReservationApi.ReservationSystem.Domain.DB_Models;
using ReservationSystem.Domain.DB_Models;
using ReservationSystem.Domain.Models.AddPnrMulti;
using ReservationSystem.Domain.Models.DBLogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface IDBRepository
    {
        public Task SaveAvailabilityResult(string? requset, string? response, int totlResults);

        public Task<List<FlightMarkup>?> GetFlightMarkup();

        public Task SaveReservationFlow(SaveReservationLog requset);

        public Task SavePassengerInfo(AddPnrMultiRequset request);

        public Task SaveBookingInfo(PnrCommitRequest request, string error, string pnrNumber);

        public Task<bool> UpdatePaymentStatus(string sessionId, string status);

        public Task<List<PassengerInfo>>? GetPassengerInfo(string sessionId);

        public Task<FlightInfo>? GetFlightInfo(string sessionId);

        public Task<bool> UpdateEmailStatus(string sessionId, bool status);

        public Task<bool> GetEmailStatus(string sessionId);
        public Task<BookingInfo>? GetBookingInfo(string sessionId);

        public Task<string> GetLastSessionId();


    }
}
