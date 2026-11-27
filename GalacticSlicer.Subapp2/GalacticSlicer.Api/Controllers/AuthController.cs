
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace GalacticSlicer.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly PlayerProgressService _progressService;

    public AuthController(
        UserManager<IdentityUser> userManager,
        IConfiguration configuration,
        PlayerProgressService progressService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _progressService = progressService;
    }

    // ============================================
    // REGISTER
    // ============================================

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDto dto)
    {
        var user = new IdentityUser
        {
            UserName = dto.Email,
            Email = dto.Email
        };

        var result = await _userManager.CreateAsync(
            user, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => new
            {
                e.Code,
                e.Description
            }));
        }

        // Automatically create progress for a new player.
        await _progressService.GetOrCreateProgressAsync(user.Id);

        return StatusCode(201, new
        {
            message = "User registered successfully"
        });
    }

    // ============================================
    // LOGIN
    // ============================================

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(
            dto.Email);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Invalid credentials"
            });
        }

        var validPassword = await _userManager
            .CheckPasswordAsync(user, dto.Password);

        if (!validPassword)
        {
            return Unauthorized(new
            {
                message = "Invalid credentials"
            });
        }

        // Generate a JWT containing the user's Identity roles.
        var token = await GenerateTokenAsync(user);

        return Ok(new
        {
            token,
            tokenType = "Bearer",
            expiresIn = 3600
        });
    }

    // ============================================
    // JWT TOKEN GENERATION
    // ============================================

    private async Task<string> GenerateTokenAsync(
        IdentityUser user)
    {
        var key = Convert.FromBase64String(
            _configuration["Jwt:Key"]!);

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256);

        // Standard JWT claims.
        var claims = new List<Claim>
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        // Load the user's roles from ASP.NET Core Identity.
        var roles = await _userManager.GetRolesAsync(user);

        // Add each role to the JWT.
        foreach (var role in roles)
        {
            claims.Add(new Claim(
                ClaimTypes.Role,
                role));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}
