using System;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using Hotel.Server.Helpers;

namespace Hotel.Server.Integrations;

public class MailgunService : IMailgunService
{
    public async Task SendEmail(string to, string subject, string text, string from = null,
        string emailServiceFromOverride = null)
    {
        if (string.IsNullOrEmpty(StaticConfiguration.MailgunApiKey) || string.IsNullOrEmpty(to))
            return;

        // used for e2e tests
        if (to.Contains("@e2e.com"))
            return;

        var project = new RestClient(new Uri(StaticConfiguration.MailgunBaseUrl));

        project.Authenticator =
            new HttpBasicAuthenticator("api",
                StaticConfiguration.MailgunApiKey);
        var request = new RestRequest();
        request.AddParameter("domain", StaticConfiguration.MailgunDomain, ParameterType.UrlSegment);
        request.Resource = "{domain}/messages";
        request.AddParameter("from", emailServiceFromOverride ?? StaticConfiguration.MailgunSenderEmail);

        if (from != null)
        {
            request.AddParameter("h:Reply-To", from);
        }

        request.AddParameter("to", to);
        request.AddParameter("subject", subject);
        request.AddParameter("html", text);
        request.Method = Method.POST;
        await project.ExecuteAsync(request);
    }
}
