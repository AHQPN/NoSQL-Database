using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ticket_Booking_System.Repositories;
using Ticket_Booking_System.Models;
using System.Threading.Tasks;
using MongoDB.Driver;

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
            
            Trip trip = await _tripRepository.GetByIdAsync(tripID);
            string customerID = Session["UserID"] as string;
            User usr = await _userRepository.GetByIdAsync(customerID);
            ViewBag.userInfo = usr;

            return View(trip);
        }
        public async Task<ActionResult> Handle_Book_Ticket(string tripID, string seats)
        {
            var seatList = seats.Split(',').Select(s => s.Trim()).ToList();

            Trip trip = await _tripRepository.GetByIdAsync(tripID);
            if (trip == null)
            {
                return HttpNotFound("Trip not found");
            }
            var models = new List<WriteModel<Trip>>();
            var bookedSeats = new List<string>();
            var failedSeats = new List<string>();

            foreach (var seat in seatList)
            {
                var filter = Builders<Trip>.Filter.And(
                    Builders<Trip>.Filter.Eq(t => t.TripID, tripID),
                    Builders<Trip>.Filter.ElemMatch(t => t.ListTicket,
                        tk => tk.SeatNum == seat && tk.Status != "Booked")
                );

                var update = Builders<Trip>.Update
                    .Set("ListTicket.$.Status", "Booked")
                    .Set("ListTicket.$.TicketID", "TK" + Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper())
                    .Inc(t => t.RemainingSeats, -1);

                models.Add(new UpdateOneModel<Trip>(filter, update));
            }

            
            var bulkResult = await _tripRepository.BulkWriteAsync(models);

            // Kiểm tra kết quả
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

            var bookingResult = new BookingResultViewModel
            {
                TripID = tripID,
                BookingID = Guid.NewGuid().ToString("N"),
                BookedSeats = bookedSeats,
                FailedSeats = failedSeats,
                ExpireTime = DateTime.Now.AddMinutes(15) 
            };

            return View("BookingResult", bookingResult);

        }
        //public ActionResult BookingResult()
        //{
           
        //    return View();
        //}
        
        public async Task<ActionResult> RollbackBooking(string seats,string tripId)
        {


            return View();
        }



    }
}