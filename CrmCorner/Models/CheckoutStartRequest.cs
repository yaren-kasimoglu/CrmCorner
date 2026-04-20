namespace CrmCorner.Models
{
    public class CheckoutStartRequest
    {
        public string PlanName { get; set; }
        public int UserCount { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}