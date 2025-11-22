using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MongoDB.Driver;
using Ticket_Booking_System.Models;

namespace Ticket_Booking_System.Repositories
{
    public class ListTicketMangement
    {
        public class TripRepository : IListTicketManagement.ITripRepository
        {
            private readonly IMongoCollection<Trip> _trips;

            public TripRepository(IMongoDatabase database)
            {
                _trips = database.GetCollection<Trip>("Trip");
            }

            public async Task<List<Trip>> GetAllAsync()
            {
                return await _trips.Find(_ => true).ToListAsync();
            }

            public async Task<Trip> GetByIdAsync(string tripId)
            {
                return await _trips.Find(t => t.TripID == tripId).FirstOrDefaultAsync();
            }

            public async Task<List<Trip>> GetUpcomingTripsAsync(DateTime fromDate)
            {
                var filter = Builders<Trip>.Filter.Gte(t => t.DepartureTime, fromDate);
                return await _trips.Find(filter).ToListAsync();
            }

            public async Task<bool> UpdateAsync(Trip trip)
            {
                var filter = Builders<Trip>.Filter.Eq(t => t.TripID, trip.TripID);
                var result = await _trips.ReplaceOneAsync(filter, trip);
                return result.IsAcknowledged && result.ModifiedCount > 0;
            }
        }

        public class BillRepository : IListTicketManagement.IBillRepository
        {
            private readonly IMongoCollection<Bill> _bills;

            public BillRepository(IMongoDatabase database)
            {
                _bills = database.GetCollection<Bill>("Bill");
            }

            public async Task<List<Bill>> GetAllAsync()
            {
                return await _bills.Find(_ => true).ToListAsync();
            }

            public async Task<Bill> GetByIdAsync(string billId)
            {
                return await _bills.Find(b => b.BillID == billId).FirstOrDefaultAsync();
            }

            public async Task<Bill> GetByTicketIdAsync(string ticketId)
            {
                var filter = Builders<Bill>.Filter.ElemMatch(
                    b => b.ListItem,
                    item => item.TicketID == ticketId
                );
                return await _bills.Find(filter).FirstOrDefaultAsync();
            }

            public async Task<List<Bill>> GetBillsByDateAsync(DateTime date)
            {
                var startOfDay = date.Date;
                var endOfDay = startOfDay.AddDays(1);

                var filter = Builders<Bill>.Filter.And(
                    Builders<Bill>.Filter.Gte(b => b.CreateAt, startOfDay),
                    Builders<Bill>.Filter.Lt(b => b.CreateAt, endOfDay)
                );

                return await _bills.Find(filter).ToListAsync();
            }
        }
    }
}