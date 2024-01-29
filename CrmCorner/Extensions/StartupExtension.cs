using CrmCorner.CustomValidation;
using CrmCorner.Models;
using Microsoft.AspNetCore.Identity;

namespace CrmCorner.Extensions
{
    public static class StartupExtension
    {
        public static void AddIdentityWithExt(this IServiceCollection services)
        {
            //Kullanıcı Kaydı yapılırken kullanıcı adı  ve şifre kuralları
            services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnoprstuvyzxw0123456789_@-";

                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;

            }).AddPasswordValidator<PasswordValidator>().AddEntityFrameworkStores<CrmCornerContext>().AddDefaultTokenProviders();
        }
    }
}
