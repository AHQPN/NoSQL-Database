using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Ticket_Booking_System.Models;

namespace Ticket_Booking_System.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly IMongoCollection<Vehicle> _vehicles;

        public VehicleRepository(IMongoDatabase database)
        {
            _vehicles = database.GetCollection<Vehicle>("Vehicles");
        }

        public async Task<List<Vehicle>> GetAllAsync()
        {
            return await _vehicles.Find(_ => true).ToListAsync();
        }

        public async Task<Vehicle> GetByIdAsync(string id)
        {
            return await _vehicles.Find(v => v.Id.ToString() == id).FirstOrDefaultAsync();
        }

        public async Task AddAsync(Vehicle vehicle)
        {
            await _vehicles.InsertOneAsync(vehicle);
        }

        public async Task UpdateAsync(string id, Vehicle vehicle)
        {
            await _vehicles.ReplaceOneAsync(v => v.Id.ToString() == id, vehicle);
        }

        public async Task DeleteAsync(string id)
        {
            await _vehicles.DeleteOneAsync(v => v.Id.ToString() == id);
        }
    }
}
