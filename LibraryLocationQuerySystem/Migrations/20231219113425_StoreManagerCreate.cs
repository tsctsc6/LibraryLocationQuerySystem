using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryLocationQuerySystem.Migrations
{
    /// <inheritdoc />
    public partial class StoreManagerCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Book",
                columns: table => new
                {
                    SortCallNumber = table.Column<string>(type: "nchar(15)", maxLength: 15, nullable: false),
                    FormCallNumber = table.Column<string>(type: "nchar(15)", maxLength: 15, nullable: false),
                    BookName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PublishingHouse = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PublicDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Type = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Book", x => new { x.SortCallNumber, x.FormCallNumber });
                });

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    LocationLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    LocationId = table.Column<byte>(type: "tinyint", nullable: false),
                    LocationParent = table.Column<byte>(type: "tinyint", nullable: false),
                    LocationName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => new { x.LocationLevel, x.LocationId, x.LocationParent });
                });

            migrationBuilder.CreateTable(
                name: "Store",
                columns: table => new
                {
                    BooksSortCallNumber = table.Column<string>(type: "nchar(15)", nullable: false),
                    BooksFormCallNumber = table.Column<string>(type: "nchar(15)", nullable: false),
                    LocationsLocationLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    LocationsLocationId = table.Column<byte>(type: "tinyint", nullable: false),
                    LocationsLocationParent = table.Column<byte>(type: "tinyint", nullable: false),
                    StoreDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StoreNum = table.Column<byte>(type: "tinyint", nullable: false),
                    RemainNum = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Store", x => new { x.BooksSortCallNumber, x.BooksFormCallNumber, x.LocationsLocationLevel, x.LocationsLocationId, x.LocationsLocationParent });
                    table.ForeignKey(
                        name: "FK_Store_Book_BooksSortCallNumber_BooksFormCallNumber",
                        columns: x => new { x.BooksSortCallNumber, x.BooksFormCallNumber },
                        principalTable: "Book",
                        principalColumns: new[] { "SortCallNumber", "FormCallNumber" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Store_Location_LocationsLocationLevel_LocationsLocationId_LocationsLocationParent",
                        columns: x => new { x.LocationsLocationLevel, x.LocationsLocationId, x.LocationsLocationParent },
                        principalTable: "Location",
                        principalColumns: new[] { "LocationLevel", "LocationId", "LocationParent" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Store_LocationsLocationLevel_LocationsLocationId_LocationsLocationParent",
                table: "Store",
                columns: new[] { "LocationsLocationLevel", "LocationsLocationId", "LocationsLocationParent" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Store");

            migrationBuilder.DropTable(
                name: "Book");

            migrationBuilder.DropTable(
                name: "Location");
        }
    }
}
