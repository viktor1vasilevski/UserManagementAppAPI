using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Context;
using Main.Constants;
using Main.DTOs.User;
using Main.Enums;
using Main.Extension;
using Main.Interfaces;
using Main.Requests.User;
using Main.Responses;
using Microsoft.Extensions.Logging;

namespace Main.Services;

public class UserService(IUnitOfWork<AppDbContext> _uow, ILogger<AuthService> _logger) : IUserService
{
    private readonly IGenericRepository<User> _userRepository = _uow.GetGenericRepository<User>();

    public ApiResponse<string> DeleteUser(Guid id)
    {
        var user = _userRepository.GetById(id);

        if (user is null)
            return new ApiResponse<string>
            {
                Success = false,
                Message = UserConstants.UserNotFound,
                NotificationType = NotificationType.NotFound
            };

        if (user.Username.ToLower() == "admin" && user.Role == Role.Admin && user.IsActive)
            return new ApiResponse<string>
            {
                Success = false,
                Message = UserConstants.CannotDeleteSuperAdmin,
                NotificationType = NotificationType.Conflict
            };

        _userRepository.Delete(user);
        _uow.SaveChanges();

        return new ApiResponse<string>
        {
            Success = true,
            Message = UserConstants.UserDeletedSuccessfully,
            NotificationType = NotificationType.Success
        };
    }

    public ApiResponse<UserDetailsDTO> EditUser(Guid id, EditUserRequest request, string modifiedBy)
    {
        var user = _userRepository.GetById(id);
        if (user is null)
            return new ApiResponse<UserDetailsDTO>
            {
                Success = false,
                NotificationType = NotificationType.NotFound,
                Message = AuthConstants.USER_NOT_FOUND
            };

        try
        {
            user.ApplyChanges(request.FirstName, request.LastName, request.IsActive, request.Role, modifiedBy);
            _userRepository.Update(user);
            _uow.SaveChanges();

            return new ApiResponse<UserDetailsDTO>
            {
                Success = true,
                NotificationType = NotificationType.Success,
                Message = UserConstants.UserUpdatedSuccessfully,
                Data = new UserDetailsDTO 
                { 
                    Id = id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.Username,
                    IsActive = user.IsActive,
                    Role = user.Role.ToString(),
                    CreatedBy = user.CreatedBy,
                    Created = user.Created,
                    LastModifiedBy = user.LastModifiedBy,
                    LastModified = user.LastModified
                }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<UserDetailsDTO>
            {
                Success = false,
                NotificationType = NotificationType.BadRequest,
                Message = ex.Message
            };
        }
    }

    public ApiResponse<UserDTO> GetUserById(Guid id)
    {
        var user = _userRepository.GetById(id);

        if (user is null)
            return new ApiResponse<UserDTO>
            {
                Success = false,
                NotificationType = NotificationType.NotFound,
                Message = UserConstants.UserNotFound
            };

        return new ApiResponse<UserDTO>
        {
            Success = true,
            NotificationType = NotificationType.Success,
            Data = new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                IsActive = user.IsActive
            }
        };
    }

    public ApiResponse<List<UserDetailsDTO>> GetUsers(UserRequest request)
    {
        var products = _userRepository.GetAsQueryableWhereIf(x =>         
            x.WhereIf(!String.IsNullOrEmpty(request.Username), x => x.Username.ToLower().Contains(request.Username.ToLower())));

        var totalCount = products.Count();

        if (request.Skip.HasValue)
            products = products.Skip(request.Skip.Value);

        if (request.Take.HasValue)
            products = products.Take(request.Take.Value);

        var usersDTO = products.Select(x => new UserDetailsDTO
        {
            Id = x.Id,
            FirstName = x.FirstName,
            LastName = x.LastName,
            Username = x.Username,
            Email = x.Email,
            Role = x.Role.ToString(),
            IsActive = x.IsActive,
            Created = x.Created,
            CreatedBy = x.CreatedBy,
            LastModified = x.LastModified,
            LastModifiedBy = x.LastModifiedBy
        }).ToList();

        return new ApiResponse<List<UserDetailsDTO>>()
        {
            Success = true,
            Data = usersDTO,
            TotalCount = totalCount,
            NotificationType = NotificationType.Success
        };
    }
}
