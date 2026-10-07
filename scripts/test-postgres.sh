#!/usr/bin/env bash
# Isolated acceptance test. Never removes volumes or touches the normal stack.
set -euo pipefail
umask 077
cd "$(dirname "$0")/.."
mkdir -p .tmp
test_dir=$(mktemp -d "$PWD/.tmp/f1-a.XXXXXX")
project="gurboya-f1a-$(openssl rand -hex 6)"
env_file="$test_dir/.env"
{
  printf 'POSTGRES_DB=gurboya\nPOSTGRES_USER=gurboya_admin\nPOSTGRES_PASSWORD='
  openssl rand -hex 32
} > "$env_file"
# Do not let exported host variables override the isolated test credentials.
unset POSTGRES_DB POSTGRES_USER POSTGRES_PASSWORD
compose() { docker compose --env-file "$env_file" -p "$project" -f compose.yaml "$@"; }
cleanup() {
  result=$?
  trap - EXIT
  if ! compose down; then
    printf 'Test konteynerleri durdurulamadi.\n' >&2
    result=1
  fi
  printf 'Yerel test dosyalari: %s\nKorunan volume: %s_postgres_data\n' "$test_dir" "$project"
  exit "$result"
}
trap cleanup EXIT
sql() {
  compose exec -T db sh -c 'PGPASSWORD="$POSTGRES_PASSWORD" exec psql -X -h db -U "$POSTGRES_USER" -d "$1" -v ON_ERROR_STOP=1 -At' sh "$1"
}
compose config --quiet
if POSTGRES_PASSWORD='' docker compose --env-file .env.example -f compose.yaml config --quiet > /dev/null 2>&1; then
  printf 'Bos parola reddedilmedi.\n' >&2
  exit 1
fi
printf 'PASS: Compose ve bos parola kontrolu\n'
compose up -d --wait --wait-timeout 120 db
container_before=$(compose ps -q db)
test "$(docker inspect --format '{{len .HostConfig.PortBindings}}' "$container_before")" = 0
test "$(docker network inspect --format '{{.Internal}}' "${project}_database")" = true
test "$(sql gurboya <<< 'SELECT count(*) FROM pg_tables WHERE schemaname = '\''public'\'';')" = 0
sql gurboya <<< 'CREATE DATABASE f1a_source;' > /dev/null
sql f1a_source <<'SQL'
CREATE TABLE infrastructure_probe (id integer PRIMARY KEY, label text NOT NULL, amount numeric(12,2) NOT NULL);
INSERT INTO infrastructure_probe VALUES (1, 'GürBoya 2,5 L', 125.50), (2, 'İstanbul', 24.25), (3, 'Kalıcılık', 0.25);
SQL
expected=$(sql f1a_source <<< "SELECT count(*) || '|' || sum(amount) || '|' || string_agg(label, ',' ORDER BY id) FROM infrastructure_probe;")
test "$expected" = '3|150.00|GürBoya 2,5 L,İstanbul,Kalıcılık'
if compose exec -T db sh -c 'PGPASSWORD=invalid-test-password psql -X -w -h db -U "$POSTGRES_USER" -d "$POSTGRES_DB" -c "SELECT 1"' > /dev/null 2>&1; then
  printf 'Yanlis parola kabul edildi.\n' >&2
  exit 1
fi
printf 'PASS: TCP baglantisi, veri yazma, yanlis parola reddi ve ag izolasyonu\n'
compose up -d --force-recreate --pull never --wait --wait-timeout 120 db
test "$container_before" != "$(compose ps -q db)"
actual=$(sql f1a_source <<< "SELECT count(*) || '|' || sum(amount) || '|' || string_agg(label, ',' ORDER BY id) FROM infrastructure_probe;")
test "$actual" = "$expected"
printf 'PASS: Yeni konteynerde veri kaliciligi (3 satir, toplam 150.00)\n'
compose exec -T db sh -c 'exec pg_dump -U "$POSTGRES_USER" -d f1a_source --format=custom --file=/tmp/f1a.dump'
compose cp db:/tmp/f1a.dump "$test_dir/f1a.dump"
test -s "$test_dir/f1a.dump"
sql gurboya <<< 'CREATE DATABASE f1a_restore;' > /dev/null
compose cp "$test_dir/f1a.dump" db:/tmp/f1a-restore.dump
compose exec -T db sh -c 'exec pg_restore -U "$POSTGRES_USER" -d f1a_restore --exit-on-error --single-transaction --no-owner --no-privileges /tmp/f1a-restore.dump'
restored=$(sql f1a_restore <<< "SELECT count(*) || '|' || sum(amount) || '|' || string_agg(label, ',' ORDER BY id) FROM infrastructure_probe;")
test "$restored" = "$expected"
test "$(sql gurboya <<< 'SELECT count(*) FROM pg_tables WHERE schemaname = '\''public'\'';')" = 0
printf 'PASS: Ayri DB geri yukleme (3 satir, toplam 150.00, Turkce metinler); ana DB bos\n'
