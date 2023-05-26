
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeneralMediaServices.Data;
using GeneralMediaServices.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace GeneralMediaServices.Controllers
{
    public class LogInController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DatabaseSettings _databaseSettings;


        public LogInController(IServiceProvider serviceProvider, DatabaseSettings databaseSettings)
        {
            _serviceProvider = serviceProvider;
            _databaseSettings = databaseSettings;
        }

        
        public IActionResult Index()
        {
            // Check if the user is already log in or not
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity != null && claimUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "PendingMessages");
            }
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Id,Username,UserPassword")] User user)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<MediaServiceDbContext>().UseSqlServer(_databaseSettings.ConnectionString);
                var dbContext = new MediaServiceDbContext(optionsBuilder.Options);

                var userList = await dbContext.user_tb.ToListAsync();

                if (ModelState.IsValid)
                {
                    var dbUser = userList.FirstOrDefault(u => u.Username.ToLower() == user.Username.ToLower() && u.UserPassword == user.UserPassword);

                    if (dbUser != null)
                    {
                        var claims = new List<Claim>() {
                            new Claim(ClaimTypes.NameIdentifier, user.Username)
                            // new Claim("Other Properties", "Example Role")
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var properties = new AuthenticationProperties()
                        {
                            AllowRefresh = true
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

                        // Add Database Settings to user
                        dbUser.Server = _databaseSettings.Server;
                        dbUser.Database = _databaseSettings.DataBase;
                        dbUser.UserId = _databaseSettings.UserId;
                        dbUser.Password = _databaseSettings.Password;

                        dbContext.user_tb.Update(dbUser);
                        await dbContext.SaveChangesAsync();

                        TempData["username"] = user.Username;
                        return RedirectToAction("Index", "PendingMessages");
                    }
                    else
                    {
                        ViewBag.AlertMessage = "Authentication failed. Please check your credentials.";
                    }
                }
                else
                {
                    ViewBag.AlertMessage = "All fields must be filled!";
                }
            }
            catch (Exception ex)
            {
                ViewBag.AlertMessage = ex.Message;
            }
            return View(user);
        }


        public IActionResult ConnectDatabase()
        {
            return View(new User());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConnectDatabase([Bind("Server,Database,UserId,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                String connectionString = $"Server={user.Server};DataBase={user.Database};" +
                (user.UserId != null ? $"User Id={user.UserId};" : "") +
                (user.Password != null ? $"Password={user.Password};" : "") +
                "Encrypt=False;MultipleActiveResultSets=True";
                /**/
                /**/
                var databaseSettings = HttpContext.RequestServices.GetService<DatabaseSettings>();
                databaseSettings.ConnectionString = connectionString;
                databaseSettings.Server = user.Server;
                databaseSettings.DataBase = user.Database;
                databaseSettings.UserId = user.UserId;
                databaseSettings.Password = user.Password;

                var optionsBuilder = new DbContextOptionsBuilder<MediaServiceDbContext>().UseSqlServer(connectionString);
                var dbContext = new MediaServiceDbContext(optionsBuilder.Options);
                
                if (!dbContext.Database.CanConnect())
                {
                    ViewBag.AlertMessage = "Failed to establish a database connection: " + $"Server={user.Server}";
                    return View(user);
                }
               
                return RedirectToAction("Index", "LogIn");
            }
            else
            {
                return View(user);
            }
            
           
        }


        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Id,Username,UserPassword,RetypePassword")] User user)
        {
                var optionsBuilder = new DbContextOptionsBuilder<MediaServiceDbContext>().UseSqlServer(_databaseSettings.ConnectionString);
                var dbContext = new MediaServiceDbContext(optionsBuilder.Options);

                var userList = await dbContext.user_tb.ToListAsync();

                if (ModelState.IsValid)
                {
                    if (userList.Any(dbUser => user.Username.ToLower() == dbUser.Username.ToLower()))
                    {
                        ViewBag.AlertMessage = "Username is already taken. Please try another username.";
                        return View(user);
                    }

                    if (user.RetypePassword.Equals(user.UserPassword))
                    {
                        // Add Database Settings to user
                        user.Server = _databaseSettings.Server;
                        user.Database = _databaseSettings.DataBase;
                        user.UserId = _databaseSettings.UserId;
                        user.Password = _databaseSettings.Password;

                        dbContext.user_tb.Add(user);
                        await dbContext.SaveChangesAsync();

                        var claims = new List<Claim>() {
                            new Claim(ClaimTypes.NameIdentifier, user.Username)
                            // new Claim("Other Properties", "Example Role")
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var properties = new AuthenticationProperties()
                        {
                            AllowRefresh = true
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
                    TempData["info message"] = "Registration is successfully completed.";
                        return RedirectToAction(nameof(Index), "PendingMessages");
                    }
                    else
                    {
                        ViewBag.AlertMessage = "Retype password is not the same as password.";
                    }
                }
                else
                {
                    ViewBag.AlertMessage = "All fields must be filled.";
                }
            return View(user);
        }


        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "LogIn");
        }

    }
}
