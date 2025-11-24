using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Ticket_Booking_System.Models;

namespace Ticket_Booking_System.Controllers
{
    public class TripController : Controller
    {
        private readonly MongoDbContext _context;
        public TripController()
        {
            _context = new MongoDbContext();
        }
        // GET: Trip
        [ChildActionOnly]
        public ActionResult PopularTrip()
        {
            var cities = _context.Station.AsQueryable()
                           .Select(s => s.City)
                           .Distinct()
                           .OrderBy(c => c)
                           .ToList();
            ViewBag.Cities = cities;

            var groups = GetPopularTrips();
            //return View(groups);
            return PartialView("~/Views/Shared/_PopularTrips.cshtml", groups);
        }
        [ChildActionOnly]
        public ActionResult SearchTrip()
        {
            var cities = _context.Station.AsQueryable()
                           .Select(s => s.City)
                           .Distinct()
                           .OrderBy(c => c)
                           .ToList();
            ViewBag.Cities = cities;
            //return View(groups);
            return PartialView("~/Views/Shared/_SearchTrip.cshtml");
        }
        [HttpGet]
        public ActionResult GFindTrip(string FromCity, string ToCity, DateTime txtDate, int SoVe)
        {
            return FindTripInternalRoute(FromCity, ToCity, txtDate, SoVe);
        }

        [HttpPost]
        public ActionResult FindTrip(string FromCity, string ToCity, DateTime txtDate, int SoVe)
        {
            return FindTripInternalRoute(FromCity, ToCity, txtDate, SoVe);
        }
        private ActionResult FindTripInternalRoute(string FromCity, string ToCity, DateTime txtDate, int SoVe)
        {
            ViewBag.FromCity = FromCity;
            ViewBag.ToCity = ToCity;
            ViewBag.Date = txtDate;
            ViewBag.SoVe = SoVe;

            var startDate = txtDate.Date;
            var endDate = startDate.AddDays(1);

            DateTime now = DateTime.Now;

            // Sửa logic: Lấy toàn bộ trip trước, rồi filter trong code
            var allTrips = _context.Trip.Find(_ => true).ToList();

            var query = allTrips.Where(t =>
            {
                var roadMapCities = t.RoadMap?.Select(r => r.City).ToList();
                if (roadMapCities == null || roadMapCities.Count < 2)
                    return false;

                int fromIndex = roadMapCities.IndexOf(FromCity);
                int toIndex = roadMapCities.IndexOf(ToCity);

                if (fromIndex == -1 || toIndex == -1 || fromIndex >= toIndex)
                    return false;

                if (t.DepartureTime < startDate || t.DepartureTime >= endDate)
                    return false;

                if (txtDate.Date == now.Date && t.DepartureTime < now)
                    return false;

                return true;
            }).ToList();

            var vehicles = _context.Vehicle.Find(_ => true).ToList();

            var result = query.Select(t =>
            {
                var emptySeats = t.ListTicket?.Count(ticket => string.Equals(ticket.Status, "Available", StringComparison.OrdinalIgnoreCase)) ?? 0;

                var tripVehicleId = t.Vehicle?.VehicleID;
                var vehicle = !string.IsNullOrEmpty(tripVehicleId) ? vehicles.FirstOrDefault(v => v.VehicleID == tripVehicleId) : null;

                return new TripWithSeatsViewModelAndVehicleInfo
                {
                    Trip = t,
                    EmptySeats = emptySeats,
                    VehicleType = vehicle?.VehicleType ?? t.Vehicle?.VehicleType,
                    RoadMapCities = t.RoadMap?.Select(r => r.City).ToList()
                };
            })
            .ToList();

            var cities = _context.Station.AsQueryable()
                               .Select(s => s.City)
                               .Distinct()
                               .OrderBy(c => c)
                               .ToList();
            ViewBag.Cities = cities;

            return View("FindTrip", result);
        }

