using CrmCorner.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.ServerVersion;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// MySQL veritabanı bağlantı dizesini alın
var connectionString = builder.Configuration.GetConnectionString("CrmConnection");

// Veritabanı bağlantısını ekleyin
builder.Services.AddDbContext<CrmcornerContext>(options =>
{
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 6, 14)));
});

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
