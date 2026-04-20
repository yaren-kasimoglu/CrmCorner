using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CrmCorner.Models;

namespace CrmCorner.Services.ChatCorner
{
    public class ChatAuthorizationService : IChatAuthorizationService
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ChatAuthorizationService(CrmCornerContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<(bool Success, string ErrorMessage)> CanViewUserAsync(string currentUserId, string targetEmail, string targetName)
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);
            if (currentUser == null)
                return (false, "Aktif kullanıcı bulunamadı.");

            AppUser targetUser = null;

            if (!string.IsNullOrWhiteSpace(targetEmail))
            {
                targetUser = await _context.Users
                    .FirstOrDefaultAsync(x => x.Email != null && x.Email.ToLower() == targetEmail.ToLower());
            }
            else if (!string.IsNullOrWhiteSpace(targetName))
            {
                targetUser = await _context.Users
                    .FirstOrDefaultAsync(x => x.NameSurname != null && x.NameSurname.ToLower().Contains(targetName.ToLower()));
            }

            if (targetUser == null)
                return (false, "Hedef kullanıcı bulunamadı.");

            if (currentUser.Id == targetUser.Id)
                return (true, null);

            var currentRoles = await _userManager.GetRolesAsync(currentUser);
            var targetRoles = await _userManager.GetRolesAsync(targetUser);

            if (currentRoles.Contains("SuperAdmin") || currentRoles.Contains("Admin"))
                return (true, null);

            if (currentRoles.Contains("TeamLeader"))
            {
                if (targetRoles.Contains("TeamLeader") || targetRoles.Contains("TeamMember"))
                    return (true, null);

                return (false, "Bu kullanıcının verilerini görüntüleme yetkiniz bulunmuyor.");
            }

            if (currentRoles.Contains("TeamMember"))
                return (false, "Sadece kendi verilerinizi görüntüleyebilirsiniz.");

            return (false, "Bu kullanıcının verilerini görüntüleme yetkiniz bulunmuyor.");
        }
    }
}