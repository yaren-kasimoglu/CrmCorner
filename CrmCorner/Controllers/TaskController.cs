using CrmCorner.Extensions;
using CrmCorner.Models;
using CrmCorner.Models.Enums;
using CrmCorner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
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
        private readonly EmailService _emailService;


        public TaskController(CrmCornerContext context, UserManager<AppUser> userManager, IWebHostEnvironment environment, IWebHostEnvironment hostingEnvironment, IConfiguration configuration, EmailService emailService = null)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _emailService = emailService;
        }

        [Authorize(Roles = "Admin")]
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
                var statusItems = new List<SelectListItem>
        {
            new SelectListItem { Text = "İlk Temas", Value = "1", Selected = true },
            new SelectListItem { Text = "Görüşme Ayarlandı", Value = "2" },
            new SelectListItem { Text = "Görüşme Gerçekleşti", Value = "3" },
            new SelectListItem { Text = "Teklif Gönderildi", Value = "4" },
            new SelectListItem { Text = "Sözleşme Aşamasında", Value = "5" },
            //new SelectListItem { Text = "Satış Tamamlandı", Value = "6" }
        };

                var currentUser = await _userManager.GetUserAsync(User);
                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");
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

                    // Nereden duydunuz alanını küçük harfe çevir
                    if (!string.IsNullOrEmpty(task.HeardFrom))
                    {
                        task.HeardFrom = task.HeardFrom.ToLower();
                    }


                    if (task.StatusId.HasValue && task.StatusId.Value != 0) // StatusId kontrolü
                    {
                        _context.TaskComps.Add(task); // Task'ı ekleyin

                        _context.SaveChanges();

                        // Outcomes Olumlu ise PostSaleInfo oluştur
                        if (task.Outcomes == OutcomeType.Olumlu  && task.OutcomeStatus==OutcomeTypeSales.Won)
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

                ViewBag.Status = new SelectList(new List<SelectListItem> { new SelectListItem { Text = "İlk Temas", Value = "1", Selected = true } }, "Value", "Text");
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
                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");
                var currentUserCompanyId = currentUser?.CompanyId;

                if (currentUserCompanyId == null)
                {
                    return RedirectToAction("AccessDenied", "Error");
                }

                // Aynı firmada oldukları kişilerin tüm müşterilerini görmeleri için sorgu
                var companyUserIds = _context.Users
                    .Where(u => u.CompanyId == currentUserCompanyId)
                    .Select(u => u.Id)
                    .ToList();

                var customerItems = _context.CustomerNs
                    .Where(c => companyUserIds.Contains(c.AppUserId))
                    .ToList()
                    .Select(d => new SelectListItem
                    {
                        Text = d.Name + " " + d.Surname + " / " + d.CompanyName,
                        Value = d.Id.ToString()
                    }).ToList();

                var users = _context.Users.Where(u => u.CompanyId == currentUserCompanyId).ToList();
                var userItems = users.Select(u => new SelectListItem
                {
                    Text = u.UserName,
                    Value = u.Id.ToString()
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
        public async Task<IActionResult> TaskEdit(TaskComp editedTask, IFormFile file)
        {
            var currentUsers = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUsers.Picture ?? "defaultpp.png");
            string selectedCurrency = editedTask.SelectedCurrency; // Bu satırı kontrol edeceğiz.

            try
            {
                var originalTask = _context.TaskComps
                    .Include(t => t.FileAttachments)
                    .Include(t => t.Customer)
                    .FirstOrDefault(t => t.TaskId == editedTask.TaskId);

                if (originalTask == null)
                {
                    return NotFound();
                }

                editedTask.ModifiedDate = DateTime.Now;
                editedTask.CreatedDate = originalTask.CreatedDate;

                // Dosya yükleme işlemi
                if (file != null && file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    var fileAttachment = new FileAttachment
                    {
                        TaskId = editedTask.TaskId,
                        FileName = fileName,
                        FilePath = filePath,
                        UploadedDate = DateTime.Now
                    };

                    _context.FileAttachments.Add(fileAttachment);
                }

                if (originalTask.FileAttachments.Any() || (file == null || file?.Length == 0))
                {
                    ModelState.Remove("file");
                }

                var status = _context.Statuses.FirstOrDefault(s => s.StatusId == editedTask.StatusId);
                if ((status?.StatusName == "Sözleşme Aşamasında")/* || status?.StatusName == "Satış Tamamlandı")*/ && !originalTask.FileAttachments.Any())
                {
                    ModelState.AddModelError("file", "Dosya yüklemesi zorunludur.");
                }

                // Nereden duydunuz alanını küçük harfe çevirme işlemi
                if (!string.IsNullOrEmpty(editedTask.HeardFrom))
                {
                    editedTask.HeardFrom = editedTask.HeardFrom.ToLower();
                }


                if (ModelState.IsValid)
                {
                    if (HttpContext.Request.Form.ContainsKey("AssignedUserId"))
                    {
                        editedTask.AssignedUserId = HttpContext.Request.Form["AssignedUserId"];
                    }

                    if (HttpContext.Request.Form.ContainsKey("OutcomeStatus"))
                    {
                        editedTask.OutcomeStatus = Enum.Parse<OutcomeTypeSales>(HttpContext.Request.Form["OutcomeStatus"]);
                    }

                    if (HttpContext.Request.Form.ContainsKey("Outcomes"))
                    {
                        editedTask.Outcomes = Enum.Parse<OutcomeType>(HttpContext.Request.Form["Outcomes"]);

                        // Eğer OutcomeType 'Olumsuz' ise OutcomeStatus 'Lost' olarak ayarlanır
                        if (editedTask.Outcomes == OutcomeType.Olumsuz)
                        {
                            editedTask.OutcomeStatus = OutcomeTypeSales.Lost;
                        }
                    }

                    var existingPostSaleInfo = _context.PostSaleInfos
                .FirstOrDefault(psi => psi.TaskCompId == editedTask.TaskId);

                    if (editedTask.OutcomeStatus == OutcomeTypeSales.Won && originalTask.OutcomeStatus != OutcomeTypeSales.Won && existingPostSaleInfo == null)
                    {
                        var postSaleInfo = new PostSaleInfo
                        {
                            TaskCompId = editedTask.TaskId,
                            IsFirstPaymentMade = false,
                            IsThereAProblem = false,
                            ProblemDescription = "",
                            IsContinuationConsidered = false,
                            IsTrustpilotReviewed = false,
                            CanUseLogo = false
                        };
                        _context.PostSaleInfos.Add(postSaleInfo);
                        await _context.SaveChangesAsync();
                    }

                    // Orijinal CreatedBy değerini koru
                    editedTask.CreatedBy = originalTask.CreatedBy;

                    // selectedCurrency değerini kontrol ediyoruz
                    originalTask.SelectedCurrency = editedTask.SelectedCurrency;

                    await LogChanges(originalTask, editedTask);

                    _context.Entry(originalTask).CurrentValues.SetValues(editedTask);
                    await _context.SaveChangesAsync();

              
                    // E-posta gönderme işlemi
                    if (editedTask.OutcomeStatus == OutcomeTypeSales.Won && originalTask.OutcomeStatus!=OutcomeTypeSales.Won)
                    {
                        var appUser = _context.Users.FirstOrDefault(u => u.Id == editedTask.UserId);
                        var assignedUser = _context.Users.FirstOrDefault(u => u.Id == editedTask.AssignedUserId);

                        string subject = "Tebrikler! Satışınız Gerçekleşti!";
                        string message = $"Tebrikler {originalTask.Customer.CompanyName} firmasıyla yaptığınız başarılı görüşmeler sonucunda satışınız gerçekleşmiştir.\nŞimdi satış sonrası takibasyon süreci!\nBaşarılar.";

                        if (appUser != null)
                        {
                            await _emailService.SendEmailAsync(appUser.Email, subject, message);
                        }

                        if (assignedUser != null)
                        {
                            await _emailService.SendEmailAsync(assignedUser.Email, subject, message);
                        }
                    }

                    return RedirectToAction("Index");
                }
                else
                {
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
                    if (property.Name == "Notes")
                    {
                        continue; // Notes alanını loglara dahil etmeden bir sonraki döngüye geç
                    }
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
                            oldValueString = ((OutcomeTypeSales)Enum.Parse(typeof(OutcomeTypeSales), originalValue.ToString())).GetDisplayName();
                            newValueString = ((OutcomeTypeSales)Enum.Parse(typeof(OutcomeTypeSales), editedValue.ToString())).GetDisplayName();

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
            var currentUsers = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUsers.Picture ?? "defaultpp.png");

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
                    .Include(e => e.Notes)  // Notları dahil ediyoruz
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
                    task.FileAttachments = new List<FileAttachment>();
                }

                return View("TaskDetail", task);
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }


        [HttpPost]
        public IActionResult AddNote(int taskId, string note)
        {
            var task = _context.TaskComps.Include(t => t.Notes).FirstOrDefault(t => t.TaskId == taskId);
            if (task != null)
            {
                var newNote = new TaskCompNote
                {
                    TaskCompId = taskId,
                    Note = note,
                    CreatedAt = DateTime.Now
                };
                task.Notes.Add(newNote);
                _context.SaveChanges();
            }
            return RedirectToAction("TaskDetail", new { id = taskId });
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
        public async Task<IActionResult> UploadFileEditPage(IFormFile file, int taskId)
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
                    return RedirectToAction("TaskEdit", new { id = taskId });
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
        public async Task<IActionResult> DeleteFileEditPage(int fileAttachmentId)
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
                return RedirectToAction("TaskEdit", new { id = fileAttachment.TaskId });
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


        //EXCEL EXPORT
        [HttpGet]
        public async Task<IActionResult> ExportTasksToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Mevcut kullanıcının kimliği ve rolünü alın
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            List<TaskComp> tasks;

            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                // Admin ise, aynı şirketteki tüm kullanıcıların görevlerini al
                var companyUserIds = _context.Users
                    .Where(u => u.CompanyId == currentUser.CompanyId)
                    .Select(u => u.Id)
                    .ToList();

                tasks = await _context.TaskComps
                    .Include(e => e.Status)
                    .Include(e => e.Customer)
                    .Include(e => e.AppUser)
                    .Where(t => companyUserIds.Contains(t.UserId) || companyUserIds.Contains(t.AssignedUserId))
                    .ToListAsync();
            }
            else
            {
                // Admin değilse, sadece kendi UserId veya AssignedUserId olduğu görevleri al
                tasks = await _context.TaskComps
                    .Include(e => e.Status)
                    .Include(e => e.Customer)
                    .Include(e => e.AppUser)
                    .Where(t => t.UserId == currentUser.Id || t.AssignedUserId == currentUser.Id)
                    .ToListAsync();
            }

            // Excel dosyasını oluştur
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Tasks");

                // Başlıkları ekle
                worksheet.Cells[1, 1].Value = "Görev Başlığı";
                worksheet.Cells[1, 2].Value = "Durum";
                worksheet.Cells[1, 3].Value = "Değer Teklifi";
                worksheet.Cells[1, 4].Value = "Açıklama";
                worksheet.Cells[1, 5].Value = "Satış Tarihi";
                worksheet.Cells[1, 6].Value = "Oluşturulma Tarihi";
                worksheet.Cells[1, 7].Value = "Müşteri";
                worksheet.Cells[1, 8].Value = "Kullanıcı";

                // Verileri doldur
                int row = 2;
                foreach (var task in tasks)
                {
                    worksheet.Cells[row, 1].Value = task.Title;
                    worksheet.Cells[row, 2].Value = task.Status?.StatusName;
                    worksheet.Cells[row, 3].Value = task.ValueOrOffer.HasValue ? task.ValueOrOffer.Value.ToString("N2") : "";
                    worksheet.Cells[row, 4].Value = task.Description;
                    worksheet.Cells[row, 5].Value = task.SalesDone?.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 6].Value = task.CreatedDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 7].Value = $"{task.Customer?.Name} {task.Customer?.Surname}";
                    worksheet.Cells[row, 8].Value = task.AppUser?.UserName;
                    row++;
                }

                // Stil ayarları
                worksheet.Cells[1, 1, 1, 8].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 1, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1, 1, 8].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Excel dosyasını indirme olarak kullanıcıya gönder
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                var fileName = $"Tasks_{DateTime.Now:yyyyMMddHHmm}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(stream, contentType, fileName);
            }
        }

        //EXCEL ıMPORT

        [HttpPost]
        public async Task<IActionResult> ImportTasksFromExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir Excel dosyası yükleyin.";
                return RedirectToAction("TaskAdd");
            }

            try
            {
                // Mevcut kullanıcı bilgisi
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "Geçerli kullanıcı bilgisi bulunamadı.";
                    return RedirectToAction("TaskAdd");
                }

                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.First();
                        var rowCount = worksheet.Dimension.Rows;
                        var tasks = new List<TaskComp>();

                        for (int row = 2; row <= rowCount; row++) // 1. satır başlık olduğu için 2'den başlıyor
                        {
                            var outcomeText = worksheet.Cells[row, 6].Value?.ToString().Trim();

                            // Eğer "Süreçte" yazılmışsa bunu "Surecte" olarak değiştir
                            if (!string.IsNullOrEmpty(outcomeText) && outcomeText.Equals("Süreçte", StringComparison.OrdinalIgnoreCase))
                            {
                                outcomeText = "Surecte";
                            }

                            // Enum dönüşümü
                            if (!Enum.TryParse(outcomeText, true, out OutcomeType outcome))
                            {
                                TempData["ErrorMessage"] = $"Sonuç Durumu sütunundaki değer geçersiz. Lütfen Excel dosyanızı kontrol edin. Satır: {row}";
                                return RedirectToAction("TaskAdd");
                            }

                            var isFinalDecisionMakerText = worksheet.Cells[row, 8].Value?.ToString().Trim().ToLower();
                            var isFinalDecisionMaker = isFinalDecisionMakerText == "evet";

                            var task = new TaskComp
                            {
                                Title = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                                ValueOrOffer = decimal.TryParse(worksheet.Cells[row, 2].Value?.ToString().Trim(), out var value) ? value : 0,
                                Description = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                                HeardFrom = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                                SalesDone = DateTime.TryParse(worksheet.Cells[row, 5].Value?.ToString().Trim(), out var date) ? date : (DateTime?)null,
                                Outcomes = outcome,
                                NegativeReason = (outcome == OutcomeType.Olumsuz) ? worksheet.Cells[row, 7].Value?.ToString().Trim() : null,
                                IsFinalDecisionMaker = isFinalDecisionMaker,
                                AssignedUserId = currentUser.Id, // Yükleyen kullanıcı AssignedUser olarak atanıyor
                                UserId = currentUser.Id, // Yükleyen kullanıcı AppUser olarak atanıyor
                              //  CustomerId = currentUser.CompanyId, // Mevcut kullanıcının şirket ID'si atanıyor
                                CreatedBy = currentUser.Id, // Görevi oluşturan olarak mevcut kullanıcı atanıyor
                                StatusId = 1 // İlk Temas durumu
                            };

                            // Eğer "Sonuç Durumu" olumsuz ise "NegativeReason" kontrol edilir
                            if (outcome == OutcomeType.Olumsuz && string.IsNullOrEmpty(task.NegativeReason))
                            {
                                TempData["ErrorMessage"] = $"Olumsuz Sonuç Durumu için Negatif Sebep sütunu boş olamaz. Satır: {row}";
                                return RedirectToAction("TaskAdd");
                            }

                            // Eğer başlık ve açıklama eksikse geçerli kayıt oluşturulamaz
                            if (string.IsNullOrEmpty(task.Title) || string.IsNullOrEmpty(task.Description))
                            {
                                TempData["ErrorMessage"] = $"Başlık ve Açıklama alanları zorunludur. Lütfen Excel dosyanızı kontrol edin. Satır: {row}";
                                return RedirectToAction("TaskAdd");
                            }

                            tasks.Add(task);
                        }

                        // Tüm görevleri veritabanına kaydet
                        _context.TaskComps.AddRange(tasks);
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["SuccessMessage"] = "Excel verileri başarıyla içe aktarıldı. Görevler mevcut kullanıcıya atandı.";
                return RedirectToAction("Index", "Task");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Excel dosyasını işlerken bir hata oluştu: " +
                    (ex.InnerException?.Message ?? ex.Message);
                return RedirectToAction("TaskAdd");
            }
        }

    }


    public class FileUploadOptions
    {
        public string UploadFolderPath { get; set; }
    }
}