using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace minimal_api.Migrations
{
    /// <inheritdoc />
    public partial class VehiclueMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Administrators",
                keyColumn: "Id",
                keyValue: new Guid("337d6b37-24bd-4aec-abc9-a35b3c76682c"));

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Brand = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Administrators",
                columns: new[] { "Id", "Email", "Password", "Profile" },
                values: new object[] { new Guid("2b72ba6f-e553-416b-a0dd-06ba7da78d16"), "adm@teste.com", "123456", "Adm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DeleteData(
                table: "Administrators",
                keyColumn: "Id",
                keyValue: new Guid("2b72ba6f-e553-416b-a0dd-06ba7da78d16"));

            migrationBuilder.InsertData(
                table: "Administrators",
                columns: new[] { "Id", "Email", "Password", "Profile" },
                values: new object[] { new Guid("337d6b37-24bd-4aec-abc9-a35b3c76682c"), "adm@teste.com", "123456", "Adm" });
        }
    }
}
