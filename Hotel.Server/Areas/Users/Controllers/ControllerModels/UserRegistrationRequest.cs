using System.ComponentModel.DataAnnotations;

namespace Hotel.Server.Users;

public class UserRegistrationRequest
{
    [Required] public string Email { get; set; }
    [Required] public string Password { get; set; }
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
}
