using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservaCabanasSite.Migrations
{
    /// <inheritdoc />
    public partial class AgregarImagenBlobACabanaImagen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImagenUrl",
                table: "CabanaImagenes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "CabanaImagenes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImagenData",
                table: "CabanaImagenes",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "NombreArchivo",
                table: "CabanaImagenes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "CabanaImagenes");

            migrationBuilder.DropColumn(
                name: "ImagenData",
                table: "CabanaImagenes");

            migrationBuilder.DropColumn(
                name: "NombreArchivo",
                table: "CabanaImagenes");

            migrationBuilder.AlterColumn<string>(
                name: "ImagenUrl",
                table: "CabanaImagenes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
