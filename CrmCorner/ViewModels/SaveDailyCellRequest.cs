public class SaveDailyCellRequest
{
    public string UserId { get; set; }
    public string CompanyName { get; set; }
    public DateTime Date { get; set; }
    public string ActivityType { get; set; }
    public string Type { get; set; } // P veya A
    public int Value { get; set; }
}