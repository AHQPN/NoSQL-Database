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

namespace Ticket_Booking_System.Controllers
{
    public class TicketController : Controller
    {
        // GET: Ticket
        private readonly IUserRepository _userRepository;
        private readonly ITripRepository _tripRepository;
        //private readonly IVehicleRepository _vehicleRepository;
        public TicketController()
        {
            var dbContext = new MongoDbContext();
            _userRepository = new UserRepository(dbContext.User.Database);
            _tripRepository = new TripRepository(dbContext.Trip.Database);

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
        public async Task<ActionResult> Book_Ticket(string tripID)
        {
            TempData.Clear();
            Trip trip = await _tripRepository.GetByIdAsync(tripID);
            string customerID = Session["UserID"] as string;
            User usr = await _userRepository.GetByIdAsync(customerID);
            ViewBag.userInfo = usr;

            return View(trip);
        }

        [HttpGet]
        public async Task<ActionResult> Handle_Book_Ticket(string tripID, string seats, string fullname, string phone, double total)
        {
            if (string.IsNullOrEmpty(tripID) || string.IsNullOrEmpty(seats))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Thiếu thông tin chuyến đi hoặc ghế.");
            }

            var seatList = seats.Split(',').Select(s => s.Trim()).ToList();

            Trip trip = await _tripRepository.GetByIdAsync(tripID);
            if (trip == null)
            {
                return HttpNotFound("Không tìm thấy chuyến đi.");
            }

            var models = new List<WriteModel<Trip>>();
            var bookedSeats = new List<string>();
            var failedSeats = new List<string>();

            // Duyệt từng ghế để đặt
            foreach (var seat in seatList)
            {
                var filter = Builders<Trip>.Filter.And(
                    Builders<Trip>.Filter.Eq(t => t.TripID, tripID),
                    Builders<Trip>.Filter.ElemMatch(t => t.ListTicket,
                        tk => tk.SeatNum == seat && tk.Status != "Booked")
                );

                var update = Builders<Trip>.Update
                    .Set("ListTicket.$.Status", "Pending")
                    .Set("ListTicket.$.TicketID", "TK" + Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper())
                    .Inc(t => t.RemainingSeats, -1);

                models.Add(new UpdateOneModel<Trip>(filter, update));
            }

            // Thực hiện cập nhật hàng loạt trong MongoDB
            var bulkResult = await _tripRepository.BulkWriteAsync(models);

            // Kiểm tra kết quả cập nhật
            if (bulkResult.ModifiedCount < seatList.Count)
            {
                trip = await _tripRepository.GetByIdAsync(tripID);

                foreach (var seat in seatList)
                {
                    var ticket = trip.ListTicket.FirstOrDefault(t => t.SeatNum == seat);
                    if (ticket != null && ticket.Status == "Booked")
                        bookedSeats.Add(seat);
                    else
                        failedSeats.Add(seat);
                }
            }
            else
            {
                bookedSeats.AddRange(seatList);
            }

            // Tạo đối tượng kết quả đặt vé
            var bookingResult = new BookingResultViewModel
            {
                TripID = tripID,
                BookingID = Guid.NewGuid().ToString("N"),
                BookedSeats = bookedSeats,
                FailedSeats = failedSeats,
                ExpireTime = DateTime.Now.AddMinutes(15)
            };

            // Nếu có ghế đặt thành công → chuyển sang trang thanh toán
            if (bookedSeats.Count > 0)
            {
                return RedirectToAction("ThanhToan", "Ticket", new
                {
                    tripID = tripID,
                    seats = string.Join(",", bookedSeats),
                    fullname = fullname ?? "",
                    phone = phone ?? "",
                    total = total
                });
            }
            else
            {
                // Nếu tất cả ghế đều thất bại → hiển thị kết quả đặt vé
                return View("BookingResult", bookingResult);
            }
        }

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

