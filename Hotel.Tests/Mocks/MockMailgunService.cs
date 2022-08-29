using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hotel.Server.Integrations;

namespace Hotel.Tests.Mocks;

public class MockMailgunService : IMailgunService
{
    public static List<MockEmail> Emails = new List<MockEmail>();

    public class MockEmail
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public string From { get; set; }
        public string EmailServiceFromOverride { get; set; }
    }

    public Task SendEmail(string to, string subject, string text, string from = null,
        string emailServiceFromOverride = null)
    {
        if (string.IsNullOrEmpty(to))
            return Task.CompletedTask;

        Emails.Add(new MockEmail
        {
            To = to,
            Subject = subject,
            Text = text,
            From = from,
            EmailServiceFromOverride = emailServiceFromOverride
        });

        return Task.CompletedTask;
    }

    public static MockEmail GetLastEmailTo(string emailAddress)
    {
        return Emails.FirstOrDefault(e => e.To == emailAddress);
    }

    public static void ClearEmailsTo(string emailAddress)
    {
        Emails.RemoveAll(e => e.To == emailAddress);
    }
}
