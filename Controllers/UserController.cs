using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Ticket_Booking_System.Models;
using Ticket_Booking_System.Repositories;
namespace Ticket_Booking_System.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController()
        {
            var dbContext = new MongoDbContext();
            _userRepository = new UserRepository(dbContext.User.Database);
        }

        // fake collection
        [HttpPost]
        public async Task<ActionResult> InsertDefaultUsers()
        {
            string rawPassword = "123";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);

            var users = new List<User>
            {
                new User
                {
                    Id = "admin01",
                    Name = "Admin",
                    Password = hashedPassword,
                    Role = "Admin"
                },
                new User
                {
                    Id = "customer01",
                    Name = "Customer",
                    Password = hashedPassword,
                    Address = "HCM",
                    Image = "",
                    Role = "Customer"
                },
                new User
                {
                    Id = "driver01",
                    Name = "Driver",
                    Password = hashedPassword,
                    Address = "Da Nang",
                    Sex = "Male",
                    Status = "Active",
                    Image = "",
                    Role = "Driver"
                },
                new User
                {
                    Id = "agent01",
                    Name = "Ticket Agent",
                    Password = hashedPassword,
                    Address = "Can Tho",
                    Sex = "Female",
                    Status = "Active",
                    Image = "",
                    Role = "TicketAgent"
                }
            };

            foreach (var user in users)
            {
                await _userRepository.AddAsync(user);
            }

            return Content("Inserted 4 default users with hashed password.");
        }
    }
}