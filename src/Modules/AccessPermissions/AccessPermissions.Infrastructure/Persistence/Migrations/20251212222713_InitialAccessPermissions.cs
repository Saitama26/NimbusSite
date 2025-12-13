using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessPermissions.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAccessPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccessPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProjectId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TaskId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessPermissions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AccessPermissions_ProjectId",
                table: "AccessPermissions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessPermissions_TaskId",
                table: "AccessPermissions",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessPermissions_TenantId",
                table: "AccessPermissions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessPermissions_TenantId_UserId",
                table: "AccessPermissions",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessPermissions_TenantId_UserId_Scope_Action_Type",
                table: "AccessPermissions",
                columns: new[] { "TenantId", "UserId", "Scope", "Action", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessPermissions_TenantId_UserId_Scope_Action_Type_ProjectI~",
                table: "AccessPermissions",
                columns: new[] { "TenantId", "UserId", "Scope", "Action", "Type", "ProjectId", "TaskId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessPermissions_UserId",
                table: "AccessPermissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessPermissions_UserId_ProjectId",
                table: "AccessPermissions",
                columns: new[] { "UserId", "ProjectId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessPermissions");
        }
    }
}
