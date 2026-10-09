#!/usr/bin/env bash
# F1-B real PostgreSQL + HTTP acceptance test; isolated project, no volume removal.
set -euo pipefail
umask 077
cd "$(dirname "$0")/.."
mkdir -p .tmp
test_dir=$(mktemp -d "$PWD/.tmp/f1-b.XXXXXX")
project="gurboya-f1b-$(openssl rand -hex 6)"
env_file="$test_dir/.env"
{
    printf 'POSTGRES_DB=gurboya\nPOSTGRES_USER=gurboya_admin\nWEB_PORT=0\n'
    for key in POSTGRES_PASSWORD APP_DB_PASSWORD MIGRATION_DB_PASSWORD ADMIN_PASSWORD; do
        printf '%s=%s\n' "$key" "$(openssl rand -hex 32)"
    done
} > "$env_file"
unset POSTGRES_DB POSTGRES_USER POSTGRES_PASSWORD APP_DB_PASSWORD MIGRATION_DB_PASSWORD ADMIN_PASSWORD WEB_PORT
compose() { docker compose --env-file "$env_file" -p "$project" -f compose.yaml -f compose.app.yaml "$@"; }
cleanup() {
    result=$?
    trap - EXIT
    compose logs --no-color > "$test_dir/containers.log" 2>&1 || result=1
    compose down > /dev/null 2>&1 || result=1
    printf 'Yerel test dosyaları: %s\nKorunan volume: %s_postgres_data\n' "$test_dir" "$project"
    exit "$result"
}
trap cleanup EXIT
sql() { compose exec -T db sh -c 'exec psql -X -U "$POSTGRES_USER" -d "$POSTGRES_DB" -v ON_ERROR_STOP=1 -At'; }
request() {
    path=$1
    expected_code=$2
    expected_text=$3
    for attempt in {1..20}; do
        code=$(curl -sS --max-time 10 -o "$test_dir/response" -w '%{http_code}' "$base_url$path") || code=000
        if [ "$code" = "$expected_code" ] && grep -Fq "$expected_text" "$test_dir/response"; then
            return 0
        fi
        sleep 1
    done
    printf 'HTTP kontrolü başarısız: %s, beklenen %s, alınan %s\n' "$path" "$expected_code" "$code" >&2
    return 1
}
compose config --quiet
compose build web
compose up -d --wait --wait-timeout 120 db
compose run --rm db-setup
compose run --rm db-setup
compose up -d web
base_url="http://$(compose port web 8080)"
web_container=$(compose ps -q web)
test "$(docker inspect --format '{{(index (index .HostConfig.PortBindings "8080/tcp") 0).HostIp}}' "$web_container")" = 127.0.0.1
test "$(compose exec -T web id -u)" != 0
request /health/live 200 'Uygulama'
request /health/ready 503 'migration_required'
request / 503 'Yeniden dene'
test "$(sql <<< "SELECT count(*) FROM information_schema.tables WHERE table_schema IN ('app','infrastructure');")" = 0
printf 'PASS: Açılış şemayı değiştirmiyor; migration eksikliği 503 ve Türkçe ekranla gösteriliyor.\n'
compose run --rm migrate
test "$(sql <<< 'SELECT count(*) FROM infrastructure."__EFMigrationsHistory";')" = 3
compose run --rm migrate
test "$(sql <<< 'SELECT count(*) FROM infrastructure."__EFMigrationsHistory";')" = 3
request /health/ready 200 'ready'
request / 200 'Sistem haz'
request /site.css 200 'font-family'
request /missing-page 404 'Sayfa'
test "$(sql <<< "SELECT count(*) FROM information_schema.tables WHERE table_schema = 'app';")" -gt 0
printf 'PASS: Migration uygulama/tekrar, hazır durumu, yerel CSS ve Türkçe 404. F2 şeması hazır.\n'
test "$(sql <<< "SELECT bool_and(NOT rolsuper AND NOT rolcreatedb AND NOT rolcreaterole AND NOT rolbypassrls) FROM pg_roles WHERE rolname IN ('gurboya_app','gurboya_migrator');")" = t
test "$(sql <<< "SELECT has_schema_privilege('gurboya_app','app','CREATE') OR has_database_privilege('gurboya_app',current_database(),'CREATE') OR has_database_privilege('gurboya_app',current_database(),'TEMP');")" = f
if compose run --rm --no-deps --entrypoint sh db-setup -c 'PGPASSWORD="$APP_DB_PASSWORD" psql -X -U gurboya_app -v ON_ERROR_STOP=1 -c "CREATE TABLE app.forbidden (id integer);"' > "$test_dir/ddl-denied.log" 2>&1; then
    printf 'Uygulama DDL yetkisi reddedilmedi.\n' >&2; exit 1
