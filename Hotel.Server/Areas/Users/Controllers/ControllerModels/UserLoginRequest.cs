using System.ComponentModel.DataAnnotations;

namespace Hotel.Server.Users;

public class UserLoginRequest
{
    [Required] public string Username { get; set; }
    [Required] public string Password { get; set; }
}
