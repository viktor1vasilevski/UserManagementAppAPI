using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;

namespace UnitTests;

public class UserTests
{
    [Fact]
    public void CreateNew_WithValidData_CreatesUserSuccessfully()
    {
        var user = User.CreateNew(
            firstName: "John",
            lastName: "Doe",
            username: "johndoe",
            email: "john@example.com",
            password: "Password123!",
            role: Role.User,
            isActive: true
        );

        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal("johndoe", user.Username);
        Assert.Equal("john@example.com", user.Email);
        Assert.True(user.IsActive);
        Assert.Equal(Role.User, user.Role);
        Assert.NotEmpty(user.PasswordHash);
        Assert.NotEmpty(user.SaltKey);
    }


    [Fact]
    public void CreateNew_WithEmptyFirstName_ThrowsDomainValidationException()
    {
        var ex = Assert.Throws<DomainValidationException>(() => User.CreateNew(
            firstName: "",
            lastName: "Doe",
            username: "johndoe",
            email: "john@example.com",
            password: "Password123!",
            role: Role.User,
            isActive: true
        ));

        Assert.Contains("firstName cannot be empty", ex.Message, StringComparison.OrdinalIgnoreCase);
    }



    [Fact]
    public void ApplyChanges_WithValidData_UpdatesUser()
    {
        var user = User.CreateNew("John", "Doe", "johndoe", "john@example.com", "Password123!", Role.User, true);

        user.ApplyChanges("Jane", "Smith", false, Role.Admin);

        Assert.Equal("Jane", user.FirstName);
        Assert.Equal("Smith", user.LastName);
        Assert.False(user.IsActive);
        Assert.Equal(Role.Admin, user.Role);
    }


    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var user = User.CreateNew("John", "Doe", "johndoe", "john@example.com", "Password123!", Role.User, true);
        Assert.True(user.VerifyPassword("Password123!"));
    }


    [Fact]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        var user = User.CreateNew("John", "Doe", "johndoe", "john@example.com", "Password123!", Role.User, true);
        Assert.False(user.VerifyPassword("WrongPassword"));
    }
}
