using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ticket_Booking_System.Models
{
    public class Station
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }   

        [BsonElement("StationName")]
        public string StationName { get; set; } // Tên tỉnh
        [BsonElement("City")]
        public string City { get; set; }
    }
}