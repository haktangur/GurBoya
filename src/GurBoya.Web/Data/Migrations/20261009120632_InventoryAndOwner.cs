using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GurBoya.Web.Data.Migrations;

/// <inheritdoc />
public partial class InventoryAndOwner : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "app");

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                PasswordHash = table.Column<string>(type: "text", nullable: true),
                SecurityStamp = table.Column<string>(type: "text", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                PhoneNumber = table.Column<string>(type: "text", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "operations",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ResultId = table.Column<long>(type: "bigint", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_operations", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "products",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Color = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Barcode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                IsPaint = table.Column<bool>(type: "boolean", nullable: false),
                PackageLiters = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: true),
                Unit = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                PurchasePrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                SalePrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                VatRate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                VatIncluded = table.Column<bool>(type: "boolean", nullable: false),
                Quantity = table.Column<long>(type: "bigint", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                Version = table.Column<long>(type: "bigint", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_products", x => x.Id);
                table.CheckConstraint("ck_product_amounts", "\"PurchasePrice\" BETWEEN 0 AND 999999999 AND \"SalePrice\" BETWEEN 0 AND 999999999 AND \"VatRate\" BETWEEN 0 AND 100 AND (\"PackageLiters\" IS NULL OR \"PackageLiters\" BETWEEN 0.001 AND 999999)");
                table.CheckConstraint("ck_product_name", "length(trim(\"Name\")) > 0");
                table.CheckConstraint("ck_product_paint", "NOT \"IsPaint\" OR (\"Unit\" = 'BOX' AND \"PackageLiters\" IS NOT NULL AND \"PackageLiters\" > 0 AND length(trim(\"Brand\")) > 0 AND length(trim(\"Color\")) > 0)");
                table.CheckConstraint("ck_product_quantity", "\"Quantity\" >= 0");
                table.CheckConstraint("ck_product_unit", "\"Unit\" IN ('BOX','PIECE','GRAM')");
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>(type: "text", nullable: false),
                ClaimType = table.Column<string>(type: "text", nullable: true),
                ClaimValue = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalSchema: "app",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            schema: "app",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "text", nullable: false),
                ProviderKey = table.Column<string>(type: "text", nullable: false),
                ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                UserId = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalSchema: "app",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            schema: "app",
            columns: table => new
            {
                UserId = table.Column<string>(type: "text", nullable: false),
                LoginProvider = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Value = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalSchema: "app",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "price_changes",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ProductId = table.Column<long>(type: "bigint", nullable: false),
                PurchasePrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                SalePrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                VatRate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                VatIncluded = table.Column<bool>(type: "boolean", nullable: false),
                Reason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                ActorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_price_changes", x => x.Id);
                table.ForeignKey(
                    name: "FK_price_changes_AspNetUsers_ActorId",
                    column: x => x.ActorId,
                    principalSchema: "app",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_price_changes_products_ProductId",
                    column: x => x.ProductId,
                    principalSchema: "app",
                    principalTable: "products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "stock_movements",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ProductId = table.Column<long>(type: "bigint", nullable: false),
                OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Delta = table.Column<long>(type: "bigint", nullable: false),
                Kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                Reason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                PurchaseNet = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                ActorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_stock_movements", x => x.Id);
                table.CheckConstraint("ck_movement_delta", "\"Delta\" <> 0 AND abs(\"Delta\"::numeric) <= 1000000000");
                table.CheckConstraint("ck_movement_kind", "(\"Kind\" = 'RECEIPT' AND \"Delta\" > 0) OR \"Kind\" = 'ADJUSTMENT'");
                table.CheckConstraint("ck_movement_reason", "length(trim(\"Reason\")) > 0");
                table.ForeignKey(
                    name: "FK_stock_movements_AspNetUsers_ActorId",
                    column: x => x.ActorId,
                    principalSchema: "app",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_stock_movements_products_ProductId",
                    column: x => x.ProductId,
                    principalSchema: "app",
                    principalTable: "products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserClaims_UserId",
            schema: "app",
            table: "AspNetUserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserLogins_UserId",
            schema: "app",
            table: "AspNetUserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            schema: "app",
            table: "AspNetUsers",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            schema: "app",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_price_changes_ActorId",
            schema: "app",
            table: "price_changes",
            column: "ActorId");

        migrationBuilder.CreateIndex(
            name: "IX_price_changes_ProductId_OccurredAt",
            schema: "app",
            table: "price_changes",
            columns: new[] { "ProductId", "OccurredAt" });

        migrationBuilder.CreateIndex(
            name: "IX_products_Barcode",
            schema: "app",
            table: "products",
            column: "Barcode",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_products_RequestId",
            schema: "app",
            table: "products",
            column: "RequestId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_stock_movements_ActorId",
            schema: "app",
            table: "stock_movements",
            column: "ActorId");

        migrationBuilder.CreateIndex(
            name: "IX_stock_movements_OperationId",
            schema: "app",
            table: "stock_movements",
            column: "OperationId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_stock_movements_ProductId_OccurredAt",
            schema: "app",
            table: "stock_movements",
            columns: new[] { "ProductId", "OccurredAt" });
        migrationBuilder.Sql("""
            CREATE FUNCTION app.guard_product() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
              IF TG_OP = 'INSERT' AND NEW."Quantity" <> 0 THEN
                RAISE EXCEPTION 'Stok bakiyesi hareketle oluşturulmalı.' USING ERRCODE = '23514';
              END IF;
              IF TG_OP = 'UPDATE' THEN
                IF NEW."Quantity" <> OLD."Quantity" AND pg_trigger_depth() < 2 THEN
                  RAISE EXCEPTION 'Stok bakiyesi doğrudan değiştirilemez.' USING ERRCODE = '23514';
                END IF;
                IF (NEW."Unit", NEW."IsPaint", NEW."PackageLiters") IS DISTINCT FROM (OLD."Unit", OLD."IsPaint", OLD."PackageLiters")
                   AND EXISTS (SELECT 1 FROM app.stock_movements WHERE "ProductId" = OLD."Id") THEN
                  RAISE EXCEPTION 'Stok geçmişi olan ürünün birimi değiştirilemez.' USING ERRCODE = '23514';
                END IF;
              END IF;
              RETURN NEW;
            END $$;
            CREATE TRIGGER product_guard BEFORE INSERT OR UPDATE ON app.products FOR EACH ROW EXECUTE FUNCTION app.guard_product();
            CREATE FUNCTION app.apply_stock_movement() RETURNS trigger LANGUAGE plpgsql AS $$
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
            CREATE TRIGGER stock_apply BEFORE INSERT ON app.stock_movements FOR EACH ROW EXECUTE FUNCTION app.apply_stock_movement();
            CREATE FUNCTION app.prevent_history_change() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
              RAISE EXCEPTION 'İşlem geçmişi değiştirilemez veya silinemez.' USING ERRCODE = '23514';
            END $$;
            CREATE TRIGGER stock_immutable BEFORE UPDATE OR DELETE ON app.stock_movements FOR EACH ROW EXECUTE FUNCTION app.prevent_history_change();
            CREATE TRIGGER price_immutable BEFORE UPDATE OR DELETE ON app.price_changes FOR EACH ROW EXECUTE FUNCTION app.prevent_history_change();
            CREATE TRIGGER operation_immutable BEFORE UPDATE OR DELETE ON app.operations FOR EACH ROW EXECUTE FUNCTION app.prevent_history_change();
            REVOKE UPDATE, DELETE ON app.stock_movements, app.price_changes, app.operations FROM gurboya_app;
            REVOKE DELETE ON app.products FROM gurboya_app;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TRIGGER product_guard ON app.products;
            DROP TRIGGER stock_apply ON app.stock_movements;
            DROP TRIGGER stock_immutable ON app.stock_movements;
            DROP TRIGGER price_immutable ON app.price_changes;
            DROP TRIGGER operation_immutable ON app.operations;
            DROP FUNCTION app.guard_product();
            DROP FUNCTION app.apply_stock_movement();
            DROP FUNCTION app.prevent_history_change();
            """);
        migrationBuilder.DropTable(
            name: "AspNetUserClaims",
            schema: "app");

        migrationBuilder.DropTable(
            name: "AspNetUserLogins",
            schema: "app");

        migrationBuilder.DropTable(
            name: "AspNetUserTokens",
            schema: "app");

        migrationBuilder.DropTable(
            name: "operations",
            schema: "app");

        migrationBuilder.DropTable(
            name: "price_changes",
            schema: "app");

        migrationBuilder.DropTable(
            name: "stock_movements",
            schema: "app");

        migrationBuilder.DropTable(
            name: "AspNetUsers",
            schema: "app");

        migrationBuilder.DropTable(
            name: "products",
            schema: "app");
    }
}
