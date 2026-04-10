using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Isopoh.Cryptography.Argon2;
using Laser.Activation.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Laser.Activation.Web.Services;

public interface IAuthService
{
    (string token, string jti, TimeSpan expiry) GenerateJwtToken(int userId, string username, string role);
    Task<(int userId, string role)?> ValidateCredentialsAsync(string username, string password);
    string HashPassword(string password);
    bool VerifyPassword(string passwordHash, string password);
}

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IConfiguration configuration, AppDbContext dbContext, ILogger<AuthService> logger)
    {
        _configuration = configuration;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<(int userId, string role)?> ValidateCredentialsAsync(string username, string password)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return null;

        if (!Argon2.Verify(user.PasswordHash, password)) return null;

        return (user.Id, user.Role);
    }

    public string HashPassword(string password)
    {
        return Argon2.Hash(password);
    }

    public bool VerifyPassword(string passwordHash, string password)
    {
        return Argon2.Verify(passwordHash, password);
    }

    public (string token, string jti, TimeSpan expiry) GenerateJwtToken(int userId, string username, string role)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "LaserActivationWebDefaultSecretKey2026!";
        var issuer = _configuration["Jwt:Issuer"] ?? "Laser.Activation.Web";
        var audience = _configuration["Jwt:Audience"] ?? "Laser.Activation.Web";
        var expireHours = double.TryParse(_configuration["Jwt:ExpireHours"], out var h) ? h : 1;
        var expiry = TimeSpan.FromHours(expireHours);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jti = Guid.NewGuid().ToString();
        var claims = new[]
        {
            new Claim("userId", userId.ToString()),
            new Claim("username", username),
            new Claim("role", role),
            new Claim(JwtRegisteredClaimNames.Jti, jti)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(expiry),
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), jti, expiry);
    }
}
