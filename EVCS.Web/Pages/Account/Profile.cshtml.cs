using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EVCS.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVCS.Web.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
            [Display(Name = "Tên đăng nhập")]
            public string? UserName { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [Display(Name = "Email")]
            public string? Email { get; set; }

            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [Display(Name = "Số điện thoại")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Ngày tạo")]
            public DateTime CreatedAt { get; set; }

            // New: Profile
            [MaxLength(128)]
            [Display(Name = "Họ và Tên")]
            public string? FullName { get; set; }

            // New: Billing
            [MaxLength(128)]
            [Display(Name = "Tên hiển thị hóa đơn")]
            public string? InvoiceDisplayName { get; set; }

            [EmailAddress(ErrorMessage = "Email không hợp lệ"), MaxLength(256)]
            [Display(Name = "Email nhận hóa đơn")]
            public string? InvoiceEmail { get; set; }

            [MaxLength(32)]
            [Display(Name = "(Doanh nghiệp) Mã số thuế")]
            public string? TaxId { get; set; }

            [MaxLength(256)]
            [Display(Name = "Địa chỉ xuất hóa đơn")]
            public string? InvoiceAddress { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            Input = new InputModel
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,

                FullName = user.FullName,
                InvoiceDisplayName = user.InvoiceDisplayName,
                InvoiceEmail = user.InvoiceEmail,
                TaxId = user.TaxId,
                InvoiceAddress = user.InvoiceAddress
            };

            StatusMessage = TempData["StatusMessage"] as string;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Identity core fields
            if (!string.Equals(Input.UserName, user.UserName, StringComparison.Ordinal))
            {
                var res = await _userManager.SetUserNameAsync(user, Input.UserName!);
                if (!res.Succeeded) { AddErrors(res); return Page(); }
            }
            if (!string.Equals(Input.Email, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var res = await _userManager.SetEmailAsync(user, Input.Email!);
                if (!res.Succeeded) { AddErrors(res); return Page(); }
            }
            if (!string.Equals(Input.PhoneNumber, user.PhoneNumber, StringComparison.Ordinal))
            {
                var res = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!res.Succeeded) { AddErrors(res); return Page(); }
            }

            // Custom fields
            user.FullName = Input.FullName?.Trim();
            user.InvoiceDisplayName = Input.InvoiceDisplayName?.Trim();
            user.InvoiceEmail = string.IsNullOrWhiteSpace(Input.InvoiceEmail) ? null : Input.InvoiceEmail!.Trim();
            user.TaxId = string.IsNullOrWhiteSpace(Input.TaxId) ? null : Input.TaxId!.Trim();
            user.InvoiceAddress = string.IsNullOrWhiteSpace(Input.InvoiceAddress) ? null : Input.InvoiceAddress!.Trim();

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded) { AddErrors(update); return Page(); }

            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Đã cập nhật hồ sơ.";
            return RedirectToPage();
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
        }
    }
}