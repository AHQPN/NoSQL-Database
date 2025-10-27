using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ticket_Booking_System.Models
{
    public class Ticket
    {
        [BsonElement("TicketID")]
        public string TicketID { get; set; }

        [BsonElement("SeatNum")]
        public string SeatNum { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; }
        [BsonElement("Price")]
        public double Price { get; set; }
        public DateTime? PendingAt { get; set; } // Thời điểm ghế chuyển sang Pending
    }

}
