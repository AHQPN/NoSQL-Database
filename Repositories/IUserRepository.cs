using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ticket_Booking_System.Models;

namespace Ticket_Booking_System.Repositories
{
    public interface IUserRepository: IRepository<User>
    {
        Task<User> GetByPhoneAndPasswordAsync(string phone, string rawPassword);
        Task<int> CountNumUser();
    }
}
