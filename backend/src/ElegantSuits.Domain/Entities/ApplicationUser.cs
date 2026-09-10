using System.ComponentModel.DataAnnotations;
using ElegantSuits.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace ElegantSuits.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(50)]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = "";

    [Required]
    [Display(Name = "Ngày sinh")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; } = DateTime.Now;

    [StringLength(200)]
    [Display(Name = "Địa chỉ")]
    public string Address { get; set; } = "";

    [Display(Name = "Ảnh đại diện")]
    public string? AvatarUrl { get; set; } = "/images/users/default-avatar.png";

    [Display(Name = "Ngày tạo")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Display(Name = "Trạng thái")]
    public bool IsActive { get; set; } = true;

    [Required]
    [Display(Name = "Giới tính")]
    public Gender Gender { get; set; } = Gender.Male;

    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có 10 chữ số")]
    [Display(Name = "Số điện thoại")]
    public override string? PhoneNumber { get; set; }

    // OAuth properties
    [Display(Name = "Nhà cung cấp đăng nhập")]
    public string? LoginProvider { get; set; }

    [Display(Name = "ID nhà cung cấp")]
    public string? ProviderKey { get; set; }

    [Display(Name = "Đăng nhập bằng OAuth")]
    public bool IsOAuthUser { get; set; } = false;
}
