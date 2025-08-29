using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ticket_Booking_System.Models
{
    public class Vehicle
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("VehicleID")]
        public string VehicleID { get; set; }

        [BsonElement("VehicleName")]
        public string VehicleName { get; set; }

        [BsonElement("Type")]
        public string Type { get; set; }  // bus, car, limousine...

        [BsonElement("Capacity")]
        public int Capacity { get; set; }
    }
}