using CrmCorner.Extensions;
using CrmCorner.Models;
using CrmCorner.Models.CrmCorner.Models;
using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using PipelineStage = CrmCorner.Models.Enums.PipelineStage;

namespace CrmCorner.Controllers
{
    public class PipelineTaskController : Controller
    {
        private readonly CrmCornerContext _context;

        public PipelineTaskController(CrmCornerContext context)
        {
            _context = context;
        }

        // 1. Görevleri Listeleme
        public IActionResult PipelineIndex()
        {
            // Enum değerlerini al ve ViewBag'e ata (PipelineStage enum'unun DisplayName varsa onu kullan, yoksa ToString())
            ViewBag.StatusList = Enum.GetValues(typeof(PipelineStage))
                                     .Cast<PipelineStage>()
                                     .ToDictionary(
                                         e => e,
                                         e => e.ToString() // veya e.GetDisplayName() eğer uzantı metodun varsa
                                     );

            // Görevleri tarihe göre getir
            var tasks = _context.PipelineTasks
                                .OrderByDescending(t => t.CreatedDate)
                                .ToList();

            return View(tasks);
        }


        // 2. Yeni Görev Formu (GET)
        public IActionResult PipelineTaskCreate()
        {
            ViewBag.StageList = Enum.GetValues(typeof(PipelineStage))
         .Cast<PipelineStage>()
         .Select(e => new SelectListItem
         {
             Value = ((int)e).ToString(),
             Text = e.GetDisplayName()
         }).ToList();

            // Eğer kullanıcı listesi gerekiyorsa:
            var users = _context.Users.ToList();
            ViewBag.Users = users.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.UserName
            }).ToList();

