namespace Main.Constants;

public static class UserConstants
{
    // Success Messages
    public const string UserCreatedSuccessfully = "User has been created successfully.";
    public const string UserUpdatedSuccessfully = "User details have been updated successfully.";
    public const string UserDeletedSuccessfully = "User has been deleted successfully.";

    // Validation and Business Rule Errors
    public const string UserNotFound = "User not found.";
    public const string UserAlreadyExists = "A user with the given username or email already exists.";
    public const string CannotDeleteSuperAdmin = "This user is a Super Admin and cannot be deleted.";
    public const string CannotEditSuperAdmin = "This user is a Super Admin and cannot be edited.";
    public const string InvalidUserData = "Provided user data is invalid.";

    // Authorization Errors
    public const string UnauthorizedAccess = "You do not have permission to perform this action.";
}

