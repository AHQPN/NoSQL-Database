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
using System.Web.Configuration;

namespace Ticket_Booking_System.Controllers
{
    public class AuthController : Controller
    {
        // GET: Auth
        private readonly IUserRepository _userRepository;
        private readonly MongoDbContext _dbContext;
        public AuthController()
        {
             _dbContext = new MongoDbContext();
            _userRepository = new UserRepository(_dbContext.User.Database);
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

            string sdt = form["numberphone"];
            string rawPassword = form["pw"];
            if (string.IsNullOrEmpty(sdt) || string.IsNullOrEmpty(rawPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tài khoản và mật khẩu!";
                TempData["ShowLogin"] = true;
                return View("Login");
            }

            var usr = await _dbContext.User
                              .Find(u => u.PhoneNum == sdt && u.Password==rawPassword)
                              .FirstOrDefaultAsync();


            if (usr == null )
            {
                ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
                TempData["ShowLogin"] = true;
                return View("Login");
            }

            if (usr.Status != "Active")
            {
                ViewBag.Error = "Tài khoản bạn đã bị đình chỉ";
                TempData["ShowLogin"] = true;
                return View("Login");
            }

            Session["UserID"] = usr.UserID;
            Session["UserName"] = usr.Name;
            Session["Role"] = usr.Role;

            Session["Avatar"] = string.IsNullOrEmpty(usr.Image)
                ? "/Content/images/default-avatar.png"
                : usr.Image;

            switch (usr.Role)
            {
                case "TicketAgent": return RedirectToAction("Dashboard", "TicketAgent");
                case "Admin": return RedirectToAction("ThongKe", "Admin");
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

            var _users = await _dbContext.User.Find(_ => true).ToListAsync();
            Regex regex = new Regex(@"^\w+([-.+']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            User usr = new User();
            string userID = DateTime.Now.ToString("ddMMyy");
            int cnt = await _userRepository.CountNumUser();
            string formattedNumber = (cnt + 1).ToString("D4");
            userID = userID + formattedNumber;
            usr.UserID = userID;
            usr.Name = a["tenkh"];
            usr.PhoneNum = a["numberphone"];
            usr.Address = a["Address"];
            usr.Password = a["pw"];
            usr.Role = "Customer";
            var existingUser = _users.FirstOrDefault(u => u.PhoneNum == usr.PhoneNum);
            if (string.IsNullOrEmpty(usr.Name) || string.IsNullOrEmpty(usr.PhoneNum) || string.IsNullOrEmpty(usr.Password) || string.IsNullOrEmpty(usr.PhoneNum) || string.IsNullOrEmpty(usr.Address))
            {
                return ErrorRegister("Không để trống các trường.",usr);
            }
            else if (existingUser != null)
            {
                return ErrorRegister("Email này đã được đăng ký trước đó.", usr);
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
                await _dbContext.User.InsertOneAsync(usr);
                TempData["Success"] = "Đăng ký thành công! Hãy đăng nhập.";
                TempData["ShowRegister"] = true;
                return View("SignUp");
            }
            return View();
        }
        public ActionResult Logout()
        {
            Session["UserID"] = null;
            Session["Role"] = null;

            // Nếu muốn clear toàn bộ session:
            // Session.Clear();
            // Session.Abandon();

            return RedirectToAction("Index", "Home"); // quay về trang chủ
        }
    }
}