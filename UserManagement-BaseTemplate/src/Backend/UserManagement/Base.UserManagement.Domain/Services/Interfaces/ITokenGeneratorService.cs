namespace Base.UserManagement.Domain.Services.Interfaces;

public interface ITokenGeneratorService
{
    string GenerateVerificationToken(string email, int length = 6);
    string GenerateSecureRandomToken(int length = 32);
}
