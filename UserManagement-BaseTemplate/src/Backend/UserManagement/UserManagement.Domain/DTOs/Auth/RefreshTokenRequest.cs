using System.ComponentModel.DataAnnotations;

namespace UserManagement.Domain.DTOs.Auth;

public record RefreshTokenRequest(
    [Required] string RefreshToken
);
