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
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new LoginResponse { Success = false, Message = "用户名和密码不能为空" });
        }

        var result = await _authService.ValidateCredentialsAsync(request.Username, request.Password);
        if (result != null)
        {
            var (userId, role) = result.Value;
            var token = _authService.GenerateJwtToken(userId, request.Username, role);
            _logger.LogInformation("User {Username} logged in successfully", request.Username);
            return Ok(new LoginResponse { Success = true, Token = token, Role = role, UserId = userId });
        }

        _logger.LogWarning("Login failed for user {Username}", request.Username);
        return Unauthorized(new LoginResponse { Success = false, Message = "用户名或密码错误" });
    }
}
