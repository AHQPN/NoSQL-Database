using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Ticket_Booking_System.Models;
using Ticket_Booking_System.Repositories;
using Ticket_Booking_System.Infrastructure;

namespace Ticket_Booking_System.Controllers
{
    public class TicketController : Controller
    {
        private const int PendingTtl = 900;
        // GET: Ticket
        private readonly IUserRepository _userRepository;
        private readonly ITripRepository _tripRepository;
        private readonly IBillRepository _billRepository;
        //private readonly IVehicleRepository _vehicleRepository;
        public TicketController()
        {
            var dbContext = new MongoDbContext();
            _userRepository = new UserRepository(dbContext.User.Database);
            _tripRepository = new TripRepository(dbContext.Trip.Database);
            _billRepository = new BillRepository(dbContext.Bill.Database);

            try
            {
                RedisManager.Initialize("localhost:6379");
            }
            catch 
            {
            }
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult FindTicket()
        {
            if (Session["UserID"] ==null)
            {
                TempData["ShowLogin"] = true;
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        public ActionResult KQTraCuuVe()
        {
            return View();
        }

        // Trang đặt vé
        public async Task<ActionResult> Book_Ticket(string tripID)
        {
            TempData.Clear();

            await _tripRepository.ReleaseExpiredPendingSeatsAsync(tripID);

            Trip trip = await _tripRepository.GetByIdAsync(tripID);
            string customerID = Session["UserID"] as string;
            User usr = await _userRepository.GetByIdAsync(customerID);
            ViewBag.userInfo = usr;

            var reserved = await RedisManager.GetReservedSeatsAsync(tripID);
            ViewBag.RedisReservedSeats = reserved;

            return View(trip);
        }

        // Xử lý đặt ghế
        [HttpGet]
        public async Task<ActionResult> Handle_Book_Ticket(string tripID, string seats, string fullname, string phone, double total)
        {
            await _tripRepository.ReleaseExpiredPendingSeatsAsync(tripID);

            if (string.IsNullOrEmpty(tripID) || string.IsNullOrEmpty(seats))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Thiếu thông tin chuyến đi hoặc ghế.");

            var seatList = seats.Split(',').Select(s => s.Trim()).ToList();
            var bookingId = Guid.NewGuid().ToString("N");

            var reserved = await RedisManager.ReserveSeatsAsync(tripID, seatList, bookingId, TimeSpan.FromSeconds(PendingTtl));
            var failed = seatList.Except(reserved).ToList();

            if (reserved.Any())
            {
                await (_tripRepository as TripRepository).MarkSeatsPendingInMongoAsync(tripID, reserved.ToList());
            }

            var bookingResult = new BookingResultViewModel
            {
                TripID = tripID,
                BookingID = bookingId,
                BookedSeats = reserved.ToList(),
                FailedSeats = failed,
                ExpireTime = DateTime.Now.AddMinutes(15)
            };

            if (reserved.Any())
            {
                return RedirectToAction("ThanhToan", new
                {
                    tripID,
                    seats = string.Join(",", reserved),
                    fullname,
                    phone,
                    total
                });
            }

            TempData["Message"] = "Tất cả ghế đã được đặt trước.";
            TempData["MessageType"] = "error";
            return View("BookingResult", bookingResult);
        }

        // Trang thanh toán
        public async Task<ActionResult> ThanhToan(string tripID, string seats, string fullname, string phone, double total)
        {
            var trip = await _tripRepository.GetByIdAsync(tripID);
            if (trip == null) return HttpNotFound("Không tìm thấy chuyến xe.");

            var seatList = seats?.Split(',').Select(s => s.Trim()).ToList() ?? new List<string>();
            var bookedTickets = trip.ListTicket
                .Where(t => seatList.Contains(t.SeatNum) && t.Status == "Pending")
                .ToList();

            ViewBag.Trip = trip;
            ViewBag.Seats = seatList;
            ViewBag.FullName = fullname;
            ViewBag.Phone = phone;
            ViewBag.Total = total;
            ViewBag.BookedTickets = bookedTickets;

            return View("~/Views/Pay/ThanhToan.cshtml");
        }

        // Xác nhận thanh toán
        [HttpPost]
        public async Task<ActionResult> PaymentConfirm(string tripID, string seats, string fullname, string phone, string bookingId, string action)
        {
            if (string.IsNullOrEmpty(tripID) || string.IsNullOrEmpty(seats))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var seatList = seats.Split(',').Select(s => s.Trim()).ToList();
            var trip = await _tripRepository.GetByIdAsync(tripID);
            if (trip == null) return HttpNotFound("Không tìm thấy chuyến xe.");

            if (action == "cancel")
            {
                await RedisManager.ReleaseSeatsAsync(tripID, seatList, bookingId);
                await _tripRepository.UpdateSeatStatusAsync(tripID, seatList, "Pending", "Available");
                
                TempData["Message"] = "Đã hủy đặt vé.";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Index", "Home");
            }

            await RedisManager.ReleaseSeatsAsync(tripID, seatList, bookingId);

            await _tripRepository.UpdateSeatStatusAsync(tripID, seatList, "Pending", "Booked");

            var userID = Session["UserID"] as string;
            var user = !string.IsNullOrEmpty(userID) ? await _userRepository.GetByIdAsync(userID) : null;

            var bill = new Bill
            {
                BillID = "BILL" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),

                CreateAt = DateTime.UtcNow.AddHours(7),
                Quantity = seatList.Count,
                Total = seatList.Count * trip.Price,
                PaymentStatus = "Paid",
                Customer = new CustomerInfo
                {
                    CustomerID = user?.UserID ?? "Guest",
                    Name = fullname,
                    PhoneNum = phone
                },
                ListItem = trip.ListTicket
                    .Where(t => seatList.Contains(t.SeatNum))
                    .Select(t => new TicketItem
                    {
                        TicketID = t.TicketID,
                        SeatNum = t.SeatNum,
                        Status = "Booked"
                    }).ToList(),
                TripInfo = new TripInfo
                {
                    TripID = trip.TripID,
                    TripName = trip.TripName,
                    DepartureTime = trip.DepartureTime,
                    ArrivalTime = trip.ArrivalTime,
                    Price = trip.Price
                }
            };

            await _billRepository.CreateAsync(bill);

            TempData["Message"] = "Thanh toán thành công!";
            TempData["MessageType"] = "success";
            return RedirectToAction("PaymentSuccess", new { billID = bill.BillID });
        }

        public async Task<ActionResult> PaymentSuccess(string billID)
        {
            var billRepo = new BillRepository(new MongoDbContext().Bill.Database);
            var bill = await billRepo.GetByIdAsync(billID);

            if (bill == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn thanh toán.";
                return RedirectToAction("Index", "Home");
            }

            return View("~/Views/Pay/PaymentSuccess.cshtml", bill);
        }

        public async Task<ActionResult> RollbackBooking(string seats, string tripId)
        {
            if (string.IsNullOrEmpty(tripId) || string.IsNullOrEmpty(seats))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);


            var seatList = seats.Split(',').Select(s => s.Trim()).ToList();


            await RedisManager.ReleaseSeatsAsync(tripId, seatList);
            await _tripRepository.UpdateSeatStatusAsync(tripId, seatList, "Pending", "Available");


            TempData["Message"] = "Vé đã được hủy do hết thời gian thanh toán.";
            TempData["MessageType"] = "danger";
            return RedirectToAction("Book_Ticket", new { tripID = tripId });
        }
    }
}