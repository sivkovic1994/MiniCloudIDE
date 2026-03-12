using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiniCloudIDE.Application.DTOs;
using MiniCloudIDE.Application.Interfaces;
using MiniCloudIDE.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MiniCloudIDE.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            if (await _userManager.FindByEmailAsync(request.Email) != null)
            {
                return new AuthResult
                {
                    Success = false,
                    StatusCode = 400,
                    Errors = new { error = "User with this email already exists" }
                };
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Username ?? request.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    StatusCode = 400,
                    Errors = new { errors = result.Errors.Select(e => e.Description) }
                };
            }

            return new AuthResult
            {
                Success = true,
                Data = new AuthResponse
                {
                    Token = GenerateJwtToken(user),
                    Email = user.Email!,
                    UserId = user.Id,
                    Username = user.UserName
                }
            };
        }

        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return new AuthResult
                {
                    Success = false,
                    StatusCode = 401,
                    Errors = new { error = "Invalid email or password" }
                };
            }

            return new AuthResult
            {
                Success = true,
                Data = new AuthResponse
                {
                    Token = GenerateJwtToken(user),
                    Email = user.Email!,
                    UserId = user.Id,
                    Username = user.UserName
                }
            };
        }

        public async Task<AuthResult> GetCurrentUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new AuthResult { Success = false, StatusCode = 404 };
            }

            return new AuthResult
            {
                Success = true,
                Data = new AuthResponse
                {
                    Token = "",
                    Email = user.Email!,
                    UserId = user.Id,
                    Username = user.UserName
                }
            };
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
