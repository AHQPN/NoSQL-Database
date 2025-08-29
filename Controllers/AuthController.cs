using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ticket_Booking_System.Models;
using BCrypt.Net;
using Ticket_Booking_System.Repositories;
using System.Threading.Tasks;

namespace Ticket_Booking_System.Controllers
{
    public class AuthController : Controller
    {
        // GET: Auth
        private readonly IUserRepository _userRepository;

        public AuthController()
        {
            var dbContext = new MongoDbContext();
            _userRepository = new UserRepository(dbContext.Users.Database);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> Login(FormCollection form)
        {
            string sdt = form["sdt"];
            string rawPassword = form["password"];
            if (string.IsNullOrEmpty(sdt) || string.IsNullOrEmpty(rawPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ số điện thoại và mật khẩu!";
                return View();
            }

            var usr = await _userRepository.GetByPhoneAndPasswordAsync(sdt,rawPassword); // Lấy user theo số điện thoại

            if (usr == null )
            {
                ViewBag.Error = "Số điện thoại hoặc mật khẩu không đúng!";
                return View();
            }

            Session["User"] = usr.UserID;
            Session["Role"] = usr.Role;

            switch (usr.Role)
            {
                case "TicketAgent": return RedirectToAction("TicketAgentSite", "Home");
                case "Admin": return RedirectToAction("AdminSite", "Home");
                case "Driver": return RedirectToAction("DriverSite", "Home");
                default: return RedirectToAction("Index", "Home");
            }
        }


        public async Task<ActionResult> SignUp(FormCollection a)
        {
            User usr = new User();
            string userID = DateTime.Now.ToString("ddMMyy");
            int cnt = await _userRepository.CountNumUser();
            string formattedNumber = (cnt + 1).ToString("D4");
            userID = userID + formattedNumber;
            usr.UserID = userID;
            usr.Name = a["username"];
            usr.PhoneNum = a["Phone"];
            usr.Address = a["Address"];
            string rawPassword = a["pw"];
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);
            usr.Password = hashedPassword;
            usr.Sex = "Name";
            usr.Email = a["email"];


            return View();
        }
    }
}