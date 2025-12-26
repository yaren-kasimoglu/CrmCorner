using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models
{
    public class FinanceInvoice
    {
        public int Id { get; set; }

        public int PeriodYear { get; set; }
        public byte PeriodMonth { get; set; } // 1-12

        public string CompanyName { get; set; }

        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }

        public decimal ExpectedNet { get; set; }
        public decimal VatRate { get; set; }   // 0.2, 0.1, 0.08, 0.01, 0
        public decimal VatAmount { get; set; }
        public decimal GrossAmount { get; set; }

        public decimal PaidAmount { get; set; }
        public decimal? ProfitLoss { get; set; }

        public string Status { get; set; } // Taslak, Orijinal, Tahsil Edildi, Beklemede, Problemli...
        public string ProblemReason { get; set; }
        public string Note { get; set; }

        public DateTime? LastReminderAt { get; set; }

        // ===== USD BAZLI SATIŞ =====
        public decimal? SaleAmountUsd { get; set; }        // Örn: 500$
        public decimal? CommissionUsd { get; set; }        // Hak ediş (USD)
        public decimal? UsdRateAtSale { get; set; }        // Opsiyonel kur

        // ===== SÖZLEŞME =====
        public int? ContractMonths { get; set; }           // Kaç aylık
        public DateTime? ContractStartDate { get; set; }   // Başlangıç
        public DateTime? ContractEndDate { get; set; }     // Bitiş (hesaplanabilir)



        public string? KimSattiUserId { get; set; }
        public string? SdrUserId { get; set; }

        [ForeignKey("KimSattiUserId")]
        public AppUser KimSattiUser { get; set; }

        [ForeignKey("SdrUserId")]
        public AppUser SdrUser { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

