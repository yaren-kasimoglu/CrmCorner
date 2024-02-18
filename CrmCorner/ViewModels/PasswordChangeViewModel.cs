using System.ComponentModel.DataAnnotations;

namespace CrmCorner.ViewModels
{
    public class PasswordChangeViewModel
    {
        [Required(ErrorMessage = "Şifre alanı boş bırakılamaz.")]
        [Display(Name = "Eski Şifre")]
        public string PasswordOld { get; set; } = null!;

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Yeni Şifre alanı boş bırakılamaz.")]
        [Display(Name = "Yeni Şifre")]
        [MinLength(6,ErrorMessage ="Şifreniz en az 6 karakter olmalıdır.")]
        public string PasswordNew { get; set; } = null!;

        [DataType(DataType.Password)]
        [Compare(nameof(PasswordNew), ErrorMessage = "Şifreler aynı olmalıdır!")]
        [Required(ErrorMessage = "Yeni Şifre Tekrar alanı boş bırakılamaz.")]
        [MinLength(6, ErrorMessage = "Şifreniz en az 6 karakter olmalıdır.")]
        [Display(Name = "Yeni Şifre Tekrarı")]
        public string PasswordNewConfirm { get; set; } = null!;
    }
}
