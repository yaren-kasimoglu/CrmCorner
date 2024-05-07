namespace CrmCorner.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public string? Message { get; set; }  // Hata mesajı için alan ekledim

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}