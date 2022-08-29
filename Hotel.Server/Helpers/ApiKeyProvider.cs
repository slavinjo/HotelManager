using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.Authentication.ApiKey;
using Hotel.Server.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Hotel.Server.Helpers;

public class ApiKeyProvider : IApiKeyProvider
{
    private readonly HotelContext _context;

    public ApiKeyProvider(HotelContext context)
    {
        _context = context;
    }

    public Task<IApiKey> ProvideAsync(string key)
    {
        throw new UnauthorizedException();
    }
}
