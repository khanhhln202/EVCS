using EVCS.Services.DTOs.Auth;
using EVCS.Services.DTOs.Profile;

namespace EVCS.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterDto dto);
        Task<AuthResult> LoginAsync(LoginDto dto);
        Task LogoutAsync();
        Task<bool> IsEmailExistsAsync(string email);
        
        // Profile Management
        Task<UserProfileDto?> GetUserProfileAsync(Guid userId);
        Task<ProfileResult> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
        Task<ProfileResult> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    }
}