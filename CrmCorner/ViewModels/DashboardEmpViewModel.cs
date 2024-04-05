namespace CrmCorner.ViewModels
{
    public class DashboardEmpViewModel
    {
        public UserViewModel User { get; set; }
        public Dictionary<string, int> TaskStatusCountsByUser { get; set; } = new Dictionary<string, int>();
    }
}

