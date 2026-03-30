using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymApi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCampoIdASocioEntrenador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Si la columna no existía, la agregamos como IDENTITY
            migrationBuilder.AddColumn<int>(
                name: "SocioEntrenadorId",
                table: "SocioEntrenador",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SocioEntrenadorId",
                table: "SocioEntrenador");
        }
    }
}
