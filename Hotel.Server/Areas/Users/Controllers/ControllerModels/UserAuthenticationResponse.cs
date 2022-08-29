using System.Collections.Generic;

namespace Hotel.Server.Users;

public class UserAuthenticationResponse
{
    public string Token { get; set; }
    public UserResponse User { get; set; }

    public UserAuthenticationResponse() { }

    public UserAuthenticationResponse(string token, UserResponse user)
    {
        this.Token = token;
        this.User = user;
    }
}
