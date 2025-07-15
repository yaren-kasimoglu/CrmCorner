using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrmCorner.Models;

namespace CrmCorner.Controllers
{
    //[Authorize]
    public class HeaderController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;

        public HeaderController(UserManager<AppUser> userManager, CrmCornerContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User); // Mevcut kullanıcıyı al

            var companyId = user.CompanyId; // Kullanıcının şirket ID'sini al

            // Şirket ID'si 9 olan tüm başlıkları getir
            var headers = await _context.TableHeaders
                                        .Where(th => th.CompanyId == companyId)
                                        .ToListAsync();

            if (!headers.Any())
            {
                ViewBag.Message = "Bu şirket için başlık bulunamadı.";
            }

            return View(headers);
        }

        // GET: Header/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var header = await _context.TableHeaders.FindAsync(id);
            if (header == null)
            {
                return NotFound();
            }

            return View(header);
        }

        // POST: Header/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ColumnKey,ColumnName,CompanyId")] TableHeader header)
        {
            if (id != header.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(header);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TableHeaderExists(header.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(header);
        }

        private bool TableHeaderExists(int id)
        {
            return _context.TableHeaders.Any(e => e.Id == id);
        }

    }
}
