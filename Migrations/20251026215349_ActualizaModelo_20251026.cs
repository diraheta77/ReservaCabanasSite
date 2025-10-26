using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservaCabanasSite.Migrations
{
    /// <inheritdoc />
    public partial class ActualizaModelo_20251026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Solo eliminar la tabla si existe
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[dbo].[Acompanantes]', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [Acompanantes]
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Acompanantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservaId = table.Column<int>(type: "int", nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Dni = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acompanantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Acompanantes_Reservas_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "Reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Acompanantes_ReservaId",
                table: "Acompanantes",
                column: "ReservaId");
        }
    }
}
