using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ticket_Booking_System.Models
{
    public class Driver
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("DriverID")]
        public string DriverID { get; set; }

        [BsonElement("DriverName")]
        public string DriverName { get; set; }

        [BsonElement("PhoneNum")]
        public string PhoneNum { get; set; }
    }
}