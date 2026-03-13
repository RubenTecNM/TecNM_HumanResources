#!/bin/bash
# Iniciar SQL Server en segundo plano
/opt/mssql/bin/sqlservr &

# Esperar a que SQL Server est&eacute; listo
echo "Esperando a que SQL Server arranque..."
sleep 30s

# Ejecutar el script de inicializaci&oacute;n
echo "Ejecutando setup.sql..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -d master -i /usr/config/setup.sql

echo "Base de datos configurada."
# Mantener el contenedor vivo
wait
