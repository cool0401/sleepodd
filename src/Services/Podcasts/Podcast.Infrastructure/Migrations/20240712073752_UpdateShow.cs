using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Podcast.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shows_Feeds_FeedId",
                table: "Shows");

            migrationBuilder.DropTable(
                name: "FeedCategory");

            migrationBuilder.DropTable(
                name: "UserSubmittedFeeds");

            migrationBuilder.DropTable(
                name: "Feeds");

            migrationBuilder.DropIndex(
                name: "IX_Shows_FeedId",
                table: "Shows");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("41cadd19-6c6a-4446-8fc4-fecaa812d504"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d5e852a9-1364-461d-bd21-2e44c9bc6642"));

            migrationBuilder.DropColumn(
                name: "FeedId",
                table: "Shows");

            migrationBuilder.CreateTable(
                name: "ShowCategories",
                columns: table => new
                {
                    ShowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowCategories", x => new { x.ShowId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_ShowCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShowCategories_Shows_ShowId",
                        column: x => x.ShowId,
                        principalTable: "Shows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("40c4004a-a269-48ec-b889-9d635434ee93"), "7e2480f1-1bba-4dd5-8ae0-5b383339e1b3", "Admin", "ADMIN" },
                    { new Guid("61c20a30-ae33-43aa-9154-85b98f031de2"), "8d0e8b0f-de2b-41e0-a9e2-611e204d3153", "User", "USER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShowCategories_CategoryId",
                table: "ShowCategories",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShowCategories");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("40c4004a-a269-48ec-b889-9d635434ee93"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("61c20a30-ae33-43aa-9154-85b98f031de2"));

            migrationBuilder.AddColumn<Guid>(
                name: "FeedId",
                table: "Shows",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Feeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feeds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSubmittedFeeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Categories = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubmittedFeeds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedCategory",
                columns: table => new
                {
                    FeedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedCategory", x => new { x.FeedId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_FeedCategory_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedCategory_Feeds_FeedId",
                        column: x => x.FeedId,
                        principalTable: "Feeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("41cadd19-6c6a-4446-8fc4-fecaa812d504"), "75dae109-c86d-41a0-b744-fcdfe69489b4", "Admin", "ADMIN" },
                    { new Guid("d5e852a9-1364-461d-bd21-2e44c9bc6642"), "d5b43ad1-c777-4755-85ab-5cd79ec17bae", "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "Feeds",
                columns: new[] { "Id", "IsFeatured", "Url" },
                values: new object[,]
                {
                    { new Guid("1d9a5366-4258-4355-9a04-80680d12e05c"), false, "https://www.m365devpodcast.com/feed.xml" },
                    { new Guid("2a57fb68-8755-4d9a-a6ee-86bf106d7874"), false, "http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master" },
                    { new Guid("54179124-9094-4091-9891-f29868298575"), true, "http://feeds.gimletcreative.com/dotfuture" },
                    { new Guid("5660e7b9-7555-4d3f-b863-df658440820b"), false, "http://feeds.codenewbie.org/cnpodcast.xml" },
                    { new Guid("57da3b70-bdfc-454e-81f0-fb4ee7ba68d3"), true, "https://s.ch9.ms/Shows/Hello-World/feed/mp3" },
                    { new Guid("5ebb45a0-5fff-49ac-a5d5-691e6314ce71"), false, "https://thedotnetcorepodcast.libsyn.com/rss" },
                    { new Guid("5fb313f5-ca48-49cd-a9bd-7ea830cfa984"), false, "https://feeds.simplecast.com/gvtxUiIf" },
                    { new Guid("6d6b95a4-88f8-4e52-bacd-362c0024362c"), false, "https://microsoftmechanics.libsyn.com/rss" },
                    { new Guid("71a2df8c-cb34-4203-b045-375695439b8b"), false, "https://devchat.tv/podcasts/adventures-in-dotnet/feed/" },
                    { new Guid("76c2dd2f-7232-4842-9808-f6a389de510e"), false, "http://awayfromthekeyboard.com/feed/" },
                    { new Guid("7941709e-dbd5-4d04-8c90-e304a4645005"), false, "https://upwards.libsyn.com/rss" },
                    { new Guid("7dd803ce-d834-4ae2-8f37-6f6e0d1977cc"), false, "https://nullpointers.io/feed/podcast.rss" },
                    { new Guid("89a51256-4674-4a11-8f2a-bd44ce325d14"), false, "https://listenbox.app/f/NRGnlt0wQqB7" },
                    { new Guid("a8791dd6-0ad8-48b7-b66e-8c6d67719626"), false, "http://feeds.feedburner.com/NoDogmaPodcast" },
                    { new Guid("bc2cab2b-d6f4-48ae-9602-3041a55ee6be"), false, "https://feeds.fireside.fm/gonemobile/rss" },
                    { new Guid("bcb81fd8-ab1d-4874-af23-35513d3d673d"), false, "https://msdevshow.libsyn.com/rss" },
                    { new Guid("c2b49169-0bb5-444a-86a4-14a476cf7620"), false, "https://feeds.simplecast.com/cRTTfxcT" },
                    { new Guid("c843a675-02a4-46c7-aea1-a78fd98d7c7a"), true, "https://feeds.fireside.fm/xamarinpodcast/rss" },
                    { new Guid("cbab58bb-fa24-46b9-b68d-ee25ddefb1a6"), false, "https://feeds.fireside.fm/mergeconflict/rss" },
                    { new Guid("da5fb742-7ceb-40cc-ac17-2d46253de3f9"), false, "https://feeds.buzzsprout.com/978640.rss" },
                    { new Guid("e2a825f2-1a5e-4b54-94dd-1544511349ab"), false, "https://feeds.soundcloud.com/users/soundcloud:users:941029057/sounds.rss" },
                    { new Guid("fa3da5bc-805e-401e-a590-f57776712170"), false, "https://intrazone.libsyn.com/rss" }
                });

            migrationBuilder.InsertData(
                table: "FeedCategory",
                columns: new[] { "CategoryId", "FeedId" },
                values: new object[,]
                {
                    { new Guid("5f923017-86da-4793-9332-7b74197acc51"), new Guid("1d9a5366-4258-4355-9a04-80680d12e05c") },
                    { new Guid("bee871ad-750b-400b-91b0-c34056c92297"), new Guid("1d9a5366-4258-4355-9a04-80680d12e05c") },
                    { new Guid("a5ae013c-14a1-4c2d-a731-47fbbd0ba527"), new Guid("2a57fb68-8755-4d9a-a6ee-86bf106d7874") },
                    { new Guid("5f923017-86da-4793-9332-7b74197acc51"), new Guid("54179124-9094-4091-9891-f29868298575") },
                    { new Guid("a5ae013c-14a1-4c2d-a731-47fbbd0ba527"), new Guid("5660e7b9-7555-4d3f-b863-df658440820b") },
                    { new Guid("5f923017-86da-4793-9332-7b74197acc51"), new Guid("57da3b70-bdfc-454e-81f0-fb4ee7ba68d3") },
                    { new Guid("2f07481d-5f3f-4bbf-923f-60e62fcfe4e7"), new Guid("5ebb45a0-5fff-49ac-a5d5-691e6314ce71") },
                    { new Guid("7322b307-1431-4203-bda8-9161b60c45d0"), new Guid("5ebb45a0-5fff-49ac-a5d5-691e6314ce71") },
                    { new Guid("a5ae013c-14a1-4c2d-a731-47fbbd0ba527"), new Guid("5ebb45a0-5fff-49ac-a5d5-691e6314ce71") },
                    { new Guid("bee871ad-750b-400b-91b0-c34056c92297"), new Guid("5ebb45a0-5fff-49ac-a5d5-691e6314ce71") },
                    { new Guid("a5ae013c-14a1-4c2d-a731-47fbbd0ba527"), new Guid("5fb313f5-ca48-49cd-a9bd-7ea830cfa984") },
                    { new Guid("2f07481d-5f3f-4bbf-923f-60e62fcfe4e7"), new Guid("6d6b95a4-88f8-4e52-bacd-362c0024362c") },
                    { new Guid("2f07481d-5f3f-4bbf-923f-60e62fcfe4e7"), new Guid("71a2df8c-cb34-4203-b045-375695439b8b") },
                    { new Guid("7322b307-1431-4203-bda8-9161b60c45d0"), new Guid("71a2df8c-cb34-4203-b045-375695439b8b") },
                    { new Guid("a5ae013c-14a1-4c2d-a731-47fbbd0ba527"), new Guid("71a2df8c-cb34-4203-b045-375695439b8b") },
                    { new Guid("bee871ad-750b-400b-91b0-c34056c92297"), new Guid("71a2df8c-cb34-4203-b045-375695439b8b") },
                    { new Guid("a5ae013c-14a1-4c2d-a731-47fbbd0ba527"), new Guid("76c2dd2f-7232-4842-9808-f6a389de510e") },
                    { new Guid("5f923017-86da-4793-9332-7b74197acc51"), new Guid("7941709e-dbd5-4d04-8c90-e304a4645005") },
                    { new Guid("a5ae013c-14a1-4c2d-a731-47fbbd0ba527"), new Guid("7dd803ce-d834-4ae2-8f37-6f6e0d1977cc") },
                    { new Guid("a5ae013c-14a1-4c2d-a731-47fbbd0ba527"), new Guid("89a51256-4674-4a11-8f2a-bd44ce325d14") },
                    { new Guid("a5ae013c-14a1-4c2d-a731-47fbbd0ba527"), new Guid("a8791dd6-0ad8-48b7-b66e-8c6d67719626") },
                    { new Guid("2f07481d-5f3f-4bbf-923f-60e62fcfe4e7"), new Guid("bc2cab2b-d6f4-48ae-9602-3041a55ee6be") },
                    { new Guid("a5ae013c-14a1-4c2d-a731-47fbbd0ba527"), new Guid("bc2cab2b-d6f4-48ae-9602-3041a55ee6be") },
                    { new Guid("2f07481d-5f3f-4bbf-923f-60e62fcfe4e7"), new Guid("bcb81fd8-ab1d-4874-af23-35513d3d673d") },
                    { new Guid("7322b307-1431-4203-bda8-9161b60c45d0"), new Guid("bcb81fd8-ab1d-4874-af23-35513d3d673d") },
                    { new Guid("a5ae013c-14a1-4c2d-a731-47fbbd0ba527"), new Guid("bcb81fd8-ab1d-4874-af23-35513d3d673d") },
                    { new Guid("bee871ad-750b-400b-91b0-c34056c92297"), new Guid("bcb81fd8-ab1d-4874-af23-35513d3d673d") },
                    { new Guid("a5ae013c-14a1-4c2d-a731-47fbbd0ba527"), new Guid("c2b49169-0bb5-444a-86a4-14a476cf7620") },
                    { new Guid("2f07481d-5f3f-4bbf-923f-60e62fcfe4e7"), new Guid("c843a675-02a4-46c7-aea1-a78fd98d7c7a") },
                    { new Guid("5f923017-86da-4793-9332-7b74197acc51"), new Guid("c843a675-02a4-46c7-aea1-a78fd98d7c7a") },
                    { new Guid("7322b307-1431-4203-bda8-9161b60c45d0"), new Guid("c843a675-02a4-46c7-aea1-a78fd98d7c7a") },
                    { new Guid("2f07481d-5f3f-4bbf-923f-60e62fcfe4e7"), new Guid("cbab58bb-fa24-46b9-b68d-ee25ddefb1a6") },
                    { new Guid("7322b307-1431-4203-bda8-9161b60c45d0"), new Guid("cbab58bb-fa24-46b9-b68d-ee25ddefb1a6") },
                    { new Guid("a5ae013c-14a1-4c2d-a731-47fbbd0ba527"), new Guid("cbab58bb-fa24-46b9-b68d-ee25ddefb1a6") },
                    { new Guid("bee871ad-750b-400b-91b0-c34056c92297"), new Guid("cbab58bb-fa24-46b9-b68d-ee25ddefb1a6") },
                    { new Guid("a5ae013c-14a1-4c2d-a731-47fbbd0ba527"), new Guid("da5fb742-7ceb-40cc-ac17-2d46253de3f9") },
                    { new Guid("a5ae013c-14a1-4c2d-a731-47fbbd0ba527"), new Guid("e2a825f2-1a5e-4b54-94dd-1544511349ab") },
                    { new Guid("5f923017-86da-4793-9332-7b74197acc51"), new Guid("fa3da5bc-805e-401e-a590-f57776712170") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shows_FeedId",
                table: "Shows",
                column: "FeedId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedCategory_CategoryId",
                table: "FeedCategory",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shows_Feeds_FeedId",
                table: "Shows",
                column: "FeedId",
                principalTable: "Feeds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
