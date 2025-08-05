using System.ComponentModel.DataAnnotations;

namespace UserManagement.Domain.DTOs.Account;

public record VerifyEmailRequest(
    [Required] string Token,
    [Required, EmailAddress] string Email
);