fi
grep -q 'permission denied' "$test_dir/ddl-denied.log"
if compose run --rm --no-deps --entrypoint sh db-setup -c 'PGPASSWORD="$APP_DB_PASSWORD" psql -X -U gurboya_app -v ON_ERROR_STOP=1 -c '\''DELETE FROM infrastructure."__EFMigrationsHistory";'\''' > "$test_dir/history-denied.log" 2>&1; then
    printf 'Migration geçmişi yazma yetkisi reddedilmedi.\n' >&2; exit 1
fi
grep -q 'permission denied' "$test_dir/history-denied.log"
printf 'PASS: Uygulama superuser değil; DDL, geçici tablo ve migration geçmişini değiştirme yetkisi yok.\n'
compose run --rm admin
compose run --rm admin
python3 scripts/test-inventory.py "$base_url" "$env_file"
test "$(sql <<'SQL'
SELECT count(*) FROM app.products p WHERE p."Quantity" <> COALESCE((SELECT sum(m."Delta") FROM app.stock_movements m WHERE m."ProductId" = p."Id"), 0);
SQL
)" = 0
before=$(sql <<< 'SELECT sum("Quantity") FROM app.products;')
if sql > "$test_dir/rollback.log" 2>&1 <<'SQL'
BEGIN;
INSERT INTO app.stock_movements ("ProductId", "OperationId", "RequestHash", "Delta", "Kind", "Reason", "PurchaseNet", "ActorId", "OccurredAt")
SELECT p."Id", gen_random_uuid(), 'rollback', 1, 'RECEIPT', 'rollback test', 1, u."Id", now() FROM app.products p CROSS JOIN app."AspNetUsers" u LIMIT 1;
SELECT 1/0;
COMMIT;
SQL
then
    printf 'Zorlanan transaction hatası oluşmadı.\n' >&2; exit 1
fi
test "$(sql <<< 'SELECT sum("Quantity") FROM app.products;')" = "$before"
if sql > "$test_dir/balance-guard.log" 2>&1 <<'SQL'
UPDATE app.products SET "Quantity" = "Quantity" + 1;
SQL
then
    printf 'Doğrudan bakiye değiştirme engellenmedi.\n' >&2; exit 1
fi
if sql > "$test_dir/immutable.log" 2>&1 <<'SQL'
DELETE FROM app.stock_movements;
SQL
then
    printf 'Geçmiş silme engellenmedi.\n' >&2; exit 1
fi
compose exec -T db sh -c 'pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" -Fc -f /tmp/f2.dump'
compose cp db:/tmp/f2.dump "$test_dir/f2.dump"
sql <<< 'CREATE DATABASE f2_restore;' > /dev/null
compose exec -T db sh -c 'pg_restore -U "$POSTGRES_USER" -d f2_restore --exit-on-error --single-transaction --no-owner --no-privileges /tmp/f2.dump'
restored=$(compose exec -T db sh -c 'psql -X -U "$POSTGRES_USER" -d f2_restore -At' <<'SQL'
SELECT sum("Quantity") FROM app.products;
SQL
)
test "$restored" = "$before"
printf 'PASS: Stok mutabakatı, transaction rollback, DB korumaları ve ayrı DB restore.\n'
compose up -d --force-recreate --pull never web
base_url="http://$(compose port web 8080)"
request /health/ready 200 'ready'
test "$(curl -sS -b "$test_dir/session.cookies" -o "$test_dir/recreated.html" -w '%{http_code}' "$base_url/Products")" = 200
printf 'PASS: Konteyner yeniden oluşturma sonrası oturum korunuyor.\n'
compose stop db
request /health/live 200 'Uygulama'
request /health/ready 503 'database_unavailable'
request / 503 'Yeniden dene'
compose up -d --wait --wait-timeout 120 db
request /health/ready 200 'ready'
request / 200 'Sistem haz'
printf 'PASS: DB kesintisinde 503, canlı uygulama ve DB dönüşünde otomatik toparlanma.\n'
compose logs --no-color > "$test_dir/containers.log"
while IFS='=' read -r key value; do
    case "$key" in
        *PASSWORD) if grep -Fq "$value" "$test_dir/containers.log"; then
            printf 'Loglarda parola bulundu.\n' >&2; exit 1
        fi ;;
    esac
done < "$env_file"
printf 'PASS: Konteyner loglarında test parolaları yok.\n'
if grep -q 'libgssapi' "$test_dir/containers.log"; then
    printf 'Beklenmeyen Kerberos bağımlılığı uyarısı.\n' >&2; exit 1
fi
