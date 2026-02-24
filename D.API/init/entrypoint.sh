#!/bin/bash
set -e

/opt/mssql/bin/sqlservr &

echo "Waiting for SQL Server port 1433..."
until (echo > /dev/tcp/localhost/1433) >/dev/null 2>&1; do
  sleep 2
done

echo "Port is open, waiting extra 5s..."
sleep 5

echo "Running init.sql..."
/opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "$MSSQL_SA_PASSWORD" \
  -C \
  -i /init/init.sql

wait
