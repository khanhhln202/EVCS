using EVCS.Services.DTOs.Auth;
using EVCS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVCS.Web.Controllers
{
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // GET /Account/Login
        [HttpGet("Login")]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST /Account/Login
        [HttpPost("Login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _authService.LoginAsync(dto);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(dto);
            }

            TempData["success"] = "Đăng nhập thành công!";

            if (!string.IsNullOrEmpty(dto.ReturnUrl) && Url.IsLocalUrl(dto.ReturnUrl))
                return Redirect(dto.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        // GET /Account/Register
        [HttpGet("Register")]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST /Account/Register
        [HttpPost("Register")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error);
                
                return View(dto);
            }

            TempData["success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        // POST /Account/Logout
        [HttpPost("Logout")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            TempData["success"] = "Đã đăng xuất.";
            return RedirectToAction("Index", "Home");
        }

        // GET /Account/AccessDenied
        [HttpGet("AccessDenied")]
        [AllowAnonymous]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            _logger.LogWarning("Access denied for path: {ReturnUrl}", returnUrl ?? "unknown");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // API endpoint để kiểm tra email (dùng cho AJAX validation)
        [HttpGet("CheckEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckEmail(string email)
        {
            var exists = await _authService.IsEmailExistsAsync(email);
            return Json(new { exists });
        }
    }
}