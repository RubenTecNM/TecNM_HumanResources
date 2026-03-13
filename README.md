# HR Management System - TecRHPermisos 🚀

Professional human resources management system designed to optimize labor permits, user management, and payroll period control. Built with .NET 10.0 and fully containerized for deployment.

> [!IMPORTANT]
> This project is Docker-ready. Run the full environment (Web + SQL Server 2022) with a single command.

---

## 🛠 Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 10.0 (MVC) |
| Database | Microsoft SQL Server 2022 |
| ORM | Entity Framework Core |
| Frontend | Bootstrap 5, SweetAlert2, Select2 |
| Reporting | ClosedXML (Excel) |

---

## 🐳 Docker Deployment

All Docker configuration files are located in the `db/` folder.

### Project Structure (Docker Files)

```
├── db/
│   ├── docker-compose.yml      # Orchestrates both services (run from here)
│   ├── Dockerfile              # SQL Server 2022 image
│   ├── entrypoint.sh           # DB health check + SQL initialization
│   ├── schemaDB.sql            # Full database schema
│   ├── InsertsDB.sql           # Seed data (users, catalogues, period)
│   └── .env.example            # Environment variable template
│
└── RecursosHumanosWeb/
    └── Dockerfile              # ASP.NET Core 10.0 web application image
```

> [!NOTE]
> There are **two Dockerfiles** — one for the database and one for the web app. Both are referenced by `db/docker-compose.yml`.

### Prerequisites
- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose v2+

### Quick Start

1. **Clone the repository**:
   ```bash
   git clone https://github.com/rriojas/itsmRH_GestionPermisos.git
   cd itsmRH_GestionPermisos
   ```

2. **Create a `.env` file** inside the `db/` folder:
   ```env
   MSSQL_SA_PASSWORD=buss1ness!2026
   ```

3. **Launch all services** from the `db/` folder:
   ```bash
   cd db
   docker-compose up -d --build
   ```

4. **Access the application** at `http://localhost:8080`

### Services

| Service | Description | Port |
|---|---|---|
| `web` | ASP.NET Core application | `8080` |
| `db` | SQL Server 2022 | `1433` |

### Database Initialization

On the **first run**, the `db` container automatically executes:
1. `db/schemaDB.sql` — Creates all tables and relationships.
2. `db/InsertsDB.sql` — Populates catalogues, users, and operational data.

### Manual Database Initialization (Alternative)

If you need to run the SQL scripts manually after starting the service:
```bash
# Get the db container ID
docker ps

# Execute the schema
docker exec -it <container_id> /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P buss1ness!2026 -i /usr/config/schemaDB.sql

# Execute the inserts
docker exec -it <container_id> /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P buss1ness!2026 -d RecursosHumanos -i /usr/config/InsertsDB.sql
```

### Useful Commands

```bash
# View live logs
docker-compose logs -f

# Stop all services
docker-compose down

# Stop and delete database volume (full reset)
docker-compose down -v

# Rebuild only the web image
docker-compose build web
```

---

## 🔐 Security & Configuration

### Local Development (User Secrets)
```bash
dotnet user-secrets set "ConnectionStrings:RecursosHumanosContext" "Server=localhost;Database=RecursosHumanos;User Id=sa;Password=buss1ness!2026;TrustServerCertificate=True;"
dotnet user-secrets set "SmtpSettings:SmtpServer" "smtp.office365.com"
dotnet user-secrets set "SmtpSettings:PasswordApp" "gsgvdlywyzzjxzsn"
```

### Production Notes
- **SMTP**: Email service for password reset uses Office 365 (`smtp.office365.com`).
- **Evidence Storage**: Ensure `wwwroot/Evidences/` has write permissions for PDF uploads.
- **Troubleshooting**: If the web service fails to connect to the DB on startup, it's because SQL Server needs ~20–30 seconds. The `entrypoint.sh` handles this automatically with a health-check loop.

### Default Credentials (Seed Data)

> [!NOTE]
> Default password for all users: **`Admin1234!`**

| Email | Role |
|---|---|
| `controlpermisos@monclova.tecnm.mx` | Administrator |
| `alfredo.salinas@monclova.tecnm.mx` | Authorizer (Area Head) |
| `izamar.mh@monclova.tecnm.mx` | Human Resources |
| `empleado@monclova.tecnm.mx` | Employee (sample) |

---

## 📋 Key Modules

- **Permit Management** — Request, review, and authorize labor permits with PDF evidence.
- **Organizational Hierarchy** — Multi-level structure: Areas → Departments → Users.
- **Audit Logging** — Full change tracking for compliance.
- **Period Management** — Payroll cut periods tied to permit requests.
- **Reporting** — Export summaries to Excel for HR processing.

---

## 📄 License
Internal use only. © 2026 TecNM Campus Monclova.
