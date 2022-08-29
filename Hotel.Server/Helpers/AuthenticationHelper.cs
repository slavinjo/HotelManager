using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Hotel.Server.Users;

namespace Hotel.Server.Helpers;

public class AuthenticationHelper
{
    public static string GenerateToken(User user)
    {
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role ?? "")
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(StaticConfiguration.AppSettingsSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(Convert.ToInt32(StaticConfiguration.AppSettingsExpirationDays)),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public static string HashPassword<T>(T user, string password) where T : class, IHasPassword
    {
        var passwordHasher = new PasswordHasher<T>();
        return passwordHasher.HashPassword(user, password);
    }

    public static bool VerifyPassword<T>(T user, string password) where T : class, IHasPassword
    {
        var passwordHasher = new PasswordHasher<T>();
        var verified = false;
        var result = passwordHasher.VerifyHashedPassword(user, user.Password, password);
        switch (result)
        {
            case PasswordVerificationResult.Success:
            case PasswordVerificationResult.SuccessRehashNeeded:
                verified = true;
                break;
            case PasswordVerificationResult.Failed:
                verified = false;
                break;
            default:
                verified = false;
                break;
        }

        return verified;
    }
}
