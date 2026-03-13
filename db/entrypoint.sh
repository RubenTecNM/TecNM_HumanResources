#!/bin/bash
# Iniciar SQL Server en segundo plano
/opt/mssql/bin/sqlservr &

# Esperar a que SQL Server esté listo de forma inteligente
echo "Esperando a que SQL Server arranque..."
SUCCESS=0
for i in {1..50};
do
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -Q "SELECT 1" > /dev/null 2>&1
    if [ $? -eq 0 ]
    then
        echo "SQL Server est&aacute; listo despues de $i intentos."
        SUCCESS=1
        break
    else
        echo "Re-intentando conexi&oacute;n..."
        sleep 2s
    fi
done

if [ $SUCCESS -eq 0 ]
then
    echo "ERROR: SQL Server no arranco a tiempo."
    exit 1
fi

# Ejecutar los scripts de inicializaci&oacute;n
echo "Ejecutando schemaDB.sql..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -d master -i /usr/config/schemaDB.sql

echo "Ejecutando InsertsDB.sql..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -d RecursosHumanos -i /usr/config/InsertsDB.sql

echo "Base de datos configurada exitosamente con esquema e inserts."
# Mantener el contenedor vivo
wait
