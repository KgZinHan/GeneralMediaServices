using GeneralMediaServices.Models;
using GeneralMediaServices.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GeneralMediaServices.Controllers
{
    public class UserController : Controller
    {
        private readonly DatabaseSettings _databaseSettings;
        public UserController(DatabaseSettings databaseSettings)
        {
            _databaseSettings = databaseSettings;
        }

        public IActionResult Index()
        {
            string username = HttpContext.User.Claims.First().Value;
            var optionsBuilder = new DbContextOptionsBuilder<MediaServiceDbContext>().UseSqlServer(_databaseSettings.ConnectionString);
            var dbContext = new MediaServiceDbContext(optionsBuilder.Options);
            var user = dbContext.user_tb.FirstOrDefault(u => u.Username == username);
            TempData["username"] = username;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(User user)
        {
            TempData["username"] = user.Username;
            var optionsBuilder = new DbContextOptionsBuilder<MediaServiceDbContext>().UseSqlServer(_databaseSettings.ConnectionString);
            var dbContext = new MediaServiceDbContext(optionsBuilder.Options);

            var oldUser = dbContext.user_tb.FirstOrDefault(u => u.Username == user.Username);

            if (oldUser != null && ModelState.IsValid)
            {
                if (user.UserPassword.Equals(oldUser.UserPassword) && user.NewPassword.Equals(user.RetypeNewPassword))
                {
                    oldUser.UserPassword = user.NewPassword;
                    dbContext.user_tb.Update(oldUser);
                    dbContext.SaveChanges();

                    TempData["info message"] = "Password is changed successfully.";
                    return RedirectToAction("Index", "PendingMessages");
                }
                else if (!oldUser.UserPassword.Equals(user.UserPassword))
                {
                    ViewBag.AlertMessage = "Your old password is not correct.";
                }
                else
                {
                    ViewBag.AlertMessage = "Your new password and retype new password are not the same. Try again!";
                }
            }
            return  View(user);
        }
    }
}