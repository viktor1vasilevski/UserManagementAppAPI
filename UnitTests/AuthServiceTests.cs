using Domain.Enums;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Context;
using Main.Constants;
using Main.Enums;
using Main.Requests.Auth;
using Main.Services;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Linq.Expressions;

namespace UnitTests;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork<AppDbContext>> _mockUnitOfWork;
    private readonly Mock<IGenericRepository<User>> _mockUserRepo;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork<AppDbContext>>();
        _mockUserRepo = new Mock<IGenericRepository<User>>();
        _mockConfig = new Mock<IConfiguration>();

        _mockUnitOfWork.Setup(u => u.GetGenericRepository<User>())
                       .Returns(_mockUserRepo.Object);

        // Setup Jwt secret config value
        _mockConfig.Setup(c => c["JwtSettings:Secret"])
                   .Returns("ASecretKeyThatIsAtLeast32CharactersLong!");

        _authService = new AuthService(_mockUnitOfWork.Object, _mockConfig.Object);
    }


    [Fact]
    public async Task UserLoginAsync_ValidAdminUser_ReturnsToken()
    {
        // Arrange
        var password = "Password123!";
        var user = User.CreateNew("Admin", "User", "adminuser", "admin@example.com", password, Role.Admin, true);

        _mockUserRepo.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(new List<User> { user });

        var request = new UserLoginRequest
        {
            Username = "adminuser",
            Password = password
        };

        // Act
        var result = await _authService.UserLoginAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(NotificationType.Success, result.NotificationType);
        Assert.Equal(AuthConstants.UserLoginSuccess, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(user.Username, result.Data.Username);
        Assert.False(string.IsNullOrEmpty(result.Data.Token));
    }

    [Fact]
    public async Task UserLoginAsync_InvalidPassword_ReturnsNotFoundResponse()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword!";
        var user = User.CreateNew("John", "Doe", "johndoe", "john@example.com", correctPassword, Role.User, true);

        _mockUserRepo.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(new List<User> { user });

        var request = new UserLoginRequest
        {
            Username = "johndoe",
            Password = wrongPassword
        };

        // Act
        var result = await _authService.UserLoginAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(NotificationType.NotFound, result.NotificationType);
        Assert.Equal(AuthConstants.InvalidPassword, result.Message);
        Assert.Null(result.Data?.Token);
    }


    [Fact]
    public async Task UserLoginAsync_UserNotFound_ReturnsNotFoundResponse()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
            It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .ReturnsAsync(new List<User>());

        var request = new UserLoginRequest
        {
            Username = "nonexistentuser",
            Password = "AnyPassword"
        };

        // Act
        var result = await _authService.UserLoginAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(NotificationType.NotFound, result.NotificationType);
        Assert.Equal(AuthConstants.UserNotFound, result.Message);
        Assert.Null(result.Data);
    }


    [Fact]
    public async Task UserRegisterAsync_UserAlreadyExists_ReturnsConflict()
    {
        // Arrange
        var request = new UserRegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "Password123!",
            Role = Role.User,
            IsActive = true
        };

        _mockUserRepo
            .Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.UserRegisterAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(NotificationType.Conflict, result.NotificationType);
        Assert.Equal(AuthConstants.AccountAlreadyExists, result.Message);
        Assert.Null(result.Data);

        _mockUserRepo.Verify(r => r.InsertAsync(It.IsAny<User>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UserRegisterAsync_ValidRequest_ReturnsSuccessAndUserData()
    {
        // Arrange
        var request = new UserRegisterRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Username = "janedoe",
            Email = "jane@example.com",
            Password = "Password123!",
            Role = Role.User,
            IsActive = true
        };

        _mockUserRepo
            .Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(false);

        _mockUserRepo
            .Setup(repo => repo.InsertAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        // Act
        var result = await _authService.UserRegisterAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(NotificationType.Success, result.NotificationType);
        Assert.Equal(AuthConstants.CustomerRegisterSuccess, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(request.FirstName, result.Data.FirstName);
        Assert.Equal(request.LastName, result.Data.LastName);
        Assert.Equal(request.Username, result.Data.Username);
        Assert.Equal(request.Email, result.Data.Email);
        Assert.Equal(request.Role, result.Data.Role);
        Assert.Equal(request.IsActive, result.Data.IsActive);

        _mockUserRepo.Verify(r => r.InsertAsync(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }


    [Fact]
    public async Task UserRegisterAsync_InvalidRequest_ThrowsDomainValidationException_ReturnsBadRequest()
    {
        // Arrange
        var request = new UserRegisterRequest
        {
            FirstName = "",
            LastName = "Doe",
            Username = "janedoe",
            Email = "jane@example.com",
            Password = "Password123!",
            Role = Role.User,
            IsActive = true
        };

        _mockUserRepo
            .Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var result = await _authService.UserRegisterAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(NotificationType.BadRequest, result.NotificationType);
        Assert.Contains("cannot be empty", result.Message);

        _mockUserRepo.Verify(r => r.InsertAsync(It.IsAny<User>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
