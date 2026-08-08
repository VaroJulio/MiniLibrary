using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.Domain.Entities;

/// <summary>
/// Entity representing an in-app notification delivered to a member.
/// </summary>
public class Notification : Entity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    // Required by EF Core
    private Notification() { }

    public static Notification Create(Guid userId, string title, string message, NotificationType type)
    {
        return new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
