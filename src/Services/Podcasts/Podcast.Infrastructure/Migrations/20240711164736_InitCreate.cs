using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Podcast.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                    { new Guid("2246d2f9-ec34-4236-9e68-8885a2be0f0f"), "087738f4-b89c-470e-9685-a3f2ee2b7969", "User", "USER" },
                    { new Guid("3ce43da1-9254-4075-8749-fccba5862654"), "0d9b67f0-1bfe-4a48-ab5d-a92efcb1d5ed", "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("2246d2f9-ec34-4236-9e68-8885a2be0f0f"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("3ce43da1-9254-4075-8749-fccba5862654"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("6ad5b041-7132-49d8-984a-b52d2d759fad"), "ce939416-c4c0-49cd-84d1-e5bac8aad7ce", "User", "USER" },
                    { new Guid("a94ba140-3848-40b4-bc52-05853a2c1df9"), "b692c4e9-2f90-48c0-bfda-77ad885b9b98", "Admin", "ADMIN" }
                });
        }
    }
}
