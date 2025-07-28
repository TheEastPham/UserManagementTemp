using System.ComponentModel.DataAnnotations;

namespace Base.UserManagement.Domain.DTOs.Account;

public record ResendVerificationRequest(
    [Required, EmailAddress] string Email
);
