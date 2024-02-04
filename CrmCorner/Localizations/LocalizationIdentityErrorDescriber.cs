using Microsoft.AspNetCore.Identity;

namespace CrmCorner.Localizations
{
    public class LocalizationIdentityErrorDescriber:IdentityErrorDescriber
    {
        public override IdentityError DuplicateUserName(string userName)
        {
            return new() { Code = "DuplicateUserName", Description = $"{userName}, Bu kullanıcı adı daha önce başka bir kullanıcı tarafından alınmıştır." };
           // return base.DuplicateUserName(userName);
        }

        public override IdentityError DuplicateEmail(string email)
        {
            return new() { Code = "DuplicateEmail", Description = $"{email}, Bu email adresi ile açılmış bir hesap bulunmaktadır." };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            return new() { Code = "PasswordTooShort", Description = $"Şifre en az 6 karanterden oluşmalıdır." };
        }

        public override IdentityError PasswordMismatch()
        {
            return new() { Code = "PasswordMismatch", Description = $"Şifre yanlış!" };
        }
    }
}
