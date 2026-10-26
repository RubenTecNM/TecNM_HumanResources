# itsmRH — Gestión de Permisos Laborales 🏛️

Sistema de gestión de recursos humanos del **TecNM Campus Monclova** para el control de permisos laborales, usuarios y catálogos organizacionales. Arquitectura **Screaming Architecture** (Clean Code) con backend ASP.NET Core 10 y frontend Vue 3.

---

## 📐 Arquitectura

```
itsmRH_GestionPermisos/
├── Backend/                         # Backend — ASP.NET Core 10 (Screaming Architecture)
│   ├── HumanResourcesProduction.sln # Solución .NET
│   ├── HumanResourcesProduction/    # API Web / Endpoints por Features
│   │   ├── Features/
│   │   │   ├── Areas/               # CRUD áreas (soft-delete + reactivar)
│   │   │   ├── Departments/         # CRUD departamentos
│   │   │   ├── Positions/           # CRUD puestos (paginado)
│   │   │   ├── Permissions/         # Gestión de permisos laborales
│   │   │   ├── Users/               # Gestión de colaboradores
│   │   │   ├── Auth/                # Autenticación JWT
│   │   │   ├── Cutoffs/             # Periodos de corte/nómina
│   │   │   ├── Sync/                # Sincronización de datos
│   │   │   ├── ApiKeys/             # Administración de API Keys
│   │   │   ├── Admin/               # Panel administrativo
│   │   │   └── Shared/              # Utilidades compartidas
│   │   └── Program.cs
│   ├── HumanResourcesProduction.Core/        # Entidades, interfaces y DTOs
│   ├── HumanResourcesProduction.Data/        # DbContext y persistencia
│   ├── HumanResourcesProduction.Services/    # Servicios de negocio (ExcelImport, etc.)
│   ├── HumanResourcesProduction.Crosscutting/# Logging y seguridad
│   └── HumanResourcesProduction.Tests/       # Pruebas unitarias backend (xUnit)
├── Frontend/                        # Frontend — Vue 3 + Vite + TypeScript
│   └── src/
│       ├── features/
│       │   ├── areas/               # Vista CRUD Áreas
│       │   ├── departments/         # Vista CRUD Departamentos
│       │   ├── organization/        # Vista Estructura Organizacional
│       │   ├── positions/           # Vista CRUD Puestos
│       │   ├── permissions/         # Vista gestión de permisos
│       │   ├── users/               # Vista gestión de usuarios
│       │   ├── auth/                # Login / sesión
│       │   ├── cutoffs/             # Periodos de nómina
│       │   └── sync/                # Sincronización
│       ├── layouts/                 # AppLayout (nav lateral)
│       ├── components/              # Componentes globales (AppModal, etc.)
│       └── assets/css/              # Estilos TecNM estandarizados
├── database/                        # Scripts SQL y configuración de base de datos
│   ├── schema_objects.sql           # Esquema de SPs, triggers y table types
│   ├── inserts.sql                  # Catálogos iniciales (áreas, departamentos, puestos)
│   └── setup.sh                     # Instalador y configurador interactivo
├── docker/                          # Configuración y despliegue de contenedores
│   ├── Dockerfile                   # Build multi-stage (backend + frontend + tests)
│   └── docker-compose.yml           # Orquestación de servicios
└── docs/                            # Documentación técnica
```

### Jerarquía organizacional

```
Área (1:N) ──► Departamento (1:N) ──► Usuario
                                         │
                                    Puesto (catálogo transversal)
```

- **Área** — Unidad macro (4 Direcciones del TecNM)
- **Departamento** — Unidad operativa (24 departamentos)
- **Puesto** — Catálogo tabular transversal (24 puestos, asignado directamente al usuario)

---

## 🗂 Módulos del sistema

| Módulo | Ruta frontend | Descripción |
|--------|--------------|-------------|
| Permisos | `/permissions` | Solicitud y aprobación de permisos laborales |
| Usuarios | `/users` | Alta, edición y baja lógica de colaboradores |
| Estructura Org. | `/organization` | Vista drill-down Áreas → Departamentos |
| Puestos | `/positions` | CRUD paginado de puestos tabulares |
| Áreas | `/areas` | CRUD de áreas con soft-delete |
| Periodos | `/cutoffs` | Gestión de periodos de nómina |
| API Keys | `/apikeys` | Administración de llaves de acceso |

> **Regla de negocio:** No existe eliminación física en ningún módulo. Todo usa **soft-delete** (`Status = false`) con opción de **reactivar** (`Status = true`).

---

## ⚡ Instalación rápida

El script `database/setup.sh` guía paso a paso con confirmaciones interactivas `[s/N]` en cada etapa:

```bash
git clone https://github.com/rriojas/itsmRH_GestionPermisos.git
cd itsmRH_GestionPermisos/database

# Modo interactivo (valida [s/N] antes de cada acción):
sudo bash setup.sh

# Modo desatendido (ejecuta todo sin preguntar):
sudo bash setup.sh -y
```

