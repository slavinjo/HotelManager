using System.ComponentModel.DataAnnotations;

namespace Hotel.Server.Users;

public class ForgotPasswordRequest
{
    [Required] public string Email { get; set; }
}