        private List<PopularRouteCardViewModel> GetPopularTrips()
        {
            var trips = _context.Trip.Find(_ => true).ToList();

            var tripsWithStats = trips.Select(t => new
            {
                Trip = t,
                BookedSeats = t.ListTicket?.Count(ticket => ticket.Status.Equals("Booked", StringComparison.OrdinalIgnoreCase)) ?? 0
            }).ToList();

            var groupedByDeparture = tripsWithStats
                .GroupBy(t => t.Trip.RoadMap.First().City)
                .Select(g => new
                {
                    Departure = g.Key,
                    TotalBooked = g.Sum(x => x.BookedSeats),
                    Trips = g.OrderByDescending(x => x.BookedSeats).ToList()
                })
                .OrderByDescending(g => g.TotalBooked)
                .Take(3) 
                .ToList();

            var result = groupedByDeparture.Select(g => new PopularRouteCardViewModel
            {
                Departure = g.Departure,
                Routes = g.Trips
                    .Take(3)
                    .Select(x => new RouteItemViewModel
                    {
                        TripID = x.Trip.TripID,
                        Departure = x.Trip.RoadMap.First().City,
                        Destination = x.Trip.RoadMap.Last().City,
                        Duration = x.Trip.Duration,
                        Date = x.Trip.DepartureTime,
                        Price = x.Trip.Price
                    })
                    .ToList()
            }).ToList();

            return result;
        }

        // GET: Trang quản lý chuyến đi
        public async Task<ActionResult> Index()
        {
            if (Session["Role"]?.ToString() != "Admin")
                return RedirectToAction("Index", "Home");

            var trips = await _context.Trip.Find(_ => true).ToListAsync();
            return View(trips);
        }

        // GET: Form thêm chuyến đi mới
        public async Task<ActionResult> Create()
        {
            if (Session["Role"]?.ToString() != "Admin")
                return RedirectToAction("Index", "Home");

            ViewBag.Stations = await _context.Station.Find(_ => true).ToListAsync();
            ViewBag.Vehicles = await _context.Vehicle.Find(_ => true).ToListAsync();
            return View();
        }

