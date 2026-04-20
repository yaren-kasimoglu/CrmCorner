namespace CrmCorner.Models.ChatCorner
{
    public class ChatCornerResponseDto
    {
        public bool Success { get; set; }
        public string Answer { get; set; }
        public string Intent { get; set; }
        public object Data { get; set; }
        public string ErrorMessage { get; set; }
    }
}