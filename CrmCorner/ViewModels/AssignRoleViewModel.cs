namespace CrmCorner.ViewModels
{
    public class AssignRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public IList<string> Roles { get; set; }
        public IList<string> SelectedRoles { get; set; }
    }
}
