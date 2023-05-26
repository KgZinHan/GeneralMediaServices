
using System.Text.Json;
using System.Text;
using GeneralMediaServices.Models;
using GeneralMediaServices.Data;
using Microsoft.EntityFrameworkCore;

namespace GeneralMediaServices.Services
{
    public class BackgroundWorkerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DatabaseSettings _databaseSettings;

        public BackgroundWorkerService(IServiceProvider serviceProvider,DatabaseSettings databaseSettings)
        {
            _serviceProvider = serviceProvider;
            _databaseSettings = databaseSettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var databaseSettings = scope.ServiceProvider.GetRequiredService<DatabaseSettings>();
                        var connectionString = databaseSettings.ConnectionString;
                        
                        var optionsBuilder = new DbContextOptionsBuilder<MediaServiceDbContext>().UseSqlServer(_databaseSettings.ConnectionString);

                        using (var dbContext = new MediaServiceDbContext(optionsBuilder.Options))
                        {
                            var notSendMessages = dbContext.media_data_tb.Where(u => u.Sent_flg == false).ToList();

                            foreach (var message in notSendMessages)
                            {
                                var updatedMessage = await SendMessage(message);
                                dbContext.media_data_tb.Update(updatedMessage);
                            }

                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken); // wait for 60 seconds before next execution
            }
        }


        // Send Message when new data is there
        private async Task<MediaService> SendMessage(MediaService msg)
        {
            msg.Sent_flg = false;
            msg.Receive_flg = false;

            //Check first Time Msg Received or not
            if (msg.Msg_received_dt.Equals(null))
            {
                msg.Msg_received_dt = DateTime.Now;
            }

            // If SMS is set, send the SMS and save the MediaService object
            if (!string.IsNullOrEmpty(msg.Sms_no))
            {
                // Send SMS
                msg.Sent_dt = DateTime.Now;
                msg.Sent_flg = true;
                msg.Receive_flg = true;
            }

            // If API is set, call the API and save the MediaService object
            if (!string.IsNullOrEmpty(msg.Api_url))
            {
                try
                {
                    using (var client = new HttpClient
                    {
                        Timeout = TimeSpan.FromSeconds(10) // Set the timeout value to 10 seconds
                    })
                    {
                        var data = new { Message = msg.Msg_desc };
                        var json = JsonSerializer.Serialize(data);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        HttpResponseMessage response = await client.PostAsync(msg.Api_url, content);

                        if (!response.IsSuccessStatusCode)
                        {
                            // post failed
                            msg.ErrorLog = "API: " + response.ReasonPhrase;
                        }
                        else
                        {
                            // post successful
                            msg.Sent_dt = DateTime.Now;
                            msg.Sent_flg = true;
                            msg.Receive_flg = true;
                            msg.ErrorLog = "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    // post failed
                    msg.ErrorLog = "API: " + ex.Message;
                }

            }

            //Both SMS and api are null
            if (string.IsNullOrEmpty(msg.Sms_no) && string.IsNullOrEmpty(msg.Api_url))
            {
                msg.ErrorLog = "Both SMS and API are null.";
            }

            return msg;
        }
    }
}
