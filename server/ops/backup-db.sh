#!/usr/bin/env bash
# Backup of the production SQLite database — installed via cron on the VPS every 30 minutes
# (see README.md, "Backup do banco de dados"). Uses `sqlite3 .backup`, not a plain file copy, so
# a backup taken while the API is writing to the database is still consistent (a raw `cp` could
# catch a half-written page and produce a corrupt backup).
#
# Adjust DB_PATH below if your ConnectionStrings:Default points somewhere else — the default
# assumes the database lives at the systemd service's working directory (the publish folder).
set -euo pipefail

DB_PATH="/var/www/atelie-bebe/publish/atelie-bebe.db"
BACKUP_DIR="/var/backups/atelie-bebe"
KEEP_COUNT=10

mkdir -p "$BACKUP_DIR"

TIMESTAMP="$(date +%Y-%m-%d_%H%M%S)"
DEST="$BACKUP_DIR/atelie-bebe_$TIMESTAMP.db"

sqlite3 "$DB_PATH" ".backup '$DEST'"
gzip "$DEST"

# Keep only the KEEP_COUNT most recent backups (newest-first listing, drop the rest).
ls -1t "$BACKUP_DIR"/atelie-bebe_*.db.gz 2>/dev/null | tail -n "+$((KEEP_COUNT + 1))" | xargs -r rm --

echo "Backup salvo em ${DEST}.gz"
