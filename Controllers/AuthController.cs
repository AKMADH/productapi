using Microsoft.AspNetCore.Mvc;
using SecureProductApi.Model;
using SecureProductApi.Models;
using SecureProductApi.Services;

namespace SecureProductApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;

    public AuthController(JwtService jwtService)
    {
        _jwtService = jwtService;
    }


    // ==========================================
    // LOGIN
    // ==========================================

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        string? role = null;

        // Validate username/password
        if (request.Username == "admin" &&
            request.Password == "1234")
        {
            role = "Admin";
        }
        else if (request.Username == "user" &&
                 request.Password == "1234")
        {
            role = "User";
        }

        if (role == null)
        {
            return Unauthorized(new
            {
                message = "Invalid username or password"
            });
        }

        // ======================================
        // Call JwtService
        // JwtService checks cache internally
        // ======================================

        var tokens =
            _jwtService.GenerateTokens(
                request.Username,
                role);

        return Ok(tokens);
    }


    // ==========================================
    // REFRESH
    // ==========================================

    [HttpPost("refresh")]
    public IActionResult Refresh(
        RefreshTokenRequest request)
    {
        var tokens =
            _jwtService.RefreshAccessToken(
                request.RefreshToken);

        if (tokens == null)
        {
            return Unauthorized(new
            {
                message = "Invalid or expired refresh token"
            });
        }

        return Ok(tokens);
    }
}