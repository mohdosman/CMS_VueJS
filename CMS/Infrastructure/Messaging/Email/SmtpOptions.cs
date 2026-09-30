namespace CMS.Infrastructure.Messaging.Email;

// The "Smtp" section of appsettings.json. Credentials, when the relay needs them, belong in user-secrets.
public sealed class SmtpOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string FromName { get; set; } = "Crisis Management System";
    public string FromAddress { get; set; } = "MH.Applications@tn.gov";
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public bool UseStartTls { get; set; } = true;
    public bool UseSsl { get; set; }
    public bool AllowInvalidCertificate { get; set; }
}
