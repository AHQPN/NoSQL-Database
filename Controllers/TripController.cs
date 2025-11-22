using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
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
                // Kiểm tra RoadMap có chứa FromCity và ToCity theo đúng thứ tự
                var roadMapCities = t.RoadMap?.Select(r => r.City).ToList();
                if (roadMapCities == null || roadMapCities.Count < 2)
                    return false;

                int fromIndex = roadMapCities.IndexOf(FromCity);
                int toIndex = roadMapCities.IndexOf(ToCity);

                // FromCity phải ở vị trí đầu tiên và ToCity phải đến sau
                if (fromIndex == -1 || toIndex == -1 || fromIndex >= toIndex)
                    return false;

                // Kiểm tra thời gian
                if (t.DepartureTime < startDate || t.DepartureTime >= endDate)
                    return false;

                // Nếu là hôm nay, chỉ lấy những chuyến khởi hành sau thời điểm hiện tại
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

    }
}