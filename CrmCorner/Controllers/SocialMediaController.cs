using CrmCorner.Models;
using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    public class SocialMediaController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly IWebHostEnvironment _environment;

        public SocialMediaController(CrmCornerContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string search, ContentType? type, int page = 1)
        {
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
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(SocialMediaContent model, IFormFile mediaFile)
        {
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
            var content = await _context.SocialMediaContents.FindAsync(id);
            if (content == null)
            {
                return NotFound();
            }

            return View(content);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var content = await _context.SocialMediaContents.FindAsync(id);
            if (content == null) return NotFound();

            content.Status = ContentStatus.Onaylandi;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "SocialMedia");
        }

        [HttpPost]
        public async Task<IActionResult> GiveFeedback(int id, string feedbackMessage)
        {
            var content = await _context.SocialMediaContents.FindAsync(id);
            if (content == null) return NotFound();

            content.FeedbackMessage = feedbackMessage;
            content.Status = ContentStatus.FeedbackVerildi;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "SocialMedia");
        }


    }
}
