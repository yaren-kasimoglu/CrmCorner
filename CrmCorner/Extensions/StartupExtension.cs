using CrmCorner.CustomValidation;
using CrmCorner.Localizations;
using CrmCorner.Models;
using Microsoft.AspNetCore.Identity;

namespace CrmCorner.Extensions
{
    public static class StartupExtension
    {
        public static void AddIdentityWithExt(this IServiceCollection services)
        {


            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(1); // şifre sıfırlama için gönderilen token ın 1 saatlik ömrü  verildi.
            });

            //Kullanıcı Kaydı yapılırken kullanıcı adı  ve şifre kuralları
            services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "ABCDEFGHIJKLMNOPRSTUVYZXabcdefghijklmnoprstuvyzxw0123456789_@-.";

                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2); //yanlış girişte 2 dk boyunca kitlensin
                options.Lockout.MaxFailedAccessAttempts = 3;//3 yanlış girişten sonra kitlensin






            }).AddPasswordValidator<PasswordValidator>()
            .AddUserValidator<UserValidator>()
            .AddErrorDescriber<LocalizationIdentityErrorDescriber>()
            .AddEntityFrameworkStores<CrmCornerContext>()
            .AddDefaultTokenProviders();
        }
    }
}
