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
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Common;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace CrmCorner.Controllers
{
    //  CRM Dashboard, Chart ve Bildirim fonksiyonları
    [Authorize(Roles = "SuperAdmin,Admin,TeamLeader,TeamMember")]
    public class HomeController : BaseController
    {
        private readonly CrmCornerContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly EmailService _emailService;
        private Timer _timer;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, CrmCornerContext context, RoleManager<AppRole> roleManager, EmailService emailService
           ) : base(userManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _roleManager = roleManager;
            _emailService = emailService;

        }



        [AllowAnonymous]
        public IActionResult Landing()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult About()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Pricing()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Refund()
        {
            return View();
        }


        public async Task<IActionResult> Index()
        {
          //  await SetLayout(); // area mı değil mi onu anlayıp layout set ediyor

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

                //var roles = await _userManager.GetRolesAsync(currentUser);
                //if (roles.Contains("Admin") || roles.Contains("SuperAdmin"))
                //{
                //    return RedirectToAction("Index", "Home");
                //}

                try
                {
                    // Kullanıcıyı detayları ile yeniden yükle
                    currentUser = await _context.Users
                        .Include(u => u.Customers)
                        .Include(u => u.TaskComps)
                        .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

                    var companyUsers = await _context.Users
                        .Where(u => u.EmailDomain == currentUser.EmailDomain)
                        .Include(u => u.Customers)
                        .ToListAsync();

                    var email = currentUser?.Email;

                    // Kullanıcı müşterileri
                    List<CustomerN> customers;

                    if (User.IsInRole("Admin") || User.IsInRole("Team Leader"))
                    {
                        customers = companyUsers
                            .Where(u => u.Customers != null)
                            .SelectMany(u => u.Customers)
                            .ToList();
                    }
                    else
                    {
                        customers = currentUser.Customers.ToList();
                    }

                    var sectorCount = customers
                        .Select(c => c.Industry)
                        .Distinct()
                        .Count();

                    // Pipeline görev sayısı
                    var pipelineTaskCount = await _context.PipelineTasks.CountAsync(t =>
                        t.AppUserId == currentUser.Id || t.ResponsibleUserId == currentUser.Id);

                    // ViewModel
                    var viewModel = new CompanyUsersViewModel
                    {
                        CurrentUser = currentUser,
                        CompanyUsers = companyUsers,
                        PipelineTaskCount = pipelineTaskCount,
                        SectorCount = sectorCount
                    };

                    // ============================================================
                    // 🟣 YENİ TODO SİSTEMİNDEN SON 5 GÖREVİ GETİRİYORUZ
                    // ============================================================

                    var latestTasks = await _context.TodoEntries
                        .Where(t => t.UserId == currentUser.Id && !t.IsDone)
                        .OrderByDescending(t => t.CreatedDate)
                        .Take(5)
                        .Select(t => new LatestTaskDto
                        {
                            Id = t.Id,
                            Text = t.Text,
                            CreatedDate = t.CreatedDate,
                            BoardId = t.TodoBoardId
                        })
                        .ToListAsync();

                    ViewBag.LatestTasks = latestTasks;

                    // Profil fotoğrafı
                    ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

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
        [AllowAnonymous]
        public IActionResult SignIn()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View();

                var hasUser = await _userManager.FindByEmailAsync(model.Email);
                if (hasUser == null)
                {
                    ModelState.AddModelError(string.Empty, "Email veya şifre yanlış");
                    return View();
                }

                if (!hasUser.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "Lütfen önce email adresinizi doğrulayın.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(hasUser, model.Password, model.RememberMe, true);

                if (result.Succeeded)
                {
                    var userModules = await _context.UserModules
                        .Where(x => x.UserId == hasUser.Id)
                        .Select(x => x.Module)
                        .ToListAsync();

                    if (userModules.Contains(ModuleType.CRM) && !userModules.Contains(ModuleType.SocialMedia))
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    if (userModules.Contains(ModuleType.SocialMedia) && !userModules.Contains(ModuleType.CRM))
                    {
                        return RedirectToAction("Dashboard", "SocialMedia");
                    }

                    if (userModules.Contains(ModuleType.CRM) && userModules.Contains(ModuleType.SocialMedia))
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    return RedirectToAction("Index", "Home");
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
        [AllowAnonymous]
        public IActionResult SignUp()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(request);
                }

                if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
                {
                    ModelState.AddModelError("Email", "Geçerli bir email adresi giriniz.");
                    return View(request);
                }

                request.Email = request.Email.Trim().ToLower();
                var emailDomain = request.Email.Split('@')[1].ToLower();

                // Aynı email ile kullanıcı var mı kontrol et
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Bu email adresi ile zaten kayıt mevcut.");
                    return View(request);
                }

                // Domain'e göre firma var mı kontrol et
                var company = _context.Companies.FirstOrDefault(c => c.EmailDomain.ToLower() == emailDomain);

                if (company == null)
                {
                    try
                    {
                        company = new Company
                        {
                            CompanyName = request.CompanyName,
                            EmailDomain = emailDomain,
                            IsApproved = false
                        };

                        _context.Companies.Add(company);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        return RedirectToAction("NotFound", "Error");
                    }
                }

                try
                {
                    var user = new AppUser
                    {
                        UserName = request.UserName,
                        Email = request.Email,
                        PhoneNumber = request.Phone,
                        NameSurname = request.NameSurname,
                        PositionName = request.PositionName,
                        CompanyName = company.CompanyName,
                        EmailDomain = emailDomain,
                        CompanyId = company.CompanyId,
                        EmailConfirmed = false
                    };

                    var identityResult = await _userManager.CreateAsync(user, request.Password);

                    if (identityResult.Succeeded)
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                        var confirmationLink = Url.Action(
                            "ConfirmEmail",
                            "Home",
                            new { userId = user.Id, token = encodedToken },
                            protocol: Request.Scheme,
                            host: Request.Host.Value
                        );

                        await _emailService.SendEmailConfirmationAsync(user.Email, confirmationLink);

                        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                        {
                            TempData["DebugConfirmationLink"] = confirmationLink;
                        }

                       // TempData["SuccessMessage"] = "Kayıt işlemi başarıyla tamamlandı. Lütfen email adresinize gelen doğrulama linkine tıklayın.";
                        return RedirectToAction("SignIn", "Home");
                    }

                    ModelState.AddModelErrorList(identityResult.Errors.Select(x => x.Description).ToList());
                    return View(request);
                }
                catch (Exception)
                {
                    return RedirectToAction("NotFound", "Error");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Geçersiz doğrulama linki.";
                return RedirectToAction("SignIn", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("SignIn", "Home");
            }

            try
            {
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Email adresiniz başarıyla doğrulandı. Giriş yapabilirsiniz.";
                    return RedirectToAction("SignIn", "Home");
                }

                TempData["ErrorMessage"] = string.Join(" | ", result.Errors.Select(x => x.Description));
                return RedirectToAction("SignIn", "Home");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Token çözülürken hata oluştu: " + ex.Message;
                return RedirectToAction("SignIn", "Home");
            }
        }
        #endregion

        [AllowAnonymous]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [AllowAnonymous]
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

        [AllowAnonymous]
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

        [AllowAnonymous]
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

            if (currentUser == null)
            {
                return Json(new
                {
                    message = new
                    {
                        hasUnreadMessages = false,
                        distinctSenderIdsCount = 0,
                        unreadMessages = new List<object>()
                    }
                });
            }

            var senderIds = await _context.ChatHistories
                .AsNoTracking()
                .Where(m => m.ReceiverId == currentUser.Id && !m.IsRead)
                .Select(m => m.SenderId)
                .Distinct()
                .ToListAsync();

            var senders = await _userManager.Users
                .AsNoTracking()
                .Where(u => senderIds.Contains(u.Id))
                .Select(u => new
                {
                    senderId = u.Id,
                    senderName = u.UserName,
                    name = u.NameSurname
                })
                .ToListAsync();

            return Json(new
            {
                message = new
                {
                    hasUnreadMessages = senders.Any(),
                    distinctSenderIdsCount = senders.Count,
                    unreadMessages = senders
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetTopbarNotifications()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new
                {
                    notifications = new List<object>(),
                    unreadCount = 0
                });
            }

            var now = DateTime.Now;
            var fiveDaysLater = now.AddDays(5);

            var todoNotifications = await _context.TodoEntries
                .AsNoTracking()
                .Where(t =>
                    !t.IsDone &&
                    (
                        t.UserId == currentUser.Id
                        || t.AssigneeId == currentUser.Id
                    ) &&
                    t.Deadline.HasValue &&
                    t.Deadline.Value <= fiveDaysLater)
                .OrderBy(t => t.Deadline)
                .Take(10)
                .Select(t => new
                {
                    id = t.Id,
                    text = t.Text,
                    boardId = t.TodoBoardId,
                    deadline = t.Deadline,
                    isAssignedToMe = t.AssigneeId == currentUser.Id && t.UserId != currentUser.Id
                })
                .ToListAsync();

            var notifications = todoNotifications.Select(t =>
            {
                string type;
                string meta;

                if (t.deadline.HasValue && t.deadline.Value < now)
                {
                    type = "overdue";
                    meta = "Süresi geçti";
                }
                else if (t.isAssignedToMe)
                {
                    type = "assigned";
                    var daysLeft = t.deadline.HasValue ? (t.deadline.Value.Date - now.Date).Days : 0;
                    meta = daysLeft <= 0 ? "Bugün son gün" : $"{daysLeft} gün kaldı";
                }
                else
                {
                    type = "deadline";
                    var daysLeft = t.deadline.HasValue ? (t.deadline.Value.Date - now.Date).Days : 0;

                    if (daysLeft <= 0)
                        meta = "Bugün son gün";
                    else if (daysLeft == 1)
                        meta = "1 gün kaldı";
                    else
                        meta = $"{daysLeft} gün kaldı";
                }

                return new
                {
                    id = t.id,
                    title = t.text,
                    type = type,
                    meta = meta,
                    boardId = t.boardId,
                    deadline = t.deadline.HasValue ? t.deadline.Value.ToString("dd.MM.yyyy HH:mm") : ""
                };
            }).ToList();

            return Json(new
            {
                notifications,
                unreadCount = notifications.Count
            });
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Contact()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Terms()
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