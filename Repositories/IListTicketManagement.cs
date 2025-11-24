using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Ticket_Booking_System.Models;

namespace Ticket_Booking_System.Repositories
{
    public class IListTicketManagement
    {
        public interface ITripRepository
        {
            Task<List<Trip>> GetAllAsync();
            Task<Trip> GetByIdAsync(string tripId);
            Task<List<Trip>> GetUpcomingTripsAsync(DateTime fromDate);
            Task<bool> UpdateAsync(Trip trip);
        }

        public interface IBillRepository
        {
            Task<List<Bill>> GetAllAsync();
            Task<Bill> GetByIdAsync(string billId);
            Task<Bill> GetByTicketIdAsync(string ticketId);
            Task<List<Bill>> GetBillsByDateAsync(DateTime date);
        }
        public interface IStationRepository
        {
            Task<List<Station>> GetAllAsync();
            Task<Station> GetByIdAsync(string stationId);
            Task AddAsync(Station station);
            Task UpdateAsync(Station station);
            Task DeleteAsync(string stationId);
        }
    }
}