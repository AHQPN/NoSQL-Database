using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Ticket_Booking_System.Models;
using Ticket_Booking_System.Models.Admin;
using Ticket_Booking_System.Repositories;

namespace Ticket_Booking_System.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly MongoDbContext _dbContext;
        public AdminController()
        {
            _dbContext = new MongoDbContext();
            _userRepository = new UserRepository(_dbContext.User.Database);
        }
        // GET: Admin
        public async Task<ThongKe> GetThongKe()
        {
            var now = DateTime.Now;
            var firstDayOfMonthLocal = new DateTime(now.Year, now.Month, 1, 0, 0, 0);

            var allBills = await _dbContext.Bill.Find(b => true).ToListAsync();
            var billsInMonth = allBills
                .Where(b => b.CreateAt.ToLocalTime() >= firstDayOfMonthLocal)
                .ToList();

            int tongVeBan = billsInMonth.Sum(b => b.ListItem != null
                ? b.ListItem.Count(i => i.Status == "Booked")
                : 0);

            decimal tongDoanhThu = billsInMonth.Sum(b => (decimal)b.Total);

            var trips = await _dbContext.Trip.Find(t => true).ToListAsync();

            int soGheCon = trips.Sum(t => t.RemainingSeats);

            int loTrinhHoatDong = trips.Count(t => t.DepartureTime.ToLocalTime() > now);

            return new ThongKe
            {
                TongVeBanTrongThang = tongVeBan,
                TongDoanhThuTrongThang = tongDoanhThu,
                SoLuongGheConTrong = soGheCon,
                LoTrinhHoatDong = loTrinhHoatDong
            };
        }
        public async Task<List<ThongKeThuHang>> GetTopTrips()
        {
            int top = 3;
            // Lấy tất cả bill
            var allBills = await _dbContext.Bill.Find(b => true).ToListAsync();

            // Nhóm theo TripID và tính số vé đã bán
            var tripStats = allBills
                .SelectMany(b => b.ListItem ?? new List<TicketItem>(), (b, ticket) => new { b.TripInfo.TripID, b.TripInfo.TripName, b.TripInfo.Price, ticket.Status })
                .Where(x => x.Status == "Booked")
                .GroupBy(x => new { x.TripID, x.TripName, x.Price })
                .Select(g => new
                {
                    TripID = g.Key.TripID,
                    TripName = g.Key.TripName,
                    SoVeDaBan = g.Count(),
                    DoanhThu = g.Sum(x => (decimal)x.Price)
                })
                .OrderByDescending(x => x.SoVeDaBan)
                .Take(top)
                .ToList();

            // Lấy thông tin ghế tối đa từng chuyến từ Trip Collection
            var trips = await _dbContext.Trip.Find(t => true).ToListAsync();

            // Chuẩn bị danh sách TripThongKe
            var result = tripStats.Select((x, index) =>
            {
                var tripInfo = trips.FirstOrDefault(t => t.TripID == x.TripID);
                int tongGhe = tripInfo != null ? tripInfo.ListTicket.Count : x.SoVeDaBan; // fallback
                double tyLe = tongGhe > 0 ? (double)x.SoVeDaBan / tongGhe * 100 : 0;

                return new ThongKeThuHang
                {
                    ThuHang = index + 1,
                    LoTrinh = x.TripName,
                    SoVeDaBan = x.SoVeDaBan,
                    DoanhThu = x.DoanhThu,
                    TyLeLapDay = Math.Round(tyLe, 2)
                };
            }).ToList();

            return result;
        }
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> ThongKe()
        {

            if (Session["UserID"] == null)
            {
                TempData["ShowLogin"] = true;
                return RedirectToAction("Index", "Home");
            }
            ViewBag.GetThongKe = await GetThongKe();
            ViewBag.GetTopTrips = await GetTopTrips();
            return View();
        }
    }
}