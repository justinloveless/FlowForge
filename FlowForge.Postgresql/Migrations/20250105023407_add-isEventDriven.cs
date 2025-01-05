using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowForge.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class addisEventDriven : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEventDriven",
                table: "WorkflowDefinitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEventDriven",
                table: "WorkflowDefinitions");
        }
    }
}