El script ofrece validación individual para:
1. **Dependencias del sistema** (`curl`, `wget`, `ca-certificates`, `libicu`, `libssl`)
2. **.NET 10 SDK** (en `/usr/share/dotnet` con variables globales en `profile.d`)
3. **Docker Engine** (repositorio oficial Debian/Ubuntu)
4. **SQL Server 2025 Express** en contenedor Docker (`nexsmart_central_db_sql`)
5. **Inicialización de base de datos** (`schema_objects.sql` e `inserts.sql`)
6. **Compilación y despliegue del contenedor de la aplicación** (`itsmrh_gestion_permisos_api`)

---

## 🛠 Configuración manual (Debian Server)

### 1. Instalar .NET 10

```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
sudo mkdir -p /usr/share/dotnet
sudo ./dotnet-install.sh --channel 10.0 --install-dir /usr/share/dotnet

sudo bash -c 'cat <<\EOF > /etc/profile.d/dotnet.sh
export DOTNET_ROOT=/usr/share/dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
EOF'

source /etc/profile.d/dotnet.sh
sudo apt install -y libicu-dev libssl-dev
dotnet --version
```

### 2. Instalar Docker

```bash
sudo apt update && sudo apt install -y ca-certificates curl gnupg
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/debian/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] \
  https://download.docker.com/linux/debian $(. /etc/os-release && echo "$VERSION_CODENAME") stable" \
  | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt update && sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin
sudo usermod -aG docker $USER
```

### 3. Desplegar SQL Server 2025 Express

```bash
docker run \
  -e 'ACCEPT_EULA=Y' \
  -e 'MSSQL_SA_PASSWORD=<tu-password>' \
  -e 'MSSQL_PID=Express' \
  -p 1433:1433 \
  --name nexsmart_central_db_sql \
  -d mcr.microsoft.com/mssql/server:2025-latest

# Verificar que está listo
docker logs nexsmart_central_db_sql | grep "ready for client"
```

### 4. Inicializar la base de datos

Ejecuta los scripts en orden desde tu cliente SQL preferido (VS Code + SQL Server Extension, DBeaver, sqlcmd):

```
database/schema_objects.sql   →  Crea tablas, relaciones e índices
database/inserts.sql          →  Carga catálogos base (Áreas, Departamentos, Puestos, Tipos de permiso)
```

Parámetros de conexión:
- **Servidor:** IP del servidor `:1433`
- **Usuario:** `sa`
- **Trust Server Certificate:** ✅

### 5. Build y despliegue del contenedor de la app

```bash
# Build (incluye tests de frontend con Vitest)
sudo docker build \
  -t itsmrh_gestion_permisos_api:refactor-screaming \
  -f docker/Dockerfile . \
  --build-arg RUN_FRONTEND_TESTS=true

# Ejecutar
sudo docker run -d \
  --name itsmrh_gestion_permisos_api \
  --restart unless-stopped \
  -p 5013:8080 \
  itsmrh_gestion_permisos_api:refactor-screaming

# Verificar
curl -I http://localhost:5013
```

---

## 🧪 Tests

El build Docker ejecuta automáticamente los tests de frontend (Vitest) durante la imagen:

```bash
# Ejecutar tests localmente
cd Frontend
npm install
npm run test
```

57 tests unitarios cubren: vistas de permisos, usuarios, áreas, departamentos y lógica de composables.

---

## 📡 API REST

Base URL: `http://<servidor>:5013/api`

| Endpoint | Método | Descripción |
|----------|--------|-------------|
| `/api/Areas` | GET / POST | Listar / crear áreas |
| `/api/Areas/{id}` | PUT | Editar área |
| `/api/Areas/{id}/deactivate` | PUT | Soft-delete |
| `/api/Areas/{id}/activate` | PUT | Reactivar |
| `/api/Departments` | GET / POST | Listar / crear departamentos |
| `/api/Departments/{id}` | PUT | Editar departamento |
| `/api/Departments/{id}/deactivate` | PUT | Soft-delete |
| `/api/Departments/{id}/activate` | PUT | Reactivar |
| `/api/Positions?page=&pageSize=&search=&status=` | GET | Puestos paginados |
| `/api/Positions` | POST | Crear puesto |
| `/api/Positions/{id}` | PUT | Editar puesto |
| `/api/Positions/{id}/deactivate` | PUT | Soft-delete |
| `/api/Positions/{id}/activate` | PUT | Reactivar |
| `/api/Permissions` | GET / POST | Permisos laborales |
| `/api/Users` | GET / POST | Usuarios / colaboradores |
| `/api/Auth/login` | POST | Autenticación JWT |

Autenticación requerida: **Bearer JWT** en header `Authorization`.

---

## 🎨 Estándares de UI (TecNM)

Toda la interfaz sigue los estándares del TecNM Campus Monclova:

- **Modales:** `<AppModal header-variant="icon" icon="fa-[icono]">`
- **Campos:** clases `.field` > `.lbl` + `.in` + `.help`
- **Filtros:** footer interno con `.filter-card-footer` (Limpiar izq. / Buscar der.)
- **Acciones:** `.tecnm-action-btn`, `.tecnm-action-btn--edit`, `.tecnm-action-btn--delete`, `.tecnm-action-btn--reactivate`
- **Confirmaciones:** `askConfirmation()` de `useAlerts`
- **Notificaciones:** `showToast()` de `useAlerts`

---

## 📄 Licencia

Uso interno. © 2026 TecNM Campus Monclova.
