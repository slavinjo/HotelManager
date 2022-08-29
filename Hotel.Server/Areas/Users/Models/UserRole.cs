namespace Hotel.Server.Users;

public class UserRole
{
    public const string Admin = "admin";
    public const string User = "user";

    // helper for permission level
    public const string Any = "any";

    public static bool IsValidRole(string modelRole)
    {
        return modelRole is UserRole.Admin or UserRole.User;
    }
}
