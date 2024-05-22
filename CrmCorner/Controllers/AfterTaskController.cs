using CrmCorner.Models;
using CrmCorner.Models.Enums;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    [Authorize]
    public class AfterTaskController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;
        public AfterTaskController(UserManager<AppUser> userManager, CrmCornerContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> ListPositiveSales()
        {
            try
            {
                var userId = _userManager.GetUserId(User); // Aktif kullanıcının UserId'sini alır

                var positiveSales = await _context.PostSaleInfos
                                                  .Include(psi => psi.TaskComp)
                                                  .ThenInclude(tc => tc.AppUser) // Satışı yapan kullanıcı bilgilerini dahil et
                                                  .Where(psi => psi.TaskComp.Outcomes == OutcomeType.Olumlu &&
                                                                psi.TaskComp.UserId == userId) // Satışı yapan kullanıcıya göre filtrele
                                                  .Select(psi => new SaleDTO
                                                  {
                                                      Id = psi.Id,
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

                return View(positiveSales);
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

                var postSaleInfo = await _context.PostSaleInfos
                                                 .Include(psi => psi.TaskComp) // İlişkili TaskComp bilgilerini de yükle
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

                var postSaleInfo = await _context.PostSaleInfos
                                                 .Include(p => p.TaskComp)
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
                    return RedirectToAction(nameof(ListPositiveSales));
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
