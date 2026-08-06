using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.Domain.Entities;

/// <summary>
/// Aggregate root representing a library user.
/// </summary>
public class User : Entity
{
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string ExternalId { get; private set; } = string.Empty;
    public string Provider { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsDeleted { get; private set; }

    // Notification preferences (Req 19.10)
    public bool EmailAlertsExpiration { get; private set; }
    public bool EmailAlertsAvailability { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public ICollection<BookLoan> Loans { get; private set; } = [];
    public ICollection<Rating> Ratings { get; private set; } = [];
    public ICollection<WishlistEntry> WishlistEntries { get; private set; } = [];
    public ICollection<Badge> Badges { get; private set; } = [];
    public ICollection<Notification> Notifications { get; private set; } = [];
    public ICollection<ReviewVote> ReviewVotes { get; private set; } = [];

    // Required by EF Core
    private User() { }

    public static User Create(
        string email,
        string fullName,
        string externalId,
        string provider,
        UserRole role = UserRole.Member)
    {
        return new User
        {
            Email = email,
            FullName = fullName,
            ExternalId = externalId,
            Provider = provider,
            Role = role,
            IsDeleted = false,
            EmailAlertsExpiration = true,
            EmailAlertsAvailability = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void AssignRole(UserRole role)
    {
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotificationPreferences(bool emailAlertsExpiration, bool emailAlertsAvailability)
    {
        EmailAlertsExpiration = emailAlertsExpiration;
        EmailAlertsAvailability = emailAlertsAvailability;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
