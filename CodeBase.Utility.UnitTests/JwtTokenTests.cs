using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bogus;
using CodeBase.Utility.JWT;
using Microsoft.AspNetCore.Identity;
using Xunit;
using Xunit.Abstractions;

namespace CodeBase.Utility.UnitTests;

public class JwtTokenTests
{
    private readonly IdentityUser _user = new();
    private const string SecretKey = "MIHcAgEBBEIBxMrOPGO6TlntJT21d7LMMYRQEcIY5r5YppitTzTPEe851yOMhDKuSC8UUAKs52ecmELabpFZ4cLS6WI4SE2FR7ugBwYFK4EEACOhgYkDgYYABACuglsMTwAyKLhwuLQYMD2VtGRKka7jAdhVX7MaRt5C6enE+peyrjugvZVxjy/vUItKNvtGrJTZ4SWSAVCIcEzykgFIKRKtZvXFxMQ0C/tofl+3ijvDzozkCyR23wlKoB+t9Hi5Vd9utJcyZkm3NT4WGrPj/JUWze99SMmi78lK7EQaeg==";
    private static readonly List<string> Roles = [ "Admin" ];
    private readonly ITestOutputHelper _output;
    private readonly Faker faker = new ();


    public JwtTokenTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    
    [Fact]
    public void GenerateToken_ShouldReturnToken()
    {
        // Act
        var token = JwtToken.GenerateToken(_user, Roles, SecretKey);

        // Assert
        Assert.NotNull(token);
        Assert.IsType<string>(token);
        _output.WriteLine(_user.Id);
    }
    
    [Fact]
    public void GenerateToken_ShouldContainExpectedClaims()
    {
        // Act
        var token = JwtToken.GenerateToken(_user, Roles, SecretKey);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims, "jwt"));       
        // Assert
         Assert.Equal(_user.Id.ToString(), claimsPrincipal.FindFirstValue(ClaimTypes.Sid));
    }

    [Fact]
    public void GenerateToken_ShouldHaveExpiration()
    {
        // Act
        var token = JwtToken.GenerateToken(_user, Roles, SecretKey);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        Assert.NotNull(jwtToken.ValidTo);
        Assert.True(jwtToken.ValidTo > DateTime.UtcNow);
    }
    
    [Fact]
    public void GenerateToken_WithEmptyRoles_ShouldReturnToken()
    {
        // Arrange
        var emptyRoles = new List<string>();

        // Act
        var token = JwtToken.GenerateToken(_user, emptyRoles, SecretKey);

        // Assert
        Assert.NotNull(token);
        Assert.IsType<string>(token);
    }

    [Fact]
    public void GenerateToken_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(() => JwtToken.GenerateToken(null, Roles, SecretKey));
    }

    [Fact]
    public void GenerateToken_WithNullRoles_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(() => JwtToken.GenerateToken(_user, null, SecretKey));
    }

    [Fact]
    public void GenerateToken_WithNullSecretKey_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => JwtToken.GenerateToken(_user, Roles, null));
    }

    [Fact]
    public void GenerateToken_WithEmptySecretKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => JwtToken.GenerateToken(_user, Roles, string.Empty));
    }

    [Fact]
    public void GenerateToken_WithInvalidSecretKey_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidSecretKey = "short_key";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => JwtToken.GenerateToken(_user, Roles, invalidSecretKey));
    }
}