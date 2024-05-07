namespace CrmCorner.Models
{
    public class PostSaleInfo
    {
        public int Id { get; set; }
        public int TaskCompId { get; set; } // TaskComp ile ilişkilendirme
        public virtual TaskComp TaskComp { get; set; }

        public bool IsFirstPaymentMade { get; set; } = false;
        public bool IsThereAProblem { get; set; } = false;
        public string ProblemDescription { get; set; }
        public bool IsContinuationConsidered { get; set; } = false;
        public bool IsTrustpilotReviewed { get; set; } = false;
        public bool CanUseLogo { get; set; } = false;
    }
}
