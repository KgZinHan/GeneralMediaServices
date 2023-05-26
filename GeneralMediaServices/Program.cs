using GeneralMediaServices.Services;
using GeneralMediaServices.Hubs;
using GeneralMediaServices.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using GeneralMediaServices.Models;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

/*builder.Services.AddSignalR();*/

var databaseSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();

// Add to handle user defined database settings
builder.Services.AddSingleton<DatabaseSettings>();

// Add session dependency injection to handle authentication and database settings
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.Zero;
});

// Add authentication dependency injection to authenticate log in user
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/LogIn/Index";
        options.ExpireTimeSpan = TimeSpan.Zero;
    });

builder.Services.AddDistributedMemoryCache();

// Add background service to handle main purposes of web app
builder.Services.AddHostedService<BackgroundWorkerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

/*app.MapHub<MessageHub>("/messageHub");*/

app.UseSession();

app.Run();

