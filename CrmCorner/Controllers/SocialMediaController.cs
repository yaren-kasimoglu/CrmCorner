using CrmCorner.Models;
using CrmCorner.Models.Enums;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
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

        public async Task<IActionResult> Index(string search, ContentType? type, int page = 1, int? selectedCompanyId = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

            var roles = await _userManager.GetRolesAsync(currentUser);
            var query = _context.SocialMediaContents.AsQueryable();

            // Adminse: Firma seçimini yapabilir
            if (roles.Contains("Admin"))
            {
                // Dropdown için şirket listesini ViewBag’e ekle
                var companies = await _context.Companies.ToListAsync();
                ViewBag.CompanyList = new SelectList(companies, "CompanyId", "CompanyName");
                ViewBag.SelectedCompanyId = selectedCompanyId;

                // Eğer admin belirli bir firma seçtiyse, o firmaya göre filtrele
                if (selectedCompanyId.HasValue)
                {
                    query = query.Where(x => x.CompanyId == selectedCompanyId.Value);
                }
            }
            else
            {
                // Admin değilse sadece kendi şirketine ait içerikleri görebilsin
                query = query.Where(x => x.CompanyId == currentUser.CompanyId);
            }

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
            ViewBag.IsAdmin = roles.Contains("Admin");


            return View(items);
        }



        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

            // HATALARI ENGELLEMEK İÇİN NULL KONTROLÜ
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı.";
                return RedirectToAction("Index");
            }

            // Company listesi ViewBag'e atanıyor
            var companies = await _context.Companies.ToListAsync();
            ViewBag.CompanyList = new SelectList(companies, "CompanyId", "CompanyName");

            return View(new SocialMediaContent()); // boş model döndürüyoruz
        }



        [HttpPost]
        [Authorize(Roles = "Admin")]
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
                            model.MediaFile = memoryStream.ToArray(); // sadece veritabanına kaydediyoruz
                        }
                    }

                    model.CreatedDate = DateTime.Now;
                    model.Status = ContentStatus.OnayBekliyor;

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

            return View(content); // Düzenlenecek içeriği View'a gönder
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
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
                    // Eğer dosya yüklenmediyse eski MediaFile aynen kalacak
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Dosya yüklenirken bir hata oluştu: " + ex.Message);
                    return View(model);
                }

                // Diğer alanları güncelle
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

            // İçerik tipine göre content type set edelim (jpg, png, mp4 gibi)
            var fileExtension = GetFileExtension(content);
            var mimeType = GetMimeType(fileExtension);

            return File(content.MediaFile, mimeType);
        }

        // Yardımcı methodlar:
        private string GetFileExtension(SocialMediaContent content)
        {
            // Basit mantık: burada istersen ContentType'a göre de ayarlayabiliriz.
            // Örneğin ContentType == Reels ise mp4 kabul ederiz gibi.
            if (content.ContentType == ContentType.Reels)
                return "mp4";
            else
                return "jpg"; // Default image
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
                .Include(c => c.Feedbacks)  // Geri bildirimleri dahil et
                .FirstOrDefaultAsync(c => c.Id == id);  // İçeriği ID ile bul

            if (content == null)
            {
                return NotFound();
            }

            // Geri bildirimleri oluşturulma tarihine göre azalan sırayla sıralıyoruz
            content.Feedbacks = content.Feedbacks.OrderByDescending(f => f.CreatedDate).ToList();

            return View(content);  // İçeriği ve geri bildirimlerini View'a gönder
        }


        // Onaylama
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");
            var content = await _context.SocialMediaContents.FindAsync(id);
            if (content == null)
            {
                return NotFound();
            }

            content.Status = ContentStatus.Onaylandi;  // Onaylandı olarak ayarlıyoruz.
            _context.Update(content);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = content.Id });
        }

  
        [HttpPost]
        public async Task<IActionResult> CancelApproval(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");
            var content = await _context.SocialMediaContents.FindAsync(id);
            if (content != null)
            {
                content.Status = ContentStatus.OnayBekliyor;  // Geri al
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", new { id = content.Id });
        }



        // Geri bildirim eklemek için metot
        [HttpPost]
        public async Task<IActionResult> SendFeedback(int id, string feedbackMessage)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");
            var content = await _context.SocialMediaContents
                .Include(c => c.Feedbacks)  // İlişkili geri bildirimleri dahil et
                .FirstOrDefaultAsync(c => c.Id == id);

            if (content == null)
            {
                return NotFound();
            }

            // Yeni bir geri bildirim oluştur
            var feedback = new Feedback
            {
                SocialMediaContentId = content.Id,
                Message = feedbackMessage,
                CreatedDate = DateTime.Now
            };

            // Geri bildirimi veritabanına ekle
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = content.Id });
        }

  
        // Geri bildirimi silmek için metot
        [HttpPost]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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

            var roles = await _userManager.GetRolesAsync(currentUser);
            var query = _context.SocialMediaContents
                .Where(c => c.ScheduledPublishDate.HasValue &&
                            c.ScheduledPublishDate.Value.Month == currentMonth &&
                            c.ScheduledPublishDate.Value.Year == currentYear);

            // Admin değilse sadece kendi şirketine ait içerikleri getir
            if (!roles.Contains("Admin"))
            {
                query = query.Where(c => c.CompanyId == currentUser.CompanyId);
            }

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
