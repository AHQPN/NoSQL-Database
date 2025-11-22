using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Ticket_Booking_System.Models;
using Ticket_Booking_System.Repositories;

namespace Ticket_Booking_System.Controllers
{
    public class BillController : Controller
    {
        // GET: Bill
        private readonly MongoDbContext _dbContext;
        public BillController()
        {
            _dbContext = new MongoDbContext();
        }
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> TraCuuHoaDon()
        {
            if (Session["UserID"] == null)
            {
                TempData["ShowLogin"] = true;
                return RedirectToAction("Index", "Home");
            }
            var userId = Session["UserID"].ToString();
            // Lọc các hóa đơn của user này
            var filter = Builders<Bill>.Filter.Eq(b => b.Customer.CustomerID, userId);
            var bills = await _dbContext.Bill
         .Find(b => b.Customer.CustomerID == userId)
         .ToListAsync();

            return View(bills);
        }
        [HttpPost]
        public async Task<ActionResult> TraCuuHoaDon(FormCollection a)
        {
            if (Session["UserID"] == null)
            {
                TempData["ShowLogin"] = true;
                return RedirectToAction("Index", "Home");
            }

            string Mahd = a["MaHD"];
            var userId = Session["UserID"].ToString();

            if (string.IsNullOrEmpty(Mahd))
            {
                ViewBag.Error = "Vui lòng nhập mã hóa đơn cần tra cứu.";
                var allBills = await _dbContext.Bill
                    .Find(b => b.Customer.CustomerID == userId)
                    .ToListAsync();
                return View(allBills);
            }

            var bill = await _dbContext.Bill
                .Find(b => b.BillID == Mahd && b.Customer.CustomerID == userId)
                .FirstOrDefaultAsync();

            if (bill == null)
            {
                ViewBag.Error = "Không tìm thấy hóa đơn này.";
                var allBills = await _dbContext.Bill
                    .Find(b => b.Customer.CustomerID == userId)
                    .ToListAsync();
                return View(allBills);
            }

            return View(new List<Bill> { bill });
        }

        public class BillDetailViewModel
        {
            public Bill Bill { get; set; }
            public User Customer { get; set; }
        }
        public async Task<ActionResult> ChiTietHoaDon(string id)
        {
            if (Session["UserID"] == null)
            {
                TempData["ShowLogin"] = true;
                return RedirectToAction("Index", "Home");
            }

            var bill = await _dbContext.Bill.Find(b => b.BillID == id).FirstOrDefaultAsync();
            if (bill == null)
            {
                return HttpNotFound("Không tìm thấy hóa đơn.");
            }

            var customer = await _dbContext.User.Find(u => u.UserID == bill.Customer.CustomerID).FirstOrDefaultAsync();
            ViewBag.Bill = bill;
            ViewBag.Customer = customer;
            return View();
        }


    }
}