using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ticket_Booking_System.Models;

namespace Ticket_Booking_System.Repositories
{
    public interface ITripRepository
    {
        Task<List<Trip>> GetAllAsync();
        Task<Trip> GetByIdAsync(string id);
        Task AddAsync(Trip trip);
        Task UpdateAsync(FilterDefinition<Trip> filter, UpdateDefinition<Trip> updateDefinition);
        Task<BulkWriteResult<Trip>> BulkWriteAsync(IEnumerable<WriteModel<Trip>> models);



    }
}
