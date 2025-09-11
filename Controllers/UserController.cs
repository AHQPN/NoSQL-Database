﻿using System.Collections.Generic;
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
                    PhoneNum = "0123456789",
                    Role = "Admin"
                },
                new User
                {
                    Id = "customer01",
                    Name = "Customer",
                    Password = hashedPassword,
                    PhoneNum = "0987654321",
                    Address = "HCM",
                    Email = "customer@example.com",
                    Image = "",
                    Role = "Customer"
                },
                new User
                {
                    Id = "driver01",
                    Name = "Driver",
                    Password = hashedPassword,
                    PhoneNum = "0111222333",
                    Address = "Da Nang",
                    Sex = "Male",
                    Email = "driver@example.com",
                    Status = "Active",
                    Image = "",
                    Role = "Driver"
                },
                new User
                {
                    Id = "agent01",
                    Name = "Ticket Agent",
                    Password = hashedPassword,
                    PhoneNum = "0222333444",
                    Address = "Can Tho",
                    Sex = "Female",
                    Email = "agent@example.com",
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