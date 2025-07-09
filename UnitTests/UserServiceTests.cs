using Domain.Enums;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Context;
using Main.Constants;
using Main.Enums;
using Main.Requests.User;
using Main.Services;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace UnitTests;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork<AppDbContext>> _mockUnitOfWork;
    private readonly Mock<IGenericRepository<User>> _mockUserRepo;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork<AppDbContext>>();
        _mockUserRepo = new Mock<IGenericRepository<User>>();

        _mockUnitOfWork.Setup(u => u.GetGenericRepository<User>())
                       .Returns(_mockUserRepo.Object);

        _userService = new UserService(_mockUnitOfWork.Object);
    }


    [Fact]
    public void DeleteUser_UserNotFound_ReturnsNotFoundResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepo.Setup(r => r.GetById(userId)).Returns((User)null);

        // Act
        var result = _userService.DeleteUser(userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(UserConstants.UserNotFound, result.Message);
        Assert.Equal(NotificationType.NotFound, result.NotificationType);

        // Verify Delete and SaveChanges are NOT called
        _mockUserRepo.Verify(r => r.Delete(It.IsAny<User>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Never);
    }


    [Fact]
    public void DeleteUser_DeleteSuperAdmin_ReturnsConflictResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var superAdminUser = User.CreateNew(
            firstName: "Admin",
            lastName: "Admin",
            username: "admin",
            email: "admin@example.com",
            password: "Test@123",
            role: Role.Admin,
            isActive: true);

        _mockUserRepo.Setup(r => r.GetById(userId)).Returns(superAdminUser);

        // Act
        var result = _userService.DeleteUser(userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(UserConstants.CannotDeleteSuperAdmin, result.Message);
        Assert.Equal(NotificationType.Conflict, result.NotificationType);

        // Verify Delete and SaveChanges are NOT called
        _mockUserRepo.Verify(r => r.Delete(It.IsAny<User>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Never);
    }


    [Fact]
    public void DeleteUser_ValidUser_DeletesUserSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.CreateNew(
            firstName: "Test",
            lastName: "Test",
            username: "test",
            email: "test@example.com",
            password: "Test@123",
            role: Role.User,
            isActive: true);

        _mockUserRepo.Setup(r => r.GetById(userId)).Returns(user);

        // Act
        var result = _userService.DeleteUser(userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(UserConstants.UserDeletedSuccessfully, result.Message);
        Assert.Equal(NotificationType.Success, result.NotificationType);

        _mockUserRepo.Verify(r => r.Delete(user), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);

    }


    [Fact]
    public void EditUser_UserNotFound_ReturnsNotFoundResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new EditUserRequest { FirstName = "Joe", LastName = "Doe", IsActive = true, Role = Role.User };

        _mockUserRepo.Setup(r => r.GetById(userId)).Returns((User)null);

        // Act
        var result = _userService.EditUser(userId, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthConstants.UserNotFound, result.Message);
        Assert.Equal(NotificationType.NotFound, result.NotificationType);

        _mockUserRepo.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Never);
    }


    [Fact]
    public void EditUser_EditSuperAdmin_ReturnsConflictResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new EditUserRequest { FirstName = "NewName", LastName = "NewLast", IsActive = true, Role = Role.Admin };

        var superAdminUser = User.CreateNew(
            firstName: "Admin",
            lastName: "Admin",
            username: "admin",
            email: "admin@example.com",
            password: "Test@123",
            role: Role.Admin,
            isActive: true);

        _mockUserRepo.Setup(r => r.GetById(userId)).Returns(superAdminUser);

        // Act
        var result = _userService.EditUser(userId, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(UserConstants.CannotEditSuperAdmin, result.Message);
        Assert.Equal(NotificationType.Conflict, result.NotificationType);

        _mockUserRepo.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Never);
    }


    [Fact]
    public void EditUser_ValidRequest_UpdatesUserSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new EditUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            IsActive = false,
            Role = Role.User
        };

        var user = User.CreateNew(
            firstName: "Jane",
            lastName: "Smith",
            username: "janesmith",
            email: "jane@example.com",
            password: "Password123!",
            role: Role.User,
            isActive: true
        );

        // Setup repository mock
        _mockUserRepo.Setup(r => r.GetById(userId)).Returns(user);

        // Act
        var result = _userService.EditUser(userId, request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(UserConstants.UserUpdatedSuccessfully, result.Message);
        Assert.Equal(NotificationType.Success, result.NotificationType);
        Assert.NotNull(result.Data);
        Assert.Equal(userId, result.Data.Id);
        Assert.Equal(request.FirstName, user.FirstName);
        Assert.Equal(request.LastName, user.LastName);
        Assert.Equal(request.IsActive, user.IsActive);
        Assert.Equal(request.Role, user.Role);

        _mockUserRepo.Verify(r => r.Update(user), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
    }


    [Fact]
    public void EditUser_InvalidRequest_ThrowsDomainValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new EditUserRequest
        {
            FirstName = "",  // invalid: empty
            LastName = "Doe",
            IsActive = true,
            Role = Role.User
        };

        var user = User.CreateNew(
            firstName: "Jane",
            lastName: "Smith",
            username: "janesmith",
            email: "jane@example.com",
            password: "Password123!",
            role: Role.User,
            isActive: true
        );

        _mockUserRepo.Setup(r => r.GetById(userId)).Returns(user);

        // Act
        var result = _userService.EditUser(userId, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(NotificationType.BadRequest, result.NotificationType);
        Assert.Contains("cannot be empty", result.Message);

        _mockUserRepo.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Never);
    }


    [Fact]
    public void GetUserById_UserExists_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.CreateNew(
            "John", "Doe", "johndoe", "john@example.com", "Password123!", Role.User, true);
        _mockUserRepo.Setup(r => r.GetById(userId)).Returns(user);

        // Act
        var result = _userService.GetUserById(userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(user.Id, result.Data.Id);
        Assert.Equal(user.FirstName, result.Data.FirstName);
    }

    [Fact]
    public void GetUserById_UserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepo.Setup(r => r.GetById(userId)).Returns((User)null);

        // Act
        var result = _userService.GetUserById(userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(NotificationType.NotFound, result.NotificationType);
    }


    [Fact]
    public void GetUsers_WithUsernameFilter_ReturnsFilteredUsers()
    {
        // Arrange
        var users = new List<User>
        {
            User.CreateNew("Alice", "Smith", "alice", "alice@example.com", "Pass123!", Role.User, true),
            User.CreateNew("Bob", "Brown", "bob", "bob@example.com", "Pass123!", Role.User, true),
            User.CreateNew("Charlie", "Davis", "charlie", "charlie@example.com", "Pass123!", Role.User, true)
        }.AsQueryable();

        _mockUserRepo.Setup(r => r.GetAsQueryableWhereIf(
                It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(),
                It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .Returns((Func<IQueryable<User>, IQueryable<User>> filter,
                      Func<IQueryable<User>, IOrderedQueryable<User>> orderBy,
                      Func<IQueryable<User>, IIncludableQueryable<User, object>> include) =>
            {
                IQueryable<User> query = users;
                if (filter != null) query = filter(query);
                if (orderBy != null) query = orderBy(query);
                if (include != null) query = include(query);
                return query;
            });

        var request = new UserRequest
        {
            Username = "ali",
            Skip = 0,
            Take = 10
        };

        // Act
        var response = _userService.GetUsers(request);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(1, response.Data.Count);
        Assert.Contains(response.Data, u => u.Username == "alice");
        Assert.Equal(1, response.TotalCount);
    }


    [Fact]
    public void GetUsers_WithPagination_ReturnsCorrectSubset()
    {
        // Arrange
        var users = new List<User>
        {
            User.CreateNew("Alice", "Smith", "alice", "alice@example.com", "Pass123!", Role.User, true),
            User.CreateNew("Bob", "Brown", "bob", "bob@example.com", "Pass123!", Role.User, true),
            User.CreateNew("Charlie", "Davis", "charlie", "charlie@example.com", "Pass123!", Role.User, true)
        }.AsQueryable();

        _mockUserRepo.Setup(r => r.GetAsQueryableWhereIf(
                It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(),
                It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
            .Returns((Func<IQueryable<User>, IQueryable<User>> filter,
                      Func<IQueryable<User>, IOrderedQueryable<User>> orderBy,
                      Func<IQueryable<User>, IIncludableQueryable<User, object>> include) =>
            {
                IQueryable<User> query = users;
                if (filter != null) query = filter(query);
                if (orderBy != null) query = orderBy(query);
                if (include != null) query = include(query);
                return query;
            });

        var request = new UserRequest
        {
            Skip = 1,
            Take = 1
        };

        // Act
        var response = _userService.GetUsers(request);

        // Assert
        Assert.True(response.Success);
        Assert.Single(response.Data);
        Assert.Equal("bob", response.Data.First().Username);
        Assert.Equal(3, response.TotalCount);
    }

}
