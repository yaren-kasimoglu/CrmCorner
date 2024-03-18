using CrmCorner.Models;
using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CrmCorner.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public TaskController(CrmCornerContext context, UserManager<AppUser> userManager, IWebHostEnvironment environment, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            Dictionary<int, string> statusNames = new Dictionary<int, string>();
            using (var context = new CrmCornerContext())
            {
                var statuses = context.Statuses.ToList();
                foreach (var status in statuses)
                {
                    statusNames.Add(status.StatusId, status.StatusName);
                }
            }
            ViewBag.StatusNames = statusNames;
            ViewBag.StatusList = statusNames;

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                var tasks = _context.TaskComps
           .Include(e => e.Customer)
           .Include(e => e.Status)
           .Include(e => e.AppUser)
           .Where(e => e.UserId == currentUser.Id || e.AssignedUserId == currentUser.Id) // AssignedUserId görevi gerçekleştiren kullanıcı için yer tutucudur
           .ToList();

                return View(tasks);
            }
            else
            {
                ViewBag.ErrorMessage = "Geçerli kullanıcı bilgisi bulunamadı.";
                return View();
            }
        }

        #region TaskEkleme
        [HttpGet]
        public async Task<IActionResult> TaskAdd()
        {

            var status = _context.Statuses.ToList();
            List<SelectListItem> statusItems = status
        .Select(d => new SelectListItem
        {
            Text = d.StatusName,
            Value = d.StatusId.ToString()
        }).ToList();

            var currentUser = await _userManager.GetUserAsync(User);

            var currentUserCompanyId = currentUser?.CompanyId;

            var users = _context.Users.Where(u => u.CompanyId == currentUserCompanyId!.Value).ToList();
            var userItems = users.Select(u => new SelectListItem
            {
                Text = u.UserName,
                Value = u.Id.ToString()
            }).ToList();


            var customer = _context.CustomerNs.Where(c => c.AppUserId == currentUser.Id).ToList();

            List<SelectListItem> customerItems = customer
            .Select(d => new SelectListItem
            {
                Text = d.Name + " " + d.Surname + " / " + d.CompanyName,
                Value = d.Id.ToString()
            }).ToList();

            ViewBag.Customer = customerItems;
            ViewBag.Status = statusItems;
            ViewBag.Users = userItems;
            return View();
        }

        [HttpPost]
        public IActionResult TaskAdd(TaskComp task)
        {
            var existingTask = _context.TaskComps.FirstOrDefault(t => t.CustomerId == task.CustomerId);

            if (existingTask != null)
            {
                TempData["ErrorMessage"] = "Bu müşteri için zaten bir görev eklenmiş.";
                return RedirectToAction("TaskAdd");
            }

            if (ModelState.IsValid)
            {
                task.CreatedDate = DateTime.Now; // Oluşturulma tarihini şimdi olarak ayarla

                if (task.StatusId.HasValue && task.StatusId.Value != 0) // StatusId kontrolü
                {
                    _context.TaskComps.Add(task); // Task'ı ekleyin
                    _context.SaveChanges(); // Değişiklikleri kaydedin
                    return RedirectToAction("Index"); // Başarılıysa, Index sayfasına yönlendir
                }
                else
                {
                    ModelState.AddModelError("", "Lütfen geçerli bir durum seçiniz.");
                }
            }

            ViewBag.Status = new SelectList(_context.Statuses.ToList(), "StatusId", "StatusName", task.StatusId);
            ViewBag.Users = new SelectList(_context.Users.ToList(), "Id", "UserName", task.UserId);
            ViewBag.Customer = new SelectList(_context.Users.ToList(), "Id", "Name", task.CustomerId);
            return View(task); // Formu ModelState hataları ile geri gönder
        }
        #endregion

        #region TaskGüncelleme/Editleme
        [HttpGet]
        public async Task<IActionResult> TaskEdit(int id)
        {

            TaskComp task = _context.TaskComps.Find(id);
            if (task == null)
            {
                return NotFound();
            }


            var status = _context.Statuses.ToList();
            List<SelectListItem> statusItems = status
        .Select(d => new SelectListItem
        {
            Text = d.StatusName,
            Value = d.StatusId.ToString()
        }).ToList();

            var currentUser = await _userManager.GetUserAsync(User);

            var currentUserCompanyId = currentUser?.CompanyId;

            var users = _context.Users.Where(u => u.CompanyId == currentUserCompanyId!.Value).ToList();
            var userItems = users.Select(u => new SelectListItem
            {
                Text = u.UserName,
                Value = u.Id.ToString()
            }).ToList();

            var customer = _context.CustomerNs.Where(c => c.AppUserId == currentUser.Id).ToList();

            List<SelectListItem> customerItems = customer
            .Select(d => new SelectListItem
            {
                Text = d.Name + " " + d.Surname + " / " + d.CompanyName,
                Value = d.Id.ToString()
            }).ToList();

            ViewBag.Users = userItems;

            ViewBag.Status = statusItems;

            ViewBag.Customer = customerItems;


            return View("TaskEdit", task);
        }

        [HttpPost]
        public async Task<IActionResult> TaskEdit(TaskComp editedTask)
        {
            if (ModelState.IsValid)
            {
                editedTask.ModifiedDate = DateTime.Now;

                var originalTask = _context.TaskComps.AsNoTracking().FirstOrDefault(t => t.TaskId == editedTask.TaskId);

                editedTask.CreatedDate = originalTask.CreatedDate; // createdDate değerini orijinal değeri ile güncelleme

                if (HttpContext.Request.Form.ContainsKey("AssignedUserId"))
                {
                    editedTask.AssignedUserId = HttpContext.Request.Form["AssignedUserId"];
                }

                // Değişiklikleri kontrol et ve log tablosuna kaydet
                await LogChanges(originalTask, editedTask);

                // task güncelle
                _context.TaskComps.Update(editedTask);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                return View("ErrorView", editedTask);
            }
        }

        private string GetCustomerNameById(int? customerId)
        {
            if (customerId == null) return null;
            var customer = _context.CustomerNs.FirstOrDefault(c => c.Id == customerId);
            return customer != null ? $"{customer.Name} {customer.Surname}" : "Bilinmiyor";
        }
        private string GetStatusNameById(int? statusId)
        {
            if (statusId == null) return null;
            var status = _context.Statuses.FirstOrDefault(c => c.StatusId == statusId);
            return status != null ? $"{status.StatusName}" : "Bilinmiyor";
        }
        private string GetUserNameById(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return "Bilinmiyor";
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return user != null ? user.NameSurname : "Bilinmiyor";
        }

        public async Task LogChanges(TaskComp originalTask, TaskComp editedTask)
        {
            foreach (var property in typeof(TaskComp).GetProperties())
            {
                var originalValue = property.GetValue(originalTask);
                var editedValue = property.GetValue(editedTask);

                if ((originalValue != null && editedValue != null) && !originalValue.Equals(editedValue))
                {
                    var oldValueString = originalValue.ToString();
                    var newValueString = editedValue.ToString();
                    string fieldName = property.Name;


                    if (property.Name == "CustomerId")
                    {
                        oldValueString = GetCustomerNameById((int?)originalValue);
                        newValueString = GetCustomerNameById((int?)editedValue);
                        fieldName = "Müşteri Bilgisi";
                    }

                    if (property.Name == "UserId")
                    {
                        var oldUser = await _userManager.FindByIdAsync(originalValue.ToString());
                        var newUser = await _userManager.FindByIdAsync(editedValue.ToString());
                        oldValueString = oldUser != null ? oldUser.NameSurname : "Unknown";
                        newValueString = newUser != null ? newUser.NameSurname : "Unknown";
                        fieldName = "Görüşmeyi Alan";
                    }

                    if (property.Name == "AssignedUserId")
                    {
                        var oldUser = await _userManager.FindByIdAsync(originalValue.ToString());
                        var newUser = await _userManager.FindByIdAsync(editedValue.ToString());
                        oldValueString = oldUser != null ? oldUser.NameSurname : "Unknown";
                        newValueString = newUser != null ? newUser.NameSurname : "Unknown";
                        fieldName = "Görüşmeyi Gerçekleştiren";
                    }

                    if (property.Name == "StatusId")
                    {
                        oldValueString = GetStatusNameById((int?)originalValue);
                        newValueString = GetStatusNameById((int?)editedValue);
                        fieldName = "Güncel Durum Bilgisi";
                    }

                    if (property.Name == "ValueOrOffer")
                    {
                        // Değerleri decimal olarak dönüştürme
                        decimal oldValueDecimal = originalValue != null ? Convert.ToDecimal(originalValue) : 0;
                        decimal newValueDecimal = editedValue != null ? Convert.ToDecimal(editedValue) : 0;

                        // Formatlı string olarak değerleri ayarlama
                        oldValueString = oldValueDecimal.ToString("N2");
                        newValueString = newValueDecimal.ToString("N2");
                        fieldName = "Değer Teklifi";
                    }

                    if (property.Name == "Description")
                    {
                        fieldName = "Açıklama";
                    }

                    if (property.Name == "IsPositiveOutcome")
                    {
                        fieldName = "Olumlu/Olumsuz";
                        oldValueString = (bool)originalValue ? "Olumlu" : "Olumsuz";
                        newValueString = (bool)editedValue ? "Olumlu" : "Olumsuz";
                    }

                    if (property.Name == "IsFinalDecisionMaker")
                    {
                        fieldName = "Son Karar Mercii Bilgisi";
                        oldValueString = (bool)originalValue ? "Evet" : "Hayır";
                        newValueString = (bool)editedValue ? "Evet" : "Hayır";
                    }

                    if (property.Name == "SalesDone")
                    {
                        fieldName = "Ön Görülen Satış Kapatma Tarihi";
                        oldValueString = ((DateTime?)originalValue)?.ToString("dd/MM/yyyy") ?? string.Empty;
                        newValueString = ((DateTime?)editedValue)?.ToString("dd/MM/yyyy") ?? string.Empty;
                    }


                    if (property.Name == "Title")
                    {
                        fieldName = "Görev Başlık";
                    }


                    TaskCompLog log = new TaskCompLog
                    {
                        TaskId = editedTask.TaskId,
                        UpdatedField = fieldName,
                        OldValue = oldValueString,
                        NewValue = newValueString,
                        UpdatedById = _userManager.GetUserId(User),
                        UpdatedAt = DateTime.Now
                    };

                    _context.TaskCompLogs.Add(log);
                }
            }
            await _context.SaveChangesAsync();
        }

        #endregion

        public async Task<IActionResult> TaskDetail(int id)
        {
            var status = _context.Statuses.ToList();
            List<SelectListItem> statusItems = status
        .Select(d => new SelectListItem
        {
            Text = d.StatusName,
            Value = d.StatusId.ToString()
        }).ToList();

            var currentUser = await _userManager.GetUserAsync(User);

            var currentUserCompanyId = currentUser?.CompanyId;

            var users = _context.Users.Where(u => u.CompanyId == currentUserCompanyId!.Value).ToList();
            var userItems = users.Select(u => new SelectListItem
            {
                Text = u.UserName,
                Value = u.Id.ToString()
            }).ToList();


            var customer = _context.CustomerNs.Where(c => c.AppUserId == currentUser.Id).ToList();

            List<SelectListItem> customerItems = customer
            .Select(d => new SelectListItem
            {
                Text = d.Name + " " + d.Surname + " / " + d.CompanyName,
                Value = d.Id.ToString()
            }).ToList();


            ViewBag.Customer = customerItems;
            ViewBag.Status = statusItems;
            ViewBag.Users = userItems;

            // TaskComp task = _context.TaskComps
            //.Include(t => t.TaskCompLogs)
            ///*     .Include(t => t.Customer)*/ // TaskComp nesnesinin Customer ilişkisini yükle
            //.FirstOrDefault(t => t.TaskId == id);

            TaskComp task = _context.TaskComps
               .Include(e => e.FileAttachments)
                .Include(e => e.Customer).
                Include(e => e.Status).
                Include(e => e.AppUser).
                FirstOrDefault(t => t.TaskId == id);

            if (task == null)
            {
                return NotFound();
            }

            var taskCompLogs = await _context.TaskCompLogs
                                    .Where(log => log.TaskId == id)
                                    .OrderByDescending(log => log.UpdatedAt)
                                    .ToListAsync();

            ViewBag.TaskCompLogs = taskCompLogs;

            if (task.FileAttachments == null)
            {
                // FileAttachments koleksiyonu null ise, boş bir liste ile başlat
                task.FileAttachments = new List<FileAttachment>();
            }


            return View("TaskDetail", task);
        }


        #region DOSYA İŞLEMLERİ

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, int taskId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("SignIn", "Home");
            }
            var userId = _userManager.GetUserId(User);

            if (file != null && file.Length > 0)
            {
                var originalFileName = Path.GetFileName(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}_{originalFileName}";

                // Yapılandırma ayarlarından yükleme yolu alınır
                var uploadFolderPath = _configuration.GetValue<string>("FileUploadOptions:UploadFolderPath");
                var relativePath = Path.Combine(uploadFolderPath, uniqueFileName);
                var absolutePath = Path.Combine(_hostingEnvironment.WebRootPath, relativePath);

                // Klasör yoksa oluştur
                var directoryPath = Path.GetDirectoryName(absolutePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var stream = new FileStream(absolutePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileAttachment = new FileAttachment
                {
                    FileName = originalFileName, // Kullanıcıya gösterilecek orijinal dosya adını saklayın
                    FilePath = "/" + relativePath, // Göreli dosya yolu
                    FileSize = file.Length,
                    FileType = file.ContentType,
                    UploadedDate = DateTime.Now,
                    TaskId = taskId
                };

                _context.FileAttachments.Add(fileAttachment);
                await _context.SaveChangesAsync();

                await LogFileAttachmentChange(null, fileAttachment, userId);
                return RedirectToAction("TaskDetail", new { id = taskId });
            }

            return View("Error");
        }

        public async Task<IActionResult> DownloadFile(int fileAttachmentId)
        {
            // Veritabanından dosya bilgisini al
            var fileAttachment = await _context.FileAttachments
                .FirstOrDefaultAsync(f => f.FileAttachmentId == fileAttachmentId);

            if (fileAttachment == null)
            {
                return NotFound(); // Dosya bulunamazsa 404 döndür
            }

            // FilePath ve WebRootPath kullanarak dosyanın tam yolunu oluştur
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, fileAttachment.FilePath.TrimStart('/'));

            // Dosyanın var olup olmadığını kontrol et
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(); // Dosya sistemde bulunamazsa 404 döndür
            }

            // Dosya içeriğini oku ve bir MemoryStream'a kopyala
            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            // İndirme işlemi için dosya içeriğini, MIME türünü ve orijinal dosya adını kullan
            var contentType = fileAttachment.FileType;
            var originalFileName = fileAttachment.FileName;

            // Kullanıcıya dosyayı indirme olarak sun
            return File(memory, contentType, originalFileName);
        }

        /// <summary>
        ///FİLE  LAR EKLENDİĞİNDE LOGLAMA VE TİMELİNE A BASMAK İÇİN YAZILAN METHOD
        /// </summary>
        public async Task LogFileAttachmentChange(FileAttachment originalFile, FileAttachment editedFile, string userId)
        {
            // Eğer orijinal dosya null ise, bu yeni bir yükleme demektir
            bool isNewUpload = originalFile == null;

            // Değişiklik log'unu oluştur
            TaskCompLog log = new TaskCompLog
            {
                TaskId = editedFile.TaskId, // ilişkili görev ID
                UpdatedField = isNewUpload ? "Dosya Yüklendi" : "Dosya Güncellendi",
                OldValue = isNewUpload ? null : originalFile.FileName, // Eğer yeni yükleme ise eski değer yok
                NewValue = editedFile.FileName,
                UpdatedById = userId,
                UpdatedAt = DateTime.Now
            };

            // Log'u veritabanına ekle
            _context.TaskCompLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> DeleteFile(int fileAttachmentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Kullanıcı doğrulanamazsa, oturum açma sayfasına yönlendir veya hata dön
                return RedirectToAction("Login");
            }
            var userId = _userManager.GetUserId(User);


            // Veritabanından dosya bilgisini al
            var fileAttachment = await _context.FileAttachments
                .FirstOrDefaultAsync(f => f.FileAttachmentId == fileAttachmentId);

            if (fileAttachment == null)
            {
                return NotFound(); // Dosya bulunamazsa 404 döndür
            }

            // Dosyanın tam yolunu oluştur
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, fileAttachment.FilePath.TrimStart('/'));

            // Dosyanın var olup olmadığını kontrol et
            if (System.IO.File.Exists(filePath))
            {
                // Dosyayı sil
                System.IO.File.Delete(filePath);
            }

            // Dosya bilgisini veritabanından sil
            _context.FileAttachments.Remove(fileAttachment);
            await _context.SaveChangesAsync();

            await LogFileAttachmentChange(fileAttachment, userId);
            return RedirectToAction("TaskDetail", new { id = fileAttachment.TaskId });
        }

        public async Task LogFileAttachmentChange(FileAttachment originalFile, string userId)
        {
            // Dosya silindiğinde originalFile parametresi dışında tüm bilgiler null olacaktır
            bool isDelete = originalFile != null;

            // Değişiklik log'unu oluştur
            TaskCompLog log = new TaskCompLog
            {
                TaskId = isDelete ? originalFile.TaskId : 0, // Silinen dosya için ilişkili görev ID
                UpdatedField = isDelete ? "Dosya Silindi" : "Unknown Operation",
                OldValue = isDelete ? originalFile.FileName : null, // Silinen dosyanın eski adı
                NewValue = null, // Dosya silindiği için yeni değer yok
                UpdatedById = userId,
                UpdatedAt = DateTime.Now
            };

            // Log'u veritabanına ekle
            _context.TaskCompLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        #endregion

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int taskId, int statusId)
        {
            var task = await _context.TaskComps.FirstOrDefaultAsync(t => t.TaskId == taskId);
            if (task != null)
            {
                task.StatusId = statusId;
                _context.Update(task);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        public IActionResult TaskDelete(int id)
        {
            TaskComp task = _context.TaskComps.Find(id);

            if (task == null)
            {
                return NotFound();
            }

            _context.TaskComps.Remove(task);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
    public class FileUploadOptions
    {
        public string UploadFolderPath { get; set; }
    }
}