using System.ComponentModel.DataAnnotations;
using ElegantSuits.Domain.Enums;

namespace ElegantSuits.Application.Features.Auth.Contracts;

public class AuthResponseDTO
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public DateTime? Expiration { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public List<string>? Roles { get; set; }
}

public class LoginUserDTO
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; } = false;
}

public class RegisterUserDTO
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPassword { get; set; } = "";

    [Required(ErrorMessage = "Họ và tên không được để trống")]
    public string FullName { get; set; } = "";

    public Gender Gender { get; set; } = Gender.Male;

    [Required(ErrorMessage = "Ngày sinh không được để trống")]
    public DateTime DateOfBirth { get; set; } = DateTime.Now;

    public string? PhoneNumber { get; set; }

    public string Address { get; set; } = "";
}

public class ExternalLoginDTO
{
    [Required(ErrorMessage = "Provider không được để trống")]
    public string Provider { get; set; } = "";

    [Required(ErrorMessage = "ProviderKey không được để trống")]
    public string ProviderKey { get; set; } = "";

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = "";

    public string? FullName { get; set; }
}
