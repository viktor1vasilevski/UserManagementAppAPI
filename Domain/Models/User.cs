using Domain.Enums;
using Domain.Exceptions;
using Domain.Helpers;
using Domain.Models.Base;

namespace Domain.Models;

public class User : AuditableBaseEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public Role Role { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string SaltKey { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private User() { }

    public static User CreateNew(
        string firstName,
        string lastName,
        string username,
        string email,
        string password,
        Role role,
        bool isActive,
        string createdBy)
    {
        ValidateRequired(username, nameof(username));
        ValidateRequired(email, nameof(email));
        ValidateRequired(password, "Password");
        ValidateCoreFields(firstName, lastName, role, createdBy, "CreatedBy");

        var salt = PasswordHelper.GenerateSalt();
        var hash = PasswordHelper.HashPassword(password, salt);

        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            Username = username,
            Email = email,
            PasswordHash = hash,
            SaltKey = Convert.ToBase64String(salt),
            Role = role,
            IsActive = isActive,
            CreatedBy = createdBy,
            Created = DateTime.UtcNow
        };
    }

    public void ApplyChanges(string firstName, string lastName, bool isActive, Role role, string modifiedBy)
    {
        ValidateCoreFields(firstName, lastName, role, modifiedBy, "LastModifiedBy");

        FirstName = firstName;
        LastName = lastName;
        IsActive = isActive;
        Role = role;
        LastModifiedBy = modifiedBy;
        LastModified = DateTime.UtcNow;
    }

    public bool VerifyPassword(string inputPassword)
    {
        return PasswordHelper.VerifyPassword(inputPassword, PasswordHash, SaltKey);
    }

    public void ChangePassword(string newPassword, string modifiedBy)
    {
        ValidateRequired(newPassword, "NewPassword");
        ValidateRequired(modifiedBy, "ModifiedBy");

        var newSalt = PasswordHelper.GenerateSalt();
        PasswordHash = PasswordHelper.HashPassword(newPassword, newSalt);
        SaltKey = Convert.ToBase64String(newSalt);
        LastModifiedBy = modifiedBy;
        LastModified = DateTime.UtcNow;
    }

    private static void ValidateRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException($"{fieldName} cannot be empty.");
    }

    private static void ValidateCoreFields(string firstName, string lastName, Role role, string userRef, string userRefName)
    {
        ValidateRequired(firstName, nameof(firstName));
        ValidateRequired(lastName, nameof(lastName));

        if (!Enum.IsDefined(typeof(Role), role))
            throw new DomainValidationException("Invalid user role specified.");

        ValidateRequired(userRef, userRefName);
    }
}
