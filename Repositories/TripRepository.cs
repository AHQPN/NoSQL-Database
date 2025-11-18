using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ticket_Booking_System.Models;
using System.Linq;

namespace Ticket_Booking_System.Repositories
{
    public class TripRepository : ITripRepository
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

        public async Task<Trip> GetByIdAsync(string id)
        {
            return await _trips.Find(t => t.TripID == id).FirstOrDefaultAsync();
        }

        public async Task AddAsync(Trip trip)
        {
            await _trips.InsertOneAsync(trip);
        }

        public async Task UpdateAsync(FilterDefinition<Trip> filter, UpdateDefinition<Trip> updateDefinition)
        {
            await _trips.UpdateOneAsync(filter, updateDefinition);
        }
        public async Task<BulkWriteResult<Trip>> BulkWriteAsync(IEnumerable<WriteModel<Trip>> models)
        {
            return await _trips.BulkWriteAsync(models);
        }

        // Cập nhật trạng thái ghế sang Pending
        public async Task<List<string>> MarkSeatsPendingAsync(string tripId, List<string> seatNums)
        {
            if (seatNums == null || seatNums.Count == 0)
                return new List<string>();

            var now = DateTime.UtcNow;

            var filter = Builders<Trip>.Filter.And(
                Builders<Trip>.Filter.Eq(t => t.TripID, tripId),
                Builders<Trip>.Filter.ElemMatch(t => t.ListTicket,
                    tk => seatNums.Contains(tk.SeatNum) && tk.Status == "Available")
            );

            var update = Builders<Trip>.Update
                .Set("ListTicket.$[elem].Status", "Pending")
                .Set("ListTicket.$[elem].PendingAt", now)
                .Set("ListTicket.$[elem].TicketID", "TK" + ObjectId.GenerateNewId().ToString().Substring(0, 5).ToUpper());

            var arrayFilter = new[]
            {
                new JsonArrayFilterDefinition<BsonDocument>(
                    "{ 'elem.SeatNum': { $in: " + seatNums.ToJson() + " }, 'elem.Status': 'Available' }")
            };

            var options = new UpdateOptions { ArrayFilters = arrayFilter };

            var result = await _trips.UpdateOneAsync(filter, update, options);

            if (result.ModifiedCount == 0)
                return new List<string>();

            var trip = await GetByIdAsync(tripId);
            if (trip == null) return new List<string>();

            var updatedSeats = trip.ListTicket
                .Where(t => seatNums.Contains(t.SeatNum) && t.Status == "Pending")
                .Select(t => t.SeatNum)
                .ToList();

            if (updatedSeats.Count > 0)
            {
                var decUpdate = Builders<Trip>.Update.Inc(t => t.RemainingSeats, -updatedSeats.Count);
                var filterTrip = Builders<Trip>.Filter.Eq(t => t.TripID, tripId);
                await _trips.UpdateOneAsync(filterTrip, decUpdate);
            }

            return updatedSeats;
        }

        // Cập nhật trạng thái ghế
        public async Task UpdateSeatStatusAsync(string tripId, List<string> seatNums, string fromStatus, string toStatus)
        {
            if (seatNums == null || seatNums.Count == 0) return;

            var filter = Builders<Trip>.Filter.Eq(t => t.TripID, tripId);
            var update = Builders<Trip>.Update
                .Set("ListTicket.$[elem].Status", toStatus)
                .Set("ListTicket.$[elem].BookingDate", DateTime.UtcNow);

            var arrayFilter = new[]
            {
                new JsonArrayFilterDefinition<BsonDocument>(
                    "{ 'elem.SeatNum': { $in: " + seatNums.ToJson() + " }, 'elem.Status': '" + fromStatus + "' }")
            };

            var options = new UpdateOptions { ArrayFilters = arrayFilter };
            var result = await _trips.UpdateOneAsync(filter, update, options);

            if (toStatus == "Available" && result.ModifiedCount > 0)
            {
                // Xóa BookingDate
                var unsetBookingDate = Builders<Trip>.Update
                    .Unset("ListTicket.$[elem].BookingDate");

                await _trips.UpdateOneAsync(filter, unsetBookingDate, new UpdateOptions
                {
                    ArrayFilters = arrayFilter
                });

                var trip = await GetByIdAsync(tripId);
                if (trip != null)
                {
                    var availableNow = trip.ListTicket
                        .Count(t => seatNums.Contains(t.SeatNum) && t.Status == "Available");

                    if (availableNow > 0)
                    {
                        var inc = Builders<Trip>.Update.Inc(t => t.RemainingSeats, availableNow);
                        await _trips.UpdateOneAsync(filter, inc);
                    }
                }
            }

            if (toStatus == "Booked" && result.ModifiedCount > 0)
            {
                var trip = await GetByIdAsync(tripId);
                if (trip != null)
                {
                    var remaining = trip.ListTicket.Count(t => t.Status != "Booked");
                    var setRemaining = Builders<Trip>.Update.Set(t => t.RemainingSeats, remaining);
                    await _trips.UpdateOneAsync(filter, setRemaining);
                }
            }
        }

        // Tự động giải phóng ghế Pending quá 15 phút
        public async Task ReleaseExpiredPendingSeatsAsync(string tripId)
        {
            var expireTime = DateTime.UtcNow.AddMinutes(-15);

            var filter = Builders<Trip>.Filter.And(
                Builders<Trip>.Filter.Eq(t => t.TripID, tripId),
                Builders<Trip>.Filter.ElemMatch(t => t.ListTicket,
                    tk => tk.Status == "Pending" && tk.PendingAt < expireTime)
            );

            var update = Builders<Trip>.Update
                .Set("ListTicket.$[elem].Status", "Available")
                .Unset("ListTicket.$[elem].PendingAt");

            var arrayFilter = new[]
            {
                new JsonArrayFilterDefinition<BsonDocument>(
                    "{ 'elem.Status': 'Pending', 'elem.PendingAt': { $lt: ISODate('" + expireTime.ToString("o") + "') } }")
            };

            var options = new UpdateOptions { ArrayFilters = arrayFilter };

            var result = await _trips.UpdateOneAsync(filter, update, options);

            if (result.ModifiedCount > 0)
            {
                var trip = await GetByIdAsync(tripId);
                if (trip != null)
                {
                    var remaining = trip.ListTicket.Count(t => t.Status == "Available");
                    var updateRemain = Builders<Trip>.Update.Set(t => t.RemainingSeats, remaining);
                    await _trips.UpdateOneAsync(Builders<Trip>.Filter.Eq(t => t.TripID, tripId), updateRemain);
                }
            }
        }

        public async Task<List<Ticket>> GetTicketsByDateAsync(DateTime date)
        {
            var trips = await GetAllAsync();

            var tickets = trips.SelectMany(trip => trip.ListTicket
                .Where(t => t.Status == "Booked" &&
                            t.BookingDate.HasValue &&
                            t.BookingDate.Value.Date == date.Date)
                .Select(t =>
                {
                    t.TripId = trip.TripID; 
                    return t;
                })
            ).ToList();

            return tickets;
        }
    }
}
