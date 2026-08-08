using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;
using MiniLibrary.Infrastructure.Data;

namespace MiniLibrary.Infrastructure.Jobs;

/// <summary>
/// Monthly background service that awards "Lector del Mes" badge to the member
/// with the most returned loans in the previous month, and "Top Reviewer" to the
/// member whose review received the most useful votes. Runs on the 1st of each month.
/// </summary>
public sealed class MonthlyBadgeJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MonthlyBadgeJob> _logger;

    public MonthlyBadgeJob(
        IServiceScopeFactory scopeFactory,
        ILogger<MonthlyBadgeJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Calculate delay until next 1st of month at 00:00 UTC
            var now = DateTime.UtcNow;
            var nextRun = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
            var delay = nextRun - now;

            await Task.Delay(delay, stoppingToken);

            try
            {
                await AwardMonthlyBadgesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MonthlyBadgeJob.");
            }
        }
    }

    private async Task AwardMonthlyBadgesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var badgeRepository = scope.ServiceProvider.GetRequiredService<IBadgeRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now = DateTime.UtcNow;
        var previousMonth = now.AddMonths(-1);
        var monthStart = new DateTime(previousMonth.Year, previousMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Lector del Mes: member with most returns in previous month
        var topReader = await dbContext.BookLoans
            .Where(l => l.ReturnedAt >= monthStart && l.ReturnedAt < monthEnd)
            .GroupBy(l => l.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync(ct);

        if (topReader is not null)
        {
            var alreadyHas = await badgeRepository.HasBadgeAsync(
                topReader.UserId, BadgeType.LectorDelMes.ToString(), ct);
            if (!alreadyHas)
            {
                var badge = Badge.Create(topReader.UserId, BadgeType.LectorDelMes);
                await badgeRepository.AddAsync(badge, ct);
                await notificationService.SendInAppAsync(
                    topReader.UserId,
                    "Badge Earned!",
                    "You are the Reader of the Month! Congratulations!",
                    NotificationType.BadgeEarned, ct);
                _logger.LogInformation("LectorDelMes badge awarded to user {UserId}.", topReader.UserId);
            }
        }

        // Top Reviewer: review with most useful votes in previous month
        var topReview = await dbContext.Ratings
            .Where(r => r.CreatedAt >= monthStart && r.CreatedAt < monthEnd && r.UsefulVotes > 0)
            .OrderByDescending(r => r.UsefulVotes)
            .FirstOrDefaultAsync(ct);

        if (topReview is not null)
        {
            var alreadyHas = await badgeRepository.HasBadgeAsync(
                topReview.UserId, BadgeType.TopReviewer.ToString(), ct);
            if (!alreadyHas)
            {
                var badge = Badge.Create(topReview.UserId, BadgeType.TopReviewer);
                await badgeRepository.AddAsync(badge, ct);
                await notificationService.SendInAppAsync(
                    topReview.UserId,
                    "Badge Earned!",
                    "Your review was the most helpful this month! You earned Top Reviewer!",
                    NotificationType.BadgeEarned, ct);
                _logger.LogInformation("TopReviewer badge awarded to user {UserId}.", topReview.UserId);
            }
        }

        // Invalidate leaderboard cache after monthly badge awards
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        await cacheService.InvalidateAsync("gamification:leaderboard", ct);
    }
}
