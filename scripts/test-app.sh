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
    for key in POSTGRES_PASSWORD APP_DB_PASSWORD MIGRATION_DB_PASSWORD; do
        printf '%s=%s\n' "$key" "$(openssl rand -hex 32)"
    done
} > "$env_file"
unset POSTGRES_DB POSTGRES_USER POSTGRES_PASSWORD APP_DB_PASSWORD MIGRATION_DB_PASSWORD WEB_PORT
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
test "$(sql <<< 'SELECT count(*) FROM infrastructure."__EFMigrationsHistory";')" = 1
compose run --rm migrate
test "$(sql <<< 'SELECT count(*) FROM infrastructure."__EFMigrationsHistory";')" = 1
request /health/ready 200 'ready'
request / 200 'Sistem haz'
request /site.css 200 'font-family'
request /missing-page 404 'Sayfa'
test "$(sql <<< "SELECT count(*) FROM information_schema.tables WHERE table_schema = 'app';")" = 0
printf 'PASS: Migration uygulama/tekrar, hazır durumu, yerel CSS ve Türkçe 404. İş tablosu yok.\n'
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
