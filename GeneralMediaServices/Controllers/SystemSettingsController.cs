using GeneralMediaServices.Data;
using GeneralMediaServices.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeneralMediaServices.Controllers
{
    public class SystemSettingsController : Controller
    {
        // GET: DatabaseSettings
        private readonly DatabaseSettings _databaseSettings;

        public SystemSettingsController(DatabaseSettings databaseSettings)
        {
            _databaseSettings = databaseSettings;
        }
        public IActionResult Index()
        {
            try
            {
                var userName = HttpContext.User.Claims.First().Value;

                if (_databaseSettings.ConnectionString != null)
                {
                    var optionsBuilder = new DbContextOptionsBuilder<MediaServiceDbContext>()
                        .UseSqlServer(_databaseSettings.ConnectionString);

                    using (var dbContext = new MediaServiceDbContext(optionsBuilder.Options))
                    {
                        var userDbConnection = dbContext.user_tb.FirstOrDefault(u => u.Username == userName);
                        TempData["username"] = userName;
                        
                        return View(userDbConnection);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.AlertMessage = ex.Message;
            }

            TempData["username"] = HttpContext.User.Claims.First().Value;
            return View(new User());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(User user)
        {
            String userName = HttpContext.User.Claims.First().Value;
            TempData["username"] = userName;
            if (ModelState.IsValid)
            {
                String connectionString = $"Server={user.Server};DataBase={user.Database};" +
                (user.UserId != null ? $"User Id={user.UserId};" : "") +
                (user.Password != null ? $"Password={user.Password};" : "") +
                "Encrypt=False;MultipleActiveResultSets=True";

                var optionsBuilder = new DbContextOptionsBuilder<MediaServiceDbContext>().UseSqlServer(connectionString);

                using (var dbContext = new MediaServiceDbContext(optionsBuilder.Options))
                {
                    if (!await dbContext.Database.CanConnectAsync())
                    {
                        ViewBag.AlertMessage = "Failed to establish a database connection: " + $"Server={user.Server}";
                        return View(user);
                    }

                    var dbUser = dbContext.user_tb.FirstOrDefault(u => u.Username == userName);
                    if (dbUser != null)
                    {
                        dbUser.Server = user.Server;
                        dbUser.Database = user.Database;
                        dbUser.UserId = user.UserId;
                        dbUser.Password = user.Password;

                        dbContext.user_tb.Update(dbUser);
                        await dbContext.SaveChangesAsync();

                        var databaseSettings = HttpContext.RequestServices.GetService<DatabaseSettings>();
                        databaseSettings.ConnectionString = connectionString;
                        databaseSettings.Server = user.Server;
                        databaseSettings.DataBase = user.Database;
                        databaseSettings.UserId = user.UserId;
                        databaseSettings.Password = user.Password;

                        TempData["info message"] = "Database Connection is successfully established.";
                        return RedirectToAction(nameof(Index), "PendingMessages");
                    }
                    else
                    {
                        ViewBag.AlertMessage = "Failed to update user information.";
                    }
                }
            }
            return View(user);
        }
    }
}
