using System.ComponentModel.DataAnnotations;

namespace EVCS.Services.DTOs.Profile
{
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(128, ErrorMessage = "Họ tên không được vượt quá 128 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [StringLength(128, ErrorMessage = "Tên hiển thị hóa đơn không được vượt quá 128 ký tự")]
        [Display(Name = "Tên hiển thị trên hóa đơn")]
        public string? InvoiceDisplayName { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(256, ErrorMessage = "Email không được vượt quá 256 ký tự")]
        [Display(Name = "Email nhận hóa đơn")]
        public string? InvoiceEmail { get; set; }

        [StringLength(256, ErrorMessage = "Địa chỉ không được vượt quá 256 ký tự")]
        [Display(Name = "Địa chỉ xuất hóa đơn")]
        public string? InvoiceAddress { get; set; }

        [StringLength(32, ErrorMessage = "Mã số thuế không được vượt quá 32 ký tự")]
        [Display(Name = "Mã số thuế")]
        public string? TaxId { get; set; }
    }
}