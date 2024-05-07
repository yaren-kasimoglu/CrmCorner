using System.ComponentModel.DataAnnotations;

namespace CrmCorner.Areas.Admin.Models
{
    public class RoleUpdateVM
    {
        public string Id { get; set; } = null!;


        [Required(ErrorMessage = "Rol Adı alanı boş bırakılamaz.")]
        [Display(Name = "Rol Adı ")]
        public string Name { get; set; } 
    }
}
