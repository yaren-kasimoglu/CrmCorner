using CrmCorner.Extensions;
using CrmCorner.Models;
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
        public IActionResult UpdateContactMethods(int id, bool linkedinContacted, bool coldCallContacted)
        {
            var task = _context.PipelineTasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return NotFound();

            task.ContactedViaLinkedIn = linkedinContacted;
            task.ContactedViaColdCall = coldCallContacted;
            _context.SaveChanges();

            return Ok();
        }


        public IActionResult PipelineDetails(int id)
        {
            var task = _context.PipelineTasks
                .Include(t => t.Notes) // Eğer notlar varsa
                .FirstOrDefault(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

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





    }
}