        // POST: Tạo chuyến đi mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TripCreateViewModel model)
        {
            if (Session["Role"]?.ToString() != "Admin")
                return RedirectToAction("Index", "Home");

            try
            {
                // Debug: Kiểm tra dữ liệu nhận được
                System.Diagnostics.Debug.WriteLine($"StationIds count: {model.StationIds?.Count ?? 0}");
                if (model.StationIds != null)
                {
                    foreach (var id in model.StationIds)
                    {
                        System.Diagnostics.Debug.WriteLine($"Station ID: {id}");
                    }
                }

                // Kiểm tra StationIds có null không
                if (model.StationIds == null || model.StationIds.Count == 0)
                {
                    TempData["Error"] = "Vui lòng chọn lộ trình (ít nhất 2 điểm)";
                    ViewBag.Stations = await _context.Station.Find(_ => true).ToListAsync();
                    ViewBag.Vehicles = await _context.Vehicle.Find(_ => true).ToListAsync();
                    return View(model);
                }

                // Validation
                var validationResult = await ValidateTripData(model);
                if (!validationResult.IsValid)
                {
                    TempData["Error"] = validationResult.Message;
                    ViewBag.Stations = await _context.Station.Find(_ => true).ToListAsync();
                    ViewBag.Vehicles = await _context.Vehicle.Find(_ => true).ToListAsync();
                    return View(model);
                }

                // Lấy thông tin xe
                var vehicle = await _context.Vehicle.Find(v => v.VehicleID == model.VehicleID).FirstOrDefaultAsync();
                if (vehicle == null)
                {
                    TempData["Error"] = "Không tìm thấy xe được chọn";
                    ViewBag.Stations = await _context.Station.Find(_ => true).ToListAsync();
                    ViewBag.Vehicles = await _context.Vehicle.Find(_ => true).ToListAsync();
                    return View(model);
                }

                // Tạo danh sách vé
                var tickets = new List<Ticket>();
                int seatCount = vehicle.Capacity;

                // Xác định prefix dựa trên loại xe
                string[] prefixes = vehicle.Capacity == 40 ? new[] { "A", "B" } : new[] { "A" };
                int seatsPerPrefix = vehicle.Capacity == 40 ? 20 : vehicle.Capacity;

                foreach (var prefix in prefixes)
                {
                    for (int i = 1; i <= seatsPerPrefix; i++)
                    {
                        tickets.Add(new Ticket
                        {
                            TicketID = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                            SeatNum = $"{prefix}{i}",
                            Status = "Available",
                            Price = model.Price
                        });
                    }
                }

                // Lấy thông tin lộ trình
                var roadMap = new List<Station>();
                foreach (var stationId in model.StationIds)
                {
                    var station = await _context.Station.Find(s => s.StationID == stationId).FirstOrDefaultAsync();
                    if (station != null)
                        roadMap.Add(station);
                }

                // Kiểm tra lại sau khi query
                if (roadMap.Count < 2)
                {
                    TempData["Error"] = "Không tìm thấy đủ thông tin bến xe. Vui lòng thử lại.";
                    ViewBag.Stations = await _context.Station.Find(_ => true).ToListAsync();
                    ViewBag.Vehicles = await _context.Vehicle.Find(_ => true).ToListAsync();
                    return View(model);
                }

                // Tạo tên chuyến tự động
                string tripName = $"{roadMap.First().City} - {roadMap.Last().City}";

                // Tạo trip mới
                var trip = new Trip
                {
                    TripID = GenerateTripID(),
                    TripName = tripName,
                    DepartureTime = model.DepartureTime,
                    ArrivalTime = model.ArrivalTime,
                    Price = model.Price,
                    RemainingSeats = vehicle.Capacity,
                    ListTicket = tickets,
                    RoadMap = roadMap,
                    Vehicle = vehicle,
                };

                await _context.Trip.InsertOneAsync(trip);

                TempData["Success"] = "Thêm chuyến đi thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                ViewBag.Stations = await _context.Station.Find(_ => true).ToListAsync();
                ViewBag.Vehicles = await _context.Vehicle.Find(_ => true).ToListAsync();
                return View(model);
            }
        }

        private async Task<ValidationResult> ValidateTripData(dynamic model, string excludeTripId = null)
        {
            // Kiểm tra thời gian
            if (model.DepartureTime <= DateTime.Now)
            {
                return new ValidationResult { IsValid = false, Message = "Thời gian khởi hành phải sau thời điểm hiện tại" };
            }

            if (model.ArrivalTime <= model.DepartureTime)
            {
                return new ValidationResult { IsValid = false, Message = "Thời gian đến phải sau thời gian khởi hành" };
            }

            var duration = (model.ArrivalTime - model.DepartureTime).TotalHours;
            if (duration > 24)
            {
                return new ValidationResult { IsValid = false, Message = "Thời gian di chuyển không được vượt quá 24 giờ" };
            }

            // Kiểm tra giá vé
            if (model.Price <= 0 || model.Price > 10000000)
            {
                return new ValidationResult { IsValid = false, Message = "Giá vé không hợp lệ (0 - 10,000,000 VNĐ)" };
            }

            // Kiểm tra lộ trình
            if (model.StationIds == null || model.StationIds.Count < 2)
            {
                return new ValidationResult { IsValid = false, Message = "Lộ trình phải có ít nhất 2 điểm (điểm đi và điểm đến)" };
            }

            // Kiểm tra trùng lặp trong lộ trình - SỬA LẠI ĐÂY
            var stationIdsList = model.StationIds as List<string>;
            if (stationIdsList != null)
            {
                var distinctCount = stationIdsList.Distinct().Count();
                if (stationIdsList.Count != distinctCount)
                {
                    return new ValidationResult { IsValid = false, Message = "Lộ trình không được có điểm trùng lặp" };
                }
            }

            // Kiểm tra xe có sẵn sàng không (không bị trùng lịch)
            var vehicleConflict = await CheckVehicleAvailability(
                model.VehicleID,
                model.DepartureTime,
                model.ArrivalTime,
                excludeTripId
            );

            if (!vehicleConflict.IsAvailable)
            {
                return new ValidationResult { IsValid = false, Message = vehicleConflict.Message };
            }

            return new ValidationResult { IsValid = true };
        }

