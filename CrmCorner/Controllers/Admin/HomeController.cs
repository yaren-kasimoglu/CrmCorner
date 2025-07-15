using CrmCorner.Areas.Admin.Models;
using CrmCorner.Controllers;
using CrmCorner.Extensions;
using CrmCorner.Models;
using CrmCorner.Models.Enums;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers.Admin
{
    public class ChartData
    {
        public string Label { get; set; }
        public decimal TotalValue { get; set; } // Değer Teklifi Toplamı
        public List<string> TaskNames { get; set; }
    }


    //[Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]
    public class HomeController : BaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<AppUser> _signInManager;


        //deneme yorum
        public HomeController(UserManager<AppUser> userManager, CrmCornerContext context = null, ILogger<HomeController> logger = null, SignInManager<AppUser> signInManager = null) : base(userManager)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            await SetLayout();
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
                         .Where(e => e.UserId == currentUser.Id && e.NotDoneList != null)
                        .Select(e => new ToDo { Id = e.Id, CreatedDate = e.CreatedDate, NotDoneList = e.NotDoneList })
     .ToList();
                    var todoListToday = _context.ToDos
                         .Where(e => e.UserId == currentUser.Id && e.NotDoneList != null)
                         .ToList();

                    var combinedData = todoList.Concat(todoListToday)
                        .OrderBy(data => data.CreatedDate - DateTime.Now)
                        .Take(5)
                        .ToList();
                    List<Tuple<string, string>> updatedList = new List<Tuple<string, string>>();
                    for (var item = 0; item < combinedData.Count; item++)
                    {
                        var url = "https://crmcorner.co/ToDoList/ToDoList/" + combinedData[item].Id.ToString();
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

        //public async Task<IActionResult> UserList()
        //{
        //    var userList=await _userManager.Users.ToListAsync();
        //    var userViewModelList = userList.Select(x => new UserViewModel()
        //    {
        //        Id = x.Id,
        //        Name = x.UserName,
        //        Email = x.Email
        //    }).ToList();
        //    return View(userViewModelList);
        //}

        #region CHARTS


        [Authorize]
        public async Task<IActionResult> HeardFromChart()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // Kullanıcının AppUser ve AssignedUser olduğu görevleri birleştir
                var taskComps = await _context.TaskComps
                                              .Where(tc => tc.UserId == userId || tc.AssignedUserId == userId)
                                              .Where(tc => !string.IsNullOrEmpty(tc.HeardFrom)) // HeardFrom boş olmayanları filtrele
                                              .ToListAsync();

                var chartData = taskComps
                                .GroupBy(tc => tc.HeardFrom.ToLower()) // Küçük harf ile gruplandır
                                .Select(group => new
                                {
                                    HeardFrom = group.Key,
                                    Count = group.Count(),
                                    TaskNames = group.Select(tc => tc.Title).ToList()
                                })
                                .ToList();

                var labels = chartData.Select(data => data.HeardFrom).ToArray();
                var dataValues = chartData.Select(data => data.Count).ToArray();
                var taskNames = chartData.Select(data => data.TaskNames).ToArray();

                return Json(new { labels, data = dataValues, taskNames });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HeardFrom chart verileri getirilirken bir hata oluştu.");
                return StatusCode(500, "İşleminiz sırasında bir hata oluştu.");
            }
        }


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
                                             .Select(group => new
                                             {
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
                taskNames
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
                                              .OrderBy(tc => tc.StatusId)
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
                                         .Select(group => new
                                         {
                                             StatusName = group.Key,
                                             TaskNames = group.Select(tc => tc.Title).Distinct().ToList(), // Görev isimlerini ayırt et
                                             Count = group.Select(tc => tc.Title).Distinct().Count() // Görev isimlerini ayırt et ve sayısını al
                                         }).ToList();

            return Json(chartData);
        }


        #endregion

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }
    }
}



//[Authorize]
//public async Task<IActionResult> ValueOfferChart()
//{
//    try
//    {
//        var userId = _userManager.GetUserId(User);
//        var user = await _context.Users
//                                 .Include(u => u.TaskComps)
//                                 .FirstOrDefaultAsync(u => u.Id == userId);

//        if (user == null)
//        {
//            return NotFound("Kullanıcı bulunamadı.");
//        }

//        // Kullanıcının AppUser ve AssignedUser olduğu görevleri birleştirelim
//        var taskCompsAsAppUser = await _context.TaskComps.Where(tc => tc.UserId == userId && tc.ValueOrOffer.HasValue).ToListAsync();
//        var taskCompsAsAssignedUser = await _context.TaskComps.Where(tc => tc.AssignedUserId == userId && tc.ValueOrOffer.HasValue).ToListAsync();
//        var allTaskComps = taskCompsAsAppUser.Concat(taskCompsAsAssignedUser).Distinct().ToList();

//        var ranges = new[]
//        {
//    new { Min = 0m, Max = 1000m, Label = "0-1000" },
//    new { Min = 1000m, Max = 2000m, Label = "1000-2000" },
//    new { Min = 2000m, Max = 3000m, Label = "2000-3000" },
//    new { Min = 3000m, Max = 4000m, Label = "3000-4000" },
//    new { Min = 4000m, Max = 5000m, Label = "4000-5000" },
//    new { Min = 5000m, Max = 6000m, Label = "5000-6000" },
//    new { Min = 6000m, Max = 7000m, Label = "6000-7000" },
//    new { Min = 7000m, Max = 100000m, Label = "7000-100000" }
//};

//        var chartData = ranges.Select(range => new
//        {
//            range.Label,
//            Count = allTaskComps
//                        .Where(tc => tc.ValueOrOffer.HasValue &&
//                                     tc.ValueOrOffer.Value >= range.Min &&
//                                     tc.ValueOrOffer.Value < range.Max)
//                        .Count(),
//            TaskNames = allTaskComps
//                        .Where(tc => tc.ValueOrOffer.HasValue &&
//                                     tc.ValueOrOffer.Value >= range.Min &&
//                                     tc.ValueOrOffer.Value < range.Max)
//                        .Select(tc => tc.Title)
//                        .ToList()
//        }).ToList();

//        var labels = chartData.Select(data => data.Label).ToArray();
//        var dataValues = chartData.Select(data => data.Count).ToArray();
//        var taskNames = chartData.Select(data => data.TaskNames).ToArray();

//        return Json(new { labels, data = dataValues, taskNames });
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "ValueOffer chart verileri getirilirken bir hata oluştu.");
//        return StatusCode(500, "İşleminiz sırasında bir hata oluştu.");
//    }
//}
