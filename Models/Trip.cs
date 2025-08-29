using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Ticket_Booking_System.Models
{
    public class Trip
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("TripID")]
        public string TripID { get; set; }

        [BsonElement("TripName")]
        public string TripName { get; set; }

        [BsonElement("DepartureTime")]
        public DateTime DepartureTime { get; set; }

        [BsonElement("ArrivalTime")]
        public DateTime ArrivalTime { get; set; }

        [BsonElement("RemainingSeats")]
        public int RemainingSeats { get; set; }

        [BsonElement("Price")]
        public double Price { get; set; }

        [BsonElement("ListTicket")]
        public List<Ticket> ListTicket { get; set; }

        [BsonElement("Roadmap")]
        public List<string> Roadmap { get; set; }

        [BsonElement("VehicleId")]
        public string VehicleId { get; set; } 
        [BsonElement("State")]
        public string State { get; set; }
    }
}