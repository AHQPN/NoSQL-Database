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

        [BsonElement("RoadMap")]
        public List<Station> RoadMap { get; set; }

        [BsonElement("Vehicle")]
        public Vehicle Vehicle { get; set; } 
        [BsonElement("State")]
        public string State { get; set; }

        [BsonIgnore]
        public string Duration
        {
            get
            {
                var duration = ArrivalTime - DepartureTime;
                return $"{(int)duration.TotalHours} giờ {duration.Minutes} phút";
            }
        }

    }
    public class RouteItemViewModel
    {
        public string Departure { get; set; }
        public string Destination { get; set; }
        public string Duration { get; set; } 
        public DateTime Date { get; set; }
        public double Price { get; set; }
        public string TripID { get; set; }
    }
    public class TripWithSeatsViewModelAndVehicleInfo
    {
        public Trip Trip { get; set; }
        public int EmptySeats { get; set; }
        public string VehicleType { get; set; }
        public List<string> RoadMapCities { get; set; }
    }


    public class PopularRouteCardViewModel
    {
        public string Departure { get; set; }
        public List<RouteItemViewModel> Routes { get; set; }
    }

}