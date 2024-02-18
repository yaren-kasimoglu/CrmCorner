using CrmCorner.Models;
using Microsoft.AspNetCore.Identity;
using CrmCorner.Hubs;
using CrmCorner.Models;
using Microsoft.EntityFrameworkCore;
using static CrmCorner.Hubs.Hubs;
using static Microsoft.EntityFrameworkCore.ServerVersion;
using CrmCorner.Extensions;

using static CrmCorner.Models.IGoogleCalendarService;

using CrmCorner.OptionsModels;
using CrmCorner.Services;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();


builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddSingleton<IFileProvider>(
    new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
);


//builder.Services.AddDbContext<CrmCornerContext>(x=>x.UseSqlServer(builder.Configuration.GetConnectionString("CrmConnection")));

// MySQL veritabanı bağlantı dizesini alın
var connectionString = builder.Configuration.GetConnectionString("CrmConnection");

// Veritabanı bağlantısını ekleyin
builder.Services.AddDbContext<CrmCornerContext>(options =>
{
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 6, 14)));
});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddIdentityWithExt();
builder.Services.AddScoped<IEmailServices, EmailServices>();


//builder.Services.AddDefaultIdentity<CrmCornerUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<CrmCornerContext>();


builder.Services.ConfigureApplicationCookie(opt =>
{
    var cookieBuilder = new CookieBuilder();
    cookieBuilder.Name = "CrmAppCookie";

    opt.LoginPath = new PathString("/Home/SignIn");
    opt.LogoutPath = new PathString("/Member/Logout");
    opt.Cookie = cookieBuilder;
    opt.ExpireTimeSpan = TimeSpan.FromDays(60); //cookie ömrü
    opt.SlidingExpiration = true;//true yapmazsak 60 gün sonra bir daha giremez. true yaptığımızda 60 gün sonra girdiğinde tekrar 60 günlük bir ömrü olur cookienin
});




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();



}
//app.Use(async (context, next) =>
//{
//    // Kullanıcı giriş yapmış mı kontrol et (Bu örnekte basit bir kontrol mekanizması kullanılmıştır, gerçek uygulamalarda güvenli kimlik doğrulama yöntemleri kullanılmalıdır.)
//    var path = context.Request.Path.ToString();
//    var isUserLoggedIn = context.User.Identity.IsAuthenticated; // Oturum yönetimi mekanizmanıza göre bu kontrol değişebilir

//    if (!isUserLoggedIn && path == "/")
//    {
//        // Kullanıcı giriş yapmamışsa ve kök dizindeyse, Giriş sayfasına yönlendir
//        context.Response.Redirect("/Home/Giris");
//    }
//    else if (isUserLoggedIn && path == "/")
//    {
//        // Kullanıcı giriş yapmışsa ve kök dizindeyse, Index sayfasına yönlendir
//        context.Response.Redirect("/Home/Index");
//    }
//    else
//    {
//        // Diğer durumlar için pipeline'ı devam ettir
//        await next();
//    }
//});




app.UseHttpsRedirection();
app.UseStaticFiles(); //wwwroot klasörünün kullanımını aktifleştirir.

app.UseRouting();

app.UseAuthentication();//kimlik yetkilendirme
app.UseAuthorization();//kimlik doğrulama


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapHub<ChatHub>("/chatHub");


app.Run();