using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GurBoya.Web.Data.Migrations;

/// <inheritdoc />
public partial class SalesAndReturns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "ck_movement_kind",
            schema: "app",
            table: "stock_movements");

        migrationBuilder.AddColumn<long>(
            name: "ReturnLineId",
            schema: "app",
            table: "stock_movements",
            type: "bigint",
            nullable: true);

        migrationBuilder.AddColumn<long>(
            name: "SaleLineId",
            schema: "app",
            table: "stock_movements",
            type: "bigint",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "sales",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                ActorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                PaymentMethod = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                DiscountKind = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                DiscountValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                Net = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_sales", x => x.Id);
                table.CheckConstraint("ck_sale_amounts", "\"Net\" >= 0 AND \"Vat\" >= 0 AND \"Total\" BETWEEN 0 AND 999999999 AND \"Net\" + \"Vat\" = \"Total\"");
                table.CheckConstraint("ck_sale_discount", "\"DiscountKind\" IN ('TL','PERCENT') AND \"DiscountValue\" >= 0 AND (\"DiscountKind\" <> 'PERCENT' OR \"DiscountValue\" <= 100)");
                table.CheckConstraint("ck_sale_payment", "\"PaymentMethod\" IN ('CASH','CARD')");
                table.ForeignKey(
                    name: "FK_sales_AspNetUsers_ActorId",
                    column: x => x.ActorId,
                    principalSchema: "app",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "sale_lines",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SaleId = table.Column<long>(type: "bigint", nullable: false),
                Position = table.Column<int>(type: "integer", nullable: false),
                ProductId = table.Column<long>(type: "bigint", nullable: false),
                ProductName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Color = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Unit = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                PackageLiters = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: true),
                Quantity = table.Column<long>(type: "bigint", nullable: false),
                UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                VatIncluded = table.Column<bool>(type: "boolean", nullable: false),
                VatRate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                IsTinted = table.Column<bool>(type: "boolean", nullable: false),
                TintFee = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                DiscountKind = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                DiscountValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                LineDiscount = table.Column<decimal>(type: "numeric(28,12)", precision: 28, scale: 12, nullable: false),
                ReceiptDiscount = table.Column<decimal>(type: "numeric(28,12)", precision: 28, scale: 12, nullable: false),
                Rounding = table.Column<decimal>(type: "numeric(28,12)", precision: 28, scale: 12, nullable: false),
                Net = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_sale_lines", x => x.Id);
                table.CheckConstraint("ck_sale_line_amounts", "\"Net\" >= 0 AND \"Vat\" >= 0 AND \"Total\" BETWEEN 0 AND 999999999 AND \"Net\" + \"Vat\" = \"Total\"");
                table.CheckConstraint("ck_sale_line_discount", "\"DiscountKind\" IN ('TL','PERCENT') AND \"DiscountValue\" >= 0 AND (\"DiscountKind\" <> 'PERCENT' OR \"DiscountValue\" <= 100) AND \"LineDiscount\" >= 0 AND \"ReceiptDiscount\" >= 0 AND abs(\"Rounding\") < 0.011");
                table.CheckConstraint("ck_sale_line_values", "\"Quantity\" BETWEEN 1 AND 1000000000 AND \"VatRate\" BETWEEN 0 AND 100 AND \"UnitPrice\" BETWEEN 0 AND 999999999 AND \"TintFee\" BETWEEN 0 AND 999999999 AND (\"IsTinted\" OR \"TintFee\" = 0) AND \"Position\" BETWEEN 1 AND 50");
                table.ForeignKey(
                    name: "FK_sale_lines_products_ProductId",
                    column: x => x.ProductId,
                    principalSchema: "app",
                    principalTable: "products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_sale_lines_sales_SaleId",
                    column: x => x.SaleId,
                    principalSchema: "app",
                    principalTable: "sales",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "sales_returns",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SaleId = table.Column<long>(type: "bigint", nullable: false),
                OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                ActorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Reason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                Kind = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                RefundMethod = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                Net = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_sales_returns", x => x.Id);
                table.CheckConstraint("ck_return_amounts", "\"Net\" >= 0 AND \"Vat\" >= 0 AND \"Total\" BETWEEN 0 AND 999999999 AND \"Net\" + \"Vat\" = \"Total\"");
                table.CheckConstraint("ck_return_kind", "\"Kind\" IN ('RETURN','CANCEL') AND \"RefundMethod\" IN ('CASH','CARD') AND length(trim(\"Reason\")) > 0");
                table.ForeignKey(
                    name: "FK_sales_returns_AspNetUsers_ActorId",
                    column: x => x.ActorId,
                    principalSchema: "app",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_sales_returns_sales_SaleId",
                    column: x => x.SaleId,
                    principalSchema: "app",
                    principalTable: "sales",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "return_lines",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SalesReturnId = table.Column<long>(type: "bigint", nullable: false),
                SaleLineId = table.Column<long>(type: "bigint", nullable: false),
                Quantity = table.Column<long>(type: "bigint", nullable: false),
                RestockQuantity = table.Column<long>(type: "bigint", nullable: false),
                Net = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_return_lines", x => x.Id);
                table.CheckConstraint("ck_return_line_amounts", "\"Net\" >= 0 AND \"Vat\" >= 0 AND \"Total\" BETWEEN 0 AND 999999999 AND \"Net\" + \"Vat\" = \"Total\"");
                table.CheckConstraint("ck_return_quantity", "\"Quantity\" BETWEEN 1 AND 1000000000 AND \"RestockQuantity\" BETWEEN 0 AND \"Quantity\"");
                table.ForeignKey(
                    name: "FK_return_lines_sale_lines_SaleLineId",
                    column: x => x.SaleLineId,
                    principalSchema: "app",
                    principalTable: "sale_lines",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_return_lines_sales_returns_SalesReturnId",
                    column: x => x.SalesReturnId,
                    principalSchema: "app",
                    principalTable: "sales_returns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_stock_movements_ReturnLineId",
            schema: "app",
            table: "stock_movements",
            column: "ReturnLineId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_stock_movements_SaleLineId",
            schema: "app",
            table: "stock_movements",
            column: "SaleLineId",
            unique: true);

        migrationBuilder.AddCheckConstraint(
            name: "ck_movement_kind",
            schema: "app",
            table: "stock_movements",
            sql: "(\"Kind\" = 'RECEIPT' AND \"Delta\" > 0) OR (\"Kind\" = 'SALE' AND \"Delta\" < 0) OR (\"Kind\" = 'RETURN' AND \"Delta\" > 0) OR \"Kind\" IN ('ADJUSTMENT','COUNT')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_movement_source",
            schema: "app",
            table: "stock_movements",
            sql: "(\"Kind\" = 'SALE' AND \"SaleLineId\" IS NOT NULL AND \"ReturnLineId\" IS NULL) OR (\"Kind\" = 'RETURN' AND \"ReturnLineId\" IS NOT NULL AND \"SaleLineId\" IS NULL) OR (\"Kind\" NOT IN ('SALE','RETURN') AND \"SaleLineId\" IS NULL AND \"ReturnLineId\" IS NULL)");

        migrationBuilder.CreateIndex(
            name: "IX_return_lines_SaleLineId",
            schema: "app",
            table: "return_lines",
            column: "SaleLineId");

        migrationBuilder.CreateIndex(
            name: "IX_return_lines_SalesReturnId_SaleLineId",
            schema: "app",
            table: "return_lines",
            columns: new[] { "SalesReturnId", "SaleLineId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_sale_lines_ProductId",
            schema: "app",
            table: "sale_lines",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_sale_lines_SaleId_Position",
            schema: "app",
            table: "sale_lines",
            columns: new[] { "SaleId", "Position" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_sales_ActorId",
            schema: "app",
            table: "sales",
            column: "ActorId");

        migrationBuilder.CreateIndex(
            name: "IX_sales_OccurredAt",
            schema: "app",
            table: "sales",
            column: "OccurredAt");

        migrationBuilder.CreateIndex(
            name: "IX_sales_OperationId",
            schema: "app",
            table: "sales",
            column: "OperationId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_sales_returns_ActorId",
            schema: "app",
            table: "sales_returns",
            column: "ActorId");

        migrationBuilder.CreateIndex(
            name: "IX_sales_returns_OccurredAt",
            schema: "app",
            table: "sales_returns",
            column: "OccurredAt");

        migrationBuilder.CreateIndex(
            name: "IX_sales_returns_OperationId",
            schema: "app",
            table: "sales_returns",
            column: "OperationId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_sales_returns_SaleId",
            schema: "app",
            table: "sales_returns",
            column: "SaleId");

        migrationBuilder.AddForeignKey(
            name: "FK_stock_movements_return_lines_ReturnLineId",
            schema: "app",
            table: "stock_movements",
            column: "ReturnLineId",
            principalSchema: "app",
            principalTable: "return_lines",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_stock_movements_sale_lines_SaleLineId",
            schema: "app",
            table: "stock_movements",
            column: "SaleLineId",
            principalSchema: "app",
            principalTable: "sale_lines",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
        migrationBuilder.Sql("""
            CREATE OR REPLACE FUNCTION app.apply_stock_movement() RETURNS trigger LANGUAGE plpgsql AS $$
            DECLARE current_product app.products%ROWTYPE;
            BEGIN
              SELECT * INTO STRICT current_product FROM app.products WHERE "Id" = NEW."ProductId" FOR UPDATE;
              IF NOT current_product."IsActive" AND NEW."Kind" <> 'RETURN' THEN
                RAISE EXCEPTION 'Ürün pasif.' USING ERRCODE = '23514';
              END IF;
              IF current_product."Quantity" + NEW."Delta" < 0 THEN
                RAISE EXCEPTION 'Stokta yok veya miktar yetersiz.' USING ERRCODE = '23514';
              END IF;
              UPDATE app.products SET "Quantity" = "Quantity" + NEW."Delta", "Version" = "Version" + 1 WHERE "Id" = NEW."ProductId";
              RETURN NEW;
            END $$;
            CREATE FUNCTION app.lock_return_sale() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
              PERFORM 1 FROM app.sales WHERE "Id" = NEW."SaleId" FOR UPDATE;
              RETURN NEW;
            END $$;
            CREATE TRIGGER return_sale_lock BEFORE INSERT ON app.sales_returns FOR EACH ROW EXECUTE FUNCTION app.lock_return_sale();

            CREATE FUNCTION app.check_sale_document() RETURNS trigger LANGUAGE plpgsql AS $$
            DECLARE sid bigint; s app.sales%ROWTYPE;
            BEGIN
              IF TG_TABLE_NAME = 'sales' THEN sid := NEW."Id"; ELSE sid := NEW."SaleId"; END IF;
              SELECT * INTO STRICT s FROM app.sales WHERE "Id" = sid;
              IF NOT EXISTS (SELECT 1 FROM app.sale_lines WHERE "SaleId" = sid)
                 OR (s."Net", s."Vat", s."Total") IS DISTINCT FROM
                    (SELECT ROW(sum("Net"), sum("Vat"), sum("Total")) FROM app.sale_lines WHERE "SaleId" = sid)
                 OR NOT EXISTS (SELECT 1 FROM app.operations WHERE "Id" = s."OperationId" AND "ResultId" = sid)
                 OR EXISTS (SELECT 1 FROM app.sale_lines l LEFT JOIN app.stock_movements m ON m."SaleLineId" = l."Id"
                            WHERE l."SaleId" = sid AND (m."Id" IS NULL OR m."Kind" <> 'SALE' OR m."ProductId" <> l."ProductId" OR m."Delta" <> -l."Quantity"))
                 OR EXISTS (SELECT 1 FROM app.sale_lines l JOIN app.products p ON p."Id" = l."ProductId" WHERE l."SaleId" = sid AND l."IsTinted" AND NOT p."IsPaint") THEN
                RAISE EXCEPTION 'Satış, toplam veya stok kaydı tutarsız.' USING ERRCODE = '23514';
              END IF;
              RETURN NULL;
            END $$;
            CREATE CONSTRAINT TRIGGER sale_integrity AFTER INSERT ON app.sales DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION app.check_sale_document();
            CREATE CONSTRAINT TRIGGER sale_line_integrity AFTER INSERT ON app.sale_lines DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION app.check_sale_document();

            CREATE FUNCTION app.check_return_document() RETURNS trigger LANGUAGE plpgsql AS $$
            DECLARE rid bigint; r app.sales_returns%ROWTYPE;
            BEGIN
              IF TG_TABLE_NAME = 'sales_returns' THEN rid := NEW."Id"; ELSE rid := NEW."SalesReturnId"; END IF;
              SELECT * INTO STRICT r FROM app.sales_returns WHERE "Id" = rid;
              IF NOT EXISTS (SELECT 1 FROM app.return_lines WHERE "SalesReturnId" = rid)
                 OR (r."Net", r."Vat", r."Total") IS DISTINCT FROM
                    (SELECT ROW(sum("Net"), sum("Vat"), sum("Total")) FROM app.return_lines WHERE "SalesReturnId" = rid)
                 OR NOT EXISTS (SELECT 1 FROM app.operations WHERE "Id" = r."OperationId" AND "ResultId" = rid)
                 OR EXISTS (SELECT 1 FROM app.return_lines l JOIN app.sale_lines s ON s."Id" = l."SaleLineId"
                            LEFT JOIN app.stock_movements m ON m."ReturnLineId" = l."Id"
                            WHERE l."SalesReturnId" = rid AND (s."SaleId" <> r."SaleId" OR s."IsTinted"
                              OR (l."RestockQuantity" > 0 AND (m."Id" IS NULL OR m."ProductId" <> s."ProductId" OR m."Kind" <> 'RETURN' OR m."Delta" <> l."RestockQuantity"))
                              OR (l."RestockQuantity" = 0 AND m."Id" IS NOT NULL)))
                 OR EXISTS (SELECT 1 FROM app.sale_lines s JOIN app.return_lines l ON l."SaleLineId" = s."Id"
                            WHERE s."SaleId" = r."SaleId" GROUP BY s."Id"
                            HAVING sum(l."Quantity") > s."Quantity" OR sum(l."Total") > s."Total" OR sum(l."Net") > s."Net" OR sum(l."Vat") > s."Vat"
                            OR sum(l."Total") <> LEAST(s."Total", ceil(s."Total" * sum(l."Quantity") / s."Quantity" * 100) / 100)) THEN
                RAISE EXCEPTION 'İade sınırı, kaynak veya stok kaydı tutarsız.' USING ERRCODE = '23514';
              END IF;
              RETURN NULL;
            END $$;
            CREATE CONSTRAINT TRIGGER return_integrity AFTER INSERT ON app.sales_returns DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION app.check_return_document();
            CREATE CONSTRAINT TRIGGER return_line_integrity AFTER INSERT ON app.return_lines DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION app.check_return_document();
            CREATE FUNCTION app.check_sales_movement() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
              IF NEW."Kind" = 'SALE' AND NOT EXISTS (SELECT 1 FROM app.sale_lines WHERE "Id" = NEW."SaleLineId" AND "ProductId" = NEW."ProductId" AND "Quantity" = -NEW."Delta") THEN
                RAISE EXCEPTION 'Satış stok kaynağı tutarsız.' USING ERRCODE = '23514';
              END IF;
              IF NEW."Kind" = 'RETURN' AND NOT EXISTS (SELECT 1 FROM app.return_lines r JOIN app.sale_lines s ON s."Id" = r."SaleLineId" WHERE r."Id" = NEW."ReturnLineId" AND s."ProductId" = NEW."ProductId" AND r."RestockQuantity" = NEW."Delta") THEN
                RAISE EXCEPTION 'İade stok kaynağı tutarsız.' USING ERRCODE = '23514';
              END IF;
              RETURN NEW;
            END $$;
            CREATE TRIGGER sales_movement_source BEFORE INSERT ON app.stock_movements FOR EACH ROW EXECUTE FUNCTION app.check_sales_movement();
            CREATE FUNCTION app.guard_document_append() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
              IF TG_TABLE_NAME = 'sale_lines' THEN
                IF EXISTS (SELECT 1 FROM app.sales s JOIN app.operations o ON o."Id" = s."OperationId" WHERE s."Id" = NEW."SaleId") THEN
                  RAISE EXCEPTION 'Tamamlanan satışa satır eklenemez.' USING ERRCODE = '23514';
                END IF;
              ELSE
                IF EXISTS (SELECT 1 FROM app.sales_returns r JOIN app.operations o ON o."Id" = r."OperationId" WHERE r."Id" = NEW."SalesReturnId") THEN
                  RAISE EXCEPTION 'Tamamlanan iadeye satır eklenemez.' USING ERRCODE = '23514';
                END IF;
              END IF;
              RETURN NEW;
            END $$;
            CREATE TRIGGER sale_line_append BEFORE INSERT ON app.sale_lines FOR EACH ROW EXECUTE FUNCTION app.guard_document_append();
            CREATE TRIGGER return_line_append BEFORE INSERT ON app.return_lines FOR EACH ROW EXECUTE FUNCTION app.guard_document_append();
            CREATE TRIGGER sale_immutable BEFORE UPDATE OR DELETE ON app.sales FOR EACH ROW EXECUTE FUNCTION app.prevent_history_change();
            CREATE TRIGGER sale_line_immutable BEFORE UPDATE OR DELETE ON app.sale_lines FOR EACH ROW EXECUTE FUNCTION app.prevent_history_change();
            CREATE TRIGGER return_immutable BEFORE UPDATE OR DELETE ON app.sales_returns FOR EACH ROW EXECUTE FUNCTION app.prevent_history_change();
            CREATE TRIGGER return_line_immutable BEFORE UPDATE OR DELETE ON app.return_lines FOR EACH ROW EXECUTE FUNCTION app.prevent_history_change();
            REVOKE UPDATE, DELETE ON app.sale_lines, app.sales_returns, app.return_lines FROM gurboya_app;
            -- SELECT FOR UPDATE requires UPDATE privilege; the immutable trigger rejects actual writes.
            REVOKE DELETE ON app.sales FROM gurboya_app;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TRIGGER sale_line_append ON app.sale_lines;
            DROP TRIGGER return_line_append ON app.return_lines;
            DROP FUNCTION app.guard_document_append();
            DROP TRIGGER sales_movement_source ON app.stock_movements;
            DROP FUNCTION app.check_sales_movement();
            DROP TRIGGER sale_integrity ON app.sales;
            DROP TRIGGER sale_line_integrity ON app.sale_lines;
            DROP FUNCTION app.check_sale_document();
            DROP TRIGGER return_integrity ON app.sales_returns;
            DROP TRIGGER return_line_integrity ON app.return_lines;
            DROP FUNCTION app.check_return_document();
            DROP TRIGGER return_sale_lock ON app.sales_returns;
            DROP FUNCTION app.lock_return_sale();
            CREATE OR REPLACE FUNCTION app.apply_stock_movement() RETURNS trigger LANGUAGE plpgsql AS $$
            DECLARE current_product app.products%ROWTYPE;
            BEGIN
              SELECT * INTO STRICT current_product FROM app.products WHERE "Id" = NEW."ProductId" FOR UPDATE;
              IF NOT current_product."IsActive" THEN
                RAISE EXCEPTION 'Ürün pasif.' USING ERRCODE = '23514';
              END IF;
              IF current_product."Quantity" + NEW."Delta" < 0 THEN
                RAISE EXCEPTION 'Stokta yok veya miktar yetersiz.' USING ERRCODE = '23514';
              END IF;
              UPDATE app.products SET "Quantity" = "Quantity" + NEW."Delta", "Version" = "Version" + 1 WHERE "Id" = NEW."ProductId";
              RETURN NEW;
            END $$;
            """);
        migrationBuilder.DropForeignKey(
            name: "FK_stock_movements_return_lines_ReturnLineId",
            schema: "app",
            table: "stock_movements");

        migrationBuilder.DropForeignKey(
            name: "FK_stock_movements_sale_lines_SaleLineId",
            schema: "app",
            table: "stock_movements");

        migrationBuilder.DropTable(
            name: "return_lines",
            schema: "app");

        migrationBuilder.DropTable(
            name: "sale_lines",
            schema: "app");

        migrationBuilder.DropTable(
            name: "sales_returns",
            schema: "app");

        migrationBuilder.DropTable(
            name: "sales",
            schema: "app");

        migrationBuilder.DropIndex(
            name: "IX_stock_movements_ReturnLineId",
            schema: "app",
            table: "stock_movements");

        migrationBuilder.DropIndex(
            name: "IX_stock_movements_SaleLineId",
            schema: "app",
            table: "stock_movements");

        migrationBuilder.DropCheckConstraint(
            name: "ck_movement_kind",
            schema: "app",
            table: "stock_movements");

        migrationBuilder.DropCheckConstraint(
            name: "ck_movement_source",
            schema: "app",
            table: "stock_movements");

        migrationBuilder.DropColumn(
            name: "ReturnLineId",
            schema: "app",
            table: "stock_movements");

        migrationBuilder.DropColumn(
            name: "SaleLineId",
            schema: "app",
            table: "stock_movements");

        migrationBuilder.AddCheckConstraint(
            name: "ck_movement_kind",
            schema: "app",
            table: "stock_movements",
            sql: "(\"Kind\" = 'RECEIPT' AND \"Delta\" > 0) OR \"Kind\" IN ('ADJUSTMENT','COUNT')");
    }
}
