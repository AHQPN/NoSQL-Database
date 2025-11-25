using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ticket_Booking_System.Models;

namespace Ticket_Booking_System.Repositories
{
    public interface IBillRepository
    {
        Task CreateAsync(Bill bill);
        Task<Bill> GetByIdAsync(string billID);
        Task<List<Bill>> GetByCustomerIdAsync(string customerId);
        //Task<Bill> GetByBookingIdAsync(string bookingId);
        Task UpdateAsync(Bill bill);
    }
}