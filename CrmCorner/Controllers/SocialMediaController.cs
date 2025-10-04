using CrmCorner.Models;
using CrmCorner.Models.Enums;
using CrmCorner.ViewModels;
using Independentsoft.Graph.Contacts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    public class SocialMediaController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;
        private readonly IWebHostEnvironment _environment;

        public SocialMediaController(UserManager<AppUser> userManager, CrmCornerContext context, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string search, ContentType? type, int page = 1)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

            var query = _context.SocialMediaContents.AsQueryable();

            // Şimdilik herkes sadece kendi şirketinin içeriklerini görecek
            query = query.Where(x => x.CompanyId == currentUser.CompanyId);

            // Arama filtresi
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search));
            }

            // Tür filtresi
            if (type.HasValue)
            {
                query = query.Where(x => x.ContentType == type.Value);
            }

            // Sayfalama işlemleri
            int pageSize = 6;
            var items = await PaginatedList<SocialMediaContent>.CreateAsync(
                query.OrderByDescending(x => x.CreatedDate),
                page,
                pageSize
            );

            // Özet bilgileri ViewBag’e gönder
            ViewBag.PendingCount = await query.CountAsync(x => x.Status == ContentStatus.OnayBekliyor);
            ViewBag.ApprovedCount = await query.CountAsync(x => x.Status == ContentStatus.Onaylandi);
            ViewBag.FeedbackCount = await query.CountAsync(x => x.Status == ContentStatus.FeedbackVerildi);

            ViewBag.Search = search;
            ViewBag.Type = type;

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı.";
                return RedirectToAction("Index");
            }

            // Company listesi (şimdilik gerekmez ama kalsın)
            var companies = await _context.Companies.ToListAsync();
            ViewBag.CompanyList = new SelectList(companies, "CompanyId", "CompanyName");

            return View(new SocialMediaContent());
        }

        [HttpPost]
        public async Task<IActionResult> Create(SocialMediaContent model, IFormFile mediaFile)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

            ModelState.Remove("MediaFile");

            if (ModelState.IsValid)
            {
                try
                {
                    if (mediaFile != null && mediaFile.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await mediaFile.CopyToAsync(memoryStream);
                            model.MediaFile = memoryStream.ToArray();
                        }
                    }

                    model.CreatedDate = DateTime.Now;
                    model.Status = ContentStatus.OnayBekliyor;
                    model.CompanyId = currentUser.CompanyId;

                    _context.SocialMediaContents.Add(model);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "SocialMedia");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "İçerik eklenirken bir hata oluştu: " + ex.Message);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var content = await _context.SocialMediaContents.FindAsync(id);
            if (content == null)
            {
                return NotFound();
            }

            return View(content);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SocialMediaContent model, IFormFile mediaFile)
        {
            ModelState.Remove("MediaFile");

            if (ModelState.IsValid)
            {
                var content = await _context.SocialMediaContents.FindAsync(model.Id);
                if (content == null)
                {
                    return NotFound();
                }

                try
                {
                    if (mediaFile != null && mediaFile.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await mediaFile.CopyToAsync(memoryStream);
                            content.MediaFile = memoryStream.ToArray();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Dosya yüklenirken bir hata oluştu: " + ex.Message);
                    return View(model);
                }

                content.Title = model.Title;
                content.Description = model.Description;
                content.ScheduledPublishDate = model.ScheduledPublishDate;
                content.ContentType = model.ContentType;
                content.Status = model.Status;

                _context.Update(content);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "SocialMedia");
            }

            return View(model);
        }

        public async Task<IActionResult> GetMedia(int id)
        {
            var content = await _context.SocialMediaContents.FindAsync(id);
            if (content == null || content.MediaFile == null)
            {
                return NotFound();
            }

            var fileExtension = GetFileExtension(content);
            var mimeType = GetMimeType(fileExtension);

            return File(content.MediaFile, mimeType);
        }

        private string GetFileExtension(SocialMediaContent content)
        {
            if (content.ContentType == ContentType.Reels)
                return "mp4";
            else
                return "jpg";
        }

        private string GetMimeType(string extension)
        {
            return extension switch
            {
                "jpg" => "image/jpeg",
                "jpeg" => "image/jpeg",
                "png" => "image/png",
                "mp4" => "video/mp4",
                _ => "application/octet-stream",
            };
        }

        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

            var content = await _context.SocialMediaContents
                .Include(c => c.Feedbacks)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (content == null)
            {
                return NotFound();
            }

            content.Feedbacks = content.Feedbacks.OrderByDescending(f => f.CreatedDate).ToList();

            return View(content);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var content = await _context.SocialMediaContents.FindAsync(id);
            if (content == null)
            {
                return NotFound();
            }

            content.Status = ContentStatus.Onaylandi;
            _context.Update(content);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = content.Id });
        }

        [HttpPost]
        public async Task<IActionResult> CancelApproval(int id)
        {
            var content = await _context.SocialMediaContents.FindAsync(id);
            if (content != null)
            {
                content.Status = ContentStatus.OnayBekliyor;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", new { id = content.Id });
        }

        [HttpPost]
        public async Task<IActionResult> SendFeedback(int id, string feedbackMessage)
        {
            var content = await _context.SocialMediaContents
                .Include(c => c.Feedbacks)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (content == null)
            {
                return NotFound();
            }

            var feedback = new Feedback
            {
                SocialMediaContentId = content.Id,
                Message = feedbackMessage,
                CreatedDate = DateTime.Now
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = content.Id });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFeedback(int feedbackId, int contentId)
        {
            var feedback = await _context.Feedbacks.FindAsync(feedbackId);
            if (feedback == null)
            {
                return NotFound();
            }

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = contentId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var content = await _context.SocialMediaContents.FindAsync(id);
            if (content == null)
            {
                return NotFound();
            }

            _context.SocialMediaContents.Remove(content);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "İçerik başarıyla silindi.";
            return RedirectToAction("Index", "SocialMedia");
        }

        public async Task<IActionResult> Calendar()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var query = _context.SocialMediaContents
                .Where(c => c.ScheduledPublishDate.HasValue &&
                            c.ScheduledPublishDate.Value.Month == currentMonth &&
                            c.ScheduledPublishDate.Value.Year == currentYear &&
                            c.CompanyId == currentUser.CompanyId);

            var contents = await query.ToListAsync();

            var calendarViewModel = new CalendarViewModel
            {
                Month = currentMonth,
                Year = currentYear,
                Contents = contents
            };

            return View(calendarViewModel);
        }
    }
}
