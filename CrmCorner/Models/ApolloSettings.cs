namespace CrmCorner.Models
{
    public class ApolloSettings
    {
        public int Id { get; set; }  // Primary Key
        public string UserId { get; set; }  // CRM kullanıcısının ID'si (Identity UserId)
        public string ApolloApiToken { get; set; }  // Kullanıcının Apollo API Tokenı
    }
}
