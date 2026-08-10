using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniLibrary.Infrastructure.Data.Migrations;

/// <summary>
/// Data migration: renames BadgeType string values from Spanish to English
/// in the Badges table to match the updated BadgeType enum.
/// </summary>
public partial class RenameBadgeTypesToEnglish : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Rename Spanish badge type values to English equivalents
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'FirstLoan' WHERE [BadgeType] = 'PrimerPrestamo';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'NoviceReader' WHERE [BadgeType] = 'LectorNovato';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'AvidReader' WHERE [BadgeType] = 'LectorAvido';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'ExpertReader' WHERE [BadgeType] = 'LectorExperto';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'Centenarian' WHERE [BadgeType] = 'Centenario';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'LiteraryCritic' WHERE [BadgeType] = 'CriticoLiterario';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'CommunityVoice' WHERE [BadgeType] = 'VozDeLaComunidad';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'Explorer' WHERE [BadgeType] = 'Explorador';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'Polymath' WHERE [BadgeType] = 'Polimata';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'Punctual' WHERE [BadgeType] = 'Puntual';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'ReaderOfTheMonth' WHERE [BadgeType] = 'LectorDelMes';");
        // TopReviewer stays the same — no update needed
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Revert English badge type values back to Spanish
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'PrimerPrestamo' WHERE [BadgeType] = 'FirstLoan';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'LectorNovato' WHERE [BadgeType] = 'NoviceReader';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'LectorAvido' WHERE [BadgeType] = 'AvidReader';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'LectorExperto' WHERE [BadgeType] = 'ExpertReader';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'Centenario' WHERE [BadgeType] = 'Centenarian';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'CriticoLiterario' WHERE [BadgeType] = 'LiteraryCritic';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'VozDeLaComunidad' WHERE [BadgeType] = 'CommunityVoice';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'Explorador' WHERE [BadgeType] = 'Explorer';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'Polimata' WHERE [BadgeType] = 'Polymath';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'Puntual' WHERE [BadgeType] = 'Punctual';");
        migrationBuilder.Sql("UPDATE [Badges] SET [BadgeType] = 'LectorDelMes' WHERE [BadgeType] = 'ReaderOfTheMonth';");
    }
}
