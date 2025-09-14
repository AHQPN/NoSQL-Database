using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ticket_Booking_System.Models
{
    public class BookingResultViewModel
    {
        public string TripID { get; set; }
        public string BookingID { get; set; }
        public List<string> BookedSeats { get; set; }
        public List<string> FailedSeats { get; set; }
        public DateTime ExpireTime { get; set; }
    }
}