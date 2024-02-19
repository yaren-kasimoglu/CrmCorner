namespace CrmCorner.Models
{
    public partial class CustomerN
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Surname { get; set; } = null!;

        public string CustomerEmail { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;
        public string CompanyName { get; set; }// müşterinin firması
        public string CompanyEmail { get; set; }

        public string? AppUserId { get; set; }
        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public AppUser? AppUser { get; set; }

        public virtual ICollection<TaskComp>? TaskComps { get; set; }
  
    }
}
