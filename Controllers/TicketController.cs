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
        public ActionResult FindTicket()
        {
            if (Session["UserID"] ==null)
            {
                TempData["ShowLogin"] = true;
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        public ActionResult KQTraCuuVe()
        {
            return View();
        }
    }
}