namespace CrmCorner.Models
{
    public class FinanceInvoiceDocument
    {
        public int Id { get; set; }

        public int FinanceInvoiceId { get; set; }

        public string FileName { get; set; }
        public string FilePath { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}

