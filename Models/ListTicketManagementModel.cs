using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ticket_Booking_System.Models
{
    public class ListTicketManagementModel
    {
        public class TripViewModel
        {
            public string TripID { get; set; }
            public string TripName { get; set; }
            public DateTime DepartureTime { get; set; }
            public DateTime ArrivalTime { get; set; }
            public int RemainingSeats { get; set; }
            public int TotalSeats { get; set; }
            public string Status { get; set; } // "available" hoặc "full"
        }

        public class TripDetailViewModel
        {
            public string TripID { get; set; }
            public string TripName { get; set; }
            public DateTime DepartureTime { get; set; }
            public DateTime ArrivalTime { get; set; }
            public double Price { get; set; }
            public string VehicleType { get; set; }
            public List<string> RoadMap { get; set; }
            public List<TicketDetailViewModel> Tickets { get; set; }
        }

        public class TicketDetailViewModel
        {
            public string TicketID { get; set; }
            public string SeatNum { get; set; }
            public string Status { get; set; } // "Available" hoặc "Booked"
        }

        public class CustomerInfoViewModel
        {
            public string CustomerID { get; set; }
            public string Name { get; set; }
            public string PhoneNum { get; set; }
            public string BillID { get; set; }
            public string CreateAt { get; set; }
            public string SeatNum { get; set; }
        }

        public class PaginatedResultViewModel<T>
        {
            public bool Success { get; set; }
            public List<T> Data { get; set; }
            public int TotalRecords { get; set; }
            public int TotalPages { get; set; }
            public int CurrentPage { get; set; }
            public string Message { get; set; }
        }

        public class ApiResponseViewModel<T>
        {
            public bool Success { get; set; }
            public T Data { get; set; }
            public string Message { get; set; }
        }
    }
}