using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class GaggleCDCharacterRecords1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "height",
                table: "cdprofile");

            migrationBuilder.AddColumn<string>(
                name: "custom_species_name",
                table: "cdprofile",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "custom_species_name",
                table: "cdprofile");

            migrationBuilder.AddColumn<float>(
                name: "height",
                table: "cdprofile",
                type: "real",
                nullable: false,
                defaultValue: 1f);
        }
    }
}
