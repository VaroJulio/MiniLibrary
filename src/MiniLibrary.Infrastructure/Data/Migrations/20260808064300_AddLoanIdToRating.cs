using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniLibrary.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanIdToRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LoanId",
                table: "Ratings",
                type: "uniqueidentifier",
                nullable: true);

            // Drop the old unique index (one rating per user per book)
            migrationBuilder.DropIndex(
                name: "IX_Ratings_UserId_BookId",
                table: "Ratings");

            // Create FK index
            migrationBuilder.CreateIndex(
                name: "IX_Ratings_LoanId",
                table: "Ratings",
                column: "LoanId");

            // New non-unique index on UserId+BookId for query performance
            migrationBuilder.CreateIndex(
                name: "IX_Ratings_UserId_BookId",
                table: "Ratings",
                columns: new[] { "UserId", "BookId" });

            // New unique filtered index: one rating per user per book per loan
            migrationBuilder.CreateIndex(
                name: "IX_Ratings_UserId_BookId_LoanId",
                table: "Ratings",
                columns: new[] { "UserId", "BookId", "LoanId" },
                unique: true,
                filter: "[LoanId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_BookLoans_LoanId",
                table: "Ratings",
                column: "LoanId",
                principalTable: "BookLoans",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_BookLoans_LoanId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_LoanId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_UserId_BookId_LoanId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_UserId_BookId",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "LoanId",
                table: "Ratings");

            // Restore the original unique index
            migrationBuilder.CreateIndex(
                name: "IX_Ratings_UserId_BookId",
                table: "Ratings",
                columns: new[] { "UserId", "BookId" },
                unique: true);
        }
    }
}
