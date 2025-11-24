using MongoDB.Bson;
using MongoDB.Driver;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Ticket_Booking_System.Infrastructure;
using Ticket_Booking_System.Models;
using Ticket_Booking_System.Repositories;

namespace Ticket_Booking_System.Infrastructure
{
    public static class RedisManager
    {
        private static Lazy<ConnectionMultiplexer> _lazyConnection;
        private static IDatabase _db => _lazyConnection.Value.GetDatabase();

        public static void Initialize(string connectionString)
        {
            _lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connectionString));
        }

        public static async Task<List<string>> ReserveSeatsAsync(string tripId, IEnumerable<string> seats, string bookingId, TimeSpan ttl)
        {
            var reserved = new List<string>();
            foreach (var seat in seats)
            {
                var key = GetSeatKey(tripId, seat);
                bool ok = await _db.StringSetAsync(key, bookingId, ttl, when: When.NotExists);
                if (ok) reserved.Add(seat);
            }
            return reserved;
        }

        public static async Task<bool> ReleaseSeatsAsync(string tripId, IEnumerable<string> seats, string bookingId = null)
        {
            var tasks = new List<Task<bool>>();
            foreach (var seat in seats)
            {
                var key = GetSeatKey(tripId, seat);
                if (bookingId == null)
                {
                    tasks.Add(_db.KeyDeleteAsync(key).ContinueWith(t => t.Result));
                }
                else
                {
                    var script = @"if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end";
                    tasks.Add(_db.ScriptEvaluateAsync(script, new RedisKey[] { key }, new RedisValue[] { bookingId })
                        .ContinueWith(t => (long)t.Result == 1));
                }
            }
            var results = await Task.WhenAll(tasks);
            return results.Any(r => r);
        }

        public static async Task<IEnumerable<string>> GetReservedSeatsAsync(string tripId)
        {
            var server = GetServer();
            if (server == null) return Enumerable.Empty<string>();

            var pattern = GetSeatKey(tripId, "*");
            var keys = server.Keys(pattern: pattern).ToArray();
            return keys.Select(k => ParseSeatFromKey(k)).Where(s => !string.IsNullOrEmpty(s));
        }

        private static string GetSeatKey(string tripId, string seat) => $"pending:{tripId}:{seat}";
        private static string ParseSeatFromKey(RedisKey key)
        {
            var s = (string)key;
            var parts = s.Split(':');
            if (parts.Length >= 3) return parts[2];
            return null;
        }

        private static IServer GetServer()
        {
            try
            {
                var conn = _lazyConnection?.Value;
                var endpoints = conn.GetEndPoints();
                return conn.GetServer(endpoints[0]);
            }
            catch { return null; }
        }
    }
}
