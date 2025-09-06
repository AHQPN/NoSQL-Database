using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ticket_Booking_System.Controllers
{
    public class TicketController : Controller
    {
        // GET: Ticket
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult FindTicket()
        {
            return View();
        }
        public ActionResult KQTraCuuVe()
        {
            return View();
        }
    }
}