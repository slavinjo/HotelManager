using System;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using Hotel.Server.Helpers;

namespace Hotel.Server.Integrations;

public interface IMailgunService
{
    public Task SendEmail(string to, string subject, string text, string from = null,
        string emailServiceFromOverride = null);
}
