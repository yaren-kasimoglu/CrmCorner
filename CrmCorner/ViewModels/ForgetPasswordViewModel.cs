using System.ComponentModel.DataAnnotations;

namespace CrmCorner.ViewModels
{
    public class ForgetPasswordViewModel
    {
        [EmailAddress(ErrorMessage = "Email Formatı yanlıştır.")]
        [Required(ErrorMessage = "Email alanı boş bırakılamaz.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

    }
}
