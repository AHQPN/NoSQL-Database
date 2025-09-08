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
using System.Text.RegularExpressions;

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

            var users = await _userRepository.GetAllAsync();
            string sdt = form["Phone-number"];
            string rawPassword = form["pw"];
            if (string.IsNullOrEmpty(sdt) || string.IsNullOrEmpty(rawPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ số điện thoại và mật khẩu!";
                TempData["ShowLogin"] = true;
                return View("Login");
            }

            var usr = await _userRepository.GetByPhoneAndPasswordAsync(sdt,rawPassword); 

            if (usr == null )
            {
                ViewBag.Error = "Số điện thoại hoặc mật khẩu không đúng!";
                TempData["ShowLogin"] = true;
                return View("Login");
            }

            Session["UserID"] = usr.UserID;
            Session["Role"] = usr.Role;

            switch (usr.Role)
            {
                case "TicketAgent": return RedirectToAction("TicketAgentSite", "Home");
                case "Admin": return RedirectToAction("AdminSite", "Home");
                case "Driver": return RedirectToAction("DriverSite", "Home");
                default: return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult ErrorRegister(string error,User usr)
        {
            ViewBag.Error = error;
            TempData["ShowRegister"] = true;
            return View("SignUp", usr);            
        }
        public async Task<ActionResult> SignUp(FormCollection a)
        {

            var _users = await _userRepository.GetAllAsync();
            Regex regex = new Regex(@"^(0|\+84)([0-9]{9})$");
            User usr = new User();
            string userID = DateTime.Now.ToString("ddMMyy");
            int cnt = await _userRepository.CountNumUser();
            string formattedNumber = (cnt + 1).ToString("D4");
            userID = userID + formattedNumber;
            usr.UserID = userID;
            usr.Name = a["tenkh"];
            usr.PhoneNum = a["Phone"];
            usr.Address = a["Address"];
            usr.Password = a["pw"];
            usr.Role = "Customer";
            var existingUser = _users.FirstOrDefault(u => u.PhoneNum == usr.PhoneNum);
            if (string.IsNullOrEmpty(usr.Name) || string.IsNullOrEmpty(usr.PhoneNum) || string.IsNullOrEmpty(usr.Password) || string.IsNullOrEmpty(usr.PhoneNum) || string.IsNullOrEmpty(usr.Address))
            {
                return ErrorRegister("Không để trống các trường.",usr);
            }
            else if (usr.PhoneNum.Length < 10 || !regex.IsMatch(usr.PhoneNum) ||!usr.PhoneNum.All(char.IsDigit))
            {
                return ErrorRegister("Số điện thoại không đúng định dạng.", usr);
            }
            else if (existingUser != null)
            {
                return ErrorRegister("Số điện thoại đã được đăng ký trước đó.", usr);
            }
            if (a["pw"] != a["confrimed-pw"])
            {
                return ErrorRegister("Mật khẩu xác nhận không khớp.", usr);
            }
            if (usr.Password.Length < 6)
            {
                return ErrorRegister("Mật khẩu phải từ 6 ký tự trở lên.", usr);
            }
            else
            {
                await _userRepository.AddAsync(usr);
                TempData["Success"] = "Đăng ký thành công! Hãy đăng nhập.";
                TempData["ShowRegister"] = true;
                return View("SignUp");
            }
            return View();
        }
    }
}