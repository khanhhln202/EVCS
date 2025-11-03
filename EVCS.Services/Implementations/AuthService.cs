using EVCS.Models.Identity;
using EVCS.Services.DTOs.Auth;
using EVCS.Services.DTOs.Profile;
using EVCS.Services.Interfaces;
using EVCS.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EVCS.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var user = await _userManager.FindByEmailAsync(email.Trim());
            return user != null;
        }

        public async Task<AuthResult> RegisterAsync(RegisterDto dto)
        {
            try
            {
                // Kiểm tra email đã tồn tại
                if (await IsEmailExistsAsync(dto.Email))
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Email đã được sử dụng",
                        Errors = new List<string> { "Email đã tồn tại trong hệ thống" }
                    };
                }

                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FullName = dto.FullName.Trim(),
                    PhoneNumber = dto.PhoneNumber?.Trim(),
                    EmailConfirmed = true, // Tự động confirm (hoặc gửi email xác nhận sau)
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Đăng ký thất bại",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                // Gán role mặc định
                await _userManager.AddToRoleAsync(user, SD.RoleDriver);

                _logger.LogInformation("User {Email} registered successfully", dto.Email);

                return new AuthResult
                {
                    Success = true,
                    Message = "Đăng ký thành công",
                    UserId = user.Id.ToString(),
                    UserName = user.UserName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", dto.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trong quá trình đăng ký",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<AuthResult> LoginAsync(LoginDto dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);

                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Email hoặc mật khẩu không chính xác"
                    };
                }

                if (!user.IsActive)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Tài khoản đã bị vô hiệu hóa"
                    };
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName!,
                    dto.Password,
                    dto.RememberMe,
                    lockoutOnFailure: true
                );

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in", dto.Email);
                    return new AuthResult
                    {
                        Success = true,
                        Message = "Đăng nhập thành công",
                        UserId = user.Id.ToString(),
                        UserName = user.UserName
                    };
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User {Email} is locked out", dto.Email);
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Tài khoản đã bị khóa do nhập sai mật khẩu nhiều lần"
                    };
                }

                return new AuthResult
                {
                    Success = false,
                    Message = "Email hoặc mật khẩu không chính xác"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", dto.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trong quá trình đăng nhập",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return null;

                return new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    InvoiceDisplayName = user.InvoiceDisplayName,
                    InvoiceEmail = user.InvoiceEmail,
                    InvoiceAddress = user.InvoiceAddress,
                    TaxId = user.TaxId,
                    CreatedAt = user.CreatedAt,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile for user {UserId}", userId);
                return null;
            }
        }

        public async Task<ProfileResult> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return new ProfileResult
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }

                user.FullName = dto.FullName.Trim();
                user.PhoneNumber = dto.PhoneNumber?.Trim();
                user.InvoiceDisplayName = dto.InvoiceDisplayName?.Trim();
                user.InvoiceEmail = dto.InvoiceEmail?.Trim();
                user.InvoiceAddress = dto.InvoiceAddress?.Trim();
                user.TaxId = dto.TaxId?.Trim();

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return new ProfileResult
                    {
                        Success = false,
                        Message = "Cập nhật thông tin thất bại",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                _logger.LogInformation("User {UserId} updated profile", userId);

                return new ProfileResult
                {
                    Success = true,
                    Message = "Cập nhật thông tin thành công",
                    Profile = await GetUserProfileAsync(userId)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                return new ProfileResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi cập nhật thông tin",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ProfileResult> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return new ProfileResult
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }

                var result = await _userManager.ChangePasswordAsync(
                    user,
                    dto.CurrentPassword,
                    dto.NewPassword
                );

                if (!result.Succeeded)
                {
                    return new ProfileResult
                    {
                        Success = false,
                        Message = "Đổi mật khẩu thất bại",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                await _signInManager.RefreshSignInAsync(user);

                _logger.LogInformation("User {UserId} changed password", userId);

                return new ProfileResult
                {
                    Success = true,
                    Message = "Đổi mật khẩu thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return new ProfileResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi đổi mật khẩu",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}