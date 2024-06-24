using CrmCorner.Models;
using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Crmf;
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
            try
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
                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

                if (currentUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(currentUser);

                    bool isAdminOrManager = roles.Contains("Admin") || roles.Contains("Manager");
                    bool isTeamLeader = roles.Contains("TeamLeader");


                    ViewData["UserRole"] = roles.Contains("Admin") ? "Admin" : "TeamMember";
                    ViewData["UserEmail"] = currentUser.Email;

                    if (isAdminOrManager)
                    {
                        var companyUsers = _context.Users.Where(u => u.EmailDomain == currentUser.EmailDomain).ToList();
                        var companyUserIds = companyUsers.Select(u => u.Id).ToList();

                        var tasks = _context.TaskComps
                            .Include(e => e.Customer)
                            .Include(e => e.Status)
                            .Include(e => e.AppUser)
                            .Where(e => companyUserIds.Contains(e.UserId) || companyUserIds.Contains(e.AssignedUserId))
                            .ToList();

                        ViewBag.Users = companyUsers;

                        return View(tasks);
                    }
                    else if (isTeamLeader)
                    {
                        var teamMembers = await _userManager.GetUsersInRoleAsync("TeamMember");
                        var teamMemberIds = teamMembers.Select(u => u.Id).ToList();
                        teamMemberIds.Add(currentUser.Id); // Add the current TeamLeader's ID to the list

                        var tasks = _context.TaskComps
                            .Include(e => e.Customer)
                            .Include(e => e.Status)
                            .Include(e => e.AppUser)
                            .Where(e => teamMemberIds.Contains(e.UserId) || teamMemberIds.Contains(e.AssignedUserId))
                            .ToList();

                        ViewBag.Users = teamMembers;

                        return View(tasks);
                    }
                    else
                    {
                        var tasks = _context.TaskComps
                            .Include(e => e.Customer)
                            .Include(e => e.Status)
                            .Include(e => e.AppUser)
                            .Where(e => e.UserId == currentUser.Id || e.AssignedUserId == currentUser.Id)
                            .ToList();
                        return View(tasks);
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Geçerli kullanıcı bilgisi bulunamadı.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }



        //YORUMA ALINDI 06.05.2024 GERİ AÇILACAK ROL İŞLEVLERİ YAPILINCA
        //public async Task<IActionResult> Index()
        //{
        //    Dictionary<int, string> statusNames = new Dictionary<int, string>();
        //    using (var context = new CrmCornerContext())
        //    {
        //        var statuses = context.Statuses.ToList();
        //        foreach (var status in statuses)
        //        {
        //            statusNames.Add(status.StatusId, status.StatusName);
        //        }
        //    }
        //    ViewBag.StatusNames = statusNames;
        //    ViewBag.StatusList = statusNames;

        //    var currentUser = await _userManager.GetUserAsync(User);

        //    if (currentUser != null)
        //    {
        //        var tasks = _context.TaskComps
        //   .Include(e => e.Customer)
        //   .Include(e => e.Status)
        //   .Include(e => e.AppUser)
        //   .Where(e => e.UserId == currentUser.Id || e.AssignedUserId == currentUser.Id) // AssignedUserId görevi gerçekleştiren kullanıcı için yer tutucudur
        //   .ToList();

        //        return View(tasks);
        //    }
        //    else
        //    {
        //        ViewBag.ErrorMessage = "Geçerli kullanıcı bilgisi bulunamadı.";
        //        return View();
        //    }
        //}

        #region TaskEkleme
        [HttpGet]
        public async Task<IActionResult> TaskAdd()
        {
            try
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
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        [HttpPost]
        public IActionResult TaskAdd(TaskComp task)
        {
            try
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

                        _context.SaveChanges();

                        // Outcomes Olumlu ise PostSaleInfo oluştur
                        if (task.Outcomes == OutcomeType.Olumlu)
                        {
                            var postSaleInfo = new PostSaleInfo
                            {
                                TaskCompId = task.TaskId,
                                // Diğer alanlar varsayılan değerlerle veya gerekli bilgilerle doldurulabilir
                                IsFirstPaymentMade = false, // Örnek varsayılan değerler
                                IsThereAProblem = false,
                                ProblemDescription = "",
                                IsContinuationConsidered = false,
                                IsTrustpilotReviewed = false,
                                TrustPilotComment = "",
                                CanUseLogo = false
                            };
                            _context.PostSaleInfos.Add(postSaleInfo);
                            _context.SaveChanges();
                        }

                        return RedirectToAction("Index"); // Başarılıysa, Index sayfasına yönlendir
                    }
                    else
                    {
                        ModelState.AddModelError("", "Lütfen geçerli bir durum seçiniz.");
                    }
                }
                else
                {
                    foreach (var state in ModelState)
                    {
                        if (state.Value.Errors.Count > 0)
                        {
                            Console.WriteLine(state.Key + ": " + state.Value.Errors[0].ErrorMessage);
                        }
                    }
                }

                ViewBag.Status = new SelectList(_context.Statuses.ToList(), "StatusId", "StatusName", task.StatusId);
                ViewBag.Users = new SelectList(_context.Users.ToList(), "Id", "UserName", task.UserId);
                ViewBag.Customer = new SelectList(_context.CustomerNs.ToList(), "Id", "Name", task.CustomerId);
                return View(task); // Formu ModelState hataları ile geri gönder
            }
            catch (Exception ex)
            {
                // Hata loglama işlemleri burada yapılabilir
                return RedirectToAction("NotFound", "Error");
            }
        }

        #endregion

        #region TaskGüncelleme/Editleme

        [HttpGet]
        public async Task<IActionResult> TaskEdit(int id)
        {
            try
            {
                TaskComp task = _context.TaskComps
                    .Include(t => t.FileAttachments)
                    .FirstOrDefault(t => t.TaskId == id);
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
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> TaskEdit(TaskComp editedTask)
        {
            try
            {
                var originalTask = _context.TaskComps
                    .Include(t => t.FileAttachments) // Include FileAttachments
                    .FirstOrDefault(t => t.TaskId == editedTask.TaskId);

                if (originalTask == null)
                {
                    return NotFound();
                }

                editedTask.ModifiedDate = DateTime.Now;
                editedTask.CreatedDate = originalTask.CreatedDate; // createdDate değerini orijinal değeri ile güncelleme

                // Mevcut dosya varsa ve yeni dosya yüklenmediyse, dosya zorunluluğunu kaldır
                if (originalTask.FileAttachments.Any())
                {
                    ModelState.Remove("file");
                }

                // Dosya zorunluluğunu sadece belirli durumlarda kontrol et
                var status = _context.Statuses.FirstOrDefault(s => s.StatusId == editedTask.StatusId);
                if ((status?.StatusName == "Sözleşme Aşamasında" || status?.StatusName == "Teklif Gönderildi" || status?.StatusName == "Satış Tamamlandı") && !originalTask.FileAttachments.Any())
                {
                    ModelState.AddModelError("file", "Dosya yüklemesi zorunludur.");
                }

                if (ModelState.IsValid)
                {
                    if (HttpContext.Request.Form.ContainsKey("AssignedUserId"))
                    {
                        editedTask.AssignedUserId = HttpContext.Request.Form["AssignedUserId"];
                    }

                    // OutcomeStatus ve Outcomes değerlerini kontrol et
                    if (HttpContext.Request.Form.ContainsKey("OutcomeStatus"))
                    {
                        editedTask.OutcomeStatus = Enum.Parse<OutcomeTypeSales>(HttpContext.Request.Form["OutcomeStatus"]);
                    }

                    if (HttpContext.Request.Form.ContainsKey("Outcomes"))
                    {
                        editedTask.Outcomes = Enum.Parse<OutcomeType>(HttpContext.Request.Form["Outcomes"]);
                    }

                    // Değişiklikleri kontrol et ve log tablosuna kaydet
                    await LogChanges(originalTask, editedTask);

                    // orijinal task veritabanından çekildiği için EF Core izliyor, tekrar eklemeye çalışmak yerine sadece güncelleme yapılmalı
                    _context.Entry(originalTask).CurrentValues.SetValues(editedTask);
                    await _context.SaveChangesAsync();

                    if (editedTask.Outcomes == OutcomeType.Olumlu && originalTask.Outcomes != OutcomeType.Olumlu)
                    {
                        var postSaleInfo = new PostSaleInfo
                        {
                            TaskCompId = editedTask.TaskId,
                            IsFirstPaymentMade = false, // Varsayılan olarak ödeme yapılmadı kabul edilir
                            IsThereAProblem = false,   // Başlangıçta problem yoktur
                            ProblemDescription = "",   // Problem açıklaması boş
                            IsContinuationConsidered = false, // Devam etme durumu başlangıçta hayır
                            IsTrustpilotReviewed = false, // Trustpilot değerlendirmesi henüz yapılmamış
                            CanUseLogo = false // Logo kullanımı izni başlangıçta hayır
                        };
                        _context.PostSaleInfos.Add(postSaleInfo);
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction("Index");
                }
                else
                {
                    // ModelState hatalarını topla ve görünümde göster
                    var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    ViewBag.ErrorMessages = errorMessages;

                    var statusList = _context.Statuses.ToList();
                    List<SelectListItem> statusItems = statusList
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

                    return View(editedTask);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }






        private string GetCustomerNameById(int? customerId)
        {
            try
            {
                if (customerId == null) return null;
                var customer = _context.CustomerNs.FirstOrDefault(c => c.Id == customerId);
                return customer != null ? $"{customer.Name} {customer.Surname}" : "Bilinmiyor";
            }
            catch (Exception ex)
            {
                return "Bilinmiyor";
            }
        }

        private string GetStatusNameById(int? statusId)
        {
            try
            {
                if (statusId == null) return null;
                var status = _context.Statuses.FirstOrDefault(c => c.StatusId == statusId);
                return status != null ? $"{status.StatusName}" : "Bilinmiyor";
            }
            catch (Exception ex)
            {
                return "Bilinmiyor";
            }
        }

        private string GetUserNameById(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId)) return "Bilinmiyor";
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                return user != null ? user.NameSurname : "Bilinmiyor";
            }
            catch (Exception ex)
            {
                return "Bilinmiyor";
            }
        }

        public async Task LogChanges(TaskComp originalTask, TaskComp editedTask)
        {
            try
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

                        if (property.Name == "Outcomes")
                        {
                            fieldName = "Süreç Durumu";
                            oldValueString = Enum.GetName(typeof(OutcomeType), originalValue);
                            newValueString = Enum.GetName(typeof(OutcomeType), editedValue);
                        }

                        if (property.Name == "OutcomeStatus")
                        {
                            fieldName = "Sonuç Durumu";
                            oldValueString = Enum.GetName(typeof(OutcomeTypeSales), originalValue);
                            newValueString = Enum.GetName(typeof(OutcomeTypeSales), editedValue);
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

                        if (property.Name == "NegativeReason")
                        {
                            fieldName = "Olumsuz Olma Nedeni";
                        }
                        if (property.Name == "FileAttachments")
                        {
                            var originalFiles = originalValue as ICollection<FileAttachment>;
                            var editedFiles = editedValue as ICollection<FileAttachment>;

                            if (originalFiles != null && editedFiles != null)
                            {
                                var addedFiles = editedFiles.Except(originalFiles).ToList();
                                var removedFiles = originalFiles.Except(editedFiles).ToList();

                                foreach (var file in addedFiles)
                                {
                                    TaskCompLog addedFileLog = new TaskCompLog
                                    {
                                        TaskId = editedTask.TaskId,
                                        UpdatedField = "Dosya Eklendi",
                                        OldValue = string.Empty,
                                        NewValue = $"{file.FileName}",
                                        UpdatedById = _userManager.GetUserId(User),
                                        UpdatedAt = DateTime.Now
                                    };
                                    _context.TaskCompLogs.Add(addedFileLog);
                                }

                                foreach (var file in removedFiles)
                                {
                                    TaskCompLog removedFileLog = new TaskCompLog
                                    {
                                        TaskId = editedTask.TaskId,
                                        UpdatedField = "Dosya Silindi",
                                        OldValue = $"{file.FileName}",
                                        NewValue = string.Empty,
                                        UpdatedById = _userManager.GetUserId(User),
                                        UpdatedAt = DateTime.Now
                                    };
                                    _context.TaskCompLogs.Add(removedFileLog);
                                }
                            }

                            continue; // Diğer log işlemlerine geçmeden devam et
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
            catch (Exception ex)
            {
                throw new Exception("Değişiklikler loglanırken bir hata meydana geldi.", ex);
            }
        }



        #endregion

        #region Taskdetail
        public async Task<IActionResult> TaskDetail(int id)
        {
            try
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

                TaskComp task = _context.TaskComps
                    .Include(e => e.FileAttachments)
                    .Include(e => e.Customer)
                    .Include(e => e.Status)
                    .Include(e => e.AppUser)
                    .FirstOrDefault(t => t.TaskId == id);

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
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }
        #endregion


        #region DOSYA İŞLEMLERİ

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, int taskId)
        {
            try
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
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        public async Task<IActionResult> DownloadFile(int fileAttachmentId)
        {
            try
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
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        /// <summary>
        ///FİLE  LAR EKLENDİĞİNDE LOGLAMA VE TİMELİNE A BASMAK İÇİN YAZILAN METHOD
        /// </summary>
        public async Task LogFileAttachmentChange(FileAttachment originalFile, FileAttachment editedFile, string userId)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception("Dosya yüklenirken veya güncellenirken bir hata meydana geldi.", ex);
            }
        }

        public async Task<IActionResult> DeleteFile(int fileAttachmentId)
        {
            try
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
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        public async Task LogFileAttachmentChange(FileAttachment originalFile, string userId)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception("Dosya silinirken bir hata meydana geldi.", ex);
            }
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
            try
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
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }


        //Statuslar arası geçiş kurallarının kontrol edileceği method
        [HttpPost]
        public async Task<IActionResult> CheckRequiredFieldsBeforeStatusChange(int taskId, int newStatusId)
        {
            try
            {
                var task = await _context.TaskComps
                    .Include(t => t.FileAttachments)
                    .FirstOrDefaultAsync(t => t.TaskId == taskId);

                if (task == null) return Json(new { isValid = false, message = "Task not found." });

                if (newStatusId == 3)
                {
                    // Görüşmeyi Gerçekleştiren kontrolü
                    if (string.IsNullOrEmpty(task.AssignedUserId))
                    {
                        return Json(new { isValid = false, message = "Görev durumunu bu aşamaya getirebilmek için 'Görüşmeyi Gerçekleştiren' alanını doldurmanız gerekmektedir." });
                    }

                    if (task.Outcomes == null)
                    {
                        return Json(new { isValid = false, message = "Görev durumunu bu aşamaya getirebilmek için 'OutcomeType' alanını doldurmanız gerekmektedir." });
                    }

                    // Olumlu Sonuç ve Satış Kapatma Tarihi kontrolü
                    if (task.Outcomes == OutcomeType.Olumlu && !task.SalesDone.HasValue)
                    {
                        return Json(new { isValid = false, message = "Satış süreciniz olumlu ilerliyorsa lütfen 'Planlanan Satış Kapatma Tarihi' alanını doldurunuz." });
                    }
                }

                if (newStatusId == 4 && string.IsNullOrEmpty(task.ValueOrOffer.ToString()))
                {
                    return Json(new { isValid = false, message = "Görev durumunu bu aşamaya getirebilmek için Değer Teklifi alanını doldurmanız gerekmektedir." });
                }

                if (newStatusId == 5)
                {
                    if (task.FileAttachments == null || task.FileAttachments.Count < 1)
                    {
                        return Json(new { isValid = false, message = "Görev durumunu bu aşamaya getirebilmek için 'Sözleşme / Anlaşma' dosyalarını yüklemeniz gerekmektedir. Sözleşme dosyalarınızın sisteme doğru bir şekilde yüklendiğinden emin olun." });
                    }
                }
                if (newStatusId == 6)
                {
                    // OutcomeTypeSales kontrolü
                    if (task.OutcomeStatus == null)
                    {
                        return Json(new { isValid = false, message = "Görev durumunu bu aşamaya getirebilmek için 'Satış için Kazanıldı/Kaybedildi' alanını doldurmanız gerekmektedir." });
                    }

                    // OutcomeType ve OutcomeTypeSales uyumluluk kontrolü
                    if (task.Outcomes == OutcomeType.Olumlu || task.Outcomes == OutcomeType.Surecte)
                    {
                        if (task.OutcomeStatus == OutcomeTypeSales.Lost)
                        {
                            return Json(new { isValid = false, message = "Görev durumu olumlu veya süreçte iken 'Kaybedildi' olarak işaretlenemez. Lütfen 'Olumsuz' olarak işaretleyiniz." });
                        }

                        if (task.OutcomeStatus == OutcomeTypeSales.Won && !task.FinalSalesDone.HasValue)
                        {
                            return Json(new { isValid = false, message = "'Kazanıldı' olarak işaretlendiğinde 'Gerçekleşen Satış Kapatma Tarihi' alanını doldurmanız gerekmektedir." });
                        }
                    }

                    if (task.Outcomes == OutcomeType.Olumsuz || task.Outcomes == OutcomeType.Surecte)
                    {
                        if (task.OutcomeStatus == OutcomeTypeSales.Won)
                        {
                            return Json(new { isValid = false, message = "Görev durumu olumsuz veya süreçte iken 'Kazanıldı' olarak işaretlenemez. Lütfen 'Olumlu' olarak işaretleyiniz." });
                        }
                    }
                }

                return Json(new { isValid = true });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, message = "An error occurred while checking required fields.", error = ex.Message });
            }
        }


        public async Task<IActionResult> FilterSalesDone(DateTime startDate, DateTime endDate)
        {
            try
            {
                var filteredTasks = await _context.TaskComps
                    .Where(t => t.SalesDone >= startDate && t.SalesDone <= endDate)
                    .ToListAsync();

                // Filtrelenmiş görevleri döndür
                return View(filteredTasks);
            }
            catch (Exception ex)
            {
                // Hata loglama işlemleri burada yapılabilir
                // Hata durumunda Error view'ı döndürülür ve hata mesajı geçilir
                ViewBag.ErrorMessage = "An error occurred while filtering the tasks: " + ex.Message;
                return View("Error");
            }
        }


        public IActionResult TaskDelete(int id)
        {
            try
            {
                TaskComp task = _context.TaskComps
                    .Include(t => t.TaskCompLogs) // İlişkili log kayıtlarını dahil et
                    .FirstOrDefault(t => t.TaskId == id);

                if (task == null)
                {
                    return NotFound();
                }

                // İlişkili TaskCompLogs kayıtlarını sil
                foreach (var log in task.TaskCompLogs.ToList())
                {
                    _context.TaskCompLogs.Remove(log);
                }

                _context.TaskComps.Remove(task);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Hata loglama işlemleri burada yapılabilir
                // Hata durumunda Error view'ı döndürülür ve hata mesajı geçilir
                ViewBag.ErrorMessage = "An error occurred while deleting the task: " + ex.Message;
                return View("Error");
            }
        }

    }


    public class FileUploadOptions
    {
        public string UploadFolderPath { get; set; }
    }
}