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
    public class HomeController : BaseController
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IEmailServices _emailServices;
        private readonly EmailService _emailService;
        private Timer _timer;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            IEmailServices emailServices, CrmCornerContext context, RoleManager<AppRole> roleManager, EmailService emailService
           ) : base(userManager)
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
            await SetLayout();//areadamı değilmi onu  nalayıp layout set ediyor
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

                var roles = await _userManager.GetRolesAsync(currentUser);
                if (roles.Contains("Admin"))//areaya yönlendiriyorum
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
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

                    var pipelineTaskCount = await _context.PipelineTasks.CountAsync(t =>
    t.AppUserId == currentUser.Id || t.ResponsibleUserId == currentUser.Id);

                    var viewModel = new CompanyUsersViewModel
                    {
                        CurrentUser = currentUser,
                        CompanyUsers = companyUsers,
                        PipelineTaskCount = pipelineTaskCount,
                        SectorCount = sectorCount
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
        //[Authorize]
        [HttpGet]
        public async Task<IActionResult> SourceChannelChart()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            // SADECE bana bağlı görevler
            var items = await _context.PipelineTasks
                .AsNoTracking()
                .Where(t =>
                    t.SourceChannel != null &&
                    (t.AppUserId == currentUserId || t.ResponsibleUserId == currentUserId))
                .Select(t => new { t.SourceChannel, t.Title })
                .ToListAsync();

            var grouped = items
                .GroupBy(x => x.SourceChannel!.Value)
                .Select(g => new
                {
                    Label = g.Key.GetDisplayName() ?? g.Key.ToString(), // enum DisplayName varsa
                    Count = g.Count(),
                    TaskNames = g.Select(x => x.Title).ToList()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return Json(new
            {
                labels = grouped.Select(x => x.Label).ToList(),
                data = grouped.Select(x => x.Count).ToList(),
                taskNames = grouped.Select(x => x.TaskNames).ToList()
            });
        }

        //[Authorize]
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

                // Verileri döviz cinsine göre gruplayalım
                var tlTasks = allTaskComps.Where(tc => tc.SelectedCurrency == "₺").ToList();
                var euTasks = allTaskComps.Where(tc => tc.SelectedCurrency == "€").ToList();
                var dollarTasks = allTaskComps.Where(tc => tc.SelectedCurrency == "$").ToList();

                // Grafikleri oluşturmak için verileri hazırlayalım
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

                // Her bir döviz için chart verilerini hazırlayalım
                var tlChartData = PrepareChartData(tlTasks, ranges);
                var euChartData = PrepareChartData(euTasks, ranges);
                var dollarChartData = PrepareChartData(dollarTasks, ranges);

                // Grafikleri frontend'e JSON olarak gönderelim
                return Json(new
                {
                    tlChart = new { labels = tlChartData.Select(c => c.Label).ToArray(), data = tlChartData.Select(c => c.TotalValue).ToArray(), taskNames = tlChartData.Select(c => c.TaskNames).ToArray() },
                    euChart = new { labels = euChartData.Select(c => c.Label).ToArray(), data = euChartData.Select(c => c.TotalValue).ToArray(), taskNames = euChartData.Select(c => c.TaskNames).ToArray() },
                    dollarChart = new { labels = dollarChartData.Select(c => c.Label).ToArray(), data = dollarChartData.Select(c => c.TotalValue).ToArray(), taskNames = dollarChartData.Select(c => c.TaskNames).ToArray() }


                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ValueOffer chart verileri getirilirken bir hata oluştu.");
                return StatusCode(500, "İşleminiz sırasında bir hata oluştu.");
            }
        }
        // Yardımcı fonksiyon: chart verilerini hazırlar
        private List<ChartData> PrepareChartData(List<TaskComp> taskComps, dynamic[] ranges)
        {
            return ranges.Select(range => new ChartData
            {
                Label = range.Label,
                TotalValue = taskComps
                                .Where(tc => tc.ValueOrOffer.HasValue &&
                                             tc.ValueOrOffer.Value >= range.Min &&
                                             tc.ValueOrOffer.Value < range.Max)
                                .Sum(tc => tc.ValueOrOffer.Value), // Toplam değeri alıyoruz
                TaskNames = taskComps
                            .Where(tc => tc.ValueOrOffer.HasValue &&
                                         tc.ValueOrOffer.Value >= range.Min &&
                                         tc.ValueOrOffer.Value < range.Max)
                            .Select(tc => tc.Title)
                            .ToList()
            }).ToList();
        }

        //   [Authorize]
        //public async Task<IActionResult> ValueOfferChart()
        //   {
        //       try
        //       {
        //           var userId = _userManager.GetUserId(User);
        //           var user = await _context.Users
        //                                    .Include(u => u.TaskComps)
        //                                    .FirstOrDefaultAsync(u => u.Id == userId);

        //           if (user == null)
        //           {
        //               return NotFound("Kullanıcı bulunamadı.");
        //           }

        //           // Kullanıcının AppUser ve AssignedUser olduğu görevleri birleştirelim
        //           var taskCompsAsAppUser = await _context.TaskComps.Where(tc => tc.UserId == userId && tc.ValueOrOffer.HasValue).ToListAsync();
        //           var taskCompsAsAssignedUser = await _context.TaskComps.Where(tc => tc.AssignedUserId == userId && tc.ValueOrOffer.HasValue).ToListAsync();
        //           var allTaskComps = taskCompsAsAppUser.Concat(taskCompsAsAssignedUser).Distinct().ToList();

        //           var ranges = new[]
        //           {
        //       new { Min = 0m, Max = 1000m, Label = "0-1000" },
        //       new { Min = 1000m, Max = 2000m, Label = "1000-2000" },
        //       new { Min = 2000m, Max = 3000m, Label = "2000-3000" },
        //       new { Min = 3000m, Max = 4000m, Label = "3000-4000" },
        //       new { Min = 4000m, Max = 5000m, Label = "4000-5000" },
        //       new { Min = 5000m, Max = 6000m, Label = "5000-6000" },
        //       new { Min = 6000m, Max = 7000m, Label = "6000-7000" },
        //       new { Min = 7000m, Max = 100000m, Label = "7000-100000" }
        //   };

        //           var chartData = ranges.Select(range => new
        //           {
        //               range.Label,
        //               Count = allTaskComps
        //                           .Where(tc => tc.ValueOrOffer.HasValue &&
        //                                        tc.ValueOrOffer.Value >= range.Min &&
        //                                        tc.ValueOrOffer.Value < range.Max)
        //                           .Count(),
        //               TaskNames = allTaskComps
        //                           .Where(tc => tc.ValueOrOffer.HasValue &&
        //                                        tc.ValueOrOffer.Value >= range.Min &&
        //                                        tc.ValueOrOffer.Value < range.Max)
        //                           .Select(tc => tc.Title)
        //                           .ToList()
        //           }).ToList();

        //           var labels = chartData.Select(data => data.Label).ToArray();
        //           var dataValues = chartData.Select(data => data.Count).ToArray();
        //           var taskNames = chartData.Select(data => data.TaskNames).ToArray();

        //           return Json(new { labels, data = dataValues, taskNames });
        //       }
        //       catch (Exception ex)
        //       {
        //           _logger.LogError(ex, "ValueOffer chart verileri getirilirken bir hata oluştu.");
        //           return StatusCode(500, "İşleminiz sırasında bir hata oluştu.");
        //       }
        //   }


        //[Authorize]
        // /Home/IndustryChart
        public async Task<IActionResult> IndustryChart()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                var tasks = await _context.PipelineTasks
                    .Include(p => p.Customer)
                    .Where(p => (p.AppUserId == userId || p.ResponsibleUserId == userId) && p.CustomerId != null)
                    .ToListAsync();

                // Customer veya Industry null olabilir; filtrele
                var chartData = tasks
                    .Where(p => p.Customer != null)
                    .GroupBy(p => p.Customer!.Industry)
                    .Select(g => new
                    {
                        Industry = g.Key, // enum? olabilir
                        Count = g.Count(),
                        CustomerNames = g.Select(x => x.CompanyName ?? x.Customer!.CompanyName ?? $"#{x.CustomerId}").Distinct().ToList()
                    })
                    .ToList();

                var labels = chartData
                    .Select(x => x.Industry == null ? "Undefined" : x.Industry.GetDisplayName().ToString())
                    .ToArray();

                var dataValues = chartData.Select(x => x.Count).ToArray();
                var customerNames = chartData.Select(x => x.CustomerNames).ToArray();

                return Json(new { labels, data = dataValues, customerNames });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching industry chart data.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }



        //[Authorize]
        // /Home/IsFinalDecisionMaker  -> İletişim yöntemi grafiği
        public async Task<IActionResult> IsFinalDecisionMaker()
        {
            var userId = _userManager.GetUserId(User);

            var tasks = await _context.PipelineTasks
                .Where(p => p.AppUserId == userId || p.ResponsibleUserId == userId)
                .ToListAsync();

            var both = tasks
                .Where(p => (p.ContactedViaLinkedIn ?? false) && (p.ContactedViaColdCall ?? false))
                .Select(x => x.Title ?? $"#{x.Id}")
                .Distinct()
                .ToList();

            var onlyLinkedIn = tasks
                .Where(p => (p.ContactedViaLinkedIn ?? false) && !(p.ContactedViaColdCall ?? false))
                .Select(x => x.Title ?? $"#{x.Id}")
                .Distinct()
                .ToList();

            var onlyColdCall = tasks
                .Where(p => (p.ContactedViaColdCall ?? false) && !(p.ContactedViaLinkedIn ?? false))
                .Select(x => x.Title ?? $"#{x.Id}")
                .Distinct()
                .ToList();

            var none = tasks
                .Where(p => !(p.ContactedViaLinkedIn ?? false) && !(p.ContactedViaColdCall ?? false))
                .Select(x => x.Title ?? $"#{x.Id}")
                .Distinct()
                .ToList();

            var labels = new[] { "LinkedIn", "Cold Call", "Her ikisi", "Hiçbiri" };
            var data = new[] { onlyLinkedIn.Count, onlyColdCall.Count, both.Count, none.Count };
            var taskNames = new List<List<string>> { onlyLinkedIn, onlyColdCall, both, none };

            return Json(new { labels, data, taskNames });
        }



        //[Authorize]
        // /Home/OutcomeStatusChart
        public async Task<IActionResult> OutcomeStatusChart()
        {
            var userId = _userManager.GetUserId(User);

            var tasks = await _context.PipelineTasks
                .Where(p => p.AppUserId == userId || p.ResponsibleUserId == userId)
                .ToListAsync();

            // Grupla
            var grouped = tasks
                .GroupBy(p => (p.OutcomeStatus?.ToString() ?? "None"))
                .Select(g => new
                {
                    Outcome = g.Key,
                    Count = g.Count(),
                    TaskNames = g.Select(x => x.Title ?? $"#{x.Id}").Distinct().ToList()
                })
                .ToList();

            // Enum isim listesini stabil sırayla üret (mevcut enum tipine göre)
            var allLabels = Enum.GetNames(typeof(OutcomeTypeSales)).ToList();
            if (!allLabels.Contains("None")) allLabels.Insert(0, "None");

            var data = new List<int>();
            var taskNames = new List<List<string>>();

            foreach (var label in allLabels)
            {
                var grp = grouped.FirstOrDefault(x => x.Outcome == label);
                if (grp == null)
                {
                    data.Add(0);
                    taskNames.Add(new List<string>());
                }
                else
                {
                    data.Add(grp.Count);
                    taskNames.Add(grp.TaskNames);
                }
            }

            return Json(new
            {
                labels = allLabels.ToArray(),
                data = data.ToArray(),
                taskNames
            });
        }


        //[Authorize]
        // /Home/UserTaskStatusChart
        public async Task<IActionResult> UserTaskStatusChart()
        {
            var userId = _userManager.GetUserId(User);

            // Kullanıcının AppUser veya ResponsibleUser olarak yer aldığı PipelineTask'lar
            var tasks = await _context.PipelineTasks
                .Where(p => p.AppUserId == userId || p.ResponsibleUserId == userId)
                .ToListAsync();

            // Null Stage'leri “Undefined” gibi bir sanal kategoriye çekmek istersen:
            var data = tasks
                .GroupBy(p => p.Stage?.ToString() ?? "Undefined")
                .Select(g => new
                {
                    statusName = g.Key,
                    TaskNames = g.Select(x => x.Title ?? $"#{x.Id}").Distinct().ToList(),
                    Count = g.Select(x => x.Title ?? $"#{x.Id}").Distinct().Count()
                })
                .OrderBy(x => x.statusName)
                .ToList();

            return Json(data);
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
                    return View();

                returnUrl ??= Url.Action("Index", "SocialMedia");

                var hasUser = await _userManager.FindByEmailAsync(model.Email);
                if (hasUser == null)
                {
                    ModelState.AddModelError(string.Empty, "Email veya şifre yanlış");
                    return View();
                }

                var result = await _signInManager.PasswordSignInAsync(hasUser, model.Password, model.RememberMe, true);

                if (result.Succeeded)
                {
                    return Redirect(returnUrl);
                }

                ModelState.AddModelError(string.Empty, "Giriş başarısız.");
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
    public class ChartData
    {
        public string Label { get; set; }
        public decimal TotalValue { get; set; } // Değer Teklifi Toplamı
        public List<string> TaskNames { get; set; }
    }

}