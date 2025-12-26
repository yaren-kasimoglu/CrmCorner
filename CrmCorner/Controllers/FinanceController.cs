using CrmCorner.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;


namespace CrmCorner.Controllers
{
    [Authorize]
    public class FinanceController : Controller
    {
        private readonly CrmCornerContext _context;

        private readonly UserManager<AppUser> _userManager;

        public FinanceController(
            CrmCornerContext context,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // LIST

        public IActionResult Invoices(int? year, int? month)
        {
            int selectedYear = year ?? DateTime.Now.Year;
            int selectedMonth = month ?? DateTime.Now.Month;
            // 🔐 Login olan kullanıcı
            var currentUser = _userManager.GetUserAsync(User).Result;

            if (currentUser == null || currentUser.CompanyId == null)
            {
                // fallback: boş liste
                ViewBag.TeamUsers = new List<SelectListItem>();
            }
            else
            {
                ViewBag.TeamUsers = _userManager.Users
                    .Where(u => u.CompanyId == currentUser.CompanyId)
                    .OrderBy(u => u.NameSurname)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.NameSurname
                    })
                    .ToList();
            }


            var invoices = _context.FinanceInvoices
                .Where(x => x.PeriodYear == selectedYear && x.PeriodMonth == selectedMonth)
                .OrderBy(x => x.CompanyName)
                .ToList();

            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedMonth = selectedMonth;

            return View(invoices);
        }

        public IActionResult InvoiceDetail(int id)
        {
            var invoice = _context.FinanceInvoices
                .FirstOrDefault(x => x.Id == id);

            if (invoice == null)
                return NotFound();

            ViewBag.Documents = _context.FinanceInvoiceDocuments
                .Where(d => d.FinanceInvoiceId == id)
                .OrderByDescending(d => d.UploadedAt)
                .ToList();

            return View(invoice);
        }


        // PARSE HELPERS

        // "35.000,00" → 35000
        private decimal ParseMoneyTR(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return 0;

            if (decimal.TryParse(
                s,
                NumberStyles.Any,
                CultureInfo.GetCultureInfo("tr-TR"),
                out var tr))
            {
                return tr;
            }

            // fallback: "35000.00"
            if (decimal.TryParse(
                s.Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var inv))
            {
                return inv;
            }

            return 0;
        }

        // "0,2" / "0.2"
        private decimal ParseRate(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return 0;

            s = s.Replace(",", ".");

            if (decimal.TryParse(
                s,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var d))
            {
                return d;
            }

            return 0;
        }

        // INLINE UPDATE
        [HttpPost]
        public IActionResult UpdateField(int id, string field, string value)
        {
            var inv = _context.FinanceInvoices.FirstOrDefault(x => x.Id == id);
            if (inv == null)
                return NotFound();

            switch (field)
            {
                case "ExpectedNet":
                    inv.ExpectedNet = ParseMoneyTR(value);
                    break;

                case "VatRate":
                    inv.VatRate = ParseRate(value);
                    break;

                case "PaidAmount":
                    inv.PaidAmount = ParseMoneyTR(value);
                    break;

                case "Status":
                    inv.Status = value ?? "";
                    break;

                case "ProblemReason":
                    inv.ProblemReason = value ?? "";
                    break;

                case "CompanyName":
                    inv.CompanyName = value ?? "";
                    break;

                case "Note":
                    inv.Note = value ?? "";
                    break;

                case "KimSattiUserId":
                    inv.KimSattiUserId = string.IsNullOrWhiteSpace(value) ? null : value;
                    break;

                case "SdrUserId":
                    inv.SdrUserId = string.IsNullOrWhiteSpace(value) ? null : value;
                    break;

                case "SaleAmountUsd":
                    inv.SaleAmountUsd = ParseMoneyTR(value);
                    break;

                case "CommissionUsd":
                    inv.CommissionUsd = ParseMoneyTR(value);
                    break;

                case "UsdRateAtSale":
                    inv.UsdRateAtSale = ParseMoneyTR(value);
                    break;

                case "ContractMonths":
                    inv.ContractMonths = int.TryParse(value, out var m) ? m : null;
                    break;

                case "ContractStartDate":
                    if (DateTime.TryParse(value, out var start))
                        inv.ContractStartDate = start;
                    break;

                default:
                    return BadRequest("Unknown field");
            }

            // 🔄 Recalculate (NET mantığı doğru)
            inv.VatAmount = inv.ExpectedNet * inv.VatRate;
            inv.GrossAmount = inv.ExpectedNet + inv.VatAmount;
          

            // 🔁 Sözleşme bitişini hesapla
            if (inv.ContractStartDate.HasValue && inv.ContractMonths.HasValue)
            {
                inv.ContractEndDate = inv.ContractStartDate.Value
                    .AddMonths(inv.ContractMonths.Value);
            }
            else
            {
                inv.ContractEndDate = null;
            }

            inv.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            return Json(new
            {
                vatAmount = inv.VatAmount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")),
                grossAmount = inv.GrossAmount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")),
                contractEndDate = inv.ContractEndDate?.ToString("yyyy-MM-dd"), // input[type=date] için
                contractEndDateText = inv.ContractEndDate?.ToString("dd.MM.yyyy") // ekranda göstermek için
            });

        }

        // CREATE EMPTY ROW
        [HttpPost]
        public IActionResult CreateRow(int year, int month)
        {
            var inv = new FinanceInvoice
            {
                PeriodYear = year,
                PeriodMonth = (byte)month,

                CompanyName = "Yeni Firma",
                InvoiceNo = null,
                InvoiceDate = null,

                ExpectedNet = 0,
                VatRate = 0.20m,
                VatAmount = 0,
                GrossAmount = 0,

                PaidAmount = 0,
                ProfitLoss = null,

                Status = "Taslak",
                ProblemReason = "",
                Note = "",

                LastReminderAt = null,

                // UserId alanları nullable → şimdilik boş
                KimSattiUserId = null,
                SdrUserId = null,

                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.FinanceInvoices.Add(inv);
            _context.SaveChanges();

            return RedirectToAction("Invoices", new { year, month });
        }

        [HttpPost]
        public async Task<IActionResult> UploadInvoiceDocument(int invoiceId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest();

            var uploadPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "invoice-documents"
            );

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var savedFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var fullPath = Path.Combine(uploadPath, savedFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var doc = new FinanceInvoiceDocument
            {
                FinanceInvoiceId = invoiceId,
                FileName = file.FileName,
                FilePath = "/uploads/invoice-documents/" + savedFileName,
                UploadedAt = DateTime.Now
            };

            _context.FinanceInvoiceDocuments.Add(doc);
            await _context.SaveChangesAsync();

            return RedirectToAction("InvoiceDetail", new { id = invoiceId });
        }

        [HttpPost]
        public IActionResult DeleteInvoiceDocument(int id)
        {
            var doc = _context.FinanceInvoiceDocuments
                .FirstOrDefault(x => x.Id == id);

            if (doc == null)
                return NotFound();

            // Fiziksel dosyayı sil
            var physicalPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                doc.FilePath.TrimStart('/')
            );

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }

            int invoiceId = doc.FinanceInvoiceId;

            _context.FinanceInvoiceDocuments.Remove(doc);
            _context.SaveChanges();

            return RedirectToAction("InvoiceDetail", new { id = invoiceId });
        }


    }
}
