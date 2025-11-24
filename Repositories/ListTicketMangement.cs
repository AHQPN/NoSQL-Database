using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MongoDB.Driver;
using Ticket_Booking_System.Models;
using static Ticket_Booking_System.Repositories.IListTicketManagement;

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
                //var filter = Builders<Trip>.Filter.Gte(t => t.DepartureTime, fromDate);

                //var dateOnly = fromDate.Date;
                //var filter = Builders<Trip>.Filter.Gte(t => t.DepartureTime, dateOnly);
                //return await _trips.Find(filter).ToListAsync();

                try
                {
                    var utcDate = fromDate.Kind == DateTimeKind.Local
                        ? fromDate.ToUniversalTime()
                        : fromDate;

                    var dateOnly = utcDate.Date;

                    System.Diagnostics.Debug.WriteLine($"Local date: {fromDate}");
                    System.Diagnostics.Debug.WriteLine($"UTC date: {utcDate}");
                    System.Diagnostics.Debug.WriteLine($"Filter with: {dateOnly}");

                    var filter = Builders<Trip>.Filter.Gte(t => t.DepartureTime, dateOnly);
                    var trips = await _trips.Find(filter).ToListAsync();

                    System.Diagnostics.Debug.WriteLine($"Found {trips.Count} trips");

                    return trips ?? new List<Trip>();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"GetUpcomingTripsAsync Error: {ex.Message}");
                    throw;
                }
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
        public class StationRepository : IListTicketManagement.IStationRepository
        {
            private readonly IMongoCollection<Station> _stationCollection;

            public StationRepository(IMongoDatabase database)
            {
                _stationCollection = database.GetCollection<Station>("Station");
            }

            public async Task<List<Station>> GetAllAsync()
            {
                return await _stationCollection.Find(_ => true).ToListAsync();
            }

            public async Task<Station> GetByIdAsync(string stationId)
            {
                return await _stationCollection
                    .Find(s => s.StationID == stationId)
                    .FirstOrDefaultAsync();
            }

            public async Task AddAsync(Station station)
            {
                await _stationCollection.InsertOneAsync(station);
            }

            public async Task UpdateAsync(Station station)
            {
                await _stationCollection.ReplaceOneAsync(
                    s => s.StationID == station.StationID,
                    station
                );
            }

            public async Task DeleteAsync(string stationId)
            {
                await _stationCollection.DeleteOneAsync(s => s.StationID == stationId);
            }
        }
    }
}