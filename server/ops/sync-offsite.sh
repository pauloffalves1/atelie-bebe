#!/usr/bin/env bash
# Off-site sync of the local SQLite backups to a cloud remote configured via `rclone config`
# (see README.md, "Backup fora do servidor"). Chained after backup-db.sh in cron so it only
# ever uploads backups that already passed the local retention prune (never more than
# KEEP_COUNT files, so this stays a cheap incremental sync).
set -euo pipefail

BACKUP_DIR="/var/backups/atelie-bebe"
REMOTE="gdrive:atelie-bebe-backups"

rclone sync "$BACKUP_DIR" "$REMOTE" --min-age 30s
