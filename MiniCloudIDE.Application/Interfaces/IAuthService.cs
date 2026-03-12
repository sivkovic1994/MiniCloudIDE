using MiniCloudIDE.Application.DTOs;

namespace MiniCloudIDE.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request);
        Task<AuthResult> LoginAsync(LoginRequest request);
        Task<AuthResult> GetCurrentUserAsync(string userId);
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public AuthResponse? Data { get; set; }
        public object? Errors { get; set; }
        public int StatusCode { get; set; } = 200;
    }
}
