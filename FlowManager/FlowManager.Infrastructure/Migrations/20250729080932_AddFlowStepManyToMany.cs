using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlowStepManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            try
            {
                migrationBuilder.DropForeignKey(
                    name: "FK_Steps_Flows_FlowId",
                    table: "Steps");
            }
            catch
            {
                // Ignore if constraint doesn't exist
            }

            migrationBuilder.AlterColumn<Guid>(
                name: "FlowId",
                table: "Steps",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "FlowSteps",
                columns: table => new
                {
                    FlowId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowSteps", x => new { x.FlowId, x.StepId });
                    table.ForeignKey(
                        name: "FK_FlowSteps_Flows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "Flows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlowSteps_Steps_StepId",
                        column: x => x.StepId,
                        principalTable: "Steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlowSteps_StepId",
                table: "FlowSteps",
                column: "StepId");

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_Flows_FlowId",
                table: "Steps",
                column: "FlowId",
                principalTable: "Flows",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Steps_Flows_FlowId",
                table: "Steps");

            migrationBuilder.DropTable(
                name: "FlowSteps");

            migrationBuilder.AlterColumn<Guid>(
                name: "FlowId",
                table: "Steps",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_Flows_FlowId",
                table: "Steps",
                column: "FlowId",
                principalTable: "Flows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
