using Main.DTOs.User;
using Main.Requests.User;
using Main.Responses;

namespace Main.Interfaces;

public interface IUserService
{
    ApiResponse<List<UserDTO>> GetUsers(UserRequest request);
    ApiResponse<string> DeleteUser(Guid id);
}
