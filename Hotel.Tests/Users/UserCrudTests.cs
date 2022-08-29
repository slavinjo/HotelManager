using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Hotel.Server.Helpers;
using Hotel.Server.Users;
using Xunit;

namespace Hotel.Tests.Users;

[Collection("Api")]
public class UserCrudTests
{
    private readonly ApiFixture _api;

    public UserCrudTests(ApiFixture api)
    {
        _api = api;
    }

    [Fact]
    public async void User_Crud_Success()
    {
        var (user, headers) = await _api.CreateUser();

        // create a new user
        var userAddData = new UserAddRequest
        {
            Email = "user_" + Guid.NewGuid() + "@e2e.com",
            FirstName = "Test",
            LastName = $"Testsson {Guid.NewGuid().ToString()[..8]}",
            Password = _api.DEFAULT_PASSWORD,
            Role = UserRole.User
        };

        var result = await _api.Request<UserResponse>("/api/v1/users", HttpMethod.Post,
            headers, userAddData, HttpStatusCode.Created);

        Assert.Equal(userAddData.Email, result.Email);
        Assert.Equal(userAddData.FirstName, result.FirstName);
        Assert.Equal(userAddData.LastName, result.LastName);
        Assert.Equal(userAddData.Role, result.Role);
     
        // log in created user
        var loginData = new UserLoginRequest {Username = userAddData.Email, Password = userAddData.Password};
        var loginResult = await _api.Request<UserAuthenticationResponse>("/api/v1/users/auth/authenticate",
            HttpMethod.Post,
            null, loginData, HttpStatusCode.OK);

        Assert.Equal(userAddData.Email, loginResult.User.Email);
        Assert.Equal(userAddData.FirstName, loginResult.User.FirstName);
        Assert.Equal(userAddData.LastName, loginResult.User.LastName);

        // find owner and new user in all users list
        var userList = await _api.Request<PagedApiResponse<UserResponse>>($"/api/v1/users?pageSize=999999",
            HttpMethod.Get, headers,
            null, HttpStatusCode.OK);

        Assert.NotNull(userList.Data.FirstOrDefault(e => e.Email == user.Email));
        Assert.NotNull(userList.Data.FirstOrDefault(e => e.Email == userAddData.Email));

        // update new user with owner credentials
        var userId = loginResult.User.Id;
        var newUserHeaders = new ApiFixture.Headers {Token = loginResult.Token};

        var userUpdateData = new UserUpdateRequest {FirstName = "Some", LastName = "User", Role = UserRole.Admin,};

        await _api.Request<UserAuthenticationResponse>($"/api/v1/users/{userId}", HttpMethod.Put, headers,
            userUpdateData, HttpStatusCode.OK);

        // update new user with its credentials
        await _api.Request<UserResponse>("/api/v1/users/me", HttpMethod.Put,
            newUserHeaders, userUpdateData, HttpStatusCode.OK);

        // check user data with owner credentials
        var newUser = await _api.Request<UserResponse>($"/api/v1/users/{userId}", HttpMethod.Get, headers,
            null, HttpStatusCode.OK);

        Assert.Equal(userAddData.Email, newUser.Email);
        Assert.Equal(userUpdateData.FirstName, newUser.FirstName);
        Assert.Equal(userUpdateData.LastName, newUser.LastName);

        // check user data with its credentials
        newUser = await _api.Request<UserResponse>("/api/v1/users/me", HttpMethod.Get, newUserHeaders,
            null, HttpStatusCode.OK);

        Assert.Equal(userAddData.Email, newUser.Email);
        Assert.Equal(userUpdateData.FirstName, newUser.FirstName);
        Assert.Equal(userUpdateData.LastName, newUser.LastName);

        // change password using its credentials
        userUpdateData.Password = "something";

        await _api.Request<UserAuthenticationResponse>("/api/v1/users/me", HttpMethod.Put,
            newUserHeaders, userUpdateData, HttpStatusCode.OK);

        // login using old password
        await _api.Request<UserAuthenticationResponse>("/api/v1/users/auth/authenticate", HttpMethod.Post,
            null, loginData, HttpStatusCode.Unauthorized);

        // login using new password
        loginData.Password = userUpdateData.Password;

        await _api.Request<UserAuthenticationResponse>("/api/v1/users/auth/authenticate", HttpMethod.Post,
            null, loginData, HttpStatusCode.OK);

        // delete user
        await _api.Request<UserResponse>($"/api/v1/users/{userId}", HttpMethod.Delete, headers,
            null, HttpStatusCode.OK);

        // fail logging in
        await _api.Request<UserAuthenticationResponse>("/api/v1/users/auth/authenticate", HttpMethod.Post,
            null, loginData, HttpStatusCode.Unauthorized);

        // fail to get user by id
        await _api.Request<UserResponse>($"/api/v1/users/{userId}", HttpMethod.Get, headers,
            null, HttpStatusCode.NotFound);
    }
}
