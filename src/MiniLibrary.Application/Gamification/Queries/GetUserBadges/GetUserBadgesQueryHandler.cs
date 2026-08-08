using MediatR;
using MiniLibrary.Application.Gamification.DTOs;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Gamification.Queries.GetUserBadges;

/// <summary>
/// Handles GetUserBadgesQuery: returns earned badges and progress toward pending ones.
/// </summary>
public sealed class GetUserBadgesQueryHandler
    : IRequestHandler<GetUserBadgesQuery, UserBadgesResponse>
{
    private readonly IBadgeRepository _badgeRepository;
    private readonly ILoanRepository _loanRepository;

    public GetUserBadgesQueryHandler(
        IBadgeRepository badgeRepository,
        ILoanRepository loanRepository)
    {
        _badgeRepository = badgeRepository;
        _loanRepository = loanRepository;
    }

    public async Task<UserBadgesResponse> Handle(
        GetUserBadgesQuery request,
        CancellationToken cancellationToken)
    {
        var badges = await _badgeRepository.GetUserBadgesAsync(request.UserId, cancellationToken);

        var earnedBadges = badges.Select(b => new BadgeResponse(
            b.BadgeType.ToString(),
            b.EarnedAt)).ToList();

        var pendingBadges = new List<BadgeProgressResponse>();

        if (request.IncludeProgress)
        {
            var earnedTypes = badges.Select(b => b.BadgeType).ToHashSet();

            // Get loan stats for progress
            var history = await _loanRepository.GetUserHistoryAsync(
                request.UserId,
                new PaginationParams(1, 1000),
                cancellationToken);
            var completedLoans = history.Items.Count(l => l.ReturnedAt is not null);
            var categoriesRead = history.Items
                .Where(l => l.ReturnedAt is not null && l.Book is not null)
                .Select(l => l.Book.Category)
                .Distinct()
                .Count();
            var onTimeReturns = history.Items.Count(l =>
                l.ReturnedAt is not null && l.ReturnedAt <= l.DueDate);

            // Calculate progress for each unearned badge
            var criteria = new (BadgeType Type, int Current, int Required)[]
            {
                (BadgeType.PrimerPrestamo, completedLoans, 1),
                (BadgeType.LectorNovato, completedLoans, 5),
                (BadgeType.LectorAvido, completedLoans, 20),
                (BadgeType.LectorExperto, completedLoans, 50),
                (BadgeType.Centenario, completedLoans, 100),
                (BadgeType.Explorador, categoriesRead, 5),
                (BadgeType.Polimata, categoriesRead, 10),
                (BadgeType.Puntual, onTimeReturns, 10),
            };

            foreach (var (type, current, required) in criteria)
            {
                if (earnedTypes.Contains(type)) continue;

                var percent = Math.Min(100, (int)((double)current / required * 100));
                pendingBadges.Add(new BadgeProgressResponse(
                    type.ToString(), current, required, percent));
            }
        }

        return new UserBadgesResponse(earnedBadges, pendingBadges);
    }
}
