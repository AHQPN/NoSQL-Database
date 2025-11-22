using System;
using System.Threading;
using Ticket_Booking_System.Models;
using Ticket_Booking_System.Repositories;

namespace Ticket_Booking_System.Infrastructure
{
    public static class JobScheduler
    {
        private static Timer _timer;

        public static void Start()
        {
            // Chạy mỗi 1 phút
            _timer = new Timer(RunJobs, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        private static async void RunJobs(object state)
        {
            try
            {
                var db = new MongoDbContext();
                var tripRepo = new TripRepository(db.GetDatabase());

                var trips = await tripRepo.GetAllAsync();
                foreach (var trip in trips)
                {
                    await tripRepo.ReleaseExpiredPendingSeatsAsync(trip.TripID);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Scheduler error: " + ex.Message);
            }
        }
    }
}
