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

        public async Task<ActionResult> LichSuVe()
        {
            if (Session["UserID"] == null)
            {
                TempData["ShowLogin"] = true;
                return RedirectToAction("Index", "Home");
            }

            string userId = Session["UserID"].ToString();

            var bills = await _dbContext.Bill
                .Find(b => b.Customer.CustomerID == userId)
                .ToListAsync();

            // Map sang TicketHistoryViewModel
            var history = bills.Select(b => new TicketHistoryViewModel
            {
                BillID = b.BillID,
                CreateAt = b.CreateAt,
                Quantity = b.Quantity,
                Total = b.Total,
                Tickets = b.ListItem,
                TripInfo = b.TripInfo,
                TripID = b.TripInfo?.TripID,
                Status = b.Status,
                PaymentStatus = b.PaymentStatus

            }).ToList();

            return View(history);
        }

        public async Task<ActionResult> MuaLai(string billID)
        {
            if (Session["UserID"] == null)
            {
                TempData["ShowLogin"] = true;
                return RedirectToAction("Index", "Home");
            }

            var db = new MongoDbContext();

            var bill = await db.Bill.Find(b => b.BillID == billID).FirstOrDefaultAsync();
            if (bill == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn.";
                return RedirectToAction("LichSuVe");
            }

            string tripID = bill.TripInfo.TripID;

            // Lấy chuyến hiện tại
            var trip = await db.Trip.Find(t => t.TripID == tripID).FirstOrDefaultAsync();
            if (trip == null || trip.DepartureTime < DateTime.Now)
            {
                TempData["Error"] = "Chuyến này không còn hoạt động.";
                return RedirectToAction("LichSuVe");
            }

            // Lấy danh sách ghế cũ từ Bill
            string oldSeats = string.Join(",", bill.ListItem.Select(i => i.SeatNum));

            return RedirectToAction("Book_Ticket", "Ticket", new { tripID = tripID, oldSeats = oldSeats });
        }
       




    }
}