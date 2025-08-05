using UserManagement.EFCore.Entities.Security;

namespace UserManagement.EFCore.Repositories.Interfaces;

public interface IEmailVerificationTokenRepository
{
    Task<EmailVerificationTokenEntity> CreateAsync(EmailVerificationTokenEntity token);
    Task<EmailVerificationTokenEntity?> GetByTokenAsync(string token);
    Task<EmailVerificationTokenEntity?> GetActiveByUserIdAsync(string userId);
    Task<bool> MarkAsUsedAsync(string tokenId);
    Task<bool> DeleteExpiredTokensAsync();
    Task<bool> DeleteByUserIdAsync(string userId);
}
