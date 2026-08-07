using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Infrastructure.Data;

namespace MiniLibrary.Infrastructure.Jobs;

/// <summary>
/// Daily background service that generates notifications for:
/// - Loans expiring in <= 3 days (title, due date, days remaining)
/// - Overdue loans (title, days overdue)
/// Sends both in-app and email notifications based on user preferences.
/// Runs once per day (Req 19.1-19.3).
/// </summary>
public sealed class LoanExpirationJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LoanExpirationJob> _logger;
    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24);

    public LoanExpirationJob(
        IServiceScopeFactory scopeFactory,
        ILogger<LoanExpirationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait a short time on startup before first run
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpirationAlertsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LoanExpirationJob.");
            }

            await Task.Delay(RunInterval, stoppingToken);
        }
    }

    private async Task ProcessExpirationAlertsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now = DateTime.UtcNow;
        var expirationThreshold = now.AddDays(3);

        // Loans expiring in <= 3 days (not yet overdue)
        var expiringLoans = await dbContext.BookLoans
            .Include(l => l.Book)
            .Include(l => l.User)
            .Where(l => l.ReturnedAt == null
                && l.DueDate > now
                && l.DueDate <= expirationThreshold)
            .ToListAsync(ct);

        foreach (var loan in expiringLoans)
        {
            var daysRemaining = (int)(loan.DueDate - now).TotalDays;

            // Check user preferences
            if (loan.User.EmailAlertsExpiration)
            {
                await notificationService.SendEmailAsync(
                    loan.User.Email,
                    $"Loan Expiring Soon: {loan.Book.Title}",
                    $"Your loan for \"{loan.Book.Title}\" is due in {daysRemaining} day(s) on {loan.DueDate:yyyy-MM-dd}.",
                    ct);
            }

            await notificationService.SendInAppAsync(
                loan.UserId,
                "Loan Expiring Soon",
                $"\"{loan.Book.Title}\" is due in {daysRemaining} day(s).",
                NotificationType.LoanExpiring,
                ct);
        }

        // Overdue loans
        var overdueLoans = await dbContext.BookLoans
            .Include(l => l.Book)
            .Include(l => l.User)
            .Where(l => l.ReturnedAt == null && l.DueDate < now)
            .ToListAsync(ct);

        foreach (var loan in overdueLoans)
        {
            var daysOverdue = (int)(now - loan.DueDate).TotalDays;

            if (loan.User.EmailAlertsExpiration)
            {
                await notificationService.SendEmailAsync(
                    loan.User.Email,
                    $"Overdue Loan: {loan.Book.Title}",
                    $"Your loan for \"{loan.Book.Title}\" is {daysOverdue} day(s) overdue. Please return it.",
                    ct);
            }

            await notificationService.SendInAppAsync(
                loan.UserId,
                "Loan Overdue",
                $"\"{loan.Book.Title}\" is {daysOverdue} day(s) overdue.",
                NotificationType.LoanOverdue,
                ct);
        }

        _logger.LogInformation(
            "LoanExpirationJob completed: {Expiring} expiring alerts, {Overdue} overdue alerts sent.",
            expiringLoans.Count,
            overdueLoans.Count);
    }
}
