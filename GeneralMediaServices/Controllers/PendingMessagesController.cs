using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeneralMediaServices.Models;
using GeneralMediaServices.Data;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace GeneralMediaServices.Controllers
{
    [Authorize]
    public class PendingMessagesController : Controller
    {
        private readonly DatabaseSettings _databaseSettings;

        public PendingMessagesController(DatabaseSettings databaseSettings)
        {
            _databaseSettings = databaseSettings;
        }
       
        public async Task<IActionResult> Index()
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<MediaServiceDbContext>().UseSqlServer(_databaseSettings.ConnectionString);
                var dbContext = new MediaServiceDbContext(optionsBuilder.Options);

                var NotSendMessages = await dbContext.media_data_tb.Where(u => u.Sent_flg == false).ToListAsync();

                for (int i = 0; i < NotSendMessages.Count; i++)
                {
                    NotSendMessages[i].NumberCount = i + 1;
                    if (NotSendMessages[i].Msg_received_dt.Equals(null))
                    {
                        var updateSendMessages = dbContext.media_data_tb.FirstOrDefault(u => u.Id == NotSendMessages[i].Id);
                        if(updateSendMessages != null && updateSendMessages.Msg_received_dt == null)
                        {
                            updateSendMessages.Msg_received_dt = DateTime.Now;
                            dbContext.media_data_tb.Update(updateSendMessages);
                        }
                    }
                }
                
                ViewBag.InfoMessage = TempData["info message"];
                TempData["username"] = HttpContext.User.Claims.First().Value;
                return View(NotSendMessages);
            }
            catch (Exception ex)
            {
                ViewBag.AlertMessage = ex.Message;
                return View(Enumerable.Empty<MediaService>());
            }
            
        }
    }
}

