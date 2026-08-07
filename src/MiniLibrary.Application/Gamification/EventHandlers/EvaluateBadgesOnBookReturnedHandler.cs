using MediatR;
using Microsoft.Extensions.Logging;
using MiniLibrary.Application.Gamification.Commands.EvaluateBadges;
using MiniLibrary.Domain.Events;

namespace MiniLibrary.Application.Gamification.EventHandlers;

/// <summary>
/// Handles BookReturnedEvent by triggering badge evaluation for the returning member.
/// </summary>
public sealed class EvaluateBadgesOnBookReturnedHandler : INotificationHandler<BookReturnedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<EvaluateBadgesOnBookReturnedHandler> _logger;

    public EvaluateBadgesOnBookReturnedHandler(
        IMediator mediator,
        ILogger<EvaluateBadgesOnBookReturnedHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(BookReturnedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new EvaluateBadgesCommand(notification.UserId), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate badges for user {UserId} after book return.", notification.UserId);
        }
    }
}
