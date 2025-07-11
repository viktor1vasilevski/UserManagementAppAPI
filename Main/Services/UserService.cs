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

namespace Main.Services;

public class UserService(IUnitOfWork<AppDbContext> _uow) : IUserService
{
    private readonly IGenericRepository<User> _userRepository = _uow.GetGenericRepository<User>();

    public ApiResponse<List<UserDetailsDTO>> GetUsers(UserRequest request)
    {
        var users = _userRepository.GetAsQueryableWhereIf(x =>         
            x.WhereIf(!String.IsNullOrEmpty(request.Username), x => x.Username.ToLower().Contains(request.Username.ToLower())));

        var totalCount = users.Count();

        if (request.Skip.HasValue)
            users = users.Skip(request.Skip.Value);

        if (request.Take.HasValue)
            users = users.Take(request.Take.Value);

        var usersDTO = users.Select(x => new UserDetailsDTO
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
    public ApiResponse<UserDetailsDTO> EditUser(Guid id, EditUserRequest request)
    {
        var user = _userRepository.GetById(id);
        if (user is null)
            return new ApiResponse<UserDetailsDTO>
            {
                Success = false,
                NotificationType = NotificationType.NotFound,
                Message = AuthConstants.UserNotFound
            };

        if (user.Username.ToLower() == AuthConstants.Admin && user.Role == Role.Admin && user.IsActive)
            return new ApiResponse<UserDetailsDTO>
            {
                Success = false,
                Message = UserConstants.CannotEditSuperAdmin,
                NotificationType = NotificationType.Conflict
            };

        try
        {
            user.ApplyChanges(request.FirstName, request.LastName, request.IsActive, request.Role);
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

        if (user.Username.ToLower() == AuthConstants.Admin && user.Role == Role.Admin && user.IsActive)
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
}
