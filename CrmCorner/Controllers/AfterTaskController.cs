using CrmCorner.Models;
using CrmCorner.Models.Enums;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    public class AfterTaskController : BaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;

        public AfterTaskController(UserManager<AppUser> userManager, CrmCornerContext context) : base(userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> ListPositiveSales()
        {
            await SetLayout();
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);
                var userEmailDomain = user.Email.Split('@')[1];

                ViewBag.PictureUrl = "/userprofilepicture/" + (user.Picture ?? "defaultpp.png");

                var tasksQuery = _context.PipelineTasks
       .Include(t => t.AppUser)
       .Include(t => t.Customer)
       .Include(t => t.PostSaleInfo)
       .Where(t => t.AppUser.Email.EndsWith(userEmailDomain))
       .Where(t => t.OutcomeStatus == OutcomeTypeSales.Won);

                if (!roles.Contains("Admin"))
                {
                    tasksQuery = tasksQuery.Where(t => t.AppUserId == user.Id || t.ResponsibleUserId == user.Id);
                }

                var result = await tasksQuery.Select(t => new SaleDTO
                {
                    Id = t.PostSaleInfo != null ? t.PostSaleInfo.Id : 0,
                    TaskCompId = t.Id,
                    TaskCompTitle = t.Title,
                    IsFirstPaymentMade = t.PostSaleInfo != null && t.PostSaleInfo.IsFirstPaymentMade,
                    IsThereAProblem = t.PostSaleInfo != null && t.PostSaleInfo.IsThereAProblem,
                    ProblemDescription = t.PostSaleInfo != null ? t.PostSaleInfo.ProblemDescription : "",
                    IsContinuationConsidered = t.PostSaleInfo != null && t.PostSaleInfo.IsContinuationConsidered,
                    IsTrustpilotReviewed = t.PostSaleInfo != null && t.PostSaleInfo.IsTrustpilotReviewed,
                    TrustPilotComment = t.PostSaleInfo != null ? t.PostSaleInfo.TrustPilotComment : "",
                    CanUseLogo = t.PostSaleInfo != null && t.PostSaleInfo.CanUseLogo
                }).ToListAsync();

                return View(result);
            }
            catch
            {
                return RedirectToAction("NotFound", "Error");
            }
        }



        public async Task<IActionResult> AfterTaskEdit(int? id)
        {
            try
            {
                if (id == null)
                    return NotFound();

                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);
                ViewBag.PictureUrl = "/userprofilepicture/" + (user.Picture ?? "defaultpp.png");

                var userEmailDomain = user.Email.Split('@')[1];

                var postSaleInfoQuery = _context.PostSaleInfos
                    .Include(p => p.PipelineTask)
                    .Where(p =>
                        p.PipelineTask.AppUser.Email.EndsWith(userEmailDomain) ||
                        p.PipelineTask.ResponsibleUserId == user.Id);

                if (!roles.Contains("Admin"))
                {
                    postSaleInfoQuery = postSaleInfoQuery.Where(p =>
                        p.PipelineTask.AppUserId == user.Id || p.PipelineTask.ResponsibleUserId == user.Id);
                }

                var postSaleInfo = await postSaleInfoQuery.FirstOrDefaultAsync(p => p.Id == id);

                if (postSaleInfo == null)
                    return NotFound();

                ViewBag.PipelineTaskId = postSaleInfo.PipelineTaskId;
                return View(postSaleInfo);
            }
            catch
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AfterTaskEdit(
            int id,
            [Bind("Id,PipelineTaskId,IsFirstPaymentMade,IsThereAProblem,ProblemDescription,IsContinuationConsidered,IsTrustpilotReviewed,TrustPilotComment,CanUseLogo")]
            PostSaleInfo model)
        {
            try
            {
                if (id != model.Id)
                    return NotFound();

                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);
                ViewBag.PictureUrl = "/userprofilepicture/" + (user.Picture ?? "defaultpp.png");
                var userEmailDomain = user.Email.Split('@')[1];

                var postSaleInfoQuery = _context.PostSaleInfos
                    .Include(p => p.PipelineTask)
                    .Where(p =>
                        p.PipelineTask.AppUser.Email.EndsWith(userEmailDomain) ||
                        p.PipelineTask.ResponsibleUserId == user.Id);

                if (!roles.Contains("Admin"))
                {
                    postSaleInfoQuery = postSaleInfoQuery.Where(p =>
                        p.PipelineTask.AppUserId == user.Id || p.PipelineTask.ResponsibleUserId == user.Id);
                }

                var postSaleInfo = await postSaleInfoQuery.FirstOrDefaultAsync(p => p.Id == id);
                if (postSaleInfo == null)
                    return NotFound();

                if (ModelState.IsValid)
                {
                    postSaleInfo.PipelineTaskId = model.PipelineTaskId;
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

                return RedirectToAction(nameof(ListPositiveSales));
            }
            catch
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        private bool PostSaleInfoExists(int id)
        {
            return _context.PostSaleInfos.Any(e => e.Id == id);
        }
    }
}
