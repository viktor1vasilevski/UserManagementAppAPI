using Domain.Enums;
using Domain.Models.Base;

namespace Domain.Models;

public class User : AuditableBaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public Role Role { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string SaltKey { get; set; }
    public bool IsActive { get; set; }
}
