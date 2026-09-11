using ElegantSuits.Domain.Enums;

namespace ElegantSuits.Application.Features.Users.Contracts;

public class UserDTO
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string Address { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
    public Gender Gender { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateUserDTO
{
    public string FullName { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string Address { get; set; } = "";
    public Gender Gender { get; set; } = Gender.Male;
    public string? AvatarUrl { get; set; }
}
