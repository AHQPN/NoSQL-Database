using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ticket_Booking_System.Models;

namespace Ticket_Booking_System.Repositories
{
    public class BillRepository : IBillRepository
    {
        private readonly IMongoCollection<Bill> _bills;

        public BillRepository(IMongoDatabase database)
        {
            _bills = database.GetCollection<Bill>("Bill");
        }

        public async Task CreateAsync(Bill bill)
        {
            await _bills.InsertOneAsync(bill);
        }

        public async Task<Bill> GetByIdAsync(string billID)
        {
            return await _bills
                .Find(b => b.BillID == billID)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Bill>> GetByCustomerIdAsync(string customerId)
        {
            return await _bills.Find(b => b.Customer.CustomerID == customerId).ToListAsync();
        }

        public async Task<Bill> GetByBookingIdAsync(string bookingId)
        {
            return await _bills.Find(b => b.BookingID == bookingId).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Bill bill)
        {
            var filter = Builders<Bill>.Filter.Eq(b => b.BillID, bill.BillID);
            await _bills.ReplaceOneAsync(filter, bill);
        }

    }
}
