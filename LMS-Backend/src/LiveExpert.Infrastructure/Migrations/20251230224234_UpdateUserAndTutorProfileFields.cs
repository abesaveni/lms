using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveExpert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserAndTutorProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Certifications",
                table: "TutorProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Education",
                table: "TutorProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GitHubUrl",
                table: "TutorProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Languages",
                table: "TutorProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInUrl",
                table: "TutorProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PortfolioUrl",
                table: "TutorProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "TutorProfiles",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Certifications",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "Education",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "GitHubUrl",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "Languages",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "LinkedInUrl",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "PortfolioUrl",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "TutorProfiles");
        }
    }
}
