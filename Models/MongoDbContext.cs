using MongoDB.Driver;
using System.Configuration;

namespace Ticket_Booking_System.Models
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString;
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("TicketBookingDB");
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Trip> Trips => _database.GetCollection<Trip>("Trip");
        public IMongoCollection<Ticket> Tickets => _database.GetCollection<Ticket>("Tickets");
        public IMongoCollection<Vehicle> Vehicles => _database.GetCollection<Vehicle>("Vehicle");
        public IMongoCollection<Bill> Bills => _database.GetCollection<Bill>("Bills");
        public IMongoCollection<News> News => _database.GetCollection<News>("News");

    }
}

