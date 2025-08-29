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
        

        [BsonElement("UserID")]
        public string UserID { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [BsonElement("Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [BsonElement("Phone_num")]
        public string PhoneNum { get; set; }
        [BsonIgnoreIfNull]

        [BsonElement("Address")]
        public string Address { get; set; }

        [BsonElement("Sex")]
        [BsonIgnoreIfNull]
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