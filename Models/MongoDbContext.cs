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

        public IMongoCollection<User> User => _database.GetCollection<User>("User");
        public IMongoCollection<Trip> Trip => _database.GetCollection<Trip>("Trip");
        public IMongoCollection<Ticket> Ticket => _database.GetCollection<Ticket>("Ticket");
        public IMongoCollection<Vehicle> Vehicle => _database.GetCollection<Vehicle>("Vehicle");
        public IMongoCollection<Station> Station => _database.GetCollection<Station>("Station");
        public IMongoCollection<Bill> Bill => _database.GetCollection<Bill>("Bill");
        public IMongoCollection<News> News => _database.GetCollection<News>("News");

    }
}

