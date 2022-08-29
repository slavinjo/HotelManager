using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Hotel.Server.Users;
using Hotel.Tests.Mocks;
using Xunit;

namespace Hotel.Tests.Users;

[Collection("Api")]
public class RegistrationAndLoginTests
{
    private readonly ApiFixture _api;

    public RegistrationAndLoginTests(ApiFixture api)
    {
        _api = api;
    }

    [Fact]
    public async void User_Registration_Success()
    {
        var (_, adminHeaders) = await _api.CreateUser();

        // create a new user and tenant
        var userRegistrationData = new UserRegistrationRequest
        {
            Email = "user_" + Guid.NewGuid() + "@e2e.com",
            FirstName = "Test",
            LastName = $"Testsson {Guid.NewGuid().ToString()[..8]}",
            Password = _api.DEFAULT_PASSWORD,
        };

        // register
        MockMailgunService.ClearEmailsTo(userRegistrationData.Email);

        var result = await _api.Request<UserAuthenticationResponse>("/api/v1/users/auth/register", HttpMethod.Post,
            null, userRegistrationData, HttpStatusCode.OK);

        Assert.Equal(userRegistrationData.Email, result.User.Email);
        Assert.Equal(userRegistrationData.FirstName, result.User.FirstName);
        Assert.Equal(userRegistrationData.LastName, result.User.LastName);

        var lastEmail = MockMailgunService.GetLastEmailTo(userRegistrationData.Email);
        Assert.NotNull(lastEmail);
        var match = Regex.Match(lastEmail.Text, @"(?<=code\=)(.*?)(?="")");
        Assert.NotNull(match);
        var code = match.Value;
        Assert.NotEmpty(code);

        // try to log in, fail because it's not activated
        var data = new UserLoginRequest
        {
            Username = userRegistrationData.Email, Password = userRegistrationData.Password
        };

        await _api.Request<UserAuthenticationResponse>("/api/v1/users/auth/authenticate", HttpMethod.Post, null, data,
            HttpStatusCode.Unauthorized);

        // activate user
        var user = await _api.GetUser(result.User.Id);

        var activationData = new UserActivationRequest {Email = user.Email, ActivationCode = user.ActivationCode};

        await _api.Request<UserAuthenticationResponse>("/api/v1/users/auth/activate", HttpMethod.Post, null,
            activationData,
            HttpStatusCode.OK);

        // try to log in after activation
        await _api.Request<UserAuthenticationResponse>("/api/v1/users/auth/authenticate", HttpMethod.Post, null, data,
            HttpStatusCode.OK);
    }

    [Fact]
    public async void User_Login_Success()
    {
        var (user, _) = await _api.CreateUser();

        // log in user
        var data = new UserLoginRequest {Username = user.Email, Password = _api.DEFAULT_PASSWORD};

        var result = await _api.Request<UserAuthenticationResponse>("/api/v1/users/auth/authenticate", HttpMethod.Post,
            null, data, HttpStatusCode.OK);
        Assert.Equal(result.User.Email, user.Email);

        data.Username = "wrong";
        await _api.Request<UserAuthenticationResponse>("/api/v1/users/auth/authenticate", HttpMethod.Post, null, data,
            HttpStatusCode.BadRequest);

        data.Username = user.Email;
        data.Password = "wrong";
        await _api.Request<UserAuthenticationResponse>("/api/v1/users/auth/authenticate", HttpMethod.Post, null, data,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async void User_ForgotPassword_Success()
    {
        // create a new user, run the forgot password flow and check that an email was sent
        var (user, _) = await _api.CreateUser();

        var data = new ForgotPasswordRequest {Email = user.Email};

        MockMailgunService.ClearEmailsTo(user.Email);

        await _api.Request<object>("/api/v1/users/auth/forgot-password", HttpMethod.Post, null, data,
            HttpStatusCode.OK);

        var lastEmail = MockMailgunService.GetLastEmailTo(user.Email);

        Assert.NotNull(lastEmail);
        var match = Regex.Match(lastEmail.Text, @"(?<=code\=)(.*?)(?="")");
        Assert.NotNull(match);
        var code = match.Value;
        Assert.NotEmpty(code);

        user = await _api.GetUser(user.Id);

        // call password reset endpoint
        var resetData = new ResetPasswordRequest
        {
            Email = user.Email, Password = "somepassword", PasswordResetCode = user.PasswordResetCode
        };

        await _api.Request<object>("/api/v1/users/auth/reset-password", HttpMethod.Post, null, resetData,
            HttpStatusCode.OK);

        // log in user
        var loginData = new UserLoginRequest {Username = user.Email, Password = "somepassword"};
        await _api.Request<UserAuthenticationResponse>("/api/v1/users/auth/authenticate", HttpMethod.Post, null,
            loginData, HttpStatusCode.OK);
    }
}
