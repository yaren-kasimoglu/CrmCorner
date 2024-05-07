namespace CrmCorner.Models
{
    public class PostSaleInfo
    {
        public int Id { get; set; }
        public int TaskCompId { get; set; } // TaskComp ile ilişkilendirme
        public virtual TaskComp TaskComp { get; set; }

        public bool IsFirstPaymentMade { get; set; }
        public bool IsThereAProblem { get; set; }
        public string ProblemDescription { get; set; }
        public bool IsContinuationConsidered { get; set; }
        public bool IsTrustpilotReviewed { get; set; }
        public bool CanUseLogo { get; set; }
    }
}
