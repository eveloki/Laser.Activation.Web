using Laser.Activation.Web.Data;
using Laser.Activation.Web.Models;
using Laser.Activation.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laser.Activation.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IRedisTokenService _redisTokenService;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IRedisTokenService redisTokenService,
        AppDbContext dbContext,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _redisTokenService = redisTokenService;
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new LoginResponse { Success = false, Message = "用户名和密码不能为空" });
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var result = await _authService.ValidateCredentialsAsync(request.Username, request.Password);

        if (result != null)
        {
            var (userId, role) = result.Value;
            var (token, jti, expiry) = _authService.GenerateJwtToken(userId, request.Username, role);

            _redisTokenService.StoreToken(jti, userId, expiry);

            _dbContext.LoginLogs.Add(new LoginLog
            {
                UserId = userId,
                Username = request.Username,
                Success = true,
                IpAddress = ip,
                LoginTime = DateTime.Now
            });
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("User {Username} logged in successfully", request.Username);
            return Ok(new LoginResponse { Success = true, Token = token, Role = role, UserId = userId });
        }

        _logger.LogWarning("Login failed for user {Username}", request.Username);
        return Unauthorized(new LoginResponse { Success = false, Message = "用户名或密码错误" });
    }

    [HttpGet("validate")]
    [Authorize]
    public IActionResult Validate()
    {
        return Ok(new { success = true });
    }
}
