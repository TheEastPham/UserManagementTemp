using System.ComponentModel.DataAnnotations;

namespace Base.UserManagement.Domain.DTOs.Account;

public record VerifyEmailRequest(
    [Required] string Token,
    [Required, EmailAddress] string Email
);
