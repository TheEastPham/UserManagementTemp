namespace Base.UserManagement.EFCore.Entities.Security;

/// <summary>
/// Email verification token entity for database
/// </summary>
public class EmailVerificationTokenEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(30);
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    
    // Navigation properties
    public virtual User.UserEntity User { get; set; } = null!;
}
