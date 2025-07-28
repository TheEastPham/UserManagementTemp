using System.ComponentModel.DataAnnotations;

namespace Base.UserManagement.Domain.DTOs.Auth;

public record RefreshTokenRequest(
    [Required] string RefreshToken
);
