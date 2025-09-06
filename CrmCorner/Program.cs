using CrmCorner.Models;
using Microsoft.AspNetCore.Identity;
using CrmCorner.Hubs;
using Microsoft.EntityFrameworkCore;
using CrmCorner.Extensions;
using CrmCorner.OptionsModels;
using CrmCorner.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http.Features;
using CrmCorner;
using CrmCorner.Controllers;
using CrmCorner.ViewModels;
using static CrmCorner.Hubs.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Hata günlüğü ayarları
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Google Calendar servisini ekleyin
builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();

// MVC ve SignalR servislerini ekleyin
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<ApolloService>();
builder.Services.AddHttpClient<ApolloHealthService>();

builder.Services.AddSignalR();

// Razor runtime derlemesini ekleyin
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AdminLayoutFilter>();
});

// Fiziksel dosya sağlayıcısını ekleyin
builder.Services.AddSingleton<IFileProvider>(
    new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
);

// Form seçeneklerini yapılandırın
builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue;
    x.MultipartHeadersLengthLimit = int.MaxValue;
});

// MySQL veritabanı bağlantısını ekleyin
var connectionString = builder.Configuration.GetConnectionString("CrmConnection");

builder.Services.AddDbContext<CrmCornerContext>(options =>
{
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 6, 18)))
           .EnableSensitiveDataLogging() // Detaylı loglama için
           .EnableDetailedErrors(); // Daha detaylı hata mesajları için
});

// E-posta ayarlarını yapılandırın
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<EmailService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

//Identity servislerini ve cookie yapılandırmasını ekleyin
builder.Services.AddIdentity<AppUser, AppRole>()
    .AddEntityFrameworkStores<CrmCornerContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<SetUserPictureFilter>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<SetUserPictureFilter>();
});


builder.Services.AddScoped<IEmailServices, EmailServices>();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Kullanıcı kilitlenme ayarları
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});
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
    opt.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Cookie ömrü
    opt.SlidingExpiration = true; // Cookie'nin süresini uzat
});

// Dosya yükleme seçeneklerini yapılandırın
//builder.Services.Configure<FileUploadOptions>(builder.Configuration.GetSection("FileUploadOptions"));




var app = builder.Build();

// HTTP istek hattını yapılandırın
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // wwwroot klasörünün kullanımını aktifleştirir

app.UseRouting();

app.UseAuthentication(); // Kimlik doğrulama
app.UseAuthorization(); // Yetkilendirme



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// SignalR hub'ını yapılandırın
app.MapHub<ChatHub>("/chatHub");

app.Run();
