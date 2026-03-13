# Gu&iacute;a de Despliegue con Docker 🐳

Esta gu&iacute;a detalla c&oacute;mo levantar el entorno completo de **TecRHPermisos** (Web + SQL Server) utilizando Docker.

## Requisitos Previos
- Docker Desktop (Windows/Mac) o Docker Engine (Linux)
- Docker Compose v2+

## Estructura del Entorno
El despliegue se basa en dos contenedores principales:
1.  **db**: Instancia de SQL Server 2022 con el esquema y datos iniciales cargados.
2.  **web**: Aplicaci&oacute;n ASP.NET Core 10.0.

---

## 1. Configuraci&oacute;n de Secretos 🔐

Para mantener la seguridad, no incluimos contrase&ntilde;as en el archivo `docker-compose.yml`.

### Usando .env
Crea un archivo llamado `.env` en la ra&iacute;z del proyecto:
```env
MSSQL_SA_PASSWORD=buss1ness!2026
SMTP_SERVER=smtp.office365.com
SMTP_PASSWORD=gsgvdlywyzzjxzsn
```

### Usando User Secrets (Desarrollo)
Si prefieres usar el gestor de secretos de .NET localmente:
```bash
dotnet user-secrets set "ConnectionStrings:RecursosHumanosContext" "Server=db;Database=RecursosHumanos;User Id=sa;Password=buss1ness!2026;TrustServerCertificate=True;"
dotnet user-secrets set "SmtpSettings:SmtpServer" "smtp.office365.com"
dotnet user-secrets set "SmtpSettings:PasswordApp" "gsgvdlywyzzjxzsn"
```

---

## 2. Comandos para Iniciar 🚀

Desde la terminal en la ra&iacute;z del proyecto, ejecuta:

### Levantar todo el sistema
```bash
docker-compose up -d
```
> El flag `-d` (detached) corre los contenedores en segundo plano.

### Ver logs en tiempo real
```bash
docker-compose logs -f
```

### Detener el sistema
```bash
docker-compose down
```

---

## 3. Preparaci&oacute;n de la Base de Datos 🛢️

Al iniciar por primera vez, el contenedor `db` ejecutar&aacute; autom&aacute;ticamente los scripts `schemaDB.sql` e `InsertsDB.sql` ubicados en la carpeta `./db` para inicializar:
- Tablas, Relaciones y Esquema completo.
- Usuarios iniciales, Cat&aacute;logos y datos operativos.

---

## 4. Troubleshooting 🛠️

- **Error de Conexi&oacute;n**: Aseg&uacute;rate de que la aplicaci&oacute;n web espere a que la base de datos est&eacute; lista. Docker Compose usa `depends_on`, pero SQL Server puede tardar unos segundos extra en arrancar.
- **Permisos de Evidencias**: Verifica que el volumen de la carpeta `wwwroot/Evidences` tenga permisos de escritura para que el contenedor pueda guardar archivos.

---

## Comandos &Uacute;tiles de Mantenimiento
- **Reconstruir la imagen web**: `docker-compose build web`
- **Limpiar vol&uacute;menes (borra la DB)**: `docker-compose down -v`
