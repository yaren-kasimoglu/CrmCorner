using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CrmCorner.Areas.Admin.Models
{
    public class AssignRoleToUserVM
    {
        [Required(ErrorMessage = "UserId is required.")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "RoleId is required.")]
        public string RoleId { get; set; }

        public List<SelectListItem> Users { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
    }
}
