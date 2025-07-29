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
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.table_constraints 
                        WHERE constraint_name = 'FK_Steps_Flows_FlowId' 
                        AND table_name = 'Steps'
                    ) THEN
                        ALTER TABLE ""Steps"" DROP CONSTRAINT ""FK_Steps_Flows_FlowId"";
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'Steps' 
                        AND column_name = 'FlowId'
                    ) THEN
                        ALTER TABLE ""Steps"" ALTER COLUMN ""FlowId"" DROP NOT NULL;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.tables 
                        WHERE table_name = 'FlowSteps'
                    ) THEN
                        CREATE TABLE ""FlowSteps"" (
                            ""FlowId"" uuid NOT NULL,
                            ""StepId"" uuid NOT NULL,
                            ""Order"" integer NOT NULL,
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            CONSTRAINT ""PK_FlowSteps"" PRIMARY KEY (""FlowId"", ""StepId""),
                            CONSTRAINT ""FK_FlowSteps_Flows_FlowId"" FOREIGN KEY (""FlowId"") REFERENCES ""Flows""(""Id"") ON DELETE CASCADE,
                            CONSTRAINT ""FK_FlowSteps_Steps_StepId"" FOREIGN KEY (""StepId"") REFERENCES ""Steps""(""Id"") ON DELETE CASCADE
                        );
                        
                        CREATE INDEX ""IX_FlowSteps_StepId"" ON ""FlowSteps"" (""StepId"");
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'Steps' 
                        AND column_name = 'FlowId'
                    ) THEN
                        ALTER TABLE ""Steps"" ADD CONSTRAINT ""FK_Steps_Flows_FlowId"" 
                        FOREIGN KEY (""FlowId"") REFERENCES ""Flows""(""Id"");
                    END IF;
                END $$;
            ");
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
