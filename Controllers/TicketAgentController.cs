using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Ticket_Booking_System.Models;
using Ticket_Booking_System.Repositories;

namespace Ticket_Booking_System.Controllers
{
    public class TicketAgentController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ITripRepository _tripRepository;

        public TicketAgentController()
        {
            var dbContext = new MongoDbContext();
            _userRepository = new UserRepository(dbContext.User.Database);
            _tripRepository = new TripRepository(dbContext.Trip.Database);
        }

        public ActionResult Index()
        {
            if (Session["nv"] == null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public async Task<ActionResult> Dashboard()
        {
            if (Session["Role"]?.ToString() != "TicketAgent")
                return RedirectToAction("Index", "Home");

            var now = DateTime.Now;

            var ticketsToday = await _tripRepository.GetTicketsByDateAsync(now.Date);

            var trips = (await _tripRepository.GetAllAsync())
                        .Where(trip => trip.DepartureTime >= now) 
                        .ToList();

            var tripStats = trips.Select(trip => new TripStatsViewModel
            {
                TripId = trip.TripID,
                TripName = trip.TripName,
                TotalSeats = trip.ListTicket.Count,
                BookedSeats = trip.ListTicket.Count(t => t.Status == "Booked"),
                AvailableSeats = trip.ListTicket.Count(t => t.Status == "Available")
            }).ToList();

            ViewBag.TicketsSoldToday = ticketsToday.Count;
            ViewBag.TripStats = tripStats;

            return View();
        }

    }
}
