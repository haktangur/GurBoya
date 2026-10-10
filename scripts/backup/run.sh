#!/bin/sh
# Single Compose worker. Only its own completed backups are eligible for retention.
set -eu
umask 077
mkdir -p /backups
chmod 700 /backups
partial=''
finish() {
    if [ -n "$partial" ]; then rm -f -- "$partial"; fi
}
trap finish EXIT
trap 'exit 0' INT TERM

backup_once() {
    name="gurboya-auto-$(date -u +%Y%m%dT%H%M%SZ)-$(cat /proc/sys/kernel/random/uuid).dump"
    partial="/backups/.$name.partial"
    if ! pg_dump --format=custom --lock-wait-timeout=60000 --file="$partial" 2>/dev/null; then
        finish; partial=''
        echo 'Yedek alınamadı; bağlantı, disk alanı ve yetkileri kontrol edin.' >&2
        return 1
    fi
    if ! pg_restore --list "$partial" > /dev/null 2>&1; then
        finish; partial=''
        echo 'Yedek arşivi okunamadı; önceki yedekler korundu.' >&2
        return 1
    fi
    mv -- "$partial" "/backups/$name" || return 1
    partial=''
    (cd /backups && sha256sum "$name" > "$name.sha256") || return 1
    date +%s > /backups/.last-success.tmp || return 1
    mv /backups/.last-success.tmp /backups/.last-success || return 1
    # Retention runs only after success; never touches manual/pre-upgrade backups.
    find /backups -maxdepth 1 -type f -name 'gurboya-auto-*.dump' -mmin +43200 -exec rm -f -- '{}' '{}.sha256' \; || return 1
    echo "Yerel yedek hazır: $name (arşiv okunabilir; geri yükleme provası ayrı yapılır)."
}

if [ "${1:-}" = '--once' ]; then
    backup_once
    exit $?
fi
while :; do
    now=$(date +%s)
    last=0
    if [ -f /backups/.last-success ]; then read -r last < /backups/.last-success || last=0; fi
    case "$last" in ''|*[!0-9]*) last=0 ;; esac
    if [ "$last" -gt "$now" ] || [ "$((now - last))" -ge 3600 ]; then
        backup_once || true
    fi
    sleep 60 &
    wait $! || true
done
