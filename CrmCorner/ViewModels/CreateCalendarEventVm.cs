namespace CrmCorner.ViewModels
{
    public class CreateCalendarEventVm
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool SendToGoogle { get; set; } = true;
    }
}
