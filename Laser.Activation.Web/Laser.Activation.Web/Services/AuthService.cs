using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Laser.Activation.Web.Services;

public interface IAuthService
{
    string GenerateJwtToken(string username);
    bool ValidateCredentials(string username, string password);
}

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool ValidateCredentials(string username, string password)
    {
        var configUsername = _configuration["Auth:Username"] ?? "admin";
        var configPassword = _configuration["Auth:Password"] ?? "admin123";

        return username == configUsername && password == configPassword;
    }

    public string GenerateJwtToken(string username)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "LaserActivationWebDefaultSecretKey2026!";
        var issuer = _configuration["Jwt:Issuer"] ?? "Laser.Activation.Web";
        var audience = _configuration["Jwt:Audience"] ?? "Laser.Activation.Web";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
