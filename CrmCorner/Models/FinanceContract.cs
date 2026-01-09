using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models
{
    public class FinanceContract
    {
        public int Id { get; set; }

        // Multi-company (senin AppUser'da CompanyId var demiştin)
        public int CompanyId { get; set; }

        public string CompanyName { get; set; }

        // Sözleşme
        public DateTime? ContractStartDate { get; set; }
        public int? ContractMonths { get; set; }
        public DateTime? ContractEndDate { get; set; } // DB'de tutacağız (UpdateField'de hesaplayacağız)

        // USD alanları (detaydan giriyorsun)
        public decimal? SaleAmountUsd { get; set; }
        public decimal? CommissionUsd { get; set; }
        public decimal? UsdRateAtSale { get; set; }

        // Kim sattı / SDR (FK olacak)
        public string? KimSattiUserId { get; set; }
        public string? SdrUserId { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // public AppUser KimSattiUser { get; set; }
        // public AppUser SdrUser { get; set; }
    }
}