        // GET: Form chỉnh sửa chuyến đi
        public async Task<ActionResult> Edit(string id)
        {
            if (Session["Role"]?.ToString() != "Admin")
                return RedirectToAction("Index", "Home");

            var trip = await _context.Trip.Find(t => t.TripID == id).FirstOrDefaultAsync();
            if (trip == null)
                return HttpNotFound();

            ViewBag.Stations = await _context.Station.Find(_ => true).ToListAsync();
            ViewBag.Vehicles = await _context.Vehicle.Find(_ => true).ToListAsync();

            var model = new TripEditViewModel
            {
                TripID = trip.TripID,
                DepartureTime = trip.DepartureTime,
                ArrivalTime = trip.ArrivalTime,
                Price = trip.Price,
                VehicleID = trip.Vehicle.VehicleID,
                StationIds = trip.RoadMap.Select(s => s.StationID).ToList()
            };

            return View(model);
        }

        // POST: Cập nhật chuyến đi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TripEditViewModel model)
        {
            if (Session["Role"]?.ToString() != "Admin")
                return RedirectToAction("Index", "Home");

            try
            {
                var trip = await _context.Trip.Find(t => t.TripID == model.TripID).FirstOrDefaultAsync();
                if (trip == null)
                    return HttpNotFound();

                // Kiểm tra nếu đã có vé được đặt, không cho thay đổi một số thông tin quan trọng
                var bookedTickets = trip.ListTicket.Count(t => t.Status == "Booked");
                if (bookedTickets > 0)
                {
                    // Chỉ cho phép thay đổi trạng thái và giá (với cảnh báo)
                    TempData["Warning"] = "Chuyến đi đã có vé được đặt. Một số thay đổi bị hạn chế.";
                }

                // Validation
                var validationResult = await ValidateTripData(model, trip.TripID);
                if (!validationResult.IsValid)
                {
                    TempData["Error"] = validationResult.Message;
                    ViewBag.Stations = await _context.Station.Find(_ => true).ToListAsync();
                    ViewBag.Vehicles = await _context.Vehicle.Find(_ => true).ToListAsync();
                    return View(model);
                }

                // Cập nhật thông tin
                var updateBuilder = Builders<Trip>.Update
                    .Set(t => t.DepartureTime, model.DepartureTime)
                    .Set(t => t.ArrivalTime, model.ArrivalTime)
                    .Set(t => t.Price, model.Price);
                // Nếu chưa có vé đặt, cho phép đổi xe và lộ trình
                if (bookedTickets == 0)
                {
                    var vehicle = await _context.Vehicle.Find(v => v.VehicleID == model.VehicleID).FirstOrDefaultAsync();
                    var roadMap = new List<Station>();

                    foreach (var stationId in model.StationIds)
                    {
                        var station = await _context.Station.Find(s => s.StationID == stationId).FirstOrDefaultAsync();
                        if (station != null)
                            roadMap.Add(station);
                    }

                    string tripName = roadMap.Count >= 2
                        ? $"{roadMap.First().City} - {roadMap.Last().City}"
                        : trip.TripName;

                    updateBuilder = updateBuilder
                        .Set(t => t.Vehicle, vehicle)
                        .Set(t => t.RoadMap, roadMap)
                        .Set(t => t.TripName, tripName);
                }

                await _context.Trip.UpdateOneAsync(
                    t => t.TripID == model.TripID,
                    updateBuilder
                );

                TempData["Success"] = "Cập nhật chuyến đi thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                ViewBag.Stations = await _context.Station.Find(_ => true).ToListAsync();
                ViewBag.Vehicles = await _context.Vehicle.Find(_ => true).ToListAsync();
                return View(model);
            }
        }

