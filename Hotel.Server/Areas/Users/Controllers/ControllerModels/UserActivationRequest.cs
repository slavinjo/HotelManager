using System.ComponentModel.DataAnnotations;

namespace Hotel.Server.Users;

public class UserActivationRequest
{
    [Required] public string Email { get; set; }
    [Required] public string ActivationCode { get; set; }
}
