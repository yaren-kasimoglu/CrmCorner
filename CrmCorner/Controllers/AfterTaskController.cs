using CrmCorner.Models;
using CrmCorner.Models.Enums;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]
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
                var companyId = user.CompanyId;

                ViewBag.PictureUrl = "/userprofilepicture/" + (user.Picture ?? "defaultpp.png");

                var positiveSalesQuery = _context.PostSaleInfos
                    .Include(psi => psi.TaskComp)
                        .ThenInclude(tc => tc.AppUser)
                    .Include(psi => psi.TaskComp.Status)
                    .Include(psi => psi.TaskComp.AssignedUser)
                    .Include(psi => psi.TaskComp.Customer)
                    .Where(psi => psi.TaskComp.AppUser.EmailDomain == userEmailDomain)
                    .Where(psi => psi.TaskComp.OutcomeStatus == OutcomeTypeSales.Won)
                    .Where(psi => psi.TaskComp.StatusId == 5);

                if (!roles.Contains("Admin"))
                {
                    positiveSalesQuery = positiveSalesQuery.Where(psi => psi.TaskComp.UserId == user.Id || psi.TaskComp.AssignedUserId == user.Id);
                }

                var positiveSales = await positiveSalesQuery
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

                return View(positiveSales);
            }
            catch (Exception)
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

                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);
                ViewBag.PictureUrl = "/userprofilepicture/" + (user.Picture ?? "defaultpp.png");

                var userEmailDomain = user.Email.Split('@')[1];

                var postSaleInfoQuery = _context.PostSaleInfos
                    .Include(psi => psi.TaskComp)
                    .Where(psi => psi.TaskComp.AppUser.Email.EndsWith(userEmailDomain) || psi.TaskComp.AssignedUser.Email.EndsWith(userEmailDomain));

                if (!roles.Contains("Admin"))
                {
                    var userId = user.Id;
                    postSaleInfoQuery = postSaleInfoQuery.Where(psi => psi.TaskComp.UserId == userId || psi.TaskComp.AssignedUserId == userId);
                }

                var postSaleInfo = await postSaleInfoQuery.FirstOrDefaultAsync(m => m.Id == id);

                if (postSaleInfo == null)
                {
                    return NotFound();
                }

                ViewBag.TaskCompId = postSaleInfo.TaskCompId;

                return View(postSaleInfo);
            }
            catch (Exception)
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

                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);
                ViewBag.PictureUrl = "/userprofilepicture/" + (user.Picture ?? "defaultpp.png");

                var userEmailDomain = user.Email.Split('@')[1];

                var postSaleInfoQuery = _context.PostSaleInfos
                    .Include(p => p.TaskComp)
                    .Where(p => p.TaskComp.AppUser.Email.EndsWith(userEmailDomain) || p.TaskComp.AssignedUser.Email.EndsWith(userEmailDomain));

                if (!roles.Contains("Admin"))
                {
                    var userId = user.Id;
                    postSaleInfoQuery = postSaleInfoQuery.Where(p => p.TaskComp.UserId == userId || p.TaskComp.AssignedUserId == userId);
                }

                var postSaleInfo = await postSaleInfoQuery.FirstOrDefaultAsync(m => m.Id == id);

                if (postSaleInfo == null)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
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
            catch (Exception)
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
