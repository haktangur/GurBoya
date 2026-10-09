#!/bin/sh
set -eu
: "${APP_DB_PASSWORD:?Uygulama parolası eksik}"
: "${MIGRATION_DB_PASSWORD:?Migration parolası eksik}"
# Disable statement logging for this administrative session: SQL contains passwords.
export PGOPTIONS='-c log_statement=none -c log_min_error_statement=panic'
if psql -X -v ON_ERROR_STOP=1 -f /setup/roles.sql > /dev/null 2>&1; then
    printf 'Veritabanı hesapları ve şema yetkileri hazır.\n'
else
    printf 'Veritabanı hesapları hazırlanamadı. Yönetim bağlantısını ve kurulum ayarlarını kontrol edin.\n' >&2
    exit 1
fi
