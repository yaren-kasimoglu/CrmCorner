using Microsoft.AspNetCore.Mvc.Rendering;


namespace CrmCorner.ViewModels

{
    public class ApolloApiViewModel
    {
        public string ApiKey { get; set; }
        public string SelectedLabelId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? LastDays { get; set; } // 15 veya 30

        public List<SelectListItem> Labels { get; set; } = new List<SelectListItem>();



    }
}
