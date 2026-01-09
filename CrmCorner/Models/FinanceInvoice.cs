using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models
{
    public class FinanceInvoice
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }

        public int PeriodYear { get; set; }
        public byte PeriodMonth { get; set; }

        public int? ContractId { get; set; }
        [ForeignKey("ContractId")]
        public FinanceContract Contract { get; set; }

        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }

        public decimal ExpectedNet { get; set; }
        public decimal VatRate { get; set; }
        public decimal VatAmount { get; set; }
        public decimal GrossAmount { get; set; }

        public decimal PaidAmount { get; set; }
        public decimal? ProfitLoss { get; set; }

        public string Status { get; set; }
        public string ProblemReason { get; set; }
        public string Note { get; set; }

        public DateTime? LastReminderAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}

