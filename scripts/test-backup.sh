#!/usr/bin/env bash
# F4 backup lifecycle, isolated synthetic DB and retained test volumes.
set -euo pipefail
umask 077
cd "$(dirname "$0")/.."
mkdir -p .tmp
test_dir=$(mktemp -d "$PWD/.tmp/f4-backup.XXXXXX")
project="gurboya-f4-backup-$(openssl rand -hex 6)"
env_file="$test_dir/.env"
printf 'POSTGRES_DB=gurboya\nPOSTGRES_USER=gurboya_admin\nPOSTGRES_PASSWORD=%s\n' "$(openssl rand -hex 32)" > "$env_file"
mkdir "$test_dir/archives"
printf 'services:\n  backup:\n    volumes:\n      - "%s:/backups"\n' "$test_dir/archives" > "$test_dir/override.yaml"
unset POSTGRES_DB POSTGRES_USER POSTGRES_PASSWORD
compose() { docker compose --env-file "$env_file" -p "$project" -f compose.yaml -f compose.backup.yaml -f "$test_dir/override.yaml" "$@"; }
cleanup() {
    result=$?
    trap - EXIT
    compose logs --no-color > "$test_dir/containers.log" 2>&1 || result=1
    compose down > /dev/null 2>&1 || result=1
    printf 'Yedek test kanıtı: %s; korunan volume: %s_postgres_data\n' "$test_dir" "$project"
    exit "$result"
}
trap cleanup EXIT
compose config --quiet
compose up -d --wait db
compose exec -T db sh -c 'psql -X -U "$POSTGRES_USER" -d "$POSTGRES_DB" -v ON_ERROR_STOP=1' <<'SQL'
CREATE TABLE proof (id integer PRIMARY KEY, label text, total numeric(18,2));
INSERT INTO proof VALUES (1,'Türkçe boya',12.34),(2,'İade',-2.01);
SQL
compose run --rm backup --once
archive=$(find "$test_dir/archives" -name '*.dump' -type f)
test -n "$archive"
compose run --rm --entrypoint sh backup -c 'cd /backups; sha256sum -c ./*.sha256; sh /backup-scripts/health.sh'
compose cp "$archive" db:/tmp/proof.dump
compose exec -T db sh -c 'createdb -U "$POSTGRES_USER" restore_proof; pg_restore -U "$POSTGRES_USER" -d restore_proof --exit-on-error --single-transaction --no-owner --no-privileges /tmp/proof.dump'
restored=$(compose exec -T db sh -c 'psql -X -U "$POSTGRES_USER" -d restore_proof -Atc "SELECT count(*),sum(total),string_agg(label, chr(124) ORDER BY id) FROM proof;"')
test "$restored" = '2|10.33|Türkçe boya|İade'
before=$(cat "$test_dir/archives/.last-success")
if compose run --rm -e PGDATABASE=does_not_exist backup --once; then
    echo 'Başarısız yedek başarılı sayıldı.' >&2; exit 1
fi
test "$(cat "$test_dir/archives/.last-success")" = "$before"
test -f "$archive"
compose run --rm --entrypoint sh backup -c 'touch /backups/gurboya-auto-expired.dump /backups/gurboya-auto-expired.dump.sha256 /backups/manual.dump; touch -d "31 days ago" /backups/gurboya-auto-expired.dump /backups/manual.dump; echo 0 > /backups/.last-success'
compose up -d backup
for attempt in {1..30}; do
    if [ "$(cat "$test_dir/archives/.last-success")" != 0 ]; then break; fi
    sleep 1
done
test "$(cat "$test_dir/archives/.last-success")" != 0
test ! -f "$test_dir/archives/gurboya-auto-expired.dump"
test ! -f "$test_dir/archives/gurboya-auto-expired.dump.sha256"
test -f "$test_dir/archives/manual.dump"
count_before=$(find "$test_dir/archives" -name 'gurboya-auto-*.dump' | wc -l)
compose restart backup
sleep 3
test "$(find "$test_dir/archives" -name 'gurboya-auto-*.dump' | wc -l)" = "$count_before"
compose exec -T backup sh /backup-scripts/health.sh
printf 'PASS F4: Dump/checksum/restore, failure preserves backups, overdue startup, 30-day retention, manual backup preservation, restart avoids duplicate.\n'
