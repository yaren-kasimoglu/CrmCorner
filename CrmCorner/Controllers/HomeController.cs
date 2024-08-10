using CrmCorner.Extensions;
using CrmCorner.Models;
using CrmCorner.Models.Enums;
using CrmCorner.Services;
using CrmCorner.ViewModels;
using Google.Apis.Gmail.v1;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Common;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;

namespace CrmCorner.Controllers
{
    public class HomeController : Controller
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IEmailServices _emailServices;
        private readonly EmailService _emailService;
        private Timer _timer;
        //deneme yorum
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            IEmailServices emailServices, CrmCornerContext context, RoleManager<AppRole> roleManager, EmailService emailService
           )
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailServices = emailServices;
            _context = context;
            _roleManager = roleManager;
            _emailService = emailService;

        }
     
        public async Task<IActionResult> Index()
        {
            try
            {
                if (!User.Identity!.IsAuthenticated)
                {
                    return RedirectToAction("SignIn");
                }

                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser == null)
                {
                    return View();
                }

                try
                {
                    currentUser = await _context.Users
                                            .Include(u => u.Customers)
                                            .Include(u => u.TaskComps)
                                            .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

                    var companyUsers = await _context.Users
                                                     .Where(u => u.EmailDomain == currentUser.EmailDomain)
                                                     .Include(u => u.Customers)
                                                     .ToListAsync();

                    var taskComps = await _context.TaskComps.ToListAsync(); // Bu satırı görevleri yüklemek için ekledim
                    var email = currentUser?.Email;

                    List<CustomerN> customers;

                    if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                    {
                        // Admin veya Manager ise aynı email domainine sahip tüm kullanıcıların müşterilerini getir
                        customers = companyUsers
                                    .Where(u => u.Customers != null)
                                    .SelectMany(u => u.Customers)
                                    .ToList();
                    }
                    else
                    {
                        // Değilse, sadece kullanıcının kendi müşterilerini getir
                        customers = currentUser.Customers.ToList();
                    }


                    // Kullanıcının sahip olduğu müşterilerde kaç farklı sektör olduğunu hesapla
                    var sectorCount = customers
                                      .Select(c => c.Industry)
                                      .Distinct()
                                      .Count();

                    var viewModel = new CompanyUsersViewModel
                    {
                        CurrentUser = currentUser,
                        CompanyUsers = companyUsers,
                        TaskComps = taskComps, // ViewModel'e TaskComps ekleyin
                        SectorCount = sectorCount // Sektör sayısını ViewModel'e ekleyin
                    };

                    ViewData["UserEmail"] = email;
                    ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

                    var todoList = _context.ToDoList
                         .Where(e => e.UserId == currentUser.Id && e.NotDoneList!=null )
                        .Select(e => new ToDo { Id = e.Id, CreatedDate = e.CreatedDate,NotDoneList=e.NotDoneList })
     .                   ToList();
                   var todoListToday = _context.ToDos
                        .Where(e => e.UserId == currentUser.Id && e.NotDoneList != null)
                        .ToList();

                    var combinedData = todoList.Concat(todoListToday)
                        .OrderBy(data => (data.CreatedDate - DateTime.Now))
                        .Take(5)
                        .ToList();
                    List<Tuple<string, string>> updatedList = new List<Tuple<string, string>>();
                    for (var item = 0; item < combinedData.Count; item++)
                    {
                        var url = "https://crmcorner.co/ToDoList/ToDoList/"+ combinedData[item].Id.ToString();
                        if (combinedData[item].NotDoneList != null && combinedData[item].NotDoneList.Contains(','))
                        {
                            var parts = combinedData[item].NotDoneList.Split(',');
                            foreach (var part in parts)
                            {
                                updatedList.Add(Tuple.Create(part, url)); // İkinci eleman için boş bir değer ekledim, gerekirse değiştirebilirsiniz
                                if (updatedList.Count > 5)
                                    break;
                            }
                        }
                        else
                        {
                            var urlt = "https://crmcorner.co/ToDoList/ToDoList/" + combinedData[item].Id.ToString();
                            updatedList.Add(Tuple.Create(combinedData[item].NotDoneList, urlt)); // İkinci eleman için boş bir değer ekledim, gerekirse değiştirebilirsiniz
                            if (updatedList.Count > 5)
                                break;
                        }
                        if (updatedList.Count > 5)
                            break;
                    }
                    ViewBag.ToDoList = updatedList;

                    return View(viewModel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Index action.");
                    return RedirectToAction("NotFound", "Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action.");
                return RedirectToAction("NotFound", "Error");
            }
        }




        #region CHARTS

        [Authorize]
     public async Task<IActionResult> ValueOfferChart()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var user = await _context.Users
                                         .Include(u => u.TaskComps)
                                         .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound("Kullanıcı bulunamadı.");
                }

                // Kullanıcının AppUser ve AssignedUser olduğu görevleri birleştirelim
                var taskCompsAsAppUser = await _context.TaskComps.Where(tc => tc.UserId == userId && tc.ValueOrOffer.HasValue).ToListAsync();
                var taskCompsAsAssignedUser = await _context.TaskComps.Where(tc => tc.AssignedUserId == userId && tc.ValueOrOffer.HasValue).ToListAsync();
                var allTaskComps = taskCompsAsAppUser.Concat(taskCompsAsAssignedUser).Distinct().ToList();

                var ranges = new[]
                {
            new { Min = 0m, Max = 1000m, Label = "0-1000" },
            new { Min = 1000m, Max = 2000m, Label = "1000-2000" },
            new { Min = 2000m, Max = 3000m, Label = "2000-3000" },
            new { Min = 3000m, Max = 4000m, Label = "3000-4000" },
            new { Min = 4000m, Max = 5000m, Label = "4000-5000" },
            new { Min = 5000m, Max = 6000m, Label = "5000-6000" },
            new { Min = 6000m, Max = 7000m, Label = "6000-7000" },
            new { Min = 7000m, Max = 100000m, Label = "7000-100000" }
        };

                var chartData = ranges.Select(range => new
                {
                    range.Label,
                    Count = allTaskComps
                                .Where(tc => tc.ValueOrOffer.HasValue &&
                                             tc.ValueOrOffer.Value >= range.Min &&
                                             tc.ValueOrOffer.Value < range.Max)
                                .Count(),
                    TaskNames = allTaskComps
                                .Where(tc => tc.ValueOrOffer.HasValue &&
                                             tc.ValueOrOffer.Value >= range.Min &&
                                             tc.ValueOrOffer.Value < range.Max)
                                .Select(tc => tc.Title)
                                .ToList()
                }).ToList();

                var labels = chartData.Select(data => data.Label).ToArray();
                var dataValues = chartData.Select(data => data.Count).ToArray();
                var taskNames = chartData.Select(data => data.TaskNames).ToArray();

                return Json(new { labels, data = dataValues, taskNames });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ValueOffer chart verileri getirilirken bir hata oluştu.");
                return StatusCode(500, "İşleminiz sırasında bir hata oluştu.");
            }
        }


        [Authorize]
        public async Task<IActionResult> IndustryChart()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

        

                var user = await _context.Users
                                          .Include(u => u.Customers)
                                          .FirstOrDefaultAsync(u => u.Id == userId);
                var roles = await _userManager.GetRolesAsync(user);

                bool isAdminOrManager = roles.Contains("Admin") || roles.Contains("Manager");

                if (user == null)
                {
                    return NotFound("User not found.");
                }


                List<CustomerN> customers;

                if (isAdminOrManager)
                {
                    // Admin veya Manager ise, aynı email domainine sahip kullanıcıların müşterilerini getir
                    var emailDomain = user.EmailDomain;
                    var companyUsers = await _context.Users
                                                     .Where(u => u.EmailDomain == emailDomain)
                                                     .Include(u => u.Customers)
                                                     .ToListAsync();
                    customers = companyUsers.SelectMany(u => u.Customers).ToList();
                }
                else
                {
                    // Değilse, sadece kullanıcının kendi müşterilerini getir
                    customers = user.Customers.ToList();
                }

                // Sektörleri gruplayıp sayılarına göre chart verisi oluşturma
                var chartData = customers
                                 .GroupBy(c => c.Industry)
                                 .Select(group => new { Industry = group.Key, Count = group.Count(), CustomerNames = group.Select(c => c.CompanyName).ToList() })
                                 .ToList();

                // labels ve data alanlarını doldur
                var labels = chartData.Select(data => data.Industry.GetDisplayName().ToString()).ToArray();
                var dataValues = chartData.Select(data => data.Count).ToArray();
                var customerNames = chartData.Select(data => data.CustomerNames).ToArray();

                return Json(new { labels, data = dataValues, customerNames });
            }
            catch (Exception ex)
            {
                // Hata günlüğüne yaz
                _logger.LogError(ex, "An error occurred while fetching industry chart data.");

                // Hata mesajıyla birlikte bir server hatası döndür
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        [Authorize]
        public async Task<IActionResult> IsFinalDecisionMaker()
        {
            var userId = _userManager.GetUserId(User);

            // Kullanıcının AppUser veya AssignedUser olduğu görevleri bul
            var userTaskComps = await _context.TaskComps
                                              .Where(tc => tc.UserId == userId || tc.AssignedUserId == userId)
                                              .ToListAsync();

            if (userTaskComps == null || !userTaskComps.Any())
            {
                return NotFound();
            }

            // Görevleri benzersiz olarak saymak için filtre uygula
            var distinctTasks = userTaskComps.GroupBy(tc => tc.TaskId).Select(group => group.First()).ToList();

            var finalDecisionMakerTasks = distinctTasks.Where(tc => tc.IsFinalDecisionMaker).Select(tc => tc.Title).Distinct().ToList();
            var notFinalDecisionMakerTasks = distinctTasks.Where(tc => !tc.IsFinalDecisionMaker).Select(tc => tc.Title).Distinct().ToList();

            var chartData = new
            {
                labels = new[] { "Evet", "Hayır" },
                data = new[] { finalDecisionMakerTasks.Count, notFinalDecisionMakerTasks.Count },
                taskNames = new[] { finalDecisionMakerTasks, notFinalDecisionMakerTasks }
            };

            return Json(chartData);
        }


        [Authorize]
        public async Task<IActionResult> OutcomeStatusChart()
        {
            var userId = _userManager.GetUserId(User);

            // AppUser ve AssignedUser olarak atanan TaskComps'ı bul ve birleşik bir liste oluştur
            var combinedTasks = await _context.TaskComps
                                              .Include(tc => tc.Status)
                                              .Where(tc => tc.UserId == userId || tc.AssignedUserId == userId)
                                              .ToListAsync();

            // Görevleri benzersiz olarak saymak için filtre uygula
            var distinctTasks = combinedTasks.GroupBy(tc => tc.TaskId).Select(group => group.First()).ToList();

            if (!distinctTasks.Any())
            {
                return NotFound();
            }

            var outcomeGroups = distinctTasks.GroupBy(tc => tc.Outcomes)
                                             .Select(group => new {
                                                 Outcome = group.Key.ToString(),
                                                 Count = group.Count(),
                                                 TaskNames = group.Select(tc => tc.Title).Distinct().ToList()
                                             }).ToList();

            // Tüm olası enum değerlerini döngü ile işle
            var labels = Enum.GetNames(typeof(OutcomeType)).ToList();
            var data = new List<int>();
            var taskNames = new List<List<string>>();

            foreach (var label in labels)
            {
                var group = outcomeGroups.FirstOrDefault(g => g.Outcome == label);
                if (group != null)
                {
                    data.Add(group.Count);
                    taskNames.Add(group.TaskNames);
                }
                else
                {
                    data.Add(0);
                    taskNames.Add(new List<string>());
                }
            }

            var chartData = new
            {
                labels = labels.ToArray(),
                data = data.ToArray(),
                taskNames = taskNames
            };

            return Json(chartData);
        }

        [Authorize]
        public async Task<IActionResult> UserTaskStatusChart()
       {
            // Aktif kullanıcının ID'sini al
            var userId = _userManager.GetUserId(User);

            // AppUser olarak atanan TaskComps'ı bul
            var appUserTasks = await _context.TaskComps
                                              .Include(tc => tc.Status)
                                              .OrderBy(tc=>tc.StatusId)
                                              .Where(tc => tc.UserId == userId)
                                              .ToListAsync();

            // AssignedUser olarak atanan TaskComps'ı bul
            var assignedUserTasks = await _context.TaskComps
                                                   .Include(tc => tc.Status)
                                                   .Where(tc => tc.AssignedUserId == userId)
                                                   .OrderBy(tc => tc.StatusId)
                                                   .ToListAsync();

            // İki listeyi birleştir ve belirli kullanıcılar için filtre uygula
            var combinedTasks = appUserTasks.Concat(assignedUserTasks)
                                            .Where(tc => tc.UserId == userId || tc.AssignedUserId == userId)
                                            .OrderBy(tc => tc.StatusId)
                                            .ToList();

            var chartData = combinedTasks.GroupBy(tc => tc.Status.StatusName)
                                         .Select(group => new {
                                             StatusName = group.Key,
                                             TaskNames = group.Select(tc => tc.Title).Distinct().ToList(), // Görev isimlerini ayırt et
                                             Count = group.Select(tc => tc.Title).Distinct().Count() // Görev isimlerini ayırt et ve sayısını al
                                         }).ToList();

            return Json(chartData);
        }


        #endregion

        public IActionResult Giris()
        {
            return View();
        }

        #region Giriş
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                returnUrl ??= Url.Action("Index", "Home"); // null olma durumuna karşılık bu değerin atanması. Basit bir kullanım yöntemi

                var hasUser = await _userManager.FindByEmailAsync(model.Email);
                if (hasUser == null)
                {
                    ModelState.AddModelError(string.Empty, "Email veya şifre yanlış");
                    return View();
                }

                // Şirket onaylı mı değil mi kontrol edildiği yer
                var company = _context.Companies.FirstOrDefault(c => c.CompanyId == hasUser.CompanyId);
                if (company == null || !(company.IsApproved ?? false))
                {
                    ModelState.AddModelError(string.Empty, "Şirketiniz sistem tarafından henüz onaylanmamış.");
                    return View();
                }

                var signInresult = await _signInManager.PasswordSignInAsync(hasUser, model.Password, model.RememberMe, true);

                if (signInresult.Succeeded)
                {
                    return Redirect(returnUrl);
                }

                if (signInresult.IsLockedOut)
                {
                    ModelState.AddModelErrorList(new List<string>() { "3 dakika boyunca giriş yapamazsınız." });
                    return View();
                }

                ModelState.AddModelErrorList(new List<string>() { $"Email veya şifre hatalı.", $"(Başarısız giriş sayısı : {await _userManager.GetAccessFailedCountAsync(hasUser)})" });

                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Bir hata oluştu: {ex.Message}");
                return View();
            }
        }


        #endregion

        #region Kayıt
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                // Kullanıcının e-posta adresinden domain'i çıkarma
                var emailDomain = request.Email.Split('@')[1].ToLower();

                // Domain'e göre bir firma var mı kontrol et
                var company = _context.Companies.FirstOrDefault(c => c.EmailDomain.ToLower() == emailDomain);

                if (company == null)
                {
                    try
                    {
                        // Yeni firma oluştur ve ekle
                        company = new Company
                        {
                            CompanyName = request.CompanyName,
                            EmailDomain = emailDomain,
                            IsApproved = false
                        };
                        _context.Companies.Add(company);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        return RedirectToAction("NotFound", "Error");
                    }
                }

                try
                {
                    // Kullanıcıyı kaydetme
                    var user = new AppUser
                    {
                        UserName = request.UserName,
                        Email = request.Email,
                        PhoneNumber = request.Phone,
                        NameSurname = request.NameSurname,
                        PositionName = request.PositionName,
                        CompanyName = company.CompanyName,  // Firma adını Company'den al
                        EmailDomain = emailDomain,
                        CompanyId = company.CompanyId
                    };

                    var identityResult = await _userManager.CreateAsync(user, request.Password);

                    if (identityResult.Succeeded)
                    {

                        TempData["SuccessMessage"] = "Kayıt işlemi başarıyla tamamlandı.";
                        return RedirectToAction("SignIn", "Home");
                    }

                    ModelState.AddModelErrorList(identityResult.Errors.Select(x => x.Description).ToList());
                    return View();
                }
                catch (Exception ex)
                {
                    return RedirectToAction("NotFound", "Error");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }


        //public async Task<IActionResult> SignUp(SignUpViewModel request)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View();
        //    }
        //    var identityResult = await _userManager.CreateAsync(new() { UserName = request.UserName, Email = request.Email, PhoneNumber = request.Phone,NameSurname=request.NameSurname,PositionName=request.PositionName,CompanyName=request.CompanyName,EmployeeCount=request.EmployeeCount,Sector=request.Sector }, request.Password);



        //    if (identityResult.Succeeded)
        //    {
        //        TempData["SucceesMessage"] = "Kayıt işlemi başarıyla tamamlandı.";
        //        return RedirectToAction("SignIn", "Home");

        //    }
        //    ModelState.AddModelErrorList(identityResult.Errors.Select(x => x.Description).ToList());
        //    return View();
        //}
        #endregion



        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel request)
        {
            try
            {
                var hasUser = await _userManager.FindByEmailAsync(request.Email);
                if (hasUser == null)
                {
                    ModelState.AddModelError(string.Empty, "Bu email adresine sahip kullanıcı bulunamamıştır.");
                    return View();
                }

                string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(hasUser);
                var passwordResetLink = Url.Action("ResetPassword", "Home", new { userId = hasUser.Id, Token = passwordResetToken }, HttpContext.Request.Scheme);

                 _emailService.SendEmailAsync(hasUser.Email, "Şifre sıfırlama linki",
                    $"<h4>Şifrenizi yenilemek için aşağıdaki linke tıklayınız.</h4><p><a href='{passwordResetLink}'>şifre yenileme link</a></p>");

                TempData["SuccessMessage"] = "Şifre yenileme linki, e-posta adresinize gönderilmiştir.";
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }

            return RedirectToAction(nameof(ForgetPassword));
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            try
            {
                TempData["userId"] = userId;
                TempData["token"] = token;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel request)
        {
            try
            {
                var userId = TempData["userId"];
                var token = TempData["token"];
                if (userId == null || token == null)
                {
                    throw new Exception("Bir hata meydana geldi.");
                }

                var hasUser = await _userManager.FindByIdAsync(userId.ToString()!);
                if (hasUser == null)
                {
                    ModelState.AddModelError(string.Empty, "Kullanıcı bulunamamıştır.");
                    return View();
                }

                IdentityResult result = await _userManager.ResetPasswordAsync(hasUser, token.ToString()!, request.Password);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Şifreniz başarıyla yenilenmiştir.";
                    return RedirectToAction("SignIn", "Home");
                }
                else
                {
                    ModelState.AddModelErrorList(result.Errors.Select(x => x.Description).ToList());
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetNotificationsStatus()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var unreadMessages = _context.ChatHistories
                                        .Where(m => m.ReceiverId == currentUser.Id && !m.IsRead)
                                        .Select(m => new { m.SenderId })
                                        .ToList();

            var senderIds = unreadMessages.Select(m => m.SenderId).Distinct().ToList();
            var senders = await _userManager.Users
                                .Where(u => senderIds.Contains(u.Id))
                                .Select(u => new { u.Id, u.UserName,u.NameSurname })
                                .ToListAsync();

            var senderDetails = senders.Select(s => new
            {
                SenderId = s.Id,
                SenderName = s.UserName,
                Name=s.NameSurname
            }).ToList();

            var jsonData = new
            {
                HasUnreadMessages = unreadMessages.Any(),
                DistinctSenderIdsCount = senderDetails.Count,
                UnreadMessages = senderDetails
            };

            return Json(new { Message = jsonData });
        }




        //#region ŞİFRE İŞLEMLERİ
        //public IActionResult ForgetPassword()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel request)
        //{
        //    try
        //    {
        //        var hasUser = await _userManager.FindByEmailAsync(request.Email);
        //        if (hasUser == null)
        //        {
        //            ModelState.AddModelError(String.Empty, "Bu email adresine sahip kullanıcı bulunamamıştır.");
        //            return View();
        //        }

        //        string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(hasUser);
        //        var passwordResetLink = Url.Action("ResetPassword", "Home", new { userId = hasUser.Id, Token = passwordResetToken }, HttpContext.Request.Scheme);

        //        await _emailServices.SendResetPasswordEmail(passwordResetLink!, hasUser.Email!);
        //        TempData["SuccessMessage"] = "Şifre yenileme linki, e-posta adresinize gönderilmiştir.";
        //    }
        //    catch (Exception ex)
        //    {
        //        return RedirectToAction("NotFound", "Error");
        //    }

        //    return RedirectToAction(nameof(ForgetPassword));
        //}

        //public IActionResult ResetPassword(string userId, string token)
        //{
        //    try
        //    {
        //        TempData["userId"] = userId;
        //        TempData["token"] = token;
        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        return RedirectToAction("NotFound", "Error");
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> ResetPassword(ResetPasswordViewModel request)
        //{
        //    try
        //    {
        //        var userId = TempData["userId"];
        //        var token = TempData["token"];
        //        if (userId == null || token == null)
        //        {
        //            throw new Exception("Bir hata meydana geldi.");
        //        }

        //        var hasUser = await _userManager.FindByIdAsync(userId.ToString()!);
        //        if (hasUser == null)
        //        {
        //            ModelState.AddModelError(String.Empty, "Kullanıcı bulunamamıştır.");
        //            return View();
        //        }

        //        IdentityResult result = await _userManager.ResetPasswordAsync(hasUser, token.ToString()!, request.Password);
        //        if (result.Succeeded)
        //        {
        //            TempData["SuccessMessage"] = "Şifreniz başarıyla yenilenmiştir.";
        //            return RedirectToAction("SignIn", "Home");
        //        }
        //        else
        //        {
        //            ModelState.AddModelErrorList(result.Errors.Select(x => x.Description).ToList());
        //            return View();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return RedirectToAction("NotFound", "Error");
        //    }
        //}
        //#endregion

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Notifications()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id; // Kullanıcı ID'sini alın

            if (userId == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            var notifications = await _context.Notifications
                                              .Where(n => n.UserId == userId)
                                              .ToListAsync(); // Kullanıcıya ait bildirimleri çekin

            return View(notifications); // Bildirimleri view'a gönderin
        }


        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Notifications));
            }
            return NotFound();
        }

    }
}