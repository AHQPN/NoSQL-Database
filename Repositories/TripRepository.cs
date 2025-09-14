using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Ticket_Booking_System.Models;

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




    }
}
