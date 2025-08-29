using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ticket_Booking_System.Models
{
    public class Ticket
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("TicketID")]
        public string TicketID { get; set; }

        [BsonElement("Seat_Num")]
        public int SeatNum { get; set; }
        [BsonElement("Status")]
        public string Status { get; set; }
    }
}