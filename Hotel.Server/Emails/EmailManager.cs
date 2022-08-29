using System.IO;
using System.Text;

namespace Hotel.Server.Emails;

public class EmailManager
{
    public static (string Subject, string Text) WelcomeMail(string firstName, string activationLink)
    {
        var resourceStream =
            typeof(EmailManager).Assembly.GetManifestResourceStream("Hotel.Server.Emails.Welcome.html");
        using var reader = new StreamReader(resourceStream, Encoding.UTF8);

        var html = reader.ReadToEnd();

        html = html.Replace("{{ FirstName }}", firstName);
        html = html.Replace("{{ ActivationLink }}", activationLink);

        return ("Welcome to HOTEL", html);
    }

    public static (string Subject, string Text) VettingMail(string firstName)
    {
        var resourceStream =
            typeof(EmailManager).Assembly.GetManifestResourceStream("Hotel.Server.Emails.Vetting.html");
        using var reader = new StreamReader(resourceStream, Encoding.UTF8);

        var html = reader.ReadToEnd();

        html = html.Replace("{{ FirstName }}", firstName);

        return ("Welcome to HOTEL", html);
    }

    public static (string Subject, string Text) AccountReady(string firstName, string appLink)
    {
        var resourceStream =
            typeof(EmailManager).Assembly.GetManifestResourceStream("Hotel.Server.Emails.AccountReady.html");
        using var reader = new StreamReader(resourceStream, Encoding.UTF8);

        var html = reader.ReadToEnd();

        html = html.Replace("{{ FirstName }}", firstName);
        html = html.Replace("{{ AppLink }}", appLink);

        return ("Welcome to HOTEL", html);
    }

    public static (string Subject, string Text) ForgotPasswordMail(string email, string resetLink)
    {
        var resourceStream =
            typeof(EmailManager).Assembly.GetManifestResourceStream("Hotel.Server.Emails.ForgotPassword.html");
        using var reader = new StreamReader(resourceStream, Encoding.UTF8);

        var html = reader.ReadToEnd();

        html = html.Replace("{{ Email }}", email);
        html = html.Replace("{{ ResetLink }}", resetLink);

        return ("HOTEL password reset", html);
    }
}
