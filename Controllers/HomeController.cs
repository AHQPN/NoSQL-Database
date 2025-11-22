using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using Ticket_Booking_System.Models;

namespace Ticket_Booking_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly MongoDbContext _context;
        public HomeController()
        {
            _context = new MongoDbContext();
        }
        public ActionResult Index()
        {
            var cities = _context.Station.AsQueryable()
                           .Select(s => s.City)
                           .Distinct()
                           .OrderBy(c => c)
                           .ToList();
            ViewBag.Cities = cities;

            ViewBag.Cities = cities;
            return View();
        }

        public ActionResult TicketAgent()
        {
            if (Session["nv"] != null)
            {
                return RedirectToAction("Dashboard", "TicketAgent");
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}