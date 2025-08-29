using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Ticket_Booking_System.Models
{
    public class Bill
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("BillID")]
        public string BillID { get; set; }

        [BsonElement("CreateAt")]
        public DateTime CreateAt { get; set; }

        [BsonElement("Quantity")]
        public int Quantity { get; set; }

        [BsonElement("Total")]
        public double Total { get; set; }

        [BsonElement("CustomerID")]
        public ObjectId CustomerID { get; set; }

        
        [BsonElement("ListItem")]
        public List<Ticket> ListItem { get; set; }
    }
}