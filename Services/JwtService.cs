using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using SecureProductApi.Models;

namespace SecureProductApi.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;

    public JwtService(
        IConfiguration configuration,
        IMemoryCache cache)
    {
        _configuration = configuration;
        _cache = cache;
    }

    public TokenResponse GenerateTokens(
        string username,
        string role)
    {
        string tokenCacheKey =
            $"token:{username}";

        // ==========================================
        // CHECK ACCESS TOKEN IN CACHE
        // ==========================================

        if (_cache.TryGetValue(
                tokenCacheKey,
                out TokenResponse? cachedToken))
        {
            return cachedToken!;
        }

        // ==========================================
        // GENERATE ACCESS TOKEN
        // ==========================================

        var accessTokenExpiresAt =
            DateTime.UtcNow.AddMinutes(5);

        var claims = new[]
        {
            new Claim(
                ClaimTypes.Name,
                username),

            new Claim(
                ClaimTypes.Role,
                role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: accessTokenExpiresAt,
            signingCredentials: credentials);

        var accessToken =
            new JwtSecurityTokenHandler()
                .WriteToken(jwt);

        // ==========================================
        // GENERATE REFRESH TOKEN
        // ==========================================

        var refreshToken =
            GenerateRefreshToken();

        var refreshTokenExpiresAt =
            DateTime.UtcNow.AddDays(7);

        var response = new TokenResponse
        {
            AccessToken = accessToken,

            RefreshToken = refreshToken,

            AccessTokenExpiresAt =
                accessTokenExpiresAt,

            RefreshTokenExpiresAt =
                refreshTokenExpiresAt
        };

        // ==========================================
        // CACHE ACCESS TOKEN
        // ==========================================

        _cache.Set(
            tokenCacheKey,
            response,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpiration =
                    accessTokenExpiresAt
            });

        // ==========================================
        // CACHE REFRESH TOKEN
        // ==========================================

        _cache.Set(
            $"refresh:{refreshToken}",
            new RefreshTokenData
            {
                Username = username,
                Role = role
            },
            new MemoryCacheEntryOptions
            {
                AbsoluteExpiration =
                    refreshTokenExpiresAt
            });

        return response;
    }


    // ==========================================
    // REFRESH TOKEN
    // ==========================================

    public TokenResponse? RefreshAccessToken(
        string refreshToken)
    {
        string refreshCacheKey =
            $"refresh:{refreshToken}";

        // Check refresh token in cache
        if (!_cache.TryGetValue(
                refreshCacheKey,
                out RefreshTokenData? data))
        {
            return null;
        }

        // Generate a new access token
        var accessTokenExpiresAt =
            DateTime.UtcNow.AddMinutes(5);

        var claims = new[]
        {
            new Claim(
                ClaimTypes.Name,
                data!.Username),

            new Claim(
                ClaimTypes.Role,
                data.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: accessTokenExpiresAt,
            signingCredentials: credentials);

        var newAccessToken =
            new JwtSecurityTokenHandler()
                .WriteToken(jwt);

        // Keep same refresh token for this simple demo
        var response = new TokenResponse
        {
            AccessToken = newAccessToken,

            RefreshToken = refreshToken,

            AccessTokenExpiresAt =
                accessTokenExpiresAt,

            RefreshTokenExpiresAt =
                DateTime.UtcNow.AddDays(7)
        };

        // Update access token cache
        _cache.Set(
            $"token:{data.Username}",
            response,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpiration =
                    accessTokenExpiresAt
            });

        return response;
    }


    private string GenerateRefreshToken()
    {
        var bytes = new byte[64];

        using var rng =
            RandomNumberGenerator.Create();

        rng.GetBytes(bytes);

        return Convert.ToBase64String(bytes);
    }
}


public class RefreshTokenData
{
    public string Username { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}