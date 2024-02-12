using System.ComponentModel.DataAnnotations;

namespace CrmCorner.ViewModels
{
    public class ResetPasswordViewModel
    {

        [Required(ErrorMessage = "Şifre alanı boş bırakılamaz.")]
        [Display(Name = "Yeni Şifre")]
        public string Password { get; set; } = null!;

        [Compare(nameof(Password), ErrorMessage = "Şifreler aynı olmalıdır!")]
        [Required(ErrorMessage = "Şifre Tekrar alanı boş bırakılamaz.")]
        [Display(Name = "Yeni Şifre Tekrar")]
        public string PasswordConfirm { get; set; } = null!;
    }
}
