namespace CrmCorner.ViewModels
{
    public class SaleDTO
    {
        public int Id { get; set; }
        public int TaskCompId { get; set; } // Eğer yeni alan eklerseniz
        public string TaskCompTitle { get; set; }
        public bool IsFirstPaymentMade { get; set; }
        public bool IsThereAProblem { get; set; }
        public string? ProblemDescription { get; set; }
        public bool IsContinuationConsidered { get; set; }
        public bool IsTrustpilotReviewed { get; set; }
        public string? TrustPilotComment { get; set; }
        public bool CanUseLogo { get; set; }
    }
}
