using GeneralMediaServices.Models;
using GeneralMediaServices.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;

namespace GeneralMediaServices.Hubs
{
    public class MessageHub : Hub
    {
        public async Task SendMessage(string user, string message)//a method that can be called by clients to send a message to all connected clients.
        {
            // Do something with the received data

            // Send data back to the client immediately
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        private readonly MediaServiceDbContext _dbContext;
        

        public MessageHub(MediaServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task UpdateMediaService(string messageDescription, string systemName, string smsNumber, string apiUrl)
        {
            var message = "test";
            // Create a new MediaService object with the updated properties
            var mediaService = new MediaService
            {
                Msg_desc = messageDescription,
                Sys_nme = systemName,
                Sms_no = smsNumber,
                Api_url = apiUrl
            };

            mediaService.Sent_flg = false;
            mediaService.Receive_flg = false;
            // If SMS is set, send the SMS and save the MediaService object
            if (!string.IsNullOrEmpty(mediaService.Sms_no))
            {
                // Send SMS
                mediaService.Sent_dt = DateTime.Now;
                mediaService.Sent_flg = true;
                mediaService.Receive_flg = true;
            }

            // If API is set, call the API and save the MediaService object
            if (!string.IsNullOrEmpty(mediaService.Api_url))
            {
                try
                {
                    using (var client = new HttpClient()) //send data to api
                    {
                        var data = new { Message = mediaService.Msg_desc };
                        var json = JsonSerializer.Serialize(data);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        HttpResponseMessage response = await client.PostAsync(mediaService.Api_url, content);
                        if (!response.IsSuccessStatusCode)
                        {
                            // post failed
                            mediaService.ErrorLog = response.ReasonPhrase;
                            mediaService.Sent_flg = true;
                        }
                        else
                        {
                            // post successful
                            mediaService.Sent_dt = DateTime.Now;
                            mediaService.Sent_flg = true;
                            mediaService.Receive_flg = true;
                        }
                    }
                }
                catch(Exception ex)
                {
                    // post failed
                    mediaService.ErrorLog = ex.Source;
                    mediaService.Sent_flg = true;
                }
                
            }

            //Both SMS and api are null
            if (string.IsNullOrEmpty(mediaService.Sms_no) && string.IsNullOrEmpty(mediaService.Api_url))
            {
                mediaService.ErrorLog = "Both SMS and API are null.";
                message = mediaService.ErrorLog;
            }

            // Create a message string with the system name and message description
            message = $"{mediaService.Sys_nme} sends {mediaService.Msg_desc}";
            
            await SaveMediaService(mediaService);

            // Send the updated data back to the client
            await Clients.All.SendAsync("ReceiveData", message);
        }

        private async Task SaveMediaService(MediaService mediaService)
        {
                mediaService.Msg_received_dt = DateTime.Now;
                _dbContext.media_data_tb.Add(mediaService);
                await _dbContext.SaveChangesAsync();
        }

        public async Task Resend(int id)
        {
            try
            {
                var mediaService = await _dbContext.media_data_tb.FindAsync(id);
                if (mediaService != null)
                {
                    var updatedData = UpdatedData(mediaService);
                    _dbContext.media_data_tb.Update(updatedData);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }


        private MediaService UpdatedData(MediaService mediaService)
        {
            if (mediaService == null)
            {
                throw new ArgumentNullException(nameof(mediaService));
            }

            mediaService.ErrorLog = null;

            if (!string.IsNullOrEmpty(mediaService.Sms_no))
            {
                // Send SMS
                mediaService.Sent_dt ??= DateTime.Now;
                /*mediaService.Resent_dt = DateTime.Now;*/
                mediaService.Sent_flg = true;
                mediaService.Receive_flg = true;
            }

            if (!string.IsNullOrEmpty(mediaService.Api_url))
            {
                // Call API
                mediaService.Sent_dt ??= DateTime.Now;
               /* mediaService.Resent_dt = DateTime.Now;*/
                mediaService.Sent_flg = true;
                mediaService.Receive_flg = true;
            }

            //Both SMS and api are null
            if (string.IsNullOrEmpty(mediaService.Sms_no) && string.IsNullOrEmpty(mediaService.Api_url))
            {
                mediaService.ErrorLog = "Both SMS and API are null.";
                mediaService.Sent_dt = null;
                /*mediaService.Resent_dt = null;*/
                mediaService.Sent_flg = false;
                mediaService.Receive_flg = false;
            }

            return mediaService;
        }


    }
}
