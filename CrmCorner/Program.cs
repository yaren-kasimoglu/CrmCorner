using CrmCorner.Models;
using Microsoft.AspNetCore.Identity;
using CrmCorner.Hubs;
using CrmCorner.Models;
using Microsoft.EntityFrameworkCore;
using static CrmCorner.Hubs.Hubs;
using static Microsoft.EntityFrameworkCore.ServerVersion;
using CrmCorner.Extensions;
using static CrmCorner.Models.IGoogleCalendarService;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();


builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

//builder.Services.AddDbContext<CrmCornerContext>(x=>x.UseSqlServer(builder.Configuration.GetConnectionString("CrmConnection")));

// MySQL veritabanı bağlantı dizesini alın
var connectionString = builder.Configuration.GetConnectionString("CrmConnection");

// Veritabanı bağlantısını ekleyin
builder.Services.AddDbContext<CrmCornerContext>(options =>
{
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 6, 14)));
});

builder.Services.AddIdentityWithExt();


//builder.Services.AddDefaultIdentity<CrmCornerUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<CrmCornerContext>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); //wwwroot klasörünün kullanımını aktifleştirir.

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapHub<ChatHub>("/chatHub");















app.Run();
