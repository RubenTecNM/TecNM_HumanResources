#!/bin/bash
# Start SQL Server in background
/opt/mssql/bin/sqlservr &

# Intelligent wait (health check loop)
echo "Waiting for SQL Server to start..."
SUCCESS=0
for i in {1..50};
do
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -Q "SELECT 1" > /dev/null 2>&1
    if [ $? -eq 0 ]
    then
        echo "SQL Server is ready after $i attempts."
        SUCCESS=1
        break
    else
        echo "Retrying connection..."
        sleep 2s
    fi
done

if [ $SUCCESS -eq 0 ]
then
    echo "ERROR: SQL Server failed to start in time."
    exit 1
fi

# Execute initialization scripts
echo "Executing schemaDB.sql..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -d master -i /usr/config/schemaDB.sql

echo "Executing InsertsDB.sql..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -d RecursosHumanos -i /usr/config/InsertsDB.sql

echo "Database successfully configured with schema and inserts."
# Keep the container alive
wait