        [HttpPost]
        public async Task<ActionResult> PaymentConfirm(string tripID, string seats, string fullname, string phone, string action)
        {
            var seatList = seats?.Split(',').Select(s => s.Trim()).ToList() ?? new List<string>();
            var trip = await _tripRepository.GetByIdAsync(tripID);
            if (trip == null) return HttpNotFound("Không tìm thấy chuyến xe.");

            var billRepo = new BillRepository(new MongoDbContext().Bill.Database);

            // 🟡 Hủy vé
            if (action == "cancel")
            {
                foreach (var seat in seatList)
                {
                    var ticket = trip.ListTicket.FirstOrDefault(t => t.SeatNum == seat);
                    if (ticket != null && ticket.Status == "Pending")
                    {
                        ticket.Status = "Available";
                        trip.RemainingSeats++;
                    }
                }

                var updateCancel = Builders<Trip>.Update
                    .Set(t => t.ListTicket, trip.ListTicket)
                    .Set(t => t.RemainingSeats, trip.RemainingSeats);

                var filter = Builders<Trip>.Filter.Eq(t => t.TripID, trip.TripID);
                await _tripRepository.UpdateAsync(filter, updateCancel);

                TempData["Message"] = "Đã hủy đặt vé.";
                TempData["MessageType"] = "warning";

                return RedirectToAction("Index", "Home");
            }

            // 🟢 Xác nhận thanh toán
            foreach (var seat in seatList)
            {
                var ticket = trip.ListTicket.FirstOrDefault(t => t.SeatNum == seat);
                if (ticket != null && ticket.Status == "Pending")
                {
                    ticket.Status = "Booked";
                }
            }

            // 🟢 Cập nhật trạng thái vé & số ghế trống
            trip.RemainingSeats = trip.ListTicket.Count(t => t.Status != "Booked");

            var updatePaid = Builders<Trip>.Update
                .Set(t => t.ListTicket, trip.ListTicket)
                .Set(t => t.RemainingSeats, trip.RemainingSeats);

            var filterPaid = Builders<Trip>.Filter.Eq(t => t.TripID, trip.TripID);
            await _tripRepository.UpdateAsync(filterPaid, updatePaid);

            // ✅ Lấy thông tin user hoặc khách vãng lai
            var userID = Session["UserID"] as string;
            User user = null;
            if (!string.IsNullOrEmpty(userID))
                user = await _userRepository.GetByIdAsync(userID);

            // ✅ Tạo hóa đơn
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
                        Status = t.Status
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

            await billRepo.CreateAsync(bill);

            // ✅ Trả về trang hóa đơn
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

        //public ActionResult BookingResult()
        //{

        //    return View();
        //}

        public async Task<ActionResult> RollbackBooking(string seats, string tripId)
        {
            if (string.IsNullOrEmpty(tripId) || string.IsNullOrEmpty(seats))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var trip = await _tripRepository.GetByIdAsync(tripId);
            if (trip == null) return HttpNotFound("Không tìm thấy chuyến đi.");

            var seatList = seats.Split(',').Select(s => s.Trim()).ToList();
            foreach (var seat in seatList)
            {
                var ticket = trip.ListTicket.FirstOrDefault(t => t.SeatNum == seat);
                if (ticket != null && (ticket.Status == "Pending" || ticket.Status == "Booked"))
                {
                    ticket.Status = "Available";
                    trip.RemainingSeats++;
                }
            }

            // Dùng Builders.Update để cập nhật chính xác vào MongoDB
            var update = Builders<Trip>.Update
                .Set(t => t.ListTicket, trip.ListTicket)
                .Set(t => t.RemainingSeats, trip.RemainingSeats);

            var filter = Builders<Trip>.Filter.Eq(t => t.TripID, trip.TripID);
            await _tripRepository.UpdateAsync(filter, update);

            TempData["Message"] = "Vé đã được hủy do hết thời gian thanh toán.";
            return RedirectToAction("Book_Ticket", new { tripID = trip.TripID });
        }
    }
}