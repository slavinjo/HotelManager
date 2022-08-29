using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Hotel.Server;
using Hotel.Server.Data;
using Hotel.Server.Helpers;
using Hotel.Server.Integrations;
using Hotel.Server.Users;
using Hotel.Tests.Mocks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hotel.Tests;

[Collection("Api")]
public class ApiFixture
{
    public readonly string DEFAULT_PASSWORD = "test123";

    private readonly TestServer _server;
    private static HttpClient _client;

    public class Headers
    {
        public string Token { get; set; }
        public string AccessKeyId { get; set; }
        public string AccessKeySecret { get; set; }
    }

    public ApiFixture()
    {
        _server = new TestServer(new WebHostBuilder()
            .UseConfiguration(new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
            ).ConfigureTestServices(services =>
            {                
                services.RemoveAll<IMailgunService>();
                services.TryAddScoped<IMailgunService, MockMailgunService>();
            })
            .UseStartup<Startup>());

        _client = _server.CreateClient();
    }

    public async Task<T> Request<T>(string url, HttpMethod method, dynamic headers, dynamic payload,
        HttpStatusCode expectedResponseCode)
    {
        var processedUrl = url;
        dynamic token;

        try
        {
            token = headers?.Token;

        }
        catch
        {
            token = headers;
        }

        var request = new HttpRequestMessage(method, processedUrl);

        if (payload != null)
        {
            if (payload is MultipartFormDataContent)
            {
                request.Content = payload;
            }
            else
            {
                request.Content = new StringContent(Json.Serialize(payload), Encoding.UTF8,
                    "application/json");
            }
        }

        if (token != null && !token.Contains("apiKey="))
        {
            request.Headers.Add("Authorization", "Bearer " + token);
        }

        var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        if (expectedResponseCode != response.StatusCode && response.StatusCode != HttpStatusCode.NotFound)
        {
            Console.WriteLine("------- ERROR RESPONSE --------");
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            Console.WriteLine("-------------------------------");
        }

        Assert.Equal(expectedResponseCode, response.StatusCode);

        var content = response.StatusCode != HttpStatusCode.NotFound
            ? await response.Content.ReadAsStringAsync()
            : "";

        var options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true,};

        if (content == "")
        {
            return default(T);
        }

        return JsonSerializer.Deserialize<T>(content, options);
    }

    private HotelContext GetHotelContext()
    {
        return _server.Host.Services.CreateScope().ServiceProvider.GetService<HotelContext>();
    }

    public T GetService<T>()
    {
        return _server.Host.Services.CreateScope().ServiceProvider.GetService<T>();
    }

    public async Task<(User, Headers)> CreateUser(User owner = null)
    {
        var newUser = owner ?? new User
        {
            Email = $"test{Guid.NewGuid()}@e2e.com",
            FirstName = "Test",
            LastName = $"Testsson {Guid.NewGuid().ToString()[..8]}",
            ActivatedAt = new DateTime(2022, 01, 01, 0, 0, 0, DateTimeKind.Utc),
            Role = UserRole.Admin,
        };

        newUser.Password = AuthenticationHelper.HashPassword(newUser, DEFAULT_PASSWORD);

        var context = GetHotelContext();
        newUser = context.Users.Add(newUser).Entity;

        await context.SaveChangesAsync();

        var headers = new ApiFixture.Headers
        {
            Token = AuthenticationHelper.GenerateToken(newUser)
        };

        return (newUser, headers);
    }

    public void SetDate(DateTime date)
    {
        SetDate(date.Year, date.Month, date.Day);
    }

    public void SetDate(int year, int month, int day)
    {
        var service = GetService<DateTimeProvider>();
        service.SetDateTime(new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc));
    }  

    public async Task<User> GetUser(Guid userId)
    {
        return await GetHotelContext().Users.FirstOrDefaultAsync(u => u.Id == userId);
    }
}
