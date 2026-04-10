using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FreeRedis;
using Isopoh.Cryptography.Argon2;
using Laser.Activation.Web.Components;
using Laser.Activation.Web.Data;
using Laser.Activation.Web.Models;
using Laser.Activation.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var serverVersion = new MySqlServerVersion(new Version(12, 2, 2));
    options.UseMySql(connectionString, serverVersion);
});

var redis = new RedisClient(builder.Configuration["Redis:ConnectionString"]
                            ?? "127.0.0.1:6379,defaultDatabase=0");
builder.Services.AddSingleton<RedisClient>(redis);
builder.Services.AddSingleton<IRedisTokenService, RedisTokenService>();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IActivationService, EcdsaActivationService>();
builder.Services.AddScoped<ICaptchaService, CaptchaService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"] ?? "LaserActivationWebSecretKey2026!@#$%^&*()";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Laser.Activation.Web",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "Laser.Activation.Web",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            NameClaimType = "username",
            RoleClaimType = "role"
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var tokenService = context.HttpContext.RequestServices
                    .GetRequiredService<IRedisTokenService>();
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrEmpty(jti) || !tokenService.IsTokenValid(jti))
                {
                    context.Fail("Token has been revoked or expired");
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddHttpClient();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var tokenService = scope.ServiceProvider.GetRequiredService<IRedisTokenService>();
    tokenService.ClearAllTokens();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("All JWT tokens cleared on startup");

    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Users.Any())
    {
        var defaultPassword = builder.Configuration["Auth:DefaultAdminPassword"] ?? "admin123";
        db.Users.Add(new User
        {
            Username = "admin",
            PasswordHash = Argon2.Hash(defaultPassword),
            Role = "Admin",
            CreatedTime = DateTime.Now
        });
        db.SaveChanges();
        logger.LogInformation("Default admin user created");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Laser.Activation.Web.Client._Imports).Assembly);

app.Run();
