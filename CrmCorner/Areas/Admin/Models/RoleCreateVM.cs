using System.ComponentModel.DataAnnotations;

namespace CrmCorner.Areas.Admin.Models
{
    public class RoleCreateVM
    {
        [Required(ErrorMessage = "Rol Adı alanı boş bırakılamaz.")]
        [Display(Name = "Rol Adı ")]
        public string RoleName { get; set; }
    }
}