        // POST: Xóa chuyến đi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            if (Session["Role"]?.ToString() != "Admin")
                return Json(new { success = false, message = "Không có quyền truy cập" });

            try
            {
                var trip = await _context.Trip.Find(t => t.TripID == id).FirstOrDefaultAsync();
                if (trip == null)
                    return Json(new { success = false, message = "Không tìm thấy chuyến đi" });

                // Kiểm tra xem có vé đã đặt không
                var bookedTickets = trip.ListTicket.Count(t => t.Status == "Booked");
                if (bookedTickets > 0)
                {
                    return Json(new { success = false, message = "Không thể xóa chuyến đi đã có vé được đặt" });
                }

                // Kiểm tra xem thời gian khởi hành đã qua chưa
                if (trip.DepartureTime < DateTime.Now)
                {
                    return Json(new { success = false, message = "Không thể xóa chuyến đi đã khởi hành" });
                }

                await _context.Trip.DeleteOneAsync(t => t.TripID == id);

                return Json(new { success = true, message = "Xóa chuyến đi thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }



        // Kiểm tra xe có bị trùng lịch không
        private async Task<VehicleAvailabilityResult> CheckVehicleAvailability(
            string vehicleId,
            DateTime departureTime,
            DateTime arrivalTime,
            string excludeTripId = null)
        {
            var filter = Builders<Trip>.Filter.And(
                Builders<Trip>.Filter.Eq(t => t.Vehicle.VehicleID, vehicleId),
                Builders<Trip>.Filter.Or(
                    // Chuyến mới bắt đầu trong khoảng thời gian của chuyến cũ
                    Builders<Trip>.Filter.And(
                        Builders<Trip>.Filter.Lte(t => t.DepartureTime, departureTime),
                        Builders<Trip>.Filter.Gte(t => t.ArrivalTime, departureTime)
                    ),
                    // Chuyến mới kết thúc trong khoảng thời gian của chuyến cũ
                    Builders<Trip>.Filter.And(
                        Builders<Trip>.Filter.Lte(t => t.DepartureTime, arrivalTime),
                        Builders<Trip>.Filter.Gte(t => t.ArrivalTime, arrivalTime)
                    ),
                    // Chuyến mới bao trùm chuyến cũ
                    Builders<Trip>.Filter.And(
                        Builders<Trip>.Filter.Gte(t => t.DepartureTime, departureTime),
                        Builders<Trip>.Filter.Lte(t => t.ArrivalTime, arrivalTime)
                    )
                )
            );

            var conflictingTrips = await _context.Trip.Find(filter).ToListAsync();

            // Loại trừ chuyến đang edit
            if (!string.IsNullOrEmpty(excludeTripId))
            {
                conflictingTrips = conflictingTrips.Where(t => t.TripID != excludeTripId).ToList();
            }

            if (conflictingTrips.Any())
            {
                var conflictTrip = conflictingTrips.First();
                return new VehicleAvailabilityResult
                {
                    IsAvailable = false,
                    Message = $"Xe đã được sử dụng cho chuyến '{conflictTrip.TripName}' từ {conflictTrip.DepartureTime:dd/MM/yyyy HH:mm} đến {conflictTrip.ArrivalTime:dd/MM/yyyy HH:mm}"
                };
            }

            return new VehicleAvailabilityResult { IsAvailable = true };
        }

        // Generate TripID
        private string GenerateTripID()
        {
            return "TRIP" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        // Helper classes
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public string Message { get; set; }
        }

        public class VehicleAvailabilityResult
        {
            public bool IsAvailable { get; set; }
            public string Message { get; set; }
        }
    }

    // ViewModels
    public class TripCreateViewModel
    {
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public double Price { get; set; }
        public string VehicleID { get; set; }
        public List<string> StationIds { get; set; }
    }

    public class TripEditViewModel : TripCreateViewModel
    {
        public string TripID { get; set; }
        public string State { get; set; }
    }


}