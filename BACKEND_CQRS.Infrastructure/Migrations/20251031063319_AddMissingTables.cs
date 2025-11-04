using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BACKEND_CQRS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectStatusId",
                table: "projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectTemplateId",
                table: "projects",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "delivery_units",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    manager_id = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_units", x => x.id);
                    table.ForeignKey(
                        name: "FK_delivery_units_users_manager_id",
                        column: x => x.manager_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "import_jobs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    started_by = table.Column<int>(type: "integer", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    finished_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    summary = table.Column<string>(type: "jsonb", nullable: true),
                    details = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_import_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_import_jobs_users_started_by",
                        column: x => x.started_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "issue_comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    issue_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<int>(type: "integer", nullable: false),
                    mention_id = table.Column<int>(type: "integer", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issue_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_issue_comments_issues_issue_id",
                        column: x => x.issue_id,
                        principalTable: "issues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_issue_comments_users_author_id",
                        column: x => x.author_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_issue_comments_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_issue_comments_users_mention_id",
                        column: x => x.mention_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_issue_comments_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "project_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "project_template",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_template", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mentions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mention_user_id = table.Column<int>(type: "integer", nullable: true),
                    issue_comments_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mentions", x => x.id);
                    table.ForeignKey(
                        name: "FK_mentions_issue_comments_issue_comments_id",
                        column: x => x.issue_comments_id,
                        principalTable: "issue_comments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mentions_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mentions_users_mention_user_id",
                        column: x => x.mention_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mentions_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_projects_delivery_unit_id",
                table: "projects",
                column: "delivery_unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_ProjectStatusId",
                table: "projects",
                column: "ProjectStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_ProjectTemplateId",
                table: "projects",
                column: "ProjectTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_units_manager_id",
                table: "delivery_units",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "IX_import_jobs_started_by",
                table: "import_jobs",
                column: "started_by");

            migrationBuilder.CreateIndex(
                name: "IX_issue_comments_author_id",
                table: "issue_comments",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_issue_comments_created_by",
                table: "issue_comments",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_issue_comments_issue_id",
                table: "issue_comments",
                column: "issue_id");

            migrationBuilder.CreateIndex(
                name: "IX_issue_comments_mention_id",
                table: "issue_comments",
                column: "mention_id");

            migrationBuilder.CreateIndex(
                name: "IX_issue_comments_updated_by",
                table: "issue_comments",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_mentions_created_by",
                table: "mentions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_mentions_issue_comments_id",
                table: "mentions",
                column: "issue_comments_id");

            migrationBuilder.CreateIndex(
                name: "IX_mentions_mention_user_id",
                table: "mentions",
                column: "mention_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_mentions_updated_by",
                table: "mentions",
                column: "updated_by");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_delivery_units_delivery_unit_id",
                table: "projects",
                column: "delivery_unit_id",
                principalTable: "delivery_units",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_project_statuses_ProjectStatusId",
                table: "projects",
                column: "ProjectStatusId",
                principalTable: "project_statuses",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_project_template_ProjectTemplateId",
                table: "projects",
                column: "ProjectTemplateId",
                principalTable: "project_template",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_projects_delivery_units_delivery_unit_id",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_project_statuses_ProjectStatusId",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_project_template_ProjectTemplateId",
                table: "projects");

            migrationBuilder.DropTable(
                name: "delivery_units");

            migrationBuilder.DropTable(
                name: "import_jobs");

            migrationBuilder.DropTable(
                name: "mentions");

            migrationBuilder.DropTable(
                name: "project_statuses");

            migrationBuilder.DropTable(
                name: "project_template");

            migrationBuilder.DropTable(
                name: "issue_comments");

            migrationBuilder.DropIndex(
                name: "IX_projects_delivery_unit_id",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_ProjectStatusId",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_ProjectTemplateId",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "ProjectStatusId",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "ProjectTemplateId",
                table: "projects");
        }
    }
}
