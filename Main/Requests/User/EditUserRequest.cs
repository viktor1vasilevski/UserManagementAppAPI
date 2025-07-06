using Domain.Enums;

namespace Main.Requests.User;

public class EditUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Role Role { get; set; }
    public bool IsActive { get; set; }
}
