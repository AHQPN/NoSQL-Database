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
        public ActionResult PopularTrip()
        {
            var trips = _context.Trips.Find(_ => true).ToList();

            var cities = trips
                .Where(t => t.RoadMap != null)
                .SelectMany(t => t.RoadMap.Select(r => r.City))
                .Distinct()
                .ToList();
            ViewBag.Cities = cities;

            var groups = GetPopularTrips();
            return View(groups);
        }

        [HttpPost]
        public ActionResult FindTrip(string FromCity, string ToCity, DateTime txtDate, int SoVe)
        {
            ViewBag.FromCity = FromCity;
            ViewBag.ToCity = ToCity;
            ViewBag.Date = txtDate;
            ViewBag.SoVe = SoVe;

            var startDate = txtDate.Date;
            var endDate = startDate.AddDays(1); 

            DateTime now = DateTime.Now;

            var query = _context.Trips.Find(
                t => t.RoadMap.First().City == FromCity &&
                     t.RoadMap.Last().City == ToCity &&
                     t.DepartureTime >= startDate &&
                     t.DepartureTime < endDate
            ).ToList();

            if (txtDate.Date == now.Date)
            {
                query = query.Where(t => t.DepartureTime >= now).ToList();
            }
            var vehicles = _context.Vehicles.Find(_ => true).ToList();

            // Map sang viewmodel
            var result = query.Select(t =>
            {
                var vehicle = vehicles.FirstOrDefault(v => v.VehicleID == t.VehicleID);

                return new TripWithSeatsViewModelAndVehicleInfo
                {
                    Trip = t,
                    EmptySeats = t.ListTicket.Count(ticket => ticket.Status == "Available"),
                    VehicleType = vehicle?.VehicleType
                };
            })
            .Where(vm => vm.EmptySeats >= SoVe)
            .ToList();

            var tripss = _context.Trips.Find(_ => true).ToList();
            var cities = tripss
                .Where(t => t.RoadMap != null)
                .SelectMany(t => t.RoadMap.Select(r => r.City))
                .Distinct()
                .ToList();
            ViewBag.Cities = cities;

            return View(result);
        }

        private List<PopularRouteCardViewModel> GetPopularTrips()
        {
            var popularTrips = _context.Trips.Aggregate()
                .Project(t => new Trip
                {
                    TripID = t.TripID,
                    TripName = t.TripName,
                    DepartureTime = t.DepartureTime,
                    ArrivalTime = t.ArrivalTime,
                    Price = t.Price,
                    RoadMap = t.RoadMap,
                    ListTicket = t.ListTicket
                })
                .ToList();

            var topTrips = popularTrips
                .OrderByDescending(t => t.ListTicket.Count(ticket => ticket.Status == "Booked"))
                .Take(20)
                .ToList();

            return topTrips
                .GroupBy(t => t.RoadMap.First().City)
                .Take(3)
                .Select(g => new PopularRouteCardViewModel
                {
                    Departure = g.Key,
                    Routes = g.Take(3).Select(trip => new RouteItemViewModel
                    {
                        Destination = trip.RoadMap.Last().City,
                        Duration = trip.Duration,
                        Date = trip.DepartureTime,
                        Price = trip.Price
                    }).ToList()
                })
                .ToList();
        }
    }
}