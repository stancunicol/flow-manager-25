using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFlowIdFromSteps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Steps_Flows_FlowId",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "FlowId",
                table: "Steps");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FlowId",
                table: "Steps",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_Flows_FlowId",
                table: "Steps",
                column: "FlowId",
                principalTable: "Flows",
                principalColumn: "Id");
        }
    }
}
