using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BACKEND_CQRS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingMissingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    entity_id = table.Column<int>(type: "integer", nullable: true),
                    details = table.Column<string>(type: "jsonb", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_logs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "custom_fields",
                columns: table => new
                {
                    projectid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_fields", x => x.projectid);
                });

            migrationBuilder.CreateTable(
                name: "jira_authorizations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    base_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    access_token = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    refresh_token = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    scopes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jira_authorizations", x => x.id);
                    table.ForeignKey(
                        name: "FK_jira_authorizations_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_jira_authorizations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "starred_projects",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_starred_projects", x => x.id);
                    table.ForeignKey(
                        name: "FK_starred_projects_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_starred_projects_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_user_id",
                table: "audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_jira_authorizations_project_id",
                table: "jira_authorizations",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_jira_authorizations_user_id",
                table: "jira_authorizations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_starred_projects_project_id",
                table: "starred_projects",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_starred_projects_user_id",
                table: "starred_projects",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "custom_fields");

            migrationBuilder.DropTable(
                name: "jira_authorizations");

            migrationBuilder.DropTable(
                name: "starred_projects");
        }
    }
}
