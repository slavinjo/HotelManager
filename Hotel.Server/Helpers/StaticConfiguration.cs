using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Hotel.Server.Helpers;

public class StaticConfiguration
{
    private static IConfiguration _configuration;

    public static string WebAppUrl => Environment.GetEnvironmentVariable("WEB_APP_URL") ??
                                      _configuration.GetValue<string>("WebAppUrl");

    public static string ConnectionStringsHotelDB => Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ??
                                                       _configuration.GetValue<string>("ConnectionStrings:HotelDB");

    public static string AppSettingsSecret => Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ??
                                              _configuration.GetValue<string>("AppSettings:Secret");

    public static string AppSettingsExpirationDays => Environment.GetEnvironmentVariable("JWT_EXPIRATION_DAYS") ??
                                                      _configuration.GetValue<string>("AppSettings:ExpirationDays");

    public static string SentryDsn => Environment.GetEnvironmentVariable("SENTRY_DSN") ??
                                      _configuration.GetValue<string>("Sentry:Dsn");

    public static string MailgunApiKey => Environment.GetEnvironmentVariable("MAILGUN_API_KEY") ??
                                          _configuration.GetValue<string>("MailgunApiKey");

    public static string MailgunBaseUrl => Environment.GetEnvironmentVariable("MAILGUN_BASE_URL") ??
                                           _configuration.GetValue<string>("MailgunBaseUrl");

    public static string MailgunDomain => Environment.GetEnvironmentVariable("MAILGUN_DOMAIN") ??
                                          _configuration.GetValue<string>("MailgunDomain");

    public static string MailgunSenderEmail => Environment.GetEnvironmentVariable("MAILGUN_SENDER_EMAIL") ??
                                               _configuration.GetValue<string>("MailgunSenderEmail");

    public static void Initialize(IConfiguration configuration)
    {
        _configuration = configuration;

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ConnectionStringsHotelDB))
            errors.Add("ConnectionStringsHotelDB");

        if (string.IsNullOrWhiteSpace(AppSettingsSecret))
            errors.Add("AppSettingsSecret");

        if (errors.Count > 0)
            throw new Exception(string.Join(", ", errors));
    }
}
