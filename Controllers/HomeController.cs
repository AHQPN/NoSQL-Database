using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Ticket_Booking_System.Controllers
{
    public class HomeController : Controller
    {
        //public ActionResult Index()
        //{
        //    return View();
        //}


        //Fake db for user
        //public async Task<ActionResult> Index()
        //{
        //    // Gọi InsertDefaultUsers của UserController
        //    UserController userController = new UserController();
        //    _ = await userController.InsertDefaultUsers();
        //    return Content("Inserted default users. Welcome to Ticket Booking System!");
        //}

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