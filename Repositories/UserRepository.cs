using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ticket_Booking_System.Models;

namespace Ticket_Booking_System.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users");
        }

        public async Task<IEnumerable<User>> GetAllAsync() =>
            await _users.Find(FilterDefinition<User>.Empty).ToListAsync();

        public async Task<User> GetByIdAsync(string id) =>
            await _users.Find(u => u.UserID == id).FirstOrDefaultAsync();

        public async Task AddAsync(User user) =>
            await _users.InsertOneAsync(user);

        public async Task UpdateAsync(string id, User user) =>
            await _users.ReplaceOneAsync(u => u.UserID == id, user);

        

        

        public async Task<bool> IsPhoneExistAsync(string phone) =>
            await _users.Find(u => u.PhoneNum == phone).AnyAsync();

        public async Task<int> CountNumUser()
        {
            
            var cnt = await _users.CountDocumentsAsync(FilterDefinition<User>.Empty);
            return (int)cnt;
        }

        public async Task<User> GetByPhoneAndPasswordAsync(string phone, string rawPassword)
        {
            
            var user = await _users.Find(u => u.PhoneNum == phone).FirstOrDefaultAsync();

            
            if (user == null || !BCrypt.Net.BCrypt.Verify(rawPassword, user.Password))
                return null;

            return user;
        }
    }
}
