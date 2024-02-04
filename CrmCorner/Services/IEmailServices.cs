namespace CrmCorner.Services
{
    public interface IEmailServices
    {
        Task SendResetPasswordEmail(string resetPasswordEmailLink, string ToEmail);
    }
}
