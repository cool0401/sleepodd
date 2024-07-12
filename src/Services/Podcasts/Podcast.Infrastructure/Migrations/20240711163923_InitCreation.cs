using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Podcast.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("7a2abead-1581-421a-ba52-e94460e1ad02"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d4a32169-63b6-49b6-abc7-d945d5639f88"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("6ad5b041-7132-49d8-984a-b52d2d759fad"), "ce939416-c4c0-49cd-84d1-e5bac8aad7ce", "User", "USER" },
                    { new Guid("a94ba140-3848-40b4-bc52-05853a2c1df9"), "b692c4e9-2f90-48c0-bfda-77ad885b9b98", "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("6ad5b041-7132-49d8-984a-b52d2d759fad"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("a94ba140-3848-40b4-bc52-05853a2c1df9"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("7a2abead-1581-421a-ba52-e94460e1ad02"), "c8761fdf-239a-43bb-8a60-63b4b1ef0ba2", "User", "USER" },
                    { new Guid("d4a32169-63b6-49b6-abc7-d945d5639f88"), "21eb0a30-0302-4edc-b45d-7dfa7b3ad33b", "Admin", "ADMIN" }
                });
        }
    }
}
