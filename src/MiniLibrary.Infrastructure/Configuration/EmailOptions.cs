namespace MiniLibrary.Infrastructure.Configuration;

/// <summary>
/// Configuration options for SMTP email delivery.
/// Bound from the "Email" configuration section.
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>SMTP server hostname (e.g., smtp.gmail.com).</summary>
    public string SmtpHost { get; set; } = "smtp.gmail.com";

    /// <summary>SMTP server port (587 for TLS, 465 for SSL).</summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>Whether to use STARTTLS.</summary>
    public bool UseTls { get; set; } = true;

    /// <summary>Sender email address.</summary>
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>Display name for the sender.</summary>
    public string SenderName { get; set; } = "MiniLibrary";

    /// <summary>Gmail App Password (not the regular account password).</summary>
    public string AppPassword { get; set; } = string.Empty;

    /// <summary>Returns true when minimal required settings are configured.</summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(SmtpHost) &&
        !string.IsNullOrWhiteSpace(SenderEmail) &&
        !string.IsNullOrWhiteSpace(AppPassword);
}
