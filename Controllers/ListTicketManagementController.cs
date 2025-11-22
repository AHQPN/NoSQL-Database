using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Ticket_Booking_System.Models;
using Ticket_Booking_System.Repositories;
using static Ticket_Booking_System.Models.ListTicketManagementModel;

namespace Ticket_Booking_System.Controllers
{
    public class ListTicketManagementController : Controller
    {
        private readonly IListTicketManagement.ITripRepository _tripRepository;
        private readonly IListTicketManagement.IBillRepository _billRepository;
        public ListTicketManagementController()
        {
            try
            {
                var dbContext = new MongoDbContext();
                _tripRepository = new ListTicketMangement.TripRepository(dbContext.Trip.Database);
                _billRepository = new ListTicketMangement.BillRepository(dbContext.Bill.Database);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MongoDB Connection Error: {ex.Message}");
                throw;
            }
        }
        // GET: ListTicketManagement
        public ActionResult Index()
        {
            if (Session["Role"]?.ToString() != "TicketAgent")
                return RedirectToAction("Index", "Home");
            return View();
        }
        [HttpGet]
        public async Task<JsonResult> GetTrips(int page = 1, int pageSize = 10, string search = "", string sortBy = "DepartureTime", string sortOrder = "asc")
        {
            try
            {
                var now = DateTime.Now;

                var allTrips = await _tripRepository.GetUpcomingTripsAsync(now);

                // search filter
                if (!string.IsNullOrEmpty(search))
                {
                    allTrips = allTrips.Where(t =>
                        t.TripName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        t.TripID.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                    ).ToList();
                }

                // sorting
                switch (sortBy)
                {
                    case "TripID":
                        allTrips = sortOrder == "asc"
                            ? allTrips.OrderBy(t => t.TripID).ToList()
                            : allTrips.OrderByDescending(t => t.TripID).ToList();
                        break;
                    case "TripName":
                        allTrips = sortOrder == "asc"
                            ? allTrips.OrderBy(t => t.TripName).ToList()
                            : allTrips.OrderByDescending(t => t.TripName).ToList();
                        break;
                    case "DepartureTime":
                    default:
                        allTrips = sortOrder == "asc"
                            ? allTrips.OrderBy(t => t.DepartureTime).ToList()
                            : allTrips.OrderByDescending(t => t.DepartureTime).ToList();
                        break;
                }

                var totalRecords = allTrips.Count;

                var pagedTrips = allTrips
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var tripViewModels = pagedTrips.Select(trip => new TripViewModel
                {
                    TripID = trip.TripID,
                    TripName = trip.TripName,
                    DepartureTime = trip.DepartureTime,
                    ArrivalTime = trip.ArrivalTime,
                    RemainingSeats = trip.RemainingSeats,
                    TotalSeats = trip.Vehicle.Capacity,
                    Status = trip.RemainingSeats > 0 ? "available" : "full"
                }).ToList();

                var result = new PaginatedResultViewModel<TripViewModel>
                {
                    Success = true,
                    Data = tripViewModels,
                    TotalRecords = totalRecords,
                    TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                    CurrentPage = page
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new PaginatedResultViewModel<TripViewModel>
                {
                    Success = false,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetTripDetails(string tripId)
        {
            try
            {
                var trip = await _tripRepository.GetByIdAsync(tripId);

                if (trip == null)
                {
                    return Json(new ApiResponseViewModel<TripDetailViewModel>
                    {
                        Success = false,
                        Message = "Không tìm thấy chuyến xe"
                    }, JsonRequestBehavior.AllowGet);
                }

                var tripDetailViewModel = new TripDetailViewModel
                {
                    TripID = trip.TripID,
                    TripName = trip.TripName,
                    DepartureTime = trip.DepartureTime,
                    ArrivalTime = trip.ArrivalTime,
                    Price = trip.Price,
                    VehicleType = trip.Vehicle.VehicleType,
                    Tickets = trip.ListTicket.Select(ticket => new TicketDetailViewModel
                    {
                        TicketID = ticket.TicketID,
                        SeatNum = ticket.SeatNum,
                        Status = ticket.Status
                    }).ToList()
                };

                return Json(new ApiResponseViewModel<TripDetailViewModel>
                {
                    Success = true,
                    Data = tripDetailViewModel
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponseViewModel<TripDetailViewModel>
                {
                    Success = false,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetCustomerInfo(string ticketId)
        {
            try
            {
                var bill = await _billRepository.GetByTicketIdAsync(ticketId);

                if (bill == null)
                {
                    return Json(new ApiResponseViewModel<CustomerInfoViewModel>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin khách hàng"
                    }, JsonRequestBehavior.AllowGet);
                }

                var ticketInfo = bill.ListItem.FirstOrDefault(item => item.TicketID == ticketId);

                // Map to ViewModel
                var customerInfoViewModel = new CustomerInfoViewModel
                {
                    CustomerID = bill.Customer.CustomerID,
                    Name = bill.Customer.Name,
                    PhoneNum = bill.Customer.PhoneNum,
                    BillID = bill.BillID,
                    CreateAt = bill.CreateAt.ToString("dd/MM/yyyy HH:mm"),
                    SeatNum = ticketInfo?.SeatNum
                };

                return Json(new ApiResponseViewModel<CustomerInfoViewModel>
                {
                    Success = true,
                    Data = customerInfoViewModel
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponseViewModel<CustomerInfoViewModel>
                {
                    Success = false,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}