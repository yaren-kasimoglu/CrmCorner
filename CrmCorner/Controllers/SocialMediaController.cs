using CrmCorner.Models;
using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    [Authorize]
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

            if (!string.IsNullOrEmpty(search))
                query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search));

            if (type.HasValue)
                query = query.Where(x => x.ContentType == type.Value);


            // Sayfalama
            int pageSize = 6;
            var items = await PaginatedList<SocialMediaContent>.CreateAsync(query.OrderByDescending(x => x.CreatedDate), page, pageSize);

            // Özet için ViewBag
            ViewBag.PendingCount = await _context.SocialMediaContents.CountAsync(x => x.Status == ContentStatus.OnayBekliyor);
            ViewBag.ApprovedCount = await _context.SocialMediaContents.CountAsync(x => x.Status == ContentStatus.Onaylandi);
            ViewBag.FeedbackCount = await _context.SocialMediaContents.CountAsync(x => x.Status == ContentStatus.FeedbackVerildi);

            ViewBag.Search = search;
            ViewBag.Type = type;
        

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");
            return View();
        }

     
        [HttpPost]
        public async Task<IActionResult> Create(SocialMediaContent model, IFormFile mediaFile)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");
            // Bu alan formda gelmediği için manuel set ediyoruz, validasyon dışında tutmalıyız
            ModelState.Remove("MediaPath");

            if (ModelState.IsValid)
            {
                if (mediaFile != null && mediaFile.Length > 0)
                {
                    var uploadFolder = Path.Combine(_environment.WebRootPath, "uploadsSocialMedia");
                    Directory.CreateDirectory(uploadFolder); // klasör yoksa oluştur

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + mediaFile.FileName;
                    var filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await mediaFile.CopyToAsync(stream);
                    }

                    model.MediaPath = "/uploadsSocialMedia/" + uniqueFileName;
                }

                model.CreatedDate = DateTime.Now;
                model.Status = 0;

                _context.SocialMediaContents.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "SocialMedia");
            }

            // Validasyon hataları varsa yazdır
            foreach (var error in ModelState)
            {
                Console.WriteLine($"Key: {error.Key}");
                foreach (var e in error.Value.Errors)
                {
                    Console.WriteLine($"  Error: {e.ErrorMessage}");
                }
            }

            return View(model);
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

    }
}
