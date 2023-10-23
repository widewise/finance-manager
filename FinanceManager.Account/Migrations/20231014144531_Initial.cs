using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinanceManager.Account.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountLimits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<string>(type: "text", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    LimitValue = table.Column<decimal>(type: "numeric", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Time = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountLimits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name", "ParentId", "RequestId", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), null, "Transfer", null, "00000000-0000-0000-0000-000000000001", 2, null },
                    { new Guid("00000000-0000-0000-0000-000000000002"), null, "Salary", null, "00000000-0000-0000-0000-000000000002", 0, null },
                    { new Guid("00000000-0000-0000-0000-000000000003"), null, "Bank", null, "00000000-0000-0000-0000-000000000003", 0, null },
                    { new Guid("00000000-0000-0000-0000-000000000004"), null, "Return", null, "00000000-0000-0000-0000-000000000004", 0, null },
                    { new Guid("00000000-0000-0000-0000-000000000005"), null, "Part time job", null, "00000000-0000-0000-0000-000000000005", 0, null },
                    { new Guid("00000000-0000-0000-0000-000000000006"), null, "Rent", null, "00000000-0000-0000-0000-000000000006", 0, null },
                    { new Guid("00000000-0000-0000-0000-000000000007"), null, "Other", null, "00000000-0000-0000-0000-000000000007", 0, null },
                    { new Guid("00000000-0000-0000-0000-000000000008"), null, "Food", null, "00000000-0000-0000-0000-000000000008", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000009"), null, "Home", null, "00000000-0000-0000-0000-000000000009", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000010"), null, "Transport", null, "00000000-0000-0000-0000-000000000010", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000011"), null, "Personal", null, "00000000-0000-0000-0000-000000000011", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000012"), null, "Services", null, "00000000-0000-0000-0000-000000000012", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000013"), null, "Rest", null, "00000000-0000-0000-0000-000000000013", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000014"), null, "Education", null, "00000000-0000-0000-0000-000000000014", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000015"), null, "Beauty", null, "00000000-0000-0000-0000-000000000015", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000016"), null, "Low", null, "00000000-0000-0000-0000-000000000016", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000017"), null, "Grocery", new Guid("00000000-0000-0000-0000-000000000008"), "00000000-0000-0000-0000-000000000017", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000018"), null, "Pub", new Guid("00000000-0000-0000-0000-000000000008"), "00000000-0000-0000-0000-000000000018", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000019"), null, "Delivery", new Guid("00000000-0000-0000-0000-000000000008"), "00000000-0000-0000-0000-000000000019", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000020"), null, "Restaurant", new Guid("00000000-0000-0000-0000-000000000008"), "00000000-0000-0000-0000-000000000020", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000021"), null, "Repair", new Guid("00000000-0000-0000-0000-000000000009"), "00000000-0000-0000-0000-000000000021", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000022"), null, "Rent", new Guid("00000000-0000-0000-0000-000000000009"), "00000000-0000-0000-0000-000000000022", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000023"), null, "Furniture", new Guid("00000000-0000-0000-0000-000000000009"), "00000000-0000-0000-0000-000000000023", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000024"), null, "Electrical appliances", new Guid("00000000-0000-0000-0000-000000000009"), "00000000-0000-0000-0000-000000000024", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000025"), null, "Public transport", new Guid("00000000-0000-0000-0000-000000000010"), "00000000-0000-0000-0000-000000000025", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000026"), null, "Taxi", new Guid("00000000-0000-0000-0000-000000000010"), "00000000-0000-0000-0000-000000000026", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000027"), null, "Airplane", new Guid("00000000-0000-0000-0000-000000000010"), "00000000-0000-0000-0000-000000000027", 1, null },
                    { new Guid("00000000-0000-0000-0000-000000000028"), null, "Train", new Guid("00000000-0000-0000-0000-000000000010"), "00000000-0000-0000-0000-000000000028", 1, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountLimits");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Currencies");
        }
    }
}
