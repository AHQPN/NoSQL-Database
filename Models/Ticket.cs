using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ticket_Booking_System.Models
{
    public class Ticket
    {
        [BsonElement("TicketID")]
        public string TicketID { get; set; }

        [BsonElement("SeatNumber")]
        public string SeatNumber { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; }
        [BsonElement("Price")]
        public double Price { get; set; }
    }

}
