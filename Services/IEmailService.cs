namespace UTHMLibrary.Services;

public interface IEmailService
{
    Task<bool> SendVerificationCodeAsync(string email, string code, string userName);
    Task<bool> SendEmailAsync(string to, string subject, string body);
}