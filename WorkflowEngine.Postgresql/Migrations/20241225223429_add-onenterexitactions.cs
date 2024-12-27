using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkflowEngine.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class addonenterexitactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TriggerWebhookOnExternalEvent",
                table: "StateDefinition");

            migrationBuilder.DropColumn(
                name: "Webhook",
                table: "StateDefinition");

            migrationBuilder.AddColumn<string>(
                name: "OnEnterActions",
                table: "StateDefinition",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OnExitActions",
                table: "StateDefinition",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnEnterActions",
                table: "StateDefinition");

            migrationBuilder.DropColumn(
                name: "OnExitActions",
                table: "StateDefinition");

            migrationBuilder.AddColumn<bool>(
                name: "TriggerWebhookOnExternalEvent",
                table: "StateDefinition",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Webhook",
                table: "StateDefinition",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
