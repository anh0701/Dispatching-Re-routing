#!/bin/bash
set -e

CONTAINER="d-mssql"
SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
SA_PASSWORD='YourStrong@Passw0rd'

run_sql() {
  local file_path="$1"
  local file_name
  file_name=$(basename "$file_path")

  echo "📦 Copy $file_name vào container..."
  podman cp "$file_path" "$CONTAINER:/tmp/$file_name"

  echo "🚀 Chạy $file_name ..."
  podman exec -i "$CONTAINER" \
    "$SQLCMD" \
    -S localhost \
    -U sa \
    -P "$SA_PASSWORD" \
    -C \
    -i "/tmp/$file_name"

  echo "✅ Done $file_name"
  echo
}

echo "=============================="
echo "   SQL Runner Menu"
echo "=============================="
echo "1) Run INIT data (insert.sql)"
echo "2) Run CHECK (check.sql)"
echo "3) Run DELETE ALL data (delete_all.sql)"
echo "0) Exit"
echo
read -rp "👉 Chọn option: " choice

case "$choice" in
  1)
    run_sql "./D.API/init/insert.sql"
    ;;
  2)
    run_sql "./D.API/init/check.sql"
    ;;
  3)
    run_sql "./D.API/init/delete_all.sql"
    ;;
  0)
    echo "Bye 👋"
    exit 0
    ;;
  *)
    echo "❌ Option không hợp lệ"
    exit 1
    ;;
esac
