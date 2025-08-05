namespace UserManagement.Domain.Services.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailVerificationAsync(string email, string firstName, string verificationToken);
    Task<bool> SendPasswordResetAsync(string email, string firstName, string resetToken);
    Task<bool> SendWelcomeEmailAsync(string email, string firstName);
}
