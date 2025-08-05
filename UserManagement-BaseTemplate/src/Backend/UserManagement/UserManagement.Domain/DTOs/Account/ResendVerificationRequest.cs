using System.ComponentModel.DataAnnotations;

namespace UserManagement.Domain.DTOs.Account;

public record ResendVerificationRequest(
    [Required, EmailAddress] string Email
);
