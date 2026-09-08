#!/usr/bin/env bash
# Off-site sync of uploaded media (product/gallery/site photos) to the same rclone remote used
# for database backups (see README.md, "Backup de mídia"). Chained after sync-offsite.sh in cron.
# Unlike the SQLite backup, there's no local retention copy to make first — the uploads folder
# itself is the source of truth, so this mirrors it straight to Drive.
set -euo pipefail

UPLOADS_DIR="/var/www/atelie-bebe/uploads"
REMOTE="gdrive:atelie-bebe-uploads-backup"

rclone sync "$UPLOADS_DIR" "$REMOTE" --min-age 30s
