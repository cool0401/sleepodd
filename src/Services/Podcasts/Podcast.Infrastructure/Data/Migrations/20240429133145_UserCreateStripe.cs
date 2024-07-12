using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Podcast.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserCreateStripe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0e84fca4-93a7-485a-ab4c-cc1daaf564d0"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("6ef4e9c7-0c93-4ff4-a9e3-d0bae0b9f343"));

            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("41cadd19-6c6a-4446-8fc4-fecaa812d504"), "75dae109-c86d-41a0-b744-fcdfe69489b4", "Admin", "ADMIN" },
                    { new Guid("d5e852a9-1364-461d-bd21-2e44c9bc6642"), "d5b43ad1-c777-4755-85ab-5cd79ec17bae", "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("41cadd19-6c6a-4446-8fc4-fecaa812d504"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d5e852a9-1364-461d-bd21-2e44c9bc6642"));

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("0e84fca4-93a7-485a-ab4c-cc1daaf564d0"), "8b42a386-1d61-4b7b-b7e1-324d24cf20e8", "User", "USER" },
                    { new Guid("6ef4e9c7-0c93-4ff4-a9e3-d0bae0b9f343"), "82092995-de1e-41dd-95ba-02e46bc78ea7", "Admin", "ADMIN" }
                });
        }
    }
}
