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
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("BillID")]
        public string BillID { get; set; }

        [BsonElement("CreateAt")]
        public DateTime CreateAt { get; set; }

        [BsonElement("Quantity")]
        public int Quantity { get; set; }

        [BsonElement("Total")]
        public double Total { get; set; }

        [BsonElement("PaymentStatus")]
        public string PaymentStatus { get; set; } = "Pending";  // Thêm tạm trạng thái thanh toán

        [BsonElement("Customer")]
        public CustomerInfo Customer { get; set; }

        [BsonElement("ListItem")]
        public List<TicketItem> ListItem { get; set; }

        [BsonElement("TripInfo")]
        public TripInfo TripInfo { get; set; }

        public string Status { get; set; }
        public string BookingID { get; set; }


    }

    public class CustomerInfo
    {
        [BsonElement("CustomerID")]
        public string CustomerID { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("PhoneNum")]
        public string PhoneNum { get; set; }
    }

    public class TicketItem
    {
        [BsonElement("TicketID")]
        public string TicketID { get; set; }

        [BsonElement("SeatNum")]
        public string SeatNum { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; }
    }

    public class TripInfo
    {
        [BsonElement("TripID")]
        public string TripID { get; set; }

        [BsonElement("TripName")]
        public string TripName { get; set; }

        [BsonElement("DepartureTime")]
        public DateTime DepartureTime { get; set; }

        [BsonElement("ArrivalTime")]
        public DateTime ArrivalTime { get; set; }

        [BsonElement("Price")]
        public double Price { get; set; }
    }
}