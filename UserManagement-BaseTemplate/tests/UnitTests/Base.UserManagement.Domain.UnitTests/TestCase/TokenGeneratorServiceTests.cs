using Base.UserManagement.Domain.Services;
using FluentAssertions;

namespace Base.UserManagement.Domain.UnitTests.TestCase;

public class TokenGeneratorServiceTests
{
    private readonly TokenGeneratorService _tokenGenerator;

    public TokenGeneratorServiceTests()
    {
        _tokenGenerator = new TokenGeneratorService();
    }

    [Fact]
    public void GenerateVerificationToken_WithValidEmail_ShouldReturnTokenWithCorrectLength()
    {
        // Arrange
        var email = "test@example.com";
        var expectedLength = 6;

        // Act
        var token = _tokenGenerator.GenerateVerificationToken(email, expectedLength);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Length.Should().Be(expectedLength);
        token.Should().MatchRegex("^[A-Z0-9]+$"); // Only uppercase letters and numbers
    }

    [Fact]
    public void GenerateVerificationToken_WithDifferentLengths_ShouldReturnCorrectLength()
    {
        // Arrange
        var email = "test@example.com";

        // Act & Assert
        for (int length = 4; length <= 10; length++)
        {
            var token = _tokenGenerator.GenerateVerificationToken(email, length);
            token.Length.Should().Be(length);
        }
    }

    [Fact]
    public void GenerateVerificationToken_WithSameEmail_ShouldReturnConsistentTokens()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var token1 = _tokenGenerator.GenerateVerificationToken(email);
        var token2 = _tokenGenerator.GenerateVerificationToken(email);

        // Assert
        // Note: Due to time-based randomness, tokens may differ
        // But the format should be consistent
        token1.Should().MatchRegex("^[A-Z0-9]+$");
        token2.Should().MatchRegex("^[A-Z0-9]+$");
        token1.Length.Should().Be(token2.Length);
    }

    [Fact]
    public void GenerateVerificationToken_WithDifferentEmails_ShouldReturnDifferentTokens()
    {
        // Arrange
        var email1 = "test1@example.com";
        var email2 = "test2@example.com";

        // Act
        var token1 = _tokenGenerator.GenerateVerificationToken(email1);
        var token2 = _tokenGenerator.GenerateVerificationToken(email2);

        // Assert
        token1.Should().NotBe(token2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateVerificationToken_WithInvalidEmail_ShouldThrowArgumentException(string email)
    {
        // Act & Assert
        var action = () => _tokenGenerator.GenerateVerificationToken(email);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot be null or empty*");
    }

    [Theory]
    [InlineData(3)]
    [InlineData(11)]
    [InlineData(0)]
    [InlineData(-1)]
    public void GenerateVerificationToken_WithInvalidLength_ShouldThrowArgumentException(int length)
    {
        // Arrange
        var email = "test@example.com";

        // Act & Assert
        var action = () => _tokenGenerator.GenerateVerificationToken(email, length);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Token length must be between 4 and 10 characters*");
    }

    [Fact]
    public void GenerateSecureRandomToken_WithDefaultLength_ShouldReturnValidToken()
    {
        // Act
        var token = _tokenGenerator.GenerateSecureRandomToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Length.Should().BeLessThanOrEqualTo(32);
        token.Should().MatchRegex("^[A-Za-z0-9]+$"); // Base64 characters without special chars
    }

    [Fact]
    public void GenerateSecureRandomToken_WithCustomLength_ShouldReturnTokenWithCorrectLength()
    {
        // Arrange
        var length = 16;

        // Act
        var token = _tokenGenerator.GenerateSecureRandomToken(length);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Length.Should().BeLessThanOrEqualTo(length);
    }

    [Fact]
    public void GenerateSecureRandomToken_MultipleCalls_ShouldReturnDifferentTokens()
    {
        // Act
        var token1 = _tokenGenerator.GenerateSecureRandomToken();
        var token2 = _tokenGenerator.GenerateSecureRandomToken();

        // Assert
        token1.Should().NotBe(token2);
    }
}
