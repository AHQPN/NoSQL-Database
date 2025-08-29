using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ticket_Booking_System.Models
{
    public class News
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("Title")]
        public string Title { get; set; }

        [BsonElement("Content")]
        public string Content { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("ImageUrl")]
        public string ImageUrl { get; set; }
    }
}