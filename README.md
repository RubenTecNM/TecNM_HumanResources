# HR Management System - TecRHPermisos 🚀

Sistema profesional de gestión de recursos humanos diseñado para optimizar permisos laborales, gestión de usuarios y control de periodos de nómina.

## 🛠 Configuración del Entorno (Debian Server)

Siga estos pasos para configurar el servidor y ejecutar el proyecto.

### 1. Instalación de .NET SDK

Ejecute los siguientes comandos para instalar el SDK de .NET.

```bash
# Descargar el script oficial de instalación de Microsoft
# Se usa porque el repositorio APT de Microsoft tiene problemas de firma SHA1 en Debian 13
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh

# Dar permisos de ejecución al script para poder iniciarlo
chmod +x dotnet-install.sh

# Instalar .NET 9 LTS (versión estable actual) para compatibilidad
./dotnet-install.sh --channel 9.0

# Instalar .NET 10 preview (versión más reciente) para usar las últimas características
./dotnet-install.sh --channel 10.0

# Agregar .NET al PATH para que el comando 'dotnet' sea reconocido globalmente
# DOTNET_ROOT indica la ubicación de la instalación
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc

# Agrega el binario de dotnet y sus herramientas al PATH del sistema
echo 'export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools' >> ~/.bashrc

# Recargar configuración del shell para aplicar los cambios en la sesión actual
source ~/.bashrc

# Instalar librerías ICU requeridas por .NET para el manejo de globalización
# Sin esto, el runtime de .NET falla al ejecutarse en sistemas Debian
sudo apt install -y libicu-dev

# Verificar la versión instalada para confirmar el éxito
dotnet --version

# Ver todos los SDKs instalados en el sistema
dotnet --list-sdks 
```

### 2. Configuración de SQL Server 2025 con Docker

Pasos para instalar Docker y desplegar SQL Server 2025 Express.

#### Instalar Docker
```bash
# Actualizar el índice de paquetes e instalar dependencias necesarias
sudo apt update
sudo apt install -y ca-certificates curl gnupg

# Configurar el llavero de seguridad para el repositorio de Docker
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/debian/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

# Agregar el repositorio oficial de Docker a las fuentes de APT
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian $(. /etc/os-release && echo \"$VERSION_CODENAME\") stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Instalar el motor de Docker y complementos de composición
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# Agregar el usuario actual al grupo docker para ejecutar comandos sin sudo
sudo usermod -aG docker $USER
```

#### Crear Contenedor SQL Server 2025 Express
```bash
# Iniciar el contenedor con la imagen oficial de Microsoft SQL Server 2025
# Se aceptan términos, se define contraseña de SA y se mapea el puerto 1433
docker run -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=buss1ness!2026' -e 'MSSQL_PID=Express' -p 1433:1433 --name sql2025 -d mcr.microsoft.com/mssql/server:2025-latest
```

#### Verificar Estado
```bash
# Listar contenedores activos para verificar que 'sql2025' está corriendo
docker ps

# Revisar los logs para confirmar que SQL Server está listo para conexiones
docker logs sql2025
# Debe aparecer: "SQL Server is now ready for client connections."
```

### 3. Conexión a la Base de Datos

#### Desde la Terminal (sqlcmd)
```bash
# Conectarse directamente al servidor dentro del contenedor
docker exec -it sql2025 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'buss1ness!2026' -C
```

#### Desde VS Code (Extensión SQL Server)
Use los siguientes parámetros para configurar su conexión:
- **Servidor:** `localhost` (o la IP del servidor, ej. `192.168.1.70`)
- **Usuario:** `sa`
- **Contraseña:** `buss1ness!2026`
- **Opción crítica:** Marcar **Trust Server Certificate**

### 4. Inicialización de la Base de Datos
Los scripts SQL se encuentran en la carpeta `db/`. Puede ejecutarlos desde la conexion al motor sql preferiblemente.

1. `db/schemaDB.sql` — Estructura de tablas y relaciones.
2. `db/InsertsDB.sql` — Datos iniciales de catálogos y usuarios.

---
## 📄 Licencia
Uso interno. © 2026 TecNM Campus Monclova.
