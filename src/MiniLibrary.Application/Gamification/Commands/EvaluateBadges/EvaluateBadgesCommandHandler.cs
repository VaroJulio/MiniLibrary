using MediatR;
using Microsoft.Extensions.Logging;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Gamification.Commands.EvaluateBadges;

/// <summary>
/// Handles EvaluateBadgesCommand by checking all badge criteria for the member
/// and awarding any that are met but not yet earned. Idempotent.
/// Generates notification (in-app + email) when badge is earned.
/// </summary>
public sealed class EvaluateBadgesCommandHandler : IRequestHandler<EvaluateBadgesCommand, Unit>
{
    private readonly IBadgeRepository _badgeRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EvaluateBadgesCommandHandler> _logger;

    private const string LeaderboardCacheKey = "gamification:leaderboard";

    public EvaluateBadgesCommandHandler(
        IBadgeRepository badgeRepository,
        ILoanRepository loanRepository,
        IRatingRepository ratingRepository,
        INotificationService notificationService,
        IUserRepository userRepository,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        ILogger<EvaluateBadgesCommandHandler> logger)
    {
        _badgeRepository = badgeRepository;
        _loanRepository = loanRepository;
        _ratingRepository = ratingRepository;
        _notificationService = notificationService;
        _userRepository = userRepository;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(EvaluateBadgesCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;

        // Get user's loan history count (returned loans)
        var history = await _loanRepository.GetUserHistoryAsync(
            userId,
            new Domain.Common.PaginationParams(1, 1000),
            cancellationToken);
        var completedLoans = history.Items.Count(l => l.ReturnedAt is not null);

        // Get user's distinct categories read
        var categoriesRead = history.Items
            .Where(l => l.ReturnedAt is not null && l.Book is not null)
            .Select(l => l.Book.Category)
            .Distinct()
            .Count();

        // Get user's ratings count
        var userBadges = await _badgeRepository.GetUserBadgesAsync(userId, cancellationToken);
        var earnedTypes = userBadges.Select(b => b.BadgeType).ToHashSet();

        // Evaluate each badge criterion
        var badgesToAward = new List<BadgeType>();

        // PrimerPrestamo: 1+ completed loan
        if (!earnedTypes.Contains(BadgeType.PrimerPrestamo) && completedLoans >= 1)
            badgesToAward.Add(BadgeType.PrimerPrestamo);

        // LectorNovato: 5+ completed loans
        if (!earnedTypes.Contains(BadgeType.LectorNovato) && completedLoans >= 5)
            badgesToAward.Add(BadgeType.LectorNovato);

        // LectorAvido: 20+ completed loans
        if (!earnedTypes.Contains(BadgeType.LectorAvido) && completedLoans >= 20)
            badgesToAward.Add(BadgeType.LectorAvido);

        // LectorExperto: 50+ completed loans
        if (!earnedTypes.Contains(BadgeType.LectorExperto) && completedLoans >= 50)
            badgesToAward.Add(BadgeType.LectorExperto);

        // Centenario: 100+ completed loans
        if (!earnedTypes.Contains(BadgeType.Centenario) && completedLoans >= 100)
            badgesToAward.Add(BadgeType.Centenario);

        // CriticoLiterario: 10+ ratings
        // (We count ratings from the user indirectly via loan-based approach or dedicated query)
        // For simplicity, check if user has submitted ratings
        var hasEnoughRatings = await CheckRatingCountAsync(userId, 10, cancellationToken);
        if (!earnedTypes.Contains(BadgeType.CriticoLiterario) && hasEnoughRatings)
            badgesToAward.Add(BadgeType.CriticoLiterario);

        // VozDeLaComunidad: 5+ ratings with useful votes
        var hasUsefulReviews = await CheckUsefulReviewsAsync(userId, 5, cancellationToken);
        if (!earnedTypes.Contains(BadgeType.VozDeLaComunidad) && hasUsefulReviews)
            badgesToAward.Add(BadgeType.VozDeLaComunidad);

        // Explorador: 5+ distinct categories read
        if (!earnedTypes.Contains(BadgeType.Explorador) && categoriesRead >= 5)
            badgesToAward.Add(BadgeType.Explorador);

        // Polimata: 10+ distinct categories read
        if (!earnedTypes.Contains(BadgeType.Polimata) && categoriesRead >= 10)
            badgesToAward.Add(BadgeType.Polimata);

        // Puntual: 10+ loans returned before due date
        var onTimeReturns = history.Items.Count(l =>
            l.ReturnedAt is not null && l.ReturnedAt <= l.DueDate);
        if (!earnedTypes.Contains(BadgeType.Puntual) && onTimeReturns >= 10)
            badgesToAward.Add(BadgeType.Puntual);

        // Award badges
        foreach (var badgeType in badgesToAward)
        {
            var badge = Badge.Create(userId, badgeType);
            await _badgeRepository.AddAsync(badge, cancellationToken);

            // Send notification
            await _notificationService.SendInAppAsync(
                userId,
                "Badge Earned!",
                $"Congratulations! You earned the \"{badgeType}\" badge.",
                NotificationType.BadgeEarned,
                cancellationToken);

            _logger.LogInformation("Badge {BadgeType} awarded to user {UserId}.", badgeType, userId);
        }

        // Invalidate leaderboard cache if any badges were awarded
        if (badgesToAward.Count > 0)
        {
            await _unitOfWork.CommitAsync(cancellationToken);
            await _cacheService.InvalidateAsync(LeaderboardCacheKey, cancellationToken);
        }

        return Unit.Value;
    }

    private async Task<bool> CheckRatingCountAsync(Guid userId, int minCount, CancellationToken ct)
    {
        var count = await _ratingRepository.GetUserRatingCountAsync(userId, ct);
        return count >= minCount;
    }

    private async Task<bool> CheckUsefulReviewsAsync(Guid userId, int minCount, CancellationToken ct)
    {
        var count = await _ratingRepository.GetUserUsefulReviewCountAsync(userId, 1, ct);
        return count >= minCount;
    }
}
