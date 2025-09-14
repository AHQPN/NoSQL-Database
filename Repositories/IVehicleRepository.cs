using System.Collections.Generic;
using System.Threading.Tasks;
using Ticket_Booking_System.Models;

namespace Ticket_Booking_System.Repositories
{
    public interface IVehicleRepository
    {
        Task<List<Vehicle>> GetAllAsync();
        Task<Vehicle> GetByIdAsync(string id);
        Task AddAsync(Vehicle vehicle);
        Task UpdateAsync(string id, Vehicle vehicle);
        Task DeleteAsync(string id);
    }
}
