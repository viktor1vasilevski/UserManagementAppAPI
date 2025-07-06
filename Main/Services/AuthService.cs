using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Context;
using Main.Constants;
using Main.DTOs.Auth;
using Main.Enums;
using Main.Interfaces;
using Main.Requests.Auth;
using Main.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Main.Services;

public class AuthService(IUnitOfWork<AppDbContext> _uow, IConfiguration _configuration, ILogger<AuthService> _logger) : IAuthService
{
    private readonly IGenericRepository<User> _userRepository = _uow.GetGenericRepository<User>();

    public async Task<ApiResponse<UserLoginDTO>> UserLoginAsync(UserLoginRequest request)
    {
        var response = await _userRepository.GetAsync(x => x.Username.ToLower() == request.Username.ToLower());
        var user = response?.FirstOrDefault();

        if (user is null)
            return new ApiResponse<UserLoginDTO>
            {
                Message = AuthConstants.USER_NOT_FOUND,
                Success = false,
                NotificationType = NotificationType.NotFound
            };

        if (!user.VerifyPassword(request.Password))
            return new ApiResponse<UserLoginDTO>
            {
                Message = AuthConstants.INVALID_PASSWORD,
                Success = false,
                NotificationType = NotificationType.NotFound
            };

        var token = user.Role == Role.Admin ? GenerateJwtToken(user) : null;

        return new ApiResponse<UserLoginDTO>
        {
            Success = true,
            NotificationType = NotificationType.Success,
            Message = AuthConstants.USER_LOGIN_SUCCESS,
            Data = new UserLoginDTO
            {
                Id = user.Id,
                Token = token,
                Username = user.Username,
                Role = user.Role.ToString(),
                IsActive = user.IsActive
            }
        };
    }

    public async Task<ApiResponse<UserRegisterDTO>> UserRegisterAsync(UserRegisterRequest request, string createdBy)
    {
        var userExist = await _userRepository.ExistsAsync(x =>
            x.Email.ToLower() == request.Email.ToLower() || x.Username.ToLower() == request.Username.ToLower());

        if (userExist)
            return new ApiResponse<UserRegisterDTO>
            {
                Success = false,
                NotificationType = NotificationType.Conflict,
                Message = AuthConstants.ACCOUNT_ALREADY_EXISTS
            };

        try
        {
            var user = User.CreateNew(
                firstName: request.FirstName,
                lastName: request.LastName,
                username: request.Username,
                email: request.Email,
                password: request.Password,
                role: request.Role,
                isActive: request.IsActive,
                createdBy: createdBy);

            await _userRepository.InsertAsync(user);
            await _uow.SaveChangesAsync();

            return new ApiResponse<UserRegisterDTO>
            {
                Success = true,
                NotificationType = NotificationType.Success,
                Message = AuthConstants.CUSTOMER_REGISTER_SUCCESS,
                Data = new UserRegisterDTO { Username = user.Username }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<UserRegisterDTO>
            {
                Success = false,
                NotificationType = NotificationType.BadRequest,
                Message = ex.Message
            };
        }
    }

    private string GenerateJwtToken(User user)
    {
        var secretKey = _configuration["JwtSettings:Secret"] ?? "AlternativeSecretKeyOfAtLeast32Characters!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[] 
        { 
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Username)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(22),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
