using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GurBoya.Web.Data.Migrations;

/// <inheritdoc />
public partial class StockCount : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "ck_movement_delta",
            schema: "app",
            table: "stock_movements");

        migrationBuilder.DropCheckConstraint(
            name: "ck_movement_kind",
            schema: "app",
            table: "stock_movements");

        migrationBuilder.DropCheckConstraint(
            name: "ck_product_quantity",
            schema: "app",
            table: "products");

        migrationBuilder.AddCheckConstraint(
            name: "ck_movement_delta",
            schema: "app",
            table: "stock_movements",
            sql: "(\"Delta\" <> 0 OR \"Kind\" = 'COUNT') AND abs(\"Delta\"::numeric) <= 1000000000");

        migrationBuilder.AddCheckConstraint(
            name: "ck_movement_kind",
            schema: "app",
            table: "stock_movements",
            sql: "(\"Kind\" = 'RECEIPT' AND \"Delta\" > 0) OR \"Kind\" IN ('ADJUSTMENT','COUNT')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_product_quantity",
            schema: "app",
            table: "products",
            sql: "\"Quantity\" BETWEEN 0 AND 1000000000");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "ck_movement_delta",
            schema: "app",
            table: "stock_movements");

        migrationBuilder.DropCheckConstraint(
            name: "ck_movement_kind",
            schema: "app",
            table: "stock_movements");

        migrationBuilder.DropCheckConstraint(
            name: "ck_product_quantity",
            schema: "app",
            table: "products");

        migrationBuilder.AddCheckConstraint(
            name: "ck_movement_delta",
            schema: "app",
            table: "stock_movements",
            sql: "\"Delta\" <> 0 AND abs(\"Delta\"::numeric) <= 1000000000");

        migrationBuilder.AddCheckConstraint(
            name: "ck_movement_kind",
            schema: "app",
            table: "stock_movements",
            sql: "(\"Kind\" = 'RECEIPT' AND \"Delta\" > 0) OR \"Kind\" = 'ADJUSTMENT'");

        migrationBuilder.AddCheckConstraint(
            name: "ck_product_quantity",
            schema: "app",
            table: "products",
            sql: "\"Quantity\" >= 0");
    }
}
