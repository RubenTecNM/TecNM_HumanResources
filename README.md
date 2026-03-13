# HR Management System - TecRHPermisos 🚀

Professional human resources management system designed to optimize labor permits, user management, and payroll period control. Built with a modern tech stack and focusing on performance and security.

> [!IMPORTANT]
> **Docker Ready Injection**: This project is now fully containerized. You can deploy the complete environment (Web + SQL Server) using a single command.

---

## 🛠 Tech Stack

- **Framework**: [.NET 10.0](https://dotnet.microsoft.com/) (ASP.NET Core MVC)
- **Database**: [Microsoft SQL Server 2022](https://www.microsoft.com/sql-server)
- **ORM**: Entity Framework Core
- **Frontend**: Bootstrap 5, Vanilla CSS (Premium Design), SweetAlert2, Select2
- **Reporting**: ClosedXML (Excel Generation)

---

## 🐳 Docker Deployment (Pre-configured)

The easiest way to get the system running with its complete database is using Docker Compose.

### Quick Start

1. **Clone the repository**:
   ```bash
   git clone https://github.com/rriojas/itsmRH_GestionPermisos.git
   cd itsmRH_GestionPermisos
   ```

2. **Configure Environment Variables**:
   Create a `.env` file in the root directory:
   ```env
   MSSQL_SA_PASSWORD=YourStrongPassword!2026
   ```

3. **Launch the services**:
   ```bash
   docker-compose up -d --build
   ```

### Services Included
- **web**: ASP.NET Core application accessible at `http://localhost:8080`
- **db**: SQL Server instance. The database is automatically initialized with the schema and seed data (Areas, Users, and Roles) using the `db/setup.sql` script.

---

## 🔐 Security & Configuration

### User Secrets
For local development, you can manage sensitive data using .NET User Secrets:
```bash
dotnet user-secrets set "ConnectionStrings:RecursosHumanosContext" "Server=db;Database=RecursosHumanos;User Id=sa;Password=YourStrongPassword!2026;TrustServerCertificate=True;"
dotnet user-secrets set "SmtpSettings:SmtpServer" "smtp.office365.com"
dotnet user-secrets set "SmtpSettings:PasswordApp" "gsgvdlywyzzjxzsn"
```

### Production Deployment
- **SMTP**: Configure your Office 365 / SMTP settings in `appsettings.json` or environment variables for email notifications.
- **Evidence Storage**: Ensure the `wwwroot/Evidences` folder has write permissions for PDF uploads.

---

## 📋 Key Modules

- **Permit Management**: Request, review, and authorize labor permits with evidence attachment.
- **Organizational Structure**: Multi-level hierarchy (Areas and Departments).
- **Audit System**: Automatic tracking of changes and security logs.
- **Reporting**: Export detailed summaries to Excel for payroll processing.

---

## 📄 License
This project is for internal institutional use. 

© 2026 TecNM Monclova.
