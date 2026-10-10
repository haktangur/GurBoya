#!/bin/sh
set -eu
read -r last < /backups/.last-success
case "$last" in ''|*[!0-9]*) exit 1 ;; esac
age=$(($(date +%s) - last))
test "$age" -ge 0 && test "$age" -le 3900
