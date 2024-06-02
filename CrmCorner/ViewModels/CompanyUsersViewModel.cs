using CrmCorner.Models;

namespace CrmCorner.ViewModels
{
    public class CompanyUsersViewModel
    {
        public AppUser CurrentUser { get; set; }
        public List<AppUser> CompanyUsers { get; set; }
        public List<TaskComp> TaskComps { get; set; } // Eğer TaskComps'ı ayrıca göstermek istiyorsanız

        public int SectorCount { get; set; } // Farklı sektör sayısı
    }
}
