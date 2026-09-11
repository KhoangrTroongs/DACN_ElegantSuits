using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Auth.Contracts;
using ElegantSuits.Domain.Entities;
using ElegantSuits.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ElegantSuits.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginUserDTO loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return new AuthResponseDTO
            {
                IsSuccess = false,
                Message = "Email hoặc mật khẩu không đúng."
            };
        }

        if (!user.IsActive)
        {
            return new AuthResponseDTO
            {
                IsSuccess = false,
                Message = "Tài khoản của bạn đã bị vô hiệu hóa."
            };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
        {
            return new AuthResponseDTO
            {
                IsSuccess = false,
                Message = "Email hoặc mật khẩu không đúng."
            };
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, userRoles);

        return new AuthResponseDTO
        {
            IsSuccess = true,
            Message = "Đăng nhập thành công.",
            Token = token,
            Expiration = DateTime.Now.AddDays(7),
            UserId = user.Id,
            UserName = user.UserName,
            Roles = userRoles.ToList()
        };
    }

    public async Task<AuthResponseDTO> RegisterAsync(RegisterUserDTO registerDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return new AuthResponseDTO
            {
                IsSuccess = false,
                Message = "Email đã tồn tại trong hệ thống."
            };
        }

        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            DateOfBirth = registerDto.DateOfBirth,
            PhoneNumber = string.IsNullOrWhiteSpace(registerDto.PhoneNumber) ? null : registerDto.PhoneNumber,
            Address = registerDto.Address ?? "",
            Gender = registerDto.Gender,
            CreatedAt = DateTime.Now,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            return new AuthResponseDTO
            {
                IsSuccess = false,
                Message = string.Join("; ", result.Errors.Select(e => e.Description))
            };
        }

        await _userManager.AddToRoleAsync(user, "User");

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        return new AuthResponseDTO
        {
            IsSuccess = true,
            Message = "Đăng ký tài khoản thành công.",
            Token = token,
            Expiration = DateTime.Now.AddDays(7),
            UserId = user.Id,
            UserName = user.UserName,
            Roles = roles.ToList()
        };
    }

    public async Task<AuthResponseDTO> ExternalLoginAsync(ExternalLoginDTO externalLoginDto)
    {
        var user = await _userManager.FindByLoginAsync(externalLoginDto.Provider, externalLoginDto.ProviderKey);
        if (user == null)
        {
            user = await _userManager.FindByEmailAsync(externalLoginDto.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = externalLoginDto.Email,
                    Email = externalLoginDto.Email,
                    FullName = !string.IsNullOrWhiteSpace(externalLoginDto.FullName) ? externalLoginDto.FullName : externalLoginDto.Email,
                    EmailConfirmed = true,
                    IsOAuthUser = true,
                    LoginProvider = externalLoginDto.Provider,
                    ProviderKey = externalLoginDto.ProviderKey,
                    DateOfBirth = DateTime.Now,
                    Gender = Gender.Male,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return new AuthResponseDTO
                    {
                        IsSuccess = false,
                        Message = string.Join("; ", createResult.Errors.Select(e => e.Description))
                    };
                }

                await _userManager.AddToRoleAsync(user, "User");
            }

            var logins = await _userManager.GetLoginsAsync(user);
            if (!logins.Any(l => l.LoginProvider == externalLoginDto.Provider && l.ProviderKey == externalLoginDto.ProviderKey))
            {
                await _userManager.AddLoginAsync(user, new UserLoginInfo(externalLoginDto.Provider, externalLoginDto.ProviderKey, externalLoginDto.Provider));
            }
        }

        if (!user.IsActive)
        {
            return new AuthResponseDTO
            {
                IsSuccess = false,
                Message = "Tài khoản của bạn đã bị vô hiệu hóa."
            };
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Any())
        {
            await _userManager.AddToRoleAsync(user, "User");
            roles = await _userManager.GetRolesAsync(user);
        }

        var token = GenerateJwtToken(user, roles);

        return new AuthResponseDTO
        {
            IsSuccess = true,
            Message = "Đăng nhập thành công.",
            Token = token,
            Expiration = DateTime.Now.AddDays(7),
            UserId = user.Id,
            UserName = user.UserName,
            Roles = roles.ToList()
        };
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var secret = _configuration["JWT:Secret"] ?? "DefaultSecretKeyWithAtLeast32Characters!";
        var issuer = _configuration["JWT:ValidIssuer"] ?? "https://localhost:5001";
        var audience = _configuration["JWT:ValidAudience"] ?? "https://localhost:5001";

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            expires: DateTime.Now.AddDays(7),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
