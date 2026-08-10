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

        // FirstLoan: 1+ completed loan
        if (!earnedTypes.Contains(BadgeType.FirstLoan) && completedLoans >= 1)
            badgesToAward.Add(BadgeType.FirstLoan);

        // NoviceReader: 5+ completed loans
        if (!earnedTypes.Contains(BadgeType.NoviceReader) && completedLoans >= 5)
            badgesToAward.Add(BadgeType.NoviceReader);

        // AvidReader: 20+ completed loans
        if (!earnedTypes.Contains(BadgeType.AvidReader) && completedLoans >= 20)
            badgesToAward.Add(BadgeType.AvidReader);

        // ExpertReader: 50+ completed loans
        if (!earnedTypes.Contains(BadgeType.ExpertReader) && completedLoans >= 50)
            badgesToAward.Add(BadgeType.ExpertReader);

        // Centenarian: 100+ completed loans
        if (!earnedTypes.Contains(BadgeType.Centenarian) && completedLoans >= 100)
            badgesToAward.Add(BadgeType.Centenarian);

        // LiteraryCritic: 10+ ratings
        // (We count ratings from the user indirectly via loan-based approach or dedicated query)
        // For simplicity, check if user has submitted ratings
        var hasEnoughRatings = await CheckRatingCountAsync(userId, 10, cancellationToken);
        if (!earnedTypes.Contains(BadgeType.LiteraryCritic) && hasEnoughRatings)
            badgesToAward.Add(BadgeType.LiteraryCritic);

        // CommunityVoice: 5+ ratings with useful votes
        var hasUsefulReviews = await CheckUsefulReviewsAsync(userId, 5, cancellationToken);
        if (!earnedTypes.Contains(BadgeType.CommunityVoice) && hasUsefulReviews)
            badgesToAward.Add(BadgeType.CommunityVoice);

        // Explorer: 5+ distinct categories read
        if (!earnedTypes.Contains(BadgeType.Explorer) && categoriesRead >= 5)
            badgesToAward.Add(BadgeType.Explorer);

        // Polymath: 10+ distinct categories read
        if (!earnedTypes.Contains(BadgeType.Polymath) && categoriesRead >= 10)
            badgesToAward.Add(BadgeType.Polymath);

        // Punctual: 10+ loans returned before due date
        var onTimeReturns = history.Items.Count(l =>
            l.ReturnedAt is not null && l.ReturnedAt <= l.DueDate);
        if (!earnedTypes.Contains(BadgeType.Punctual) && onTimeReturns >= 10)
            badgesToAward.Add(BadgeType.Punctual);

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
