using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Avatar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HashPassword = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsEmailConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Birthday = table.Column<DateOnly>(type: "date", nullable: true),
                    GithubLink = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LinkedInLink = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PasswordChangedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: false, defaultValue: 2)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "User" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Avatar", "Bio", "Birthday", "CreatedAt", "Email", "FullName", "GithubLink", "HashPassword", "IsActive", "IsEmailConfirmed", "LinkedInLink", "PasswordChangedAt", "RoleId", "Username" },
                values: new object[,]
                {
                    { 1, "https://example.com/avatar1.png", "Admin user bio", new DateOnly(1900, 1, 1), new DateTime(2025, 8, 25, 7, 22, 44, 124, DateTimeKind.Utc).AddTicks(1142), "admin@gmail.com ", "Admin User", "", "$2a$11$mOtkdH9PFIixNDPH0X2rVO8S5tcuLfx2M9c6rSt7dpD5lPOPl.SWC", true, true, "", null, 1, "admin" },
                    { 2, "https://example.com/avatar2.png", "Regular user bio", new DateOnly(1900, 1, 1), new DateTime(2025, 8, 25, 7, 22, 44, 243, DateTimeKind.Utc).AddTicks(7087), "user@gmail.com", "Regular User", "", "$2a$11$YTbIuNJmntsCoXPaVMjkeuwT/l8J6Q8Jye.kxDCX3hsowuMp6jhEu", true, true, "", null, 2, "user" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Role_Name",
                table: "Role",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                table: "User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}