            return View();
        }

        // 3. Yeni Görev Kaydı (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PipelineTaskCreate(PipelineTask task)
        {
            if (ModelState.IsValid)
            {
                // 1. Müşteri bilgilerini CustomerN tablosuna kaydet
                var newCustomer = new CustomerN
                {
                    Name = task.CustomerName,
                    Surname = task.CustomerSurname,
                    CompanyName = task.CompanyName,
                    PhoneNumber = task.Phone,
                    CustomerEmail = task.Email,
                    LinkedinUrl = task.Email, // veya ayrıysa task.LinkedinUrl olarak da olabilir
                    CreatedDate = DateTime.Now,
                    AppUserId = task.ResponsibleUserId
                };

                _context.CustomerNs.Add(newCustomer);
                _context.SaveChanges();

                // 2. Görevi PipelineTasks tablosuna kaydet
                task.CreatedDate = DateTime.Now;
                _context.PipelineTasks.Add(task);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Görev ve müşteri başarıyla eklendi.";
                return RedirectToAction("PipelineIndex");
            }

            return View(task);
        }

        [HttpPost]
        public IActionResult UpdateContactMethods(int taskId, bool contactedViaLinkedIn, bool contactedViaColdCall)
        {
            var task = _context.PipelineTasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
                return NotFound();

            task.ContactedViaLinkedIn = contactedViaLinkedIn;
            task.ContactedViaColdCall = contactedViaColdCall;
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost]
        public IActionResult UpdateStage(int id, int newStage)
        {
            var task = _context.PipelineTasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return NotFound();

            task.Stage = (PipelineStage)newStage;
            _context.SaveChanges();

            return Ok();
        }



        public IActionResult PipelineDetails(int id)
        {
            var task = _context.PipelineTasks
     .Include(t => t.Notes)
     .Include(t => t.AppUser)
     .Include(t => t.Customer)
     .Include(t => t.FileAttachments)
     .FirstOrDefault(t => t.Id == id);


            if (task == null)
                return NotFound();

            var logs = _context.PipelineTaskLogs
                .Include(l => l.UpdatedBy) // Düzenleyen kullanıcıyı görmek için gerekli
                .Where(l => l.PipelineTaskId == id)
                .OrderByDescending(l => l.UpdatedAt)
                .ToList();

            ViewBag.Logs = logs;

            return View(task);
        }



        [HttpPost]
        public IActionResult AddNote(int taskId, string note)
        {
            var task = _context.PipelineTasks.Include(t => t.Notes).FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                var newNote = new PipelineTaskNote
                {
                    PipelineTaskId = taskId,
                    Note = note,
                    CreatedAt = DateTime.Now
                };
                task.Notes.Add(newNote);
                _context.SaveChanges();
            }
            return RedirectToAction("Details", new { id = taskId });
        }

        [HttpGet]
        public IActionResult PipelineEdit(int id)
        {
            var task = _context.PipelineTasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return NotFound();

            // Kullanıcı listesi
            ViewBag.Users = _context.Users
                .Select(u => new SelectListItem
                {
                    Text = u.UserName,
                    Value = u.Id
                }).ToList();

            // Müşteri listesi
            ViewBag.Customer = _context.CustomerNs
                .Select(c => new SelectListItem
                {
                    Text = $"{c.Name} {c.Surname} / {c.CompanyName}",
                    Value = c.Id.ToString()
                }).ToList();

            return View(task);
        }

        [HttpPost]
        public IActionResult PipelineEdit(PipelineTask model)
        {
            var existing = _context.PipelineTasks.FirstOrDefault(t => t.Id == model.Id);
            if (existing == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            existing.AppUserId = userId;

            var now = DateTime.Now;

            var loggedFields = new HashSet<string>();

            void LogIfChanged(string fieldName, object? oldVal, object? newVal)
            {
                // Null kontrolü ve eşitlik karşılaştırması
                var oldStr = oldVal?.ToString();
                var newStr = newVal?.ToString();

                // Aynıysa loglama
                if (oldStr == newStr || string.IsNullOrWhiteSpace(oldStr) && string.IsNullOrWhiteSpace(newStr))
                    return;

                // Log kaydı oluştur
                _context.PipelineTaskLogs.Add(new PipelineTaskLog
                {
                    PipelineTaskId = model.Id,
                    UpdatedField = fieldName,
                    OldValue = oldStr,
                    NewValue = newStr,
                    UpdatedAt = now,
                    UpdatedById = userId
                });
            }


            // Karşılaştır ve logla
            LogIfChanged(nameof(model.Title), existing.Title, model.Title);
            LogIfChanged(nameof(model.Description), existing.Description, model.Description);
            LogIfChanged(nameof(model.Value), existing.Value?.ToString("N2"), model.Value?.ToString("N2"));
            LogIfChanged(nameof(model.Currency), existing.Currency, model.Currency);
            LogIfChanged(nameof(model.Stage), existing.Stage?.ToString(), model.Stage?.ToString());
            LogIfChanged(nameof(model.ExpectedCloseDate), existing.ExpectedCloseDate?.ToString("yyyy-MM-dd"), model.ExpectedCloseDate?.ToString("yyyy-MM-dd"));
            LogIfChanged(nameof(model.CustomerName), existing.CustomerName, model.CustomerName);
            LogIfChanged(nameof(model.CustomerSurname), existing.CustomerSurname, model.CustomerSurname);
            LogIfChanged(nameof(model.CompanyName), existing.CompanyName, model.CompanyName);
            LogIfChanged(nameof(model.Phone), existing.Phone, model.Phone);
            LogIfChanged(nameof(model.Email), existing.Email, model.Email);
            LogIfChanged(nameof(model.LinkedinUrl), existing.LinkedinUrl, model.LinkedinUrl);
            LogIfChanged(nameof(model.OutcomeStatus), existing.OutcomeStatus?.ToString(), model.OutcomeStatus?.ToString());
            LogIfChanged(nameof(model.Source), existing.Source, model.Source);
            LogIfChanged(nameof(model.SourceChannel), existing.SourceChannel, model.SourceChannel);
            LogIfChanged(nameof(model.ResponsibleUserId), existing.ResponsibleUserId, model.ResponsibleUserId);
            LogIfChanged(nameof(model.Outcomes), existing.Outcomes?.ToString(), model.Outcomes?.ToString());
            LogIfChanged(nameof(model.ContactedViaLinkedIn), existing.ContactedViaLinkedIn?.ToString(), model.ContactedViaLinkedIn?.ToString());
            LogIfChanged(nameof(model.ContactedViaColdCall), existing.ContactedViaColdCall?.ToString(), model.ContactedViaColdCall?.ToString());
            LogIfChanged(nameof(model.CustomerId), existing.CustomerId?.ToString(), model.CustomerId?.ToString());

            // Verileri güncelle
            existing.Title = model.Title;
            existing.Description = model.Description;
            existing.Value = model.Value;
            existing.Currency = model.Currency;
            existing.Stage = model.Stage;
            existing.ExpectedCloseDate = model.ExpectedCloseDate;
            existing.CustomerName = model.CustomerName;
            existing.CustomerSurname = model.CustomerSurname;
            existing.CompanyName = model.CompanyName;
            existing.Phone = model.Phone;
            existing.OutcomeStatus = model.OutcomeStatus;
            existing.Email = model.Email;
            existing.LinkedinUrl = model.LinkedinUrl;
            existing.Source = model.Source;
            existing.SourceChannel = model.SourceChannel;
            existing.ResponsibleUserId = model.ResponsibleUserId;
            existing.Outcomes = model.Outcomes;
            existing.ContactedViaLinkedIn = model.ContactedViaLinkedIn;
            existing.ContactedViaColdCall = model.ContactedViaColdCall;
            existing.CustomerId = model.CustomerId;

            _context.SaveChanges();

            return RedirectToAction("PipelineIndex");
        }


        [HttpPost]
        public async Task<IActionResult> UploadPipelineFile(IFormFile file, int pipelineTaskId)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen geçerli bir dosya seçin.";
                return RedirectToAction("PipelineDetails", new { id = pipelineTaskId });
            }

            var fileName = Path.GetFileName(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var relativePath = Path.Combine("uploads/pipeline", uniqueFileName);
            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

            // Klasörü oluştur (varsa atlar)
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

            using (var stream = new FileStream(absolutePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileAttachment = new PipelineTaskFileAttachment
            {
                PipelineTaskId = pipelineTaskId,
                FileName = fileName,
                FilePath = "/" + relativePath.Replace("\\", "/"),
                FileSize = file.Length,
                FileType = file.ContentType,
                UploadedDate = DateTime.Now
            };

            _context.PipelineTaskFileAttachments.Add(fileAttachment);
            await _context.SaveChangesAsync();

            return RedirectToAction("PipelineDetails", new { id = pipelineTaskId });
        }


        public async Task<IActionResult> DownloadPipelineFile(int id)
        {
            var file = await _context.PipelineTaskFileAttachments.FirstOrDefaultAsync(f => f.Id == id);
            if (file == null)
                return NotFound();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(path))
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, file.FileType, file.FileName);
        }

        public async Task<IActionResult> DeletePipelineFile(int id)
        {
            var file = await _context.PipelineTaskFileAttachments.FirstOrDefaultAsync(f => f.Id == id);
            if (file == null)
                return NotFound();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.TrimStart('/'));

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            _context.PipelineTaskFileAttachments.Remove(file);
            await _context.SaveChangesAsync();

            return RedirectToAction("PipelineDetails", new { id = file.PipelineTaskId });
        }



        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var task = _context.PipelineTasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return NotFound();

            // 1. İlişkili log kayıtlarını sil
            var logs = _context.PipelineTaskLogs.Where(l => l.PipelineTaskId == id).ToList();
            _context.PipelineTaskLogs.RemoveRange(logs);

            // 2. İlişkili müşteri kaydını sil (CustomerN)
            var customer = _context.CustomerNs.FirstOrDefault(c => c.Id == task.Id);
            if (customer != null)
                _context.CustomerNs.Remove(customer);

            // 3. Görevi sil
            _context.PipelineTasks.Remove(task);

            _context.SaveChanges();

            return Json(new { success = true });
        }



    }
}
