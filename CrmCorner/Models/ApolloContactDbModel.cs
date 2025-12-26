namespace CrmCorner.Models
{
    public class ApolloContactDbModel
    {
        public int Id { get; set; }
        public string PersonId { get; set; }

        public int? CompanyId { get; set; } 

        public string? Email { get; set; } // UNIQUE
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Title { get; set; }
        public string? CompanyName { get; set; }  // ❗ OrganizationName yerine bunu kullanacağız
        public string? SourceLabelId { get; set; }
        public string? SourceLabelName { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? LinkedinUrl { get; set; }
        public string? Location { get; set; }
        public string? Headline { get; set; }

        // ❗ Eksik olanlar:
        public string? Phone { get; set; }
    }
}
