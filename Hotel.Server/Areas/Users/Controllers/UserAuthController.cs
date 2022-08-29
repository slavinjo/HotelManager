using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hotel.Server.Helpers;

namespace Hotel.Server.Users;

[ApiController]
[Route("/api/v1/users/auth")]
public class UserAuthController : ControllerBase
{
    private readonly UserService _userService;

    public UserAuthController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("authenticate")]
    public async Task<ActionResult<UserAuthenticationResponse>> Authenticate([FromBody] UserLoginRequest model)
    {
        var authResponse = await _userService.Authenticate(model.Username, model.Password);

        return Ok(authResponse);
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserAuthenticationResponse>> Register([FromBody] UserRegistrationRequest model)
    {
        var authResponse = await _userService.Register(model);

        return Ok(authResponse);
    }

    [HttpPost("activate")]
    public async Task<ActionResult<UserAuthenticationResponse>> Activate([FromBody] UserActivationRequest model)
    {
        var authResponse = await _userService.Activate(model);
        return Ok(authResponse);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model)
    {
        await _userService.ForgotPassword(model);
        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
    {
        await _userService.ResetPassword(model);
        return Ok();
    }
}
