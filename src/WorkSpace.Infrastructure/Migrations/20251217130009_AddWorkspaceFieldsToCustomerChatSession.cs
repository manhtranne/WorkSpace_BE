using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceFieldsToCustomerChatSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "CustomerChatSessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkspaceName",
                table: "CustomerChatSessions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "CustomerChatSessions");

            migrationBuilder.DropColumn(
                name: "WorkspaceName",
                table: "CustomerChatSessions");
        }
    }
}
