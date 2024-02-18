using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace CrmCorner.ViewModels
{
    public class SignUpViewModel
    {
        public SignUpViewModel()
        {
            
        }

        public SignUpViewModel(string userName, string email, string phone, string password)
        {
            UserName = userName;
            Email = email;
            Phone = phone;
            Password = password;
        }

        [Required(ErrorMessage ="Kullanıcı Ad alanı boş bırakılamaz.")]
        [Display(Name="Kullanıcı Adı")]
        public string UserName { get; set; } = null!;

        [EmailAddress(ErrorMessage ="Email Formatı yanlıştır.")]
        [Required(ErrorMessage = "Email alanı boş bırakılamaz.")]
        [Display(Name = "Email")]
        public string  Email { get; set; } = null!;

        [Required(ErrorMessage = "Telefon alanı boş bırakılamaz.")]
        [Display(Name = "Telefon")]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "İsim soyisim alanı boş bırakılamaz.")]
        [Display(Name = "İsim Soyisim")]
        public string NameSurname { get; set; }

        [Required(ErrorMessage = "Pozisyon alanı boş bırakılamaz.")]
        [Display(Name = "Pozisyon")]
        public string PositionName { get; set; }

        [Required(ErrorMessage = "Kurum Adı alanı boş bırakılamaz.")]
        [Display(Name = "Firma Adı")]
        public string CompanyName { get; set; } = null!;

        [Required(ErrorMessage = "Çalışan Sayısı alanı boş bırakılamaz.")]
        [Display(Name = "Çalışan Sayısı")]
        public int EmployeeCount { get; set; }

        [Required(ErrorMessage = "Sektör alanı boş bırakılamaz.")]
        [Display(Name = "Sektör")]
        public string Sector { get; set; }


        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Şifre alanı boş bırakılamaz.")]
        [MinLength(6, ErrorMessage = "Şifreniz en az 6 karakter olmalıdır.")]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = null!;


        [DataType(DataType.Password)]
        [Compare(nameof(Password),ErrorMessage ="Şifreler aynı olmalıdır!")]
        [Required(ErrorMessage = "Şifre Tekrar alanı boş bırakılamaz.")]
        [MinLength(6, ErrorMessage = "Şifreniz en az 6 karakter olmalıdır.")]
        [Display(Name = "Şifre Tekrarı")]
        public string? PasswordConfirm { get; set; } = null!;
    }
}
