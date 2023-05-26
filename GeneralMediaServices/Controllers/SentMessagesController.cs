
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeneralMediaServices.Data;
using GeneralMediaServices.Models;
using Microsoft.AspNetCore.Authorization;

namespace GeneralMediaServices.Controllers
{
    [Authorize]
    public class SentMessagesController : Controller
    {
        private readonly DatabaseSettings _databaseSettings;

        public SentMessagesController(DatabaseSettings databaseSettings)
        {
            _databaseSettings = databaseSettings;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<MediaServiceDbContext>().UseSqlServer(_databaseSettings.ConnectionString);
                var dbContext = new MediaServiceDbContext(optionsBuilder.Options);

                var SentMessages = await dbContext.media_data_tb.Where(u => u.Sent_flg == true).ToListAsync();

                for (int i = 0; i < SentMessages.Count; i++)
                {
                    SentMessages[i].NumberCount = i + 1;
                }

                TempData["username"] = HttpContext.User.Claims.First().Value;
                return View(SentMessages);
            }
            catch (Exception ex)
            {
                ViewBag.message = ex.Message;
                return View(Enumerable.Empty<MediaService>());
            }
        }
    }
}
