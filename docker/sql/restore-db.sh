#!/bin/bash
set -euo pipefail

sql_pid=""

cleanup() {
  if [[ -n "$sql_pid" ]]; then
    echo "Shutting down SQL Server..."
    kill "$sql_pid" >/dev/null 2>&1 || true
    wait "$sql_pid" 2>/dev/null || true
  fi
}
trap cleanup EXIT INT TERM

echo "Starting SQL Server..."
/opt/mssql/bin/sqlservr &
sql_pid=$!

SQLCMD_BIN=${SQLCMD_BIN:-}
if [[ -x "/opt/mssql-tools18/bin/sqlcmd" ]]; then
  SQLCMD_BIN="/opt/mssql-tools18/bin/sqlcmd"
elif [[ -x "/opt/mssql-tools/bin/sqlcmd" ]]; then
  SQLCMD_BIN="/opt/mssql-tools/bin/sqlcmd"
else
  echo "sqlcmd was not found in /opt/mssql-tools18/bin or /opt/mssql-tools/bin" >&2
  exit 1
fi

sqlcmd_exec() {
  "$SQLCMD_BIN" -S localhost -U SA -P "$SA_PASSWORD" -C -b -V16 "$@"
}

ensure_schema_patches() {
  echo "Applying supplemental schema patches..."
  sqlcmd_exec -d "$DB_NAME" -i /scripts/post-restore.sql
}

echo "Waiting for SQL Server to become available..."
for attempt in {1..60}; do
  if sqlcmd_exec -Q "SELECT 1" >/dev/null 2>&1; then
    break
  fi
  sleep 2
done

if ! sqlcmd_exec -Q "SELECT 1" >/dev/null 2>&1; then
  echo "SQL Server did not start within the allocated time." >&2
  exit 1
fi

: "${DB_NAME:=WarehouseManager}"
: "${DB_BACKUP_FILE:=/var/opt/mssql/backup/WarehouseManager.bak}"
: "${DB_LOGICAL_DATA_NAME:=WarehouseManager}"
: "${DB_LOGICAL_LOG_NAME:=WarehouseManager_log}"
: "${DB_DATA_DIR:=/var/opt/mssql/data}"

if [[ ! -f "$DB_BACKUP_FILE" ]]; then
  echo "Backup file not found at $DB_BACKUP_FILE" >&2
  exit 1
fi

db_exists=$(sqlcmd_exec -h -1 -W -Q "SET NOCOUNT ON; SELECT DB_ID(N'${DB_NAME}')" | tr -d '[:space:]')
echo "Database '${DB_NAME}' check result: '${db_exists}'"

if [[ -z "$db_exists" || "$db_exists" == "NULL" ]]; then
  echo "Restoring database '$DB_NAME' from backup..."
  sqlcmd_exec -Q "
    RESTORE DATABASE [${DB_NAME}]
    FROM DISK = N'${DB_BACKUP_FILE}'
    WITH MOVE N'${DB_LOGICAL_DATA_NAME}' TO N'${DB_DATA_DIR}/${DB_NAME}.mdf',
         MOVE N'${DB_LOGICAL_LOG_NAME}' TO N'${DB_DATA_DIR}/${DB_NAME}_log.ldf',
         RECOVERY, REPLACE, STATS = 5;
  "
  echo "Database '$DB_NAME' has been restored."
else
  echo "Database '$DB_NAME' already exists. Skipping restore."
fi

ensure_schema_patches

wait "$sql_pid"

