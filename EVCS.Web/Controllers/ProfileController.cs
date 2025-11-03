using EVCS.Services.DTOs.Profile;
using EVCS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EVCS.Web.Controllers
{
    [Route("Profile")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IAuthService authService, ILogger<ProfileController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        // GET /Profile
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Invalid user ID in claims");
                return RedirectToAction("Login", "Account");
            }

            var profile = await _authService.GetUserProfileAsync(userId);
            if (profile == null)
            {
                TempData["error"] = "Không thể tải thông tin tài khoản";
                return RedirectToAction("Index", "Home");
            }

            return View(profile);
        }

        // GET /Profile/Edit
        [HttpGet("Edit")]
        public async Task<IActionResult> Edit()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return RedirectToAction("Login", "Account");

            var profile = await _authService.GetUserProfileAsync(userId);
            if (profile == null)
            {
                TempData["error"] = "Không thể tải thông tin tài khoản";
                return RedirectToAction(nameof(Index));
            }

            var dto = new UpdateProfileDto
            {
                FullName = profile.FullName ?? string.Empty,
                PhoneNumber = profile.PhoneNumber,
                InvoiceDisplayName = profile.InvoiceDisplayName,
                InvoiceEmail = profile.InvoiceEmail,
                InvoiceAddress = profile.InvoiceAddress,
                TaxId = profile.TaxId
            };

            return View(dto);
        }

        // POST /Profile/Edit
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return RedirectToAction("Login", "Account");

            var result = await _authService.UpdateProfileAsync(userId, dto);

            if (!result.Success)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error);

                TempData["error"] = result.Message;
                return View(dto);
            }

            TempData["success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // GET /Profile/ChangePassword
        [HttpGet("ChangePassword")]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST /Profile/ChangePassword
        [HttpPost("ChangePassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return RedirectToAction("Login", "Account");

            var result = await _authService.ChangePasswordAsync(userId, dto);

            if (!result.Success)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error);

                TempData["error"] = result.Message;
                return View(dto);
            }

            TempData["success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}