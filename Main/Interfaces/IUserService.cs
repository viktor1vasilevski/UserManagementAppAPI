using Main.DTOs.User;
using Main.Requests.User;
using Main.Responses;

namespace Main.Interfaces;

public interface IUserService
{
    ApiResponse<List<UserDetailsDTO>> GetUsers(UserRequest request);
    ApiResponse<UserDTO> GetUserById(Guid id);
    ApiResponse<UserDetailsDTO> EditUser(Guid id, EditUserRequest request, string modifiedBy);
    ApiResponse<string> DeleteUser(Guid id);
}
