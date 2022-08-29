using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Hotel.Server.Data;
using Hotel.Server.Emails;
using Hotel.Server.Helpers;
using Hotel.Server.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Server.Users;

public class UserService
{
    private readonly HotelContext _context;
    private readonly DateTimeProvider _dateTimeProvider;
    private readonly NotificationService _notificationService;
    private readonly IMapper _mapper;

    private readonly string _selectSql = $@"
             SELECT DISTINCT ON (u.id)
                 u.*
             FROM
                 ""user"" u             
             WHERE
                 (@id is null OR u.id = @id)
                 AND (@email is null OR u.email = @email)";

    private object getSelectSqlParams(Guid? id = null, string email = null, bool? isPublic = null)
    {
        return new {id, email, isPublic};
    }

    public UserService(HotelContext context, DateTimeProvider dateTimeProvider, NotificationService notificationService,
        IMapper mapper)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    private async Task<UserAuthenticationResponse> generateAuthenticationResponse(User user)
    {
        var userData = await GetUserApi(user.Id);

        return new UserAuthenticationResponse(
            AuthenticationHelper.GenerateToken(user), userData);
    }

    public async Task<UserAuthenticationResponse> Authenticate(string email, string password)
    {
        if(!EmailHelper.IsValidEmail(email))
            throw new BadRequestException("Invalid email format");

        var user = await _context.Users.FirstOrDefaultAsync(e => e.Email == email) ??
                   throw new UnauthorizedException("Invalid username or password");

        if (!AuthenticationHelper.VerifyPassword(user, password))
            throw new UnauthorizedException("Invalid username or password");

        if (!user.ActivatedAt.HasValue)
            throw new UnauthorizedException("User is not active");

        return await generateAuthenticationResponse(user);
    }

    public async Task<UserAuthenticationResponse> Register(UserRegistrationRequest model)
    {
        if (!EmailHelper.IsValidEmail(model.Email))
            throw new BadRequestException("Invalid email format");

        var user = await GetUserByEmail(model.Email);

        if (user != null)
            throw new ConflictException("Email already registered");

        await _context.SaveChangesAsync();

        user = _mapper.Map<User>(model);
        user.Role = UserRole.User;
        user.ActivationCode = Guid.NewGuid().ToString();
        user.Password = AuthenticationHelper.HashPassword(user, model.Password);

        user = await AddUser(user);

        var activationLink = StaticConfiguration.WebAppUrl +
                             $"/activation?email={WebUtility.UrlEncode(model.Email)}&code={user.ActivationCode}";

        var (emailSubject, emailMessage) = EmailManager.WelcomeMail(model.FirstName, activationLink);

        await _notificationService.SendEmail(model.Email, emailSubject, emailMessage);

        return await generateAuthenticationResponse(user);
    }

    public async Task<List<User>> GetUsers(Guid? id = null, string email = null)
    {
        var items = await _context.Database.GetDbConnection()
            .QueryAsync<User>(_selectSql, getSelectSqlParams(id: id, email: email));
        return items.ToList();
    }

    public async Task<User> GetUserById(Guid id)
    {
        var user = await _context.Database.GetDbConnection()
            .QueryFirstOrDefaultAsync<User>(_selectSql, getSelectSqlParams(id: id));

        return user;
    }

    public async Task<User> GetUserByEmail(string email)
    {
        var user = await _context.Database.GetDbConnection()
            .QueryFirstOrDefaultAsync<User>(_selectSql, getSelectSqlParams(email: email));

        return user;
    }

    public async Task<User> AddUser(User user)
    {
        var newUser = _context.Users.Add(user).Entity;
        await _context.SaveChangesAsync();

        return newUser;
    }

    public async Task<User> UpdateUser(User user)
    {
        var newUser = _context.Users.Update(user).Entity;
        await _context.SaveChangesAsync();

        return newUser;
    }

    public async Task DeleteUser(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task<UserResponse> GetUserApi(Guid? id = null, string email = null)
    {
        var user = id.HasValue
            ? await GetUserById(id.Value)
            : await GetUserByEmail(email);
        return _mapper.Map<UserResponse>(user);
    }

    public async Task<PagedApiResponse<T>> GetUsersApi<T>(RequestParameters requestParameters = null,
        bool onlyPublic = false)
    {
        var users = await PagedApiResponse<User>.GetFromSql(_context, _selectSql,
            getSelectSqlParams(isPublic: onlyPublic ? true : null),
            requestParameters);

        var result = new PagedApiResponse<T>
        {
            Meta = users.Meta, Data = users.Data.Select(e => _mapper.Map<T>(e)).ToList()
        };

        return result;
    }

    public async Task<UserAuthenticationResponse> Activate(UserActivationRequest model)
    {
        if (!EmailHelper.IsValidEmail(model.Email))
            throw new BadRequestException("Invalid email format");

        var user = await this.GetUserByEmail(model.Email) ?? throw new BadRequestException("user not found");

        if (user.ActivatedAt.HasValue)
            throw new BadRequestException("user already activated");

        if (user.ActivationCode != model.ActivationCode)
            throw new BadRequestException("invalid activation code");

        user.ActivatedAt = _dateTimeProvider.UtcNow;

        await this.UpdateUser(user);

        var (emailSubject, emailMessage) = EmailManager.VettingMail(user.FirstName);

        await _notificationService.SendEmail(model.Email, emailSubject, emailMessage);

        return await generateAuthenticationResponse(user);
    }

    public async Task ForgotPassword(ForgotPasswordRequest model)
    {
        if (!EmailHelper.IsValidEmail(model.Email))
            throw new BadRequestException("Invalid email format");

        var user = await this.GetUserByEmail(model.Email) ?? throw new BadRequestException("user not found");

        user.PasswordResetCode = Guid.NewGuid().ToString();
        await this.UpdateUser(user);

        var resetLink = StaticConfiguration.WebAppUrl +
                        $"/reset-password?email={WebUtility.UrlEncode(user.Email)}&code={user.PasswordResetCode}";

        var (emailSubject, emailMessage) = EmailManager.ForgotPasswordMail(user.Email, resetLink);

        await _notificationService.SendEmail(model.Email, emailSubject, emailMessage);
    }

    public async Task ResetPassword(ResetPasswordRequest model)
    {
        if (!EmailHelper.IsValidEmail(model.Email))
            throw new BadRequestException("Invalid email format");

        var user = await this.GetUserByEmail(model.Email) ?? throw new BadRequestException("user not found");

        if (user.PasswordResetCode != model.PasswordResetCode)
            throw new BadRequestException("invalid code");

        user.Password = AuthenticationHelper.HashPassword(user, model.Password);
        user.PasswordResetCode = null;
        await this.UpdateUser(user);
    }
}
