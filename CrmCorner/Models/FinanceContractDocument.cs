using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models
{
    public class FinanceContractDocument
    {
        public int Id { get; set; }

        public int FinanceContractId { get; set; }

        [ForeignKey("FinanceContractId")]
        public FinanceContract Contract { get; set; }

        public string FileName { get; set; }
        public string FilePath { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}
