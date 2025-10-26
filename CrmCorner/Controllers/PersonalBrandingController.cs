using CrmCorner.Models;
using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    public class PersonalBrandingController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;

        public PersonalBrandingController(UserManager<AppUser> userManager, CrmCornerContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            var query = _context.PersonalBrandingContents
                                .Include(x => x.Company)
                                .Include(x => x.PersonalUser)
                                .OrderByDescending(x => x.CreatedDate)
                                .AsQueryable();


            if (!(roles.Contains("SocialMediaAdmin") || roles.Contains("Admin") || roles.Contains("SuperAdmin")))
            {
                query = query.Where(x => x.CompanyId == currentUser.CompanyId);
            }


            var list = await query.ToListAsync();
            return View(list);
        }

        [Authorize(Roles = "SuperAdmin,Admin,SocialMediaAdmin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Şirket listesi
            var companies = await _context.Companies
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
            ViewBag.CompanyList = new SelectList(companies, "CompanyId", "CompanyName");

            // Kullanıcı listesi (kişisel hesap için)
            // Burada UserManager üzerinden gidiyoruz ki kesin dolsun
            var users = await _userManager.Users
                .OrderBy(u => u.NameSurname) // AppUser içinde FullName yoksa -> u.UserName / u.NameSurname / u.Email kullan
                .Select(u => new { u.Id, u.NameSurname })
                .ToListAsync();

            ViewBag.UserList = new SelectList(users, "Id", "NameSurname");

            return View();
        }


        [Authorize(Roles = "SuperAdmin,Admin,SocialMediaAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create(PersonalBrandingContent model, IFormFile mediaFile)
        {
            if (mediaFile != null)
            {
                using var ms = new MemoryStream();
                await mediaFile.CopyToAsync(ms);
                model.MediaFile = ms.ToArray();
            }

            // 🔹 Zorunlu alanlar
            model.CreatedDate = DateTime.Now;
            model.Status = ContentStatus.OnayBekliyor;

            _context.PersonalBrandingContents.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "SuperAdmin,Admin,SocialMediaAdmin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var content = await _context.PersonalBrandingContents.FindAsync(id);
            if (content == null) return NotFound();
            return View(content);
        }

        [Authorize(Roles = "SuperAdmin,Admin,SocialMediaAdmin")]
        [HttpPost]
        public async Task<IActionResult> Edit(PersonalBrandingContent model, IFormFile mediaFile)
        {
            var content = await _context.PersonalBrandingContents.FindAsync(model.Id);
            if (content == null) return NotFound();

            content.Title = model.Title;
            content.Description = model.Description;
            content.EstimatedPublishDate = model.EstimatedPublishDate;
            content.Status = model.Status;

            if (mediaFile != null)
            {
                using var ms = new MemoryStream();
                await mediaFile.CopyToAsync(ms);
                content.MediaFile = ms.ToArray();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "SuperAdmin,Admin,SocialMediaAdmin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var content = await _context.PersonalBrandingContents.FindAsync(id);
            if (content == null) return NotFound();

            _context.PersonalBrandingContents.Remove(content);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser?.Picture ?? "defaultpp.png");

            var content = await _context.PersonalBrandingContents
                .Include(c => c.Company)
                .Include(x => x.PersonalUser)
                .Include(c => c.Feedbacks) // Feedback navigation varsa çeker
                .FirstOrDefaultAsync(c => c.Id == id);

            if (content == null)
                return NotFound();

            // Şirket görünürlüğü (admin değilse sadece kendi şirketini görsün)
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (!(roles.Contains("SocialMediaAdmin") || roles.Contains("Admin") || roles.Contains("SuperAdmin")))
            {
                if (currentUser == null || content.CompanyId != currentUser.CompanyId)
                    return Forbid();
            }

            // View tarafında en yeniler üstte görünsün
            content.Feedbacks ??= new List<PersonalBrandingFeedback>();
            content.Feedbacks = content.Feedbacks.OrderByDescending(f => f.CreatedDate).ToList();


            return View(content);
        }


        [Authorize(Roles = "SuperAdmin,Admin,SocialMediaAdmin,SocialMediaUser")]
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var content = await _context.PersonalBrandingContents.FindAsync(id);
            if (content == null) return NotFound();

            content.Status = ContentStatus.Onaylandi;
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "SuperAdmin,Admin,SocialMediaAdmin,SocialMediaUser")]
        [HttpPost]
        public async Task<IActionResult> CancelApproval(int id)
        {
            var content = await _context.PersonalBrandingContents.FindAsync(id);
            if (content == null) return NotFound();

            content.Status = ContentStatus.OnayBekliyor;
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id });
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("PersonalBranding/GetMedia/{id}")]
        public async Task<IActionResult> GetMedia(int id)
        {
            var content = await _context.PersonalBrandingContents
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (content == null || content.MediaFile == null)
                return NotFound();

            // İçerik türünü byte header’dan tahmin et
            string mimeType;
            var header = content.MediaFile.Take(10).ToArray();

            if (header.Length >= 3 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E) // PNG
                mimeType = "image/png";
            else if (header.Length >= 2 && header[0] == 0xFF && header[1] == 0xD8) // JPG
                mimeType = "image/jpeg";
            else if (header.Length >= 4 && header[0] == 0x00 && header[1] == 0x00 && header[2] == 0x00 && header[3] == 0x18)
                mimeType = "video/mp4"; // çok kaba bir heuristik; istersen ContentType alanı ekleyebiliriz
            else
                mimeType = "application/octet-stream";

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            Response.Headers["Content-Disposition"] = "inline";

            return File(content.MediaFile, mimeType);

        }

        // Yeni geri bildirim ekleme
        [HttpPost]
        public async Task<IActionResult> SendFeedback(int id, string feedbackMessage)
        {
            if (string.IsNullOrWhiteSpace(feedbackMessage))
                return RedirectToAction("Details", new { id });

            var currentUser = await _userManager.GetUserAsync(User);

            var feedback = new PersonalBrandingFeedback
            {
                PersonalBrandingContentId = id,
                Message = feedbackMessage.Trim(),
                CreatedDate = DateTime.Now,
                CreatedById = currentUser?.Id
            };

            _context.PersonalBrandingFeedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id });
        }

        // Feedback silme (isteğe bağlı)
        [Authorize(Roles = "SuperAdmin,Admin,SocialMediaAdmin")]
        [HttpPost]
        public async Task<IActionResult> DeleteFeedback(int feedbackId, int contentId)
        {
            var feedback = await _context.PersonalBrandingFeedbacks.FindAsync(feedbackId);
            if (feedback == null) return NotFound();

            _context.PersonalBrandingFeedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = contentId });
        }


    }
}
