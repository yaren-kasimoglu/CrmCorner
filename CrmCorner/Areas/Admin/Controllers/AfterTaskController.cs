using CrmCorner.Controllers;
using CrmCorner.Models;
using CrmCorner.Models.Enums;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class AfterTaskController : BaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;
        public AfterTaskController(UserManager<AppUser> userManager, CrmCornerContext context) : base(userManager)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> ListPositiveSalesAdmin()
        {
            await SetLayout();
            try
            {
                var user = await _userManager.GetUserAsync(User); // Aktif kullanıcıyı al
                var userEmailDomain = user.Email.Split('@')[1]; // Kullanıcının e-posta domainini al
                var companyId = user.CompanyId; // Aktif kullanıcının şirket ID'si

                ViewBag.PictureUrl = "/userprofilepicture/" + (user.Picture ?? "defaultpp.png");

                // Başlıkları veritabanından kullanıcıya özel olarak yükle
                var headers = await _context.TableHeaders
                                            .Where(th => th.CompanyId == companyId)
                                            .ToDictionaryAsync(th => th.ColumnKey, th => th.ColumnName);

                ViewBag.Headers = headers; // ViewBag'e başlıkları ekle

                // Belirtilen şirketin tüm tasklarını listele
                var companyTasksQuery = _context.PostSaleInfos
                                                 .Include(psi => psi.TaskComp)
                                                     .ThenInclude(tc => tc.AppUser)
                                                 .Include(psi => psi.TaskComp.Status)
                                                 .Include(psi => psi.TaskComp.AssignedUser)
                                                 .Include(psi => psi.TaskComp.Customer)
                                                 .Where(psi => psi.TaskComp.AppUser.EmailDomain == userEmailDomain)
                                                 .Where(psi => psi.TaskComp.OutcomeStatus == OutcomeTypeSales.Won)
                                                 .Where(psi => psi.TaskComp.StatusId == 5);



                // DTO'ya projeksiyon yap
                var companyTasks = await companyTasksQuery
                                            .Select(psi => new SaleDTO
                                            {
                                                Id = psi.Id,
                                                TaskCompId = psi.TaskComp.TaskId,
                                                TaskCompTitle = psi.TaskComp.Title,
                                                IsFirstPaymentMade = psi.IsFirstPaymentMade,
                                                IsThereAProblem = psi.IsThereAProblem,
                                                ProblemDescription = psi.ProblemDescription,
                                                IsContinuationConsidered = psi.IsContinuationConsidered,
                                                IsTrustpilotReviewed = psi.IsTrustpilotReviewed,
                                                TrustPilotComment = psi.TrustPilotComment,
                                                CanUseLogo = psi.CanUseLogo
                                            })
                                            .ToListAsync();

                return View(companyTasks);
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }




        public async Task<IActionResult> AfterTaskEdit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User); // Kullanıcı nesnesini al
                ViewBag.PictureUrl = "/userprofilepicture/" + (user.Picture ?? "defaultpp.png");

                var userEmailDomain = user.Email.Split('@')[1]; // Kullanıcının e-posta domainini al

                var postSaleInfo = await _context.PostSaleInfos
                                                 .Include(psi => psi.TaskComp) // İlişkili TaskComp bilgilerini de yükle
                                                 .Where(psi => psi.TaskComp.AppUser.Email.EndsWith(userEmailDomain) || psi.TaskComp.AssignedUser.Email.EndsWith(userEmailDomain)) // E-posta domainine göre filtrele
                                                 .FirstOrDefaultAsync(m => m.Id == id);

                if (postSaleInfo == null)
                {
                    return NotFound();
                }

                // Gizli alan için TaskCompId'yi view modelinde veya ViewBag'de taşı
                ViewBag.TaskCompId = postSaleInfo.TaskCompId;

                return View(postSaleInfo);
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AfterTaskEdit(int id, [Bind("Id,TaskCompId,IsFirstPaymentMade,IsThereAProblem,ProblemDescription,IsContinuationConsidered,IsTrustpilotReviewed,TrustPilotComment,CanUseLogo")] PostSaleInfo model)
        {
            try
            {
                if (id != model.Id)
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User); // Kullanıcı nesnesini al
                ViewBag.PictureUrl = "/userprofilepicture/" + (user.Picture ?? "defaultpp.png");

                var userEmailDomain = user.Email.Split('@')[1]; // Kullanıcının e-posta domainini al

                var postSaleInfo = await _context.PostSaleInfos
                                                 .Include(p => p.TaskComp)
                                                 .Where(p => p.TaskComp.AppUser.Email.EndsWith(userEmailDomain) || p.TaskComp.AssignedUser.Email.EndsWith(userEmailDomain)) // E-posta domainine göre filtrele
                                                 .FirstOrDefaultAsync(m => m.Id == id);

                if (postSaleInfo == null)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    // Güncelleme işlemi için alanları kopyala
                    postSaleInfo.TaskCompId = model.TaskCompId;
                    postSaleInfo.IsFirstPaymentMade = model.IsFirstPaymentMade;
                    postSaleInfo.IsThereAProblem = model.IsThereAProblem;
                    postSaleInfo.ProblemDescription = model.ProblemDescription;
                    postSaleInfo.IsContinuationConsidered = model.IsContinuationConsidered;
                    postSaleInfo.IsTrustpilotReviewed = model.IsTrustpilotReviewed;
                    postSaleInfo.TrustPilotComment = model.TrustPilotComment;
                    postSaleInfo.CanUseLogo = model.CanUseLogo;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ListPositiveSalesAdmin));
                }

                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }




        private bool PostSaleInfoExists(int id)
        {
            return _context.PostSaleInfos.Any(e => e.Id == id);
        }

        //public async Task<IActionResult> AddPostSaleInfo(int taskId)
        //{
        //    var task = await _context.TaskComps
        //                             .Include(t => t.Status)
        //                             .Include(t => t.AppUser)
        //                             .Include(t => t.AssignedUser)
        //                             .Include(t => t.Customer)
        //                             .FirstOrDefaultAsync(t => t.TaskId == taskId && t.Outcomes == OutcomeType.Olumlu);

        //    if (task == null)
        //    {
        //        return NotFound();
        //    }

        //    var postSaleInfo = new PostSaleInfo
        //    {
        //        TaskCompId = taskId
        //    };

        //    return View(postSaleInfo);
        //}

        //[HttpPost]
        //public async Task<IActionResult> AddPostSaleInfo(PostSaleInfo postSaleInfo)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(postSaleInfo);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction("PositiveTasks"); // Kazanılan görevler listesine yönlendirme
        //    }

        //    return View(postSaleInfo); // Validasyon hatası varsa formu tekrar göster
        //}



    }
}
