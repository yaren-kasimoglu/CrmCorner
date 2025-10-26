using Microsoft.AspNetCore.Identity;

namespace CrmCorner.Models
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();

            // Roller
            if (!await roleManager.RoleExistsAsync("SocialMediaAdmin"))
                await roleManager.CreateAsync(new AppRole { Name = "SocialMediaAdmin" });

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new AppRole { Name = "User" });

            // Kullanıcı
            var user = await userManager.FindByNameAsync("YarenKasimoglu");
            if (user == null)
            {
                new AppUser
                {
                    UserName = "yarenkasSaaskontrol",
                    Email = "yaren@saascorner.co",
                    CompanyName = "SAAS Corner",   // 🔥 eklendi
                    CompanyId = 9,                 // varsa (opsiyonel)
                    EmailDomain = "saascorner.co"  // senin sisteminde genelde kullanılıyor
                };

                // şifre: Test123!
                var result = await userManager.CreateAsync(user, "Yy.134679*");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "SocialMediaAdmin");
                }
            }
        }
    }
}
