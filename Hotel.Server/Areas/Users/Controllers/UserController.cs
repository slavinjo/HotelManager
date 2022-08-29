using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Hotel.Server.Helpers;

namespace Hotel.Server.Users;

[ApiController]
[Route("/api/v1/users")]
public class UserController : ControllerBaseExtended
{
    private readonly UserService _userService;
    private readonly DateTimeProvider _dateTimeProvider;
    private readonly IMapper _mapper;

    public UserController(UserService userService, DateTimeProvider dateTimeProvider, IMapper mapper)
    {
        _userService = userService;
        _dateTimeProvider = dateTimeProvider;
        _mapper = mapper;
    }

    [HttpGet]
    [PermissionLevel(UserRole.Admin)]
    public async Task<ActionResult<PagedApiResponse<UserResponse>>> GetAll([FromQuery] RequestParameters requestParams)
    {
        var users = await _userService.GetUsersApi<UserResponse>(requestParams);
        return Ok(users);
    }

    [HttpGet("me")]
    [PermissionLevel(UserRole.Any)]
    public async Task<ActionResult<UserResponse>> GetCurrentUser()
    {
        return await _userService.GetUserApi(this.UserId);
    }

    [HttpPut("me")]
    [PermissionLevel(UserRole.Any)]
    public async Task<ActionResult<UserResponse>> UpdateCurrentUser(UserUpdateRequest model)
    {
        var user = await _userService.GetUserById(this.UserId);
        model.Role = user.Role;

        return await Update(model, this.UserId);
    }

    [HttpGet("{id}")]
    [PermissionLevel(UserRole.Admin)]
    public async Task<ActionResult<UserResponse>> GetUser(Guid id)
    {
        var user = await _userService.GetUserApi(id) ?? throw new NotFoundException();

        return Ok(user);
    }

    [HttpGet("email")]
    public async Task<ActionResult<UserEmailResponse>> CheckUserEmail([FromQuery] string email)
    {
        var user = await _userService.GetUserByEmail(email);

        return Ok(new UserEmailResponse {Exists = user != null});
    }

    [HttpPost]
    [PermissionLevel(UserRole.Admin)]
    public async Task<ActionResult<UserResponse>> Add([FromBody] UserAddRequest model)
    {
        if (!UserRole.IsValidRole(model.Role))
            throw new BadRequestException("Invalid user role");

        var user = _mapper.Map<User>(model);

        if (model.Password != null)
            user.Password = AuthenticationHelper.HashPassword(user, model.Password);

        user.ActivatedAt = _dateTimeProvider.UtcNow;

        user = await _userService.AddUser(user);

        var result = await _userService.GetUserApi(user.Id);

        return Created("", result);
    }

    [HttpPut("{id}")]
    [PermissionLevel(UserRole.Admin)]
    public async Task<ActionResult<UserResponse>> Update([FromBody] UserUpdateRequest model, Guid id)
    {
        var user = await _userService.GetUserById(id) ?? throw new NotFoundException();

        _mapper.Map(model, user);

        if (model.Password != null)
        {
            user.Password = AuthenticationHelper.HashPassword(user, model.Password);
        }

        user = await _userService.UpdateUser(user);
        var result = await _userService.GetUserApi(user.Id);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [PermissionLevel(UserRole.Admin)]
    public async Task<ActionResult<UserResponse>> Delete(Guid id)
    {
        var user = await _userService.GetUserById(id) ?? throw new NotFoundException();

        var result = await _userService.GetUserApi(user.Id);
        await _userService.DeleteUser(user);

        return Ok(result);
    }
}
