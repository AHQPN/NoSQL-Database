using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Ticket_Booking_System.Models;
using Ticket_Booking_System.Models.admin;
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
            var allBills = await _dbContext.Bill.Find(b => true).ToListAsync();

            var tripStats = allBills
                .SelectMany(
                    b => b.ListItem ?? new List<TicketItem>(),
                    (b, ticket) => new
                    {
                        b.TripInfo.TripID,
                        b.TripInfo.TripName,
                        b.TripInfo.Price,
                        ticket.Status
                    })
                .Where(x => x.Status == "Booked")
                .GroupBy(x => new { x.TripID, x.TripName, x.Price })
                .Select(g => new
                {
                    TripID = g.Key.TripID,
                    TripName = g.Key.TripName,
                    SoVeDaBan = g.Count(),
                    DoanhThu = g.Sum(x => (decimal)x.Price)
                })
                .ToList();

            var trips = await _dbContext.Trip.Find(t => true).ToListAsync();

            var result = tripStats.Select(x =>
            {
                var tripInfo = trips.FirstOrDefault(t => t.TripID == x.TripID);
                int tongGhe = tripInfo != null ? tripInfo.Vehicle.Capacity : x.SoVeDaBan;
                double tyLe = tongGhe > 0 ? (double)x.SoVeDaBan / tongGhe * 100 : 0;

                return new ThongKeThuHang
                {
                    LoTrinh = x.TripName,
                    SoVeDaBan = x.SoVeDaBan,
                    DoanhThu = x.DoanhThu,
                    TyLeLapDay = Math.Round(tyLe, 2)
                };
            })
            .OrderByDescending(x => x.SoVeDaBan)
            .ThenByDescending(x => x.DoanhThu)
            .ThenByDescending(x => x.TyLeLapDay)
            .Select((x, index) =>
            {
                x.ThuHang = index + 1;
                return x;
            })
            .ToList();

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
        public async Task<List<KhachHangThanThiet>> GetThongKeKhachHang()
        {
            var now = DateTime.Now;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

            var bills = await _dbContext.Bill.Find(b => true).ToListAsync();

            var khachHang = bills
                .GroupBy(b => b.Customer.CustomerID)
                .Select(g =>
                {
                    var customerBills = g.ToList();


                    var validBills = customerBills.Where(b => b.PaymentStatus == "Paid").ToList();

                    var soLanMua = validBills.Count;


                    var soVeDaDat = validBills.Sum(b => b.ListItem?.Count(t => t.Status == "Booked") ?? 0);
 
                    var tongTien = validBills.Sum(b =>
                        (b.ListItem?.Count(t => t.Status == "Booked") ?? 0) *
                        ((decimal?)(b.TripInfo?.Price) ?? 0m)
                    );
         
                    var diem = (int)(tongTien / 10000);

                    string phanHang = diem >= 200 ? "Kim Cương" : (diem >= 100 ? "Vàng" : "Bạc");

                    var billsInMonth = validBills.Where(b => b.CreateAt >= firstDayOfMonth).ToList();
                    var chiTietChuyen = billsInMonth
                        .SelectMany(b => b.ListItem
                                          .Where(t => t.Status == "Booked")
                                          .Select(t => b.TripInfo?.TripName ?? "Chưa có"))
                        .GroupBy(trip => trip)
                        .Select(g2 => new { TripName = g2.Key, SoVe = g2.Count() })
                        .OrderByDescending(x => x.SoVe)
                        .ToList();

                    var chuyenThuongXuyen = chiTietChuyen.FirstOrDefault()?.TripName ?? "";
                    var soVeChuyen = chiTietChuyen.FirstOrDefault()?.SoVe ?? 0;

                    // Vé hủy tổng (PaymentStatus = "Canceled")
                    var huyVe = customerBills
                        .Where(b => b.PaymentStatus == "Canceled")
                        .SelectMany(b => b.ListItem
                                          .Select(t => new HuyVe
                                          {
                                              TripName = b.TripInfo?.TripName ?? "Chưa có",
                                              SoVe = 1,
                                              CancelDate = b.CreateAt
                                          }))
                        .ToList();

                    // Vé hủy trong tháng
                    var huyVeTrongThang = huyVe
                        .Where(h => h.CancelDate >= firstDayOfMonth)
                        .GroupBy(h => h.TripName)
                        .Select(g2 => new { TripName = g2.Key, SoVe = g2.Count() })
                        .OrderByDescending(x => x.SoVe)
                        .ToList();

                    return new KhachHangThanThiet
                    {
                        CustomerID = g.Key,
                        Name = g.First().Customer?.Name ?? "Chưa có",
                        PhoneNum = g.First().Customer?.PhoneNum ?? "",
                        TongTienDaChi = tongTien,
                        SoLanMua = soLanMua,
                        SoVeDaDat = soVeDaDat,
                        DiemTichLuy = diem,
                        PhanHang = phanHang,
                        ChuyenThuongXuyen = chuyenThuongXuyen,
                        SoVeChuyen = soVeChuyen,
                        ChiTietChuyen = chiTietChuyen.Select(c => $"{c.TripName} ({c.SoVe} vé)").ToList(),
                        HuyVe = huyVe,
                        HuyVeTrongThang = huyVeTrongThang
                            .Select(c => $"{c.TripName} ({c.SoVe} vé hủy)").ToList()
                    };
                })
                .OrderByDescending(x => x.DiemTichLuy)
                .ToList();

            return khachHang;
        }


        public async Task<ActionResult> KhachHangThanThiet()
        {
            if (Session["UserID"] == null)
            {
                TempData["ShowLogin"] = true;
                return RedirectToAction("Index", "Home");
            }

            var model = await GetThongKeKhachHang();
            return View(model);
        }







    }
}