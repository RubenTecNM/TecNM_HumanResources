# HR Management System - TecRHPermisos 🚀

Professional human resources management system designed to optimize labor permits, user management, and payroll period control. Built with .NET 10.0 and fully containerized for deployment.

> [!IMPORTANT]
> This project supports **two ways to run it**: with Docker (recommended, zero setup) or locally with the .NET SDK + SQL Server.

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

## 🐳 Option A — Docker (Zero Setup, Recommended)

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

## 💻 Option B — Local Development (No Docker)

Use this option if you prefer to run the app directly with the .NET SDK and a local SQL Server instance.

### Prerequisites

| Tool | Version | Download |
|---|---|---|
| .NET SDK | 10.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| SQL Server | 2019 or 2022 | [microsoft.com/sql-server](https://www.microsoft.com/sql-server/sql-server-downloads) |
| SQL Server Management Studio (optional) | latest | [SSMS](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms) |
| Git | any | [git-scm.com](https://git-scm.com) |

### Step 1 — Clone the Repository

```bash
git clone https://github.com/rriojas/itsmRH_GestionPermisos.git
cd itsmRH_GestionPermisos
```

### Step 2 — Set Up the Database

1. Open **SQL Server Management Studio** or use `sqlcmd` from your terminal.
2. Connect to your local SQL Server instance (`localhost` or `.\SQLEXPRESS`).
3. Run the schema script to create the database and all tables:
   ```bash
   sqlcmd -S localhost -U sa -P <your_password> -i db/schemaDB.sql
   ```
4. Run the seed data script to populate catalogues, users, and the initial period:
   ```bash
   sqlcmd -S localhost -U sa -P <your_password> -d RecursosHumanos -i db/InsertsDB.sql
   ```

> [!NOTE]
> If you use Windows Authentication instead of a SQL login, replace `-U sa -P <password>` with `-E` in the commands above.

### Step 3 — Configure the Connection String (User Secrets)

Navigate to the web project folder and set up your secrets so the connection string is never committed to source control:

```bash
cd RecursosHumanosWeb

dotnet user-secrets set "ConnectionStrings:RecursosHumanosContext" \
  "Server=localhost;Database=RecursosHumanos;User Id=sa;Password=<your_password>;TrustServerCertificate=True;"
```

Optionally configure SMTP for email notifications:
```bash
dotnet user-secrets set "SmtpSettings:SmtpServer" "smtp.office365.com"
dotnet user-secrets set "SmtpSettings:PasswordApp" "<your_smtp_app_password>"
```

### Step 4 — Restore Dependencies

```bash
# From the root of the repo
dotnet restore
```

### Step 5 — Run the Application

```bash
cd RecursosHumanosWeb
dotnet run
```

The application will be available at `http://localhost:5000` (or the URL shown in the terminal output).

> [!TIP]
> For hot-reload during development, use `dotnet watch run` instead of `dotnet run`.

### Step 6 — Log In

Use one of the default accounts created by the seed script:

| Email | Role | Password |
|---|---|---|
| `controlpermisos@monclova.tecnm.mx` | Administrator | `Admin1234!` |
| `alfredo.salinas@monclova.tecnm.mx` | Authorizer (Area Head) | `Admin1234!` |
| `izamar.mh@monclova.tecnm.mx` | Human Resources | `Admin1234!` |
| `empleado@monclova.tecnm.mx` | Employee (sample) | `Admin1234!` |

---

## 🔐 Security & Configuration

### Production Notes
- **SMTP**: Email service for password reset uses Office 365 (`smtp.office365.com`).
- **Evidence Storage**: Ensure `wwwroot/Evidences/` has write permissions for PDF uploads.
- **Troubleshooting**: If the web service fails to connect to the DB on startup, it's because SQL Server needs ~20–30 seconds. The `entrypoint.sh` handles this automatically with a health-check loop.

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
