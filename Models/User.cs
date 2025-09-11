using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Ticket_Booking_System.Models
{
    public class User
    {
        [BsonId] // Map _id trong MongoDB
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("UserID")] // Map field UserID trong MongoDB
        public string UserID { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [BsonElement("Password")]
        public string Password { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("Address")]
        public string Address { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("Sex")]
        public string Sex { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("Status")]
        public string Status { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("Image")]
        public string Image { get; set; }

        [BsonElement("Role")]
        public string Role { get; set; }
    }
}