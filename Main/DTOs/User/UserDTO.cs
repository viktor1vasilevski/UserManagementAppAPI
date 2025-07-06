using Domain.Enums;

namespace Main.DTOs.User;

public class UserDTO
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Role Role { get; set; }
    public bool IsActive { get; set; }
}
