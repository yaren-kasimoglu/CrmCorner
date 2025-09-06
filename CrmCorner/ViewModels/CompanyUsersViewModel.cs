using CrmCorner.Models;
using System.Collections.Generic;

namespace CrmCorner.ViewModels
{
    public class CompanyUsersViewModel
    {
        public AppUser CurrentUser { get; set; }
        public List<AppUser> CompanyUsers { get; set; }

        // ESKİ: public List<TaskComp> TaskComps { get; set; }
        // YENİ:
        public int PipelineTaskCount { get; set; }            // kartta sayı göstermek için
        public List<PipelineTask> PipelineTasks { get; set; } // istersen liste de basarsın

        public int SectorCount { get; set; }

        // (İsteğe bağlı) ToDo’yu ViewBag yerine ViewModel’den vermek istersen:
        public List<Tuple<string, string>> UpcomingTodos { get; set; }

        public CompanyUsersViewModel()
        {
            CompanyUsers = new List<AppUser>();
            PipelineTasks = new List<PipelineTask>();
            UpcomingTodos = new List<Tuple<string, string>>();
        }
    }
}
