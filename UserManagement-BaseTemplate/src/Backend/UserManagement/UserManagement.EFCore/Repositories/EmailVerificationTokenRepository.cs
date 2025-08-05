using UserManagement.EFCore.Data;
using UserManagement.EFCore.Entities.Security;
using UserManagement.EFCore.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.EFCore.Repositories;

public class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly UserManagementDbContext _context;

    public EmailVerificationTokenRepository(UserManagementDbContext context)
    {
        _context = context;
    }

    public async Task<EmailVerificationTokenEntity> CreateAsync(EmailVerificationTokenEntity token)
    {
        _context.EmailVerificationTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }

    public async Task<EmailVerificationTokenEntity?> GetByTokenAsync(string token)
    {
        return await _context.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<EmailVerificationTokenEntity?> GetActiveByUserIdAsync(string userId)
    {
        return await _context.EmailVerificationTokens
            .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> MarkAsUsedAsync(string tokenId)
    {
        var token = await _context.EmailVerificationTokens.FindAsync(tokenId);
        if (token == null) return false;

        token.IsUsed = true;
        token.UsedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteExpiredTokensAsync()
    {
        var expiredTokens = await _context.EmailVerificationTokens
            .Where(t => t.ExpiresAt <= DateTime.UtcNow || t.IsUsed)
            .ToListAsync();

        if (expiredTokens.Any())
        {
            _context.EmailVerificationTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> DeleteByUserIdAsync(string userId)
    {
        var tokens = await _context.EmailVerificationTokens
            .Where(t => t.UserId == userId)
            .ToListAsync();

        if (tokens.Any())
        {
            _context.EmailVerificationTokens.RemoveRange(tokens);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
}
