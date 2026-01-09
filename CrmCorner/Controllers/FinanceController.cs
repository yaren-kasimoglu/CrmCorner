using CrmCorner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;


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

        public async Task<IActionResult> Invoices(int? year, int? month)
        {
            int selectedYear = year ?? DateTime.Now.Year;
            int selectedMonth = month ?? DateTime.Now.Month;

            // 🔐 Login olan kullanıcı
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            if (currentUser.CompanyId == null)
            {
                ViewBag.TeamUsers = new List<SelectListItem>();
                ViewBag.SelectedYear = selectedYear;
                ViewBag.SelectedMonth = selectedMonth;
                return View(new List<FinanceInvoice>());
            }

            int companyId = currentUser.CompanyId;

            // ✅ Aynı şirketteki kişiler dropdown
            ViewBag.TeamUsers = await _userManager.Users
                .Where(u => u.CompanyId == companyId)
                .OrderBy(u => u.NameSurname)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.NameSurname
                })
                .ToListAsync();

            // ✅ Ay aralığı (kesişim kontrolü için)
            var monthStart = new DateTime(selectedYear, selectedMonth, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // ✅ Bu ay ile kesişen sözleşmeler (Aralık'ta başlayıp Şubat'ta biten => Aralık/Ocak/Şubat dahil)
            var activeContracts = await _context.FinanceContracts
                .Where(c =>
                    c.CompanyId == companyId &&
                    c.ContractStartDate.HasValue &&
                    c.ContractEndDate.HasValue &&
                    c.ContractStartDate.Value <= monthEnd &&
                    c.ContractEndDate.Value >= monthStart
                )
                .ToListAsync();

            // ✅ Bu ayın mevcut invoice'ları (company filtreli)
            var existingInvoices = await _context.FinanceInvoices
                .Where(i =>
                    i.CompanyId == companyId &&
                    i.PeriodYear == selectedYear &&
                    i.PeriodMonth == (byte)selectedMonth
                )
                .ToListAsync();

            // ✅ Eksik sözleşmeler için bu aya invoice oluştur
            bool addedAny = false;

            foreach (var contract in activeContracts)
            {
                bool exists = existingInvoices.Any(i => i.ContractId == contract.Id);

                if (!exists)
                {
                    _context.FinanceInvoices.Add(new FinanceInvoice
                    {
                        CompanyId = companyId,
                        ContractId = contract.Id,

                        PeriodYear = selectedYear,
                        PeriodMonth = (byte)selectedMonth,

                        // Finans defaultlar
                        ExpectedNet = 0,
                        VatRate = 0,
                        VatAmount = 0,
                        GrossAmount = 0,
                        PaidAmount = 0,

                        Status = "Taslak",
                        ProblemReason = "",
                        Note = "",

                        LastReminderAt = null,

                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });

                    addedAny = true;
                }
            }

            if (addedAny)
                await _context.SaveChangesAsync();

            // ✅ Ekrana basılacak liste (Contract include)
            var invoices = await _context.FinanceInvoices
                .Include(x => x.Contract)
                .Where(i =>
                    i.CompanyId == companyId &&
                    i.PeriodYear == selectedYear &&
                    i.PeriodMonth == (byte)selectedMonth
                )
                // ✅ Sıralama Contract şirket adına göre
                .OrderBy(i => i.Contract != null ? i.Contract.CompanyName : "")
                .ToListAsync();

            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedMonth = selectedMonth;

            return View(invoices);
        }


        public async Task<IActionResult> InvoiceDetail(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.CompanyId == null) return Forbid();

            var invoice = await _context.FinanceInvoices
                .Include(x => x.Contract)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (invoice == null) return NotFound();

            if (invoice.CompanyId != currentUser.CompanyId)
                return Forbid();

            ViewBag.InvoiceDocuments = await _context.FinanceInvoiceDocuments
                .Where(d => d.FinanceInvoiceId == id)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            if (invoice.ContractId.HasValue)
            {
                ViewBag.ContractDocuments = await _context.FinanceContractDocuments
                    .Where(d => d.FinanceContractId == invoice.ContractId.Value)
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();
            }
            else
            {
                ViewBag.ContractDocuments = new List<FinanceContractDocument>();
            }

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
        public async Task<IActionResult> UpdateField(int id, string field, string value)
        {
            // ✅ Invoice + Contract birlikte çek
            var inv = await _context.FinanceInvoices
                .Include(x => x.Contract)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (inv == null)
                return NotFound();

            // ✅ Login user company kontrolü
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.CompanyId == null)
                return Forbid();

            if (inv.CompanyId != currentUser.CompanyId)
                return Forbid();

            // ✅ Contract'ı güvenli şekilde resolve et (fallback dahil)
            FinanceContract contract = inv.Contract;

            // Eğer include ile gelmediyse ama ContractId var ise DB'den çek
            if (contract == null && inv.ContractId.HasValue)
            {
                contract = await _context.FinanceContracts.FirstOrDefaultAsync(x => x.Id == inv.ContractId.Value);
                inv.Contract = contract;
            }

            // Eğer hiç contract yoksa (eski/test data), otomatik oluştur ve invoice'a bağla
            if (contract == null && !inv.ContractId.HasValue)
            {
                contract = new FinanceContract
                {
                    CompanyId = inv.CompanyId,
                    CompanyName = "Yeni Firma",

                    ContractStartDate = null,
                    ContractMonths = null,
                    ContractEndDate = null,

                    SaleAmountUsd = null,
                    CommissionUsd = null,
                    UsdRateAtSale = null,

                    KimSattiUserId = null,
                    SdrUserId = null,

                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.FinanceContracts.Add(contract);
                await _context.SaveChangesAsync(); // contract.Id lazım

                inv.ContractId = contract.Id;
                inv.Contract = contract;
            }

            bool contractChanged = false;

            switch (field)
            {
                // ===== INVOICE ALANLARI =====
                case "ExpectedNet":
                    inv.ExpectedNet = ParseMoneyTR(value);
                    break;

                case "VatRate":
                    {
                        var r = ParseRate(value);
                        if (r > 1) r = r / 100m; // 20 => 0.20
                        inv.VatRate = r;
                        break;
                    }

                case "PaidAmount":
                    inv.PaidAmount = ParseMoneyTR(value);
                    break;

                case "Status":
                    inv.Status = value ?? "";
                    break;

                case "ProblemReason":
                    inv.ProblemReason = value ?? "";
                    break;

                case "Note":
                    inv.Note = value ?? "";
                    break;

                // ===== CONTRACT ALANLARI =====
                case "ContractCompanyName":
                    if (contract == null) return BadRequest("Contract not found for this invoice.");
                    contract.CompanyName = value ?? "";
                    contractChanged = true;
                    break;

                case "ContractKimSattiUserId":
                    if (contract == null) return BadRequest("Contract not found for this invoice.");
                    contract.KimSattiUserId = string.IsNullOrWhiteSpace(value) ? null : value;
                    contractChanged = true;
                    break;

                case "ContractSdrUserId":
                    if (contract == null) return BadRequest("Contract not found for this invoice.");
                    contract.SdrUserId = string.IsNullOrWhiteSpace(value) ? null : value;
                    contractChanged = true;
                    break;

                case "ContractMonths":
                    if (contract == null) return BadRequest("Contract not found for this invoice.");
                    contract.ContractMonths = int.TryParse(value, out var m) ? m : (int?)null;
                    contractChanged = true;
                    break;

                case "ContractStartDate":
                    if (contract == null) return BadRequest("Contract not found for this invoice.");

                    // input[type=date] => yyyy-MM-dd
                    if (DateTime.TryParseExact(value, "yyyy-MM-dd",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
                    {
                        contract.ContractStartDate = start;
                    }
                    else
                    {
                        contract.ContractStartDate = null;
                    }

                    contractChanged = true;
                    break;

                case "ContractSaleAmountUsd":
                    if (contract == null) return BadRequest("Contract not found for this invoice.");
                    contract.SaleAmountUsd = ParseMoneyTR(value);
                    contractChanged = true;
                    break;

                case "ContractCommissionUsd":
                    if (contract == null) return BadRequest("Contract not found for this invoice.");
                    contract.CommissionUsd = ParseMoneyTR(value);
                    contractChanged = true;
                    break;

                case "ContractUsdRateAtSale":
                    if (contract == null) return BadRequest("Contract not found for this invoice.");
                    contract.UsdRateAtSale = ParseMoneyTR(value);
                    contractChanged = true;
                    break;

                default:
                    return BadRequest("Unknown field");
            }

            // ===== INVOICE HESAPLAMA =====
            inv.VatAmount = inv.ExpectedNet * inv.VatRate;
            inv.GrossAmount = inv.ExpectedNet + inv.VatAmount;
            inv.UpdatedAt = DateTime.Now;

            // ===== CONTRACT END DATE + AYLIK SATIR ÜRETME =====
            string contractEndDateForJson = null;
            string contractEndDateTextForJson = null;

            if (contractChanged && contract != null)
            {
                if (contract.ContractStartDate.HasValue && contract.ContractMonths.HasValue)
                {
                    // ✅ dahil bitiş (Şubat da görünsün)
                    contract.ContractEndDate = contract.ContractStartDate.Value
                        .AddMonths(contract.ContractMonths.Value)
                        .AddDays(-1);
                }
                else
                {
                    contract.ContractEndDate = null;
                }

                contract.UpdatedAt = DateTime.Now;

                // ✅ sözleşme ayları boyunca invoice satırları garanti
                EnsureInvoicesForContract(contract);

                contractEndDateForJson = contract.ContractEndDate?.ToString("yyyy-MM-dd");
                contractEndDateTextForJson = contract.ContractEndDate?.ToString("dd.MM.yyyy");
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                vatAmount = inv.VatAmount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")),
                grossAmount = inv.GrossAmount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")),
                contractEndDate = contractEndDateForJson,
                contractEndDateText = contractEndDateTextForJson,
                contractCompanyName = contract?.CompanyName // istersen UI'da başlığı anlık güncellersin
            });
        }



        // ✅ Contract süresi boyunca (ay bazında) invoice satırlarını garanti altına alır
        private void EnsureInvoicesForContract(FinanceContract c)
        {
            if (c == null) return;
            if (c.CompanyId <= 0) return;
            if (!c.ContractStartDate.HasValue) return;

            DateTime? end = c.ContractEndDate;

            if (!end.HasValue && c.ContractMonths.HasValue)
                end = c.ContractStartDate.Value.AddMonths(c.ContractMonths.Value).AddDays(-1);

            if (!end.HasValue) return;

            var cursor = new DateTime(c.ContractStartDate.Value.Year, c.ContractStartDate.Value.Month, 1);
            var last = new DateTime(end.Value.Year, end.Value.Month, 1);

            // ✅ tek seferde çek
            var existingKeys = _context.FinanceInvoices
                .Where(i => i.CompanyId == c.CompanyId && i.ContractId == c.Id)
                .Select(i => new { i.PeriodYear, i.PeriodMonth })
                .ToList()
                .Select(x => $"{x.PeriodYear}-{x.PeriodMonth}")
                .ToHashSet();

            while (cursor <= last)
            {
                int y = cursor.Year;
                byte m = (byte)cursor.Month;

                var key = $"{y}-{m}";
                if (!existingKeys.Contains(key))
                {
                    _context.FinanceInvoices.Add(new FinanceInvoice
                    {
                        CompanyId = c.CompanyId,
                        ContractId = c.Id,
                        PeriodYear = y,
                        PeriodMonth = m,

                        ExpectedNet = 0,
                        VatRate = 0,
                        VatAmount = 0,
                        GrossAmount = 0,
                        PaidAmount = 0,

                        Status = "Taslak",
                        ProblemReason = "",
                        Note = "",
                        LastReminderAt = null,

                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });

                    existingKeys.Add(key);
                }

                cursor = cursor.AddMonths(1);
            }
        }

        // CREATE EMPTY ROW (Contract + Invoice)
        [HttpPost]
        public async Task<IActionResult> CreateRow(int year, int month)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            if (currentUser.CompanyId == null)
                return BadRequest("CompanyId bulunamadı.");

            int companyId = currentUser.CompanyId;

            // 1) Önce Contract oluştur (master)
            var contract = new FinanceContract
            {
                CompanyId = companyId,
                CompanyName = "Yeni Firma",

                ContractStartDate = null,
                ContractMonths = null,
                ContractEndDate = null,

                SaleAmountUsd = null,
                CommissionUsd = null,
                UsdRateAtSale = null,

                KimSattiUserId = null, // istersen currentUser.Id verebilirsin
                SdrUserId = null,

                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.FinanceContracts.Add(contract);
            await _context.SaveChangesAsync(); // ✅ Contract.Id lazım

            // 2) Seçili ay için Invoice oluştur (period record)
            var inv = new FinanceInvoice
            {
                CompanyId = companyId,
                ContractId = contract.Id,

                PeriodYear = year,
                PeriodMonth = (byte)month,

                ExpectedNet = 0,
                VatRate = 0.20m,   // istersen 0 da yapabilirsin
                VatAmount = 0,
                GrossAmount = 0,

                PaidAmount = 0,

                Status = "Taslak",
                ProblemReason = "",
                Note = "",

                LastReminderAt = null,

                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.FinanceInvoices.Add(inv);
            await _context.SaveChangesAsync();

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

        [HttpPost]
        public async Task<IActionResult> UploadContractDocument(int invoiceId, int contractId, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest();

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contract-documents");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            var savedFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var fullPath = Path.Combine(uploadPath, savedFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(stream);

            var doc = new FinanceContractDocument
            {
                FinanceContractId = contractId,
                FileName = file.FileName,
                FilePath = "/uploads/contract-documents/" + savedFileName,
                UploadedAt = DateTime.Now
            };

            _context.FinanceContractDocuments.Add(doc);

            var contract = await _context.FinanceContracts.FirstOrDefaultAsync(x => x.Id == contractId);
            if (contract != null)
            {
                var extractedName = ExtractCustomerNameFromContractFileName(file.FileName);

                // ✅ çektiyse yaz, çekmediyse dokunma
                if (!string.IsNullOrWhiteSpace(extractedName))
                {
                    contract.CompanyName = extractedName;
                    contract.UpdatedAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("InvoiceDetail", new { id = invoiceId });
        }


        [HttpPost]
        public IActionResult DeleteContractDocument(int id, int invoiceId)
        {
            var doc = _context.FinanceContractDocuments.FirstOrDefault(x => x.Id == id);
            if (doc == null) return NotFound();

            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", doc.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(physicalPath)) System.IO.File.Delete(physicalPath);

            _context.FinanceContractDocuments.Remove(doc);
            _context.SaveChanges();

            return RedirectToAction("InvoiceDetail", new { id = invoiceId });
        }


        [HttpPost]
        public async Task<IActionResult> DeleteContract(int? contractId, int returnYear, int returnMonth)
        {
            if (!contractId.HasValue || contractId.Value <= 0)
                return BadRequest("ContractId boş.");

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.CompanyId == null) return Forbid();

            var contract = await _context.FinanceContracts
                .FirstOrDefaultAsync(x => x.Id == contractId.Value);

            if (contract == null) return NotFound();

            // ✅ Multi-company güvenlik
            if (contract.CompanyId != currentUser.CompanyId)
                return Forbid();

            // 1) Contract'a bağlı tüm invoice'ları çek
            var invoices = await _context.FinanceInvoices
                .Where(i => i.ContractId == contract.Id)
                .ToListAsync();

            var invoiceIds = invoices.Select(i => i.Id).ToList();

            // 2) Invoice belgeleri (tüm aylar)
            var invoiceDocs = await _context.FinanceInvoiceDocuments
                .Where(d => invoiceIds.Contains(d.FinanceInvoiceId))
                .ToListAsync();

            foreach (var d in invoiceDocs)
                TryDeletePhysicalFile(d.FilePath);

            _context.FinanceInvoiceDocuments.RemoveRange(invoiceDocs);

            // 3) Contract belgeleri
            var contractDocs = await _context.FinanceContractDocuments
                .Where(d => d.FinanceContractId == contract.Id)
                .ToListAsync();

            foreach (var d in contractDocs)
                TryDeletePhysicalFile(d.FilePath);

            _context.FinanceContractDocuments.RemoveRange(contractDocs);

            // 4) Invoice satırlarını sil
            _context.FinanceInvoices.RemoveRange(invoices);

            // 5) Contract'ı sil
            _context.FinanceContracts.Remove(contract);

            await _context.SaveChangesAsync();

            return RedirectToAction("Invoices", new { year = returnYear, month = returnMonth });
        }

        private void TryDeletePhysicalFile(string relativePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(relativePath)) return;

                var physicalPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    relativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                );

                if (System.IO.File.Exists(physicalPath))
                    System.IO.File.Delete(physicalPath);
            }
            catch
            {
                // loglamak istersen buraya koyabiliriz; kullanıcıyı bloklamasın
            }
        }


        //HELPER METHODS

        private static string NormalizeForSearch(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";

            // Unicode normalize (combining karakterleri ayır)
            var normalized = input.Normalize(NormalizationForm.FormD);

            // Diacritics temizle (Ş, İ vb. gibi karmaşayı azaltır)
            var sb = new StringBuilder();
            foreach (var ch in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private string ExtractCustomerNameFromContractFileName(string originalFileName)
        {
            if (string.IsNullOrWhiteSpace(originalFileName)) return null;

            var name = Path.GetFileNameWithoutExtension(originalFileName).Trim();

            // "(1) (1) (1)" gibi ekleri temizle
            name = Regex.Replace(name, @"(\s*\(\d+\))+(\s*)$", "", RegexOptions.IgnoreCase).Trim();

            // Unicode normalize + diacritics temizle
            var clean = NormalizeForSearch(name);

            // ✅ Asıl kural: "& SAAS Corner" varsa, & öncesi müşteri adıdır
            // Örn: "Abeille.ai & SAAS Corner DANISMANLIK VE HIZMET SOZLESMESI"
            var match = Regex.Match(
                clean,
                @"^(?<customer>.+?)\s*&\s*SAAS\s*Corner\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
            );

            if (!match.Success) return null;

            var customer = match.Groups["customer"].Value.Trim();

            // Son güvenlik temizliği
            customer = customer.Trim('-', '–', '—').Trim();

            return customer.Length >= 2 ? customer : null;
        }

    }
}
