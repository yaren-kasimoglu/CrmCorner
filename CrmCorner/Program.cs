using CrmCorner;
using CrmCorner.Controllers;
using CrmCorner.Extensions;
using CrmCorner.Filters;
using CrmCorner.Hubs;
using CrmCorner.Models;
using CrmCorner.OptionModels;
using CrmCorner.Services;
using Google.Apis.Gmail.v1;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using static CrmCorner.Hubs.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Hata günlüğü ayarları
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Google Calendar servisini ekleyin
builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();

// HttpClient servisleri
builder.Services.AddHttpClient<ApolloService>();
builder.Services.AddHttpClient<ApolloHealthService>();

// SignalR
builder.Services.AddSignalR();

// MVC + Razor Runtime Compilation + Global Filters
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AdminLayoutFilter>();
    options.Filters.Add<SetUserPictureFilter>();
    options.Filters.Add<RoleAccessFilter>();
})
.AddRazorRuntimeCompilation();

// Fiziksel dosya sağlayıcısı
builder.Services.AddSingleton<IFileProvider>(
    new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
);

// Form seçenekleri
builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue;
    x.MultipartHeadersLengthLimit = int.MaxValue;
});

// MySQL veritabanı bağlantısı
var connectionString = builder.Configuration.GetConnectionString("CrmConnection");

builder.Services.AddDbContext<CrmCornerContext>(options =>
{
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 6, 18)))
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors();
});

// SMTP ayarları
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// Email servisleri
builder.Services.AddSingleton<EmailService>();


// Identity
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<CrmCornerContext>()
.AddDefaultTokenProviders();

// Filtreler
builder.Services.AddScoped<SetUserPictureFilter>();
builder.Services.AddScoped<RoleAccessFilter>();

// Cookie ayarları
builder.Services.ConfigureApplicationCookie(opt =>
{
    var cookieBuilder = new CookieBuilder
    {
        Name = "CrmAppCookie",
        HttpOnly = true,
        IsEssential = true,
    };

    opt.LoginPath = new PathString("/Home/SignIn");
    opt.LogoutPath = new PathString("/Member/Logout");
    opt.AccessDeniedPath = new PathString("/Member/AccessDenied");
    opt.Cookie = cookieBuilder;
    opt.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    opt.SlidingExpiration = true;
});

var app = builder.Build();

// HTTP istek hattı
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Landing}/{id?}");

app.MapHub<ChatHub>("/chatHub");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services);
}

app.Run();