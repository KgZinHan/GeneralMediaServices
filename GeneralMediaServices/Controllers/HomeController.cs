using GeneralMediaServices.Models;
using GeneralMediaServices.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GeneralMediaServices.Controllers
{
    public class HomeController : Controller
    {
       
        public HomeController()
        {
        }

        public IActionResult Index()
        {
            return RedirectToAction("ConnectDatabase","LogIn");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}