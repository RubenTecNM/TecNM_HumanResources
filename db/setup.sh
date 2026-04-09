#!/usr/bin/env bash
# =============================================================================
#  setup.sh — RecursosHumanosWeb · Script de Instalación Automatizada
#  Autor   : TecNM Campus Monclova
#  Versión : 3.0.0
#  Fecha   : 2026-03-19
#
#  ARQUITECTURA:
#    Internet → Nginx (:80) → Kestrel nativo (:5000) → SQL Server en Docker (:1433)
#
#    • SQL Server 2025 Express  →  Contenedor Docker  (datos en volumen persistente)
#    • ASP.NET MVC + Web API    →  Nativo en servidor  (dotnet publish → /var/www/)
#    • Kestrel                  →  Servicio systemd   (recursoshumanosweb.service)
#    • Nginx                    →  Reverse proxy       (puerto 80 → 5000)
#
#  ESTRUCTURA ESPERADA DEL REPOSITORIO:
#    <raíz>/   (ej: itsmRH_GestionPermisos/)
#    ├── db/
#    │   ├── setup.sh          ← Este script
#    │   ├── schemaDB.sql
#    │   └── InsertsDB.sql
#    └── RecursosHumanosWeb/
#        ├── RecursosHumanosWeb.csproj
#        ├── appsettings.json
#        └── ...
#
#  USO:
#    sudo bash db/setup.sh [opciones]
#
#  OPCIONES:
#    --reset-db      Fuerza recreación de la BD sin preguntar
#    --reset-sql     Elimina y recrea el contenedor SQL Server (conserva volumen de datos)
#    --reset-all     Elimina contenedor SQL Server Y borra el volumen de datos  ⚠ DESTRUCTIVO
#    --redeploy      Solo republica y reinicia la app ASP.NET (no toca SQL ni BD)
#    --skip-db       Omite la inicialización de la BD
#    --help          Muestra esta ayuda
# =============================================================================

set -euo pipefail
IFS=$'\n\t'

# ──────────────────────────────────────────────────────────────────────────────
# FLAGS
# ──────────────────────────────────────────────────────────────────────────────
FLAG_RESET_DB=false
FLAG_RESET_SQL=false
FLAG_RESET_ALL=false
FLAG_REDEPLOY=false
FLAG_SKIP_DB=false

for arg in "$@"; do
    case "$arg" in
        --reset-db)   FLAG_RESET_DB=true ;;
        --reset-sql)  FLAG_RESET_SQL=true ;;
        --reset-all)  FLAG_RESET_ALL=true; FLAG_RESET_SQL=true; FLAG_RESET_DB=true ;;
        --redeploy)   FLAG_REDEPLOY=true ;;
        --skip-db)    FLAG_SKIP_DB=true ;;
        --help|-h)
            grep "^#  " "${BASH_SOURCE[0]}" | sed 's/^#  //'
            exit 0 ;;
        *)
            echo "Flag desconocido: '$arg'  —  usa --help para ver opciones."
            exit 1 ;;
    esac
done

# ──────────────────────────────────────────────────────────────────────────────
# CONFIGURACIÓN — Editar según el entorno
# ──────────────────────────────────────────────────────────────────────────────

# ── SQL Server (Docker) ───────────────────────────────────────────────────────
MSSQL_IMAGE="mcr.microsoft.com/mssql/server:2025-latest"
MSSQL_CONTAINER="rh_sql"
MSSQL_VOLUME="rh_sqldata"
MSSQL_PORT="1433"
MSSQL_SA_PASSWORD="buss1ness!2026"         # ⚠  Cambiar en producción
MSSQL_PID="Express"
MSSQL_DB_NAME="RecursosHumanos"

# ── App ASP.NET (nativa) ──────────────────────────────────────────────────────
APP_PORT="5000"                            # Puerto Kestrel; Nginx hace proxy aquí
APP_PUBLISH_DIR="/var/www/RecursosHumanosWeb"
APP_SERVICE="recursoshumanosweb"           # Nombre del servicio systemd
APP_USER="www-data"                        # Usuario del sistema que corre la app
DOTNET_CHANNEL="10.0"
DOTNET_INSTALL_DIR="/usr/share/dotnet"

# ── Nginx ─────────────────────────────────────────────────────────────────────
NGINX_SERVER_NAME="_"                      # "_" = cualquier host; reemplazar con dominio real

# ── SMTP ──────────────────────────────────────────────────────────────────────
SMTP_EMAIL_SENDER="controlpermisos@monclova.tecnm.mx"
SMTP_SERVER="smtp.office365.com"
SMTP_PORT="587"
SMTP_PASSWORD_APP="gsgvdlywyzzjxzsn"

# ── Rutas del repositorio ─────────────────────────────────────────────────────
DB_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$DB_DIR")"
ASPNET_DIR="$PROJECT_ROOT/RecursosHumanosWeb"
SCHEMA_SQL="$DB_DIR/schemaDB.sql"
INSERTS_SQL="$DB_DIR/InsertsDB.sql"
DACPAC_FILE="$DB_DIR/RHdatabase-schema.dacpac"   # ← nombre de tu archivo
SQLPACKAGE_AVAILABLE=false

# ── Tiempos de espera ─────────────────────────────────────────────────────────
SQL_READY_TIMEOUT=120

# ──────────────────────────────────────────────────────────────────────────────
# COLORES Y UTILIDADES DE LOG
# ──────────────────────────────────────────────────────────────────────────────
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'
CYAN='\033[0;36m'; BOLD='\033[1m'; RESET='\033[0m'

log_section() {
    echo ""
    echo -e "${CYAN}${BOLD}══════════════════════════════════════════════════════════${RESET}"
    echo -e "${CYAN}${BOLD}  $1${RESET}"
    echo -e "${CYAN}${BOLD}══════════════════════════════════════════════════════════${RESET}"
}
log_info()  { echo -e "  ${GREEN}✔${RESET}  $1"; }
log_warn()  { echo -e "  ${YELLOW}⚠${RESET}  $1"; }
log_error() { echo -e "  ${RED}✘${RESET}  $1" >&2; }
log_step()  { echo -e "  ${BOLD}→${RESET}  $1"; }
log_done()  { echo -e "\n${GREEN}${BOLD}  ✅  $1${RESET}\n"; }

die() {
    echo ""
    log_error "ERROR FATAL: $1"
    echo -e "  ${RED}${BOLD}Script detenido en línea ${BASH_LINENO[0]}. Revisa el mensaje anterior.${RESET}" >&2
    echo ""
    exit 1
}

# ──────────────────────────────────────────────────────────────────────────────
# DETECCIÓN DEL SISTEMA OPERATIVO
# ──────────────────────────────────────────────────────────────────────────────
detect_os() {
    [[ ! -f /etc/os-release ]] && \
        die "No se encontró /etc/os-release. Este script requiere Debian 11+ o Ubuntu 22.04+."

    # shellcheck source=/dev/null
    source /etc/os-release

    OS_ID="${ID:-}"
    OS_CODENAME="${VERSION_CODENAME:-}"
    OS_VERSION_ID="${VERSION_ID:-}"

    # Debian 13 puede no tener VERSION_CODENAME en instalaciones mínimas
    if [[ -z "$OS_CODENAME" ]]; then
        if [[ "$OS_ID" == "debian" && "${OS_VERSION_ID%%.*}" == "13" ]]; then
            OS_CODENAME="trixie"
            log_warn "VERSION_CODENAME vacío — inferido como 'trixie' para Debian 13."
        else
            die "No se pudo determinar el codename del OS.\nEjecuta: source /etc/os-release && echo \$VERSION_CODENAME\ny reporta el resultado."
        fi
    fi

    case "$OS_ID" in
        debian|ubuntu) ;;
        *) log_warn "OS '$OS_ID' no es Debian ni Ubuntu. El script puede no funcionar correctamente." ;;
    esac

    # Arquitectura normalizada para APT/Docker
    case "$(uname -m)" in
        x86_64)  DPKG_ARCH="amd64" ;;
        aarch64) DPKG_ARCH="arm64" ;;
        armv7l)  DPKG_ARCH="armhf" ;;
        *) die "Arquitectura '$(uname -m)' no soportada." ;;
    esac

    log_info "Sistema  : ${PRETTY_NAME:-$OS_ID $OS_VERSION_ID}"
    log_info "Codename : $OS_CODENAME"
    log_info "Arq.     : $(uname -m) → $DPKG_ARCH"
}

# ──────────────────────────────────────────────────────────────────────────────
# VERIFICACIONES PREVIAS
# ──────────────────────────────────────────────────────────────────────────────
check_root() {
    if [[ "$EUID" -ne 0 ]]; then
        die "Ejecuta el script como root:\n  sudo bash db/setup.sh"
    fi
    log_info "Ejecutando como root — OK"
}

check_ram() {
    local ram_kb
    ram_kb=$(grep MemTotal /proc/meminfo | awk '{print $2}')
    local ram_gb=$(( ram_kb / 1024 / 1024 ))
    if [[ $ram_kb -lt 1900000 ]]; then
        die "RAM insuficiente: ${ram_gb}GB disponibles. SQL Server Express requiere mínimo 2GB.\nEste servidor no puede correr SQL Server."
    elif [[ $ram_kb -lt 3000000 ]]; then
        log_warn "RAM disponible: ${ram_gb}GB. SQL Server Express puede ser lento con menos de 3GB."
    else
        log_info "RAM disponible: ${ram_gb}GB — OK"
    fi
}

check_internet() {
    log_step "Verificando conectividad con servicios clave..."
    local -a urls=(
        "https://download.docker.com"
        "https://mcr.microsoft.com"
        "https://dot.net"
    )
    local failed=0
    for url in "${urls[@]}"; do
        if curl -sf --max-time 7 "$url" > /dev/null 2>&1; then
            log_info "Alcanzable → $url"
        else
            log_warn "Sin respuesta → $url"
            failed=$((failed + 1))
        fi
    done

    if [[ $failed -eq ${#urls[@]} ]]; then
        die "Sin acceso a internet. Verifica conexión y que el puerto 443 saliente no esté bloqueado."
    elif [[ $failed -gt 0 ]]; then
        log_warn "$failed endpoint(s) no respondieron — pueden ocurrir errores durante la descarga."
    fi
}

check_sql_files() {
    log_step "Verificando archivos SQL..."
    local missing=0
    for f in "$SCHEMA_SQL" "$INSERTS_SQL"; do
        if [[ ! -f "$f" ]]; then
            log_error "Faltante: $f"
            missing=$((missing + 1))
        fi
    done
    if [[ $missing -gt 0 ]]; then
        die "$missing archivo(s) SQL no encontrado(s).\nEjecuta desde la raíz del repo: sudo bash db/setup.sh"
    fi
    log_info "schemaDB.sql ✔   InsertsDB.sql ✔"
}

check_aspnet_project() {
    log_step "Verificando proyecto ASP.NET en: $ASPNET_DIR"
    if [[ ! -d "$ASPNET_DIR" ]]; then
        die "Directorio del proyecto no encontrado: $ASPNET_DIR\nEstructura esperada:\n  $PROJECT_ROOT/\n  ├── db/\n  └── RecursosHumanosWeb/"
    fi

    local csproj_count
    csproj_count=$(find "$ASPNET_DIR" -maxdepth 1 -name "*.csproj" 2>/dev/null | wc -l)
    if [[ "$csproj_count" -eq 0 ]]; then
        die "No se encontró ningún .csproj en $ASPNET_DIR"
    fi

    CSPROJ_FILE=$(find "$ASPNET_DIR" -maxdepth 1 -name "*.csproj" | head -1 | xargs basename)
    ASSEMBLY_NAME="${CSPROJ_FILE%.csproj}"
    log_info "Proyecto   : $CSPROJ_FILE"
    log_info "Ensamblado : ${ASSEMBLY_NAME}.dll"
}

check_ports() {
    log_step "Verificando disponibilidad de puertos..."
    local -A port_names=(["$MSSQL_PORT"]="SQL Server" ["$APP_PORT"]="Kestrel" ["80"]="Nginx HTTP")
    for port in "${!port_names[@]}"; do
        if ss -tlnp 2>/dev/null | grep -q ":${port}[[:space:]]"; then
            log_warn "Puerto $port (${port_names[$port]}) ya está en uso."
        fi
    done
    log_info "Verificación de puertos completada."
}

# ──────────────────────────────────────────────────────────────────────────────
# MODO REDEPLOY — Solo republica la app, sin tocar SQL ni BD
# ──────────────────────────────────────────────────────────────────────────────
run_redeploy() {
    log_section "MODO REDEPLOY — Republicando la app ASP.NET"

    # Necesitamos dotnet disponible
    export DOTNET_ROOT="$DOTNET_INSTALL_DIR"
    export PATH="$PATH:$DOTNET_INSTALL_DIR:$DOTNET_INSTALL_DIR/tools"

    if ! command -v dotnet &>/dev/null && [[ ! -x "$DOTNET_INSTALL_DIR/dotnet" ]]; then
        die ".NET SDK no encontrado. Ejecuta primero la instalación completa sin --redeploy."
    fi

    publish_aspnet_app

    log_step "Reiniciando servicio $APP_SERVICE..."
    systemctl restart "$APP_SERVICE" || \
        die "No se pudo reiniciar el servicio $APP_SERVICE.\nRevisa: systemctl status $APP_SERVICE"

    log_done "Redeploy completado. La app está corriendo con el nuevo build."
    echo -e "  Logs en vivo: ${BOLD}journalctl -u $APP_SERVICE -f${RESET}"
    echo ""
    exit 0
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 1 — DEPENDENCIAS DEL SISTEMA
# ──────────────────────────────────────────────────────────────────────────────
install_system_deps() {
    log_section "PASO 1 — Dependencias del sistema"

    log_step "Actualizando índice APT..."
    apt-get update -qq

    log_step "Instalando paquetes base..."
    DEBIAN_FRONTEND=noninteractive apt-get install -y -qq \
        curl wget gnupg2 ca-certificates \
        apt-transport-https software-properties-common \
        iproute2 nginx \
        2>&1 | grep -E "^(Inst|Conf)" || true

    log_done "Dependencias del sistema listas."
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 2 — .NET SDK (instalado globalmente en el servidor)
#
# Se instala en /usr/share/dotnet para que todos los usuarios y servicios
# systemd puedan usarlo sin configuración adicional.
# ──────────────────────────────────────────────────────────────────────────────
install_dotnet() {
    log_section "PASO 2 — .NET $DOTNET_CHANNEL SDK (nativo en el servidor)"

    # Exportar para que esté disponible en esta sesión
    export DOTNET_ROOT="$DOTNET_INSTALL_DIR"
    export PATH="$PATH:$DOTNET_INSTALL_DIR:$DOTNET_INSTALL_DIR/tools"

    # Verificar si ya está instalado y es la versión correcta
    if command -v dotnet &>/dev/null || [[ -x "$DOTNET_INSTALL_DIR/dotnet" ]]; then
        local installed_ver
        installed_ver=$("$DOTNET_INSTALL_DIR/dotnet" --version 2>/dev/null || dotnet --version 2>/dev/null || echo "desconocida")
        log_warn ".NET ya instalado (versión: $installed_ver)."
        log_info "Omitiendo reinstalación. Si necesitas actualizarlo, elimina $DOTNET_INSTALL_DIR primero."
        return 0
    fi

    log_step "Descargando instalador oficial de .NET desde Microsoft..."
    local tmp_installer
    tmp_installer=$(mktemp /tmp/dotnet-install.XXXXXX.sh)

    if ! wget -q --show-progress -O "$tmp_installer" https://dot.net/v1/dotnet-install.sh; then
        rm -f "$tmp_installer"
        die "No se pudo descargar el instalador de .NET.\nURL: https://dot.net/v1/dotnet-install.sh\nVerifica tu conexión."
    fi
    chmod +x "$tmp_installer"

    log_step "Instalando .NET $DOTNET_CHANNEL SDK en $DOTNET_INSTALL_DIR..."
    log_info "(Primera instalación — puede tardar varios minutos)"

    mkdir -p "$DOTNET_INSTALL_DIR"
    if ! "$tmp_installer" \
        --channel "$DOTNET_CHANNEL" \
        --install-dir "$DOTNET_INSTALL_DIR" \
        --no-path 2>&1 | grep -E "^(dotnet-install|Downloading|Extracting|Installing|.NET)" || true; then
        rm -f "$tmp_installer"
        die "Error durante la instalación de .NET SDK.\nIntenta ejecutar manualmente:\n  bash $tmp_installer --channel $DOTNET_CHANNEL --install-dir $DOTNET_INSTALL_DIR"
    fi
    rm -f "$tmp_installer"

    # Verificar que el binario existe
    [[ ! -x "$DOTNET_INSTALL_DIR/dotnet" ]] && \
        die ".NET SDK descargado pero el binario no está en $DOTNET_INSTALL_DIR/dotnet\nContenido del directorio:\n  $(find "$DOTNET_INSTALL_DIR" -maxdepth 1 2>/dev/null | head -10)"

    # Configurar variables de entorno globales (para todos los usuarios y servicios)
    log_step "Configurando variables de entorno globales en /etc/profile.d/dotnet.sh..."
    cat > /etc/profile.d/dotnet.sh << 'DOTNET_ENV'
# .NET SDK — Variables de entorno globales (RecursosHumanosWeb)
export DOTNET_ROOT=/usr/share/dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
DOTNET_ENV
    chmod 644 /etc/profile.d/dotnet.sh

    local version
    version=$("$DOTNET_INSTALL_DIR/dotnet" --version 2>/dev/null || echo "instalado")
    log_done ".NET SDK $version instalado en $DOTNET_INSTALL_DIR."
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 3 — DOCKER ENGINE (solo para SQL Server)
#
# Problema conocido: Debian 13 "trixie" NO tiene repositorio Docker propio.
# Solución: verificar disponibilidad del repo y hacer fallback a "bookworm".
# ──────────────────────────────────────────────────────────────────────────────
install_docker() {
    log_section "PASO 3 — Docker Engine (para SQL Server)"

    if command -v docker &>/dev/null; then
        log_warn "Docker ya instalado: $(docker --version)"
        systemctl enable docker --quiet 2>/dev/null || true
        systemctl start  docker         2>/dev/null || true
        if ! docker info &>/dev/null; then
            die "Docker está instalado pero el daemon no responde.\nRevisa:\n  systemctl status docker\n  journalctl -xeu docker.service"
        fi
        log_info "Docker daemon activo. Omitiendo reinstalación."
        return 0
    fi

    # ── Limpiar paquetes conflictivos ─────────────────────────────────────────
    # docker.io viene del repo base de Debian/Ubuntu y colisiona con docker-ce oficial
    log_step "Eliminando paquetes Docker conflictivos del repo base de Debian/Ubuntu..."
    for pkg in docker.io docker-doc docker-compose podman-docker containerd runc; do
        if dpkg -l "$pkg" &>/dev/null 2>&1; then
            log_warn "Eliminando conflicto: $pkg"
            apt-get remove -y -qq "$pkg" 2>/dev/null || true
        fi
    done

    # ── Clave GPG del repositorio oficial de Docker ───────────────────────────
    log_step "Importando clave GPG de Docker..."
    install -m 0755 -d /etc/apt/keyrings

    local gpg_url
    [[ "$OS_ID" == "ubuntu" ]] \
        && gpg_url="https://download.docker.com/linux/ubuntu/gpg" \
        || gpg_url="https://download.docker.com/linux/debian/gpg"

    if ! curl -fsSL "$gpg_url" | gpg --dearmor -o /etc/apt/keyrings/docker.gpg 2>/dev/null; then
        die "No se pudo obtener la clave GPG de Docker.\nURL: $gpg_url"
    fi
    chmod a+r /etc/apt/keyrings/docker.gpg
    log_info "Clave GPG importada."

    # ── Selección de repositorio con fallback automático ──────────────────────
    # Debian 13 trixie: Docker aún no publica repo propio → usar bookworm (compatible)
    local repo_base="https://download.docker.com/linux/${OS_ID}"
    local docker_codename="$OS_CODENAME"

    log_step "Verificando disponibilidad del repositorio Docker para '${docker_codename}'..."
    if ! curl -sf --max-time 8 "${repo_base}/dists/${docker_codename}/Release" >/dev/null 2>&1; then
        log_warn "No existe repositorio Docker para '${OS_ID}/${docker_codename}'."
        case "$OS_ID" in
            debian) docker_codename="bookworm" ;;
            ubuntu) docker_codename="noble"    ;;
            *) die "Sin repositorio Docker compatible para $OS_ID $OS_CODENAME.\nInstala Docker manualmente: https://docs.docker.com/engine/install/" ;;
        esac
        log_warn "Usando repositorio '${docker_codename}' como fallback (binarios compatibles con tu sistema)."
    else
        log_info "Repositorio Docker disponible para '${docker_codename}'."
    fi

    log_step "Registrando repositorio Docker en APT..."
    echo "deb [arch=${DPKG_ARCH} signed-by=/etc/apt/keyrings/docker.gpg] \
${repo_base} ${docker_codename} stable" \
        > /etc/apt/sources.list.d/docker.list

    log_step "Instalando Docker CE, CLI, Containerd y plugins..."
    apt-get update -qq
    if ! DEBIAN_FRONTEND=noninteractive apt-get install -y -qq \
        docker-ce docker-ce-cli containerd.io \
        docker-buildx-plugin docker-compose-plugin \
        2>&1 | grep -E "^(Inst|Conf)" || true; then
        die "Error al instalar Docker CE.\nEjecuta manualmente para ver el error:\n  apt-get install docker-ce docker-ce-cli containerd.io"
    fi

    log_step "Habilitando y arrancando Docker..."
    systemctl enable docker --quiet
    systemctl start  docker

    # Esperar a que el socket responda
    local retries=8
    until docker info &>/dev/null || [[ $retries -eq 0 ]]; do
        sleep 2; retries=$((retries - 1))
    done
    if ! docker info &>/dev/null; then
        die "Docker instalado pero el daemon no arrancó.\nRevisa:\n  systemctl status docker\n  journalctl -xeu docker.service"
    fi

    log_done "Docker $(docker --version | awk '{print $3}' | tr -d ',') instalado y activo."
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 3.5 — SQLPACKAGE (para publicar .dacpac)
# ──────────────────────────────────────────────────────────────────────────────
install_sqlpackage() {
    log_section "PASO 3.5 — sqlpackage (publicación de .dacpac)"

    if command -v sqlpackage &>/dev/null; then
        log_warn "sqlpackage ya instalado: $(sqlpackage --version 2>/dev/null | head -1)"
        return 0
    fi

    export DOTNET_ROOT="$DOTNET_INSTALL_DIR"
    export PATH="$PATH:$DOTNET_INSTALL_DIR:$DOTNET_INSTALL_DIR/tools"

    log_step "Instalando sqlpackage como herramienta global de .NET..."
    if ! "$DOTNET_INSTALL_DIR/dotnet" tool install -g microsoft.sqlpackage 2>&1; then
        log_warn "No se pudo instalar sqlpackage. Se usará schemaDB.sql como fallback."
        SQLPACKAGE_AVAILABLE=false
        return 0
    fi

    # Agregar tools al PATH global
    export PATH="$PATH:$HOME/.dotnet/tools"
    SQLPACKAGE_AVAILABLE=true
    log_done "sqlpackage instalado correctamente."
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 4 — CONTENEDOR SQL SERVER 2025
# ──────────────────────────────────────────────────────────────────────────────
setup_mssql_container() {
    log_section "PASO 4 — SQL Server 2025 Express en Docker"

    # ── --reset-all: eliminar volumen (DESTRUCTIVO) ───────────────────────────
    if [[ "$FLAG_RESET_ALL" == "true" ]]; then
        log_warn "--reset-all: Se eliminarán el contenedor Y el volumen de datos."
        log_warn "⚠  TODOS LOS DATOS DE LA BASE DE DATOS SERÁN BORRADOS PERMANENTEMENTE."
        read -rp "  ¿Confirmar eliminación total? Escribe 'SI' en mayúsculas para confirmar: " ans
        if [[ "$ans" == "SI" ]]; then
            log_step "Eliminando contenedor y volumen..."
            docker stop   "$MSSQL_CONTAINER" 2>/dev/null || true
            docker rm     "$MSSQL_CONTAINER" 2>/dev/null || true
            docker volume rm "$MSSQL_VOLUME" 2>/dev/null || true
            log_info "Contenedor y volumen eliminados."
        else
            log_warn "Reset cancelado. Se conservan contenedor y datos."
            FLAG_RESET_ALL=false
            FLAG_RESET_SQL=false
            FLAG_RESET_DB=false
        fi

    # ── --reset-sql: eliminar solo el contenedor (conserva el volumen de datos) ─
    elif [[ "$FLAG_RESET_SQL" == "true" ]]; then
        log_warn "--reset-sql: Se eliminará el contenedor SQL Server."
        log_info "El volumen de datos '$MSSQL_VOLUME' se conservará."
        read -rp "  ¿Confirmar eliminación del contenedor? [s/N]: " ans
        if [[ "$ans" =~ ^[sS]$ ]]; then
            docker stop "$MSSQL_CONTAINER" 2>/dev/null || true
            docker rm   "$MSSQL_CONTAINER" 2>/dev/null || true
            log_info "Contenedor eliminado. Los datos persisten en el volumen."
        else
            log_warn "Reset cancelado."
            FLAG_RESET_SQL=false
        fi
    fi

    # ── Descargar imagen ──────────────────────────────────────────────────────
    log_step "Descargando imagen SQL Server 2025 (puede tardar varios minutos la primera vez)..."
    log_info "Imagen: $MSSQL_IMAGE  (~1.5 GB)"
    if ! docker pull "$MSSQL_IMAGE"; then
        die "No se pudo descargar la imagen de SQL Server.\nImagen: $MSSQL_IMAGE\nVerifica conexión a internet y que mcr.microsoft.com sea accesible."
    fi

    # ── Crear contenedor si no existe ─────────────────────────────────────────
    if docker ps -a --format '{{.Names}}' | grep -q "^${MSSQL_CONTAINER}$"; then
        local state
        state=$(docker inspect --format='{{.State.Status}}' "$MSSQL_CONTAINER" 2>/dev/null || echo "unknown")
        log_info "Contenedor '$MSSQL_CONTAINER' ya existe (estado: $state)."

        if [[ "$state" != "running" ]]; then
            log_step "Iniciando contenedor detenido..."
            docker start "$MSSQL_CONTAINER" || \
                die "No se pudo iniciar el contenedor '$MSSQL_CONTAINER'.\nRevisa: docker logs $MSSQL_CONTAINER"
        fi
    else
        log_step "Creando contenedor '$MSSQL_CONTAINER' en puerto $MSSQL_PORT..."
        docker run \
            --name "$MSSQL_CONTAINER" \
            -e "ACCEPT_EULA=Y" \
            -e "MSSQL_SA_PASSWORD=$MSSQL_SA_PASSWORD" \
            -e "MSSQL_PID=$MSSQL_PID" \
            -p "127.0.0.1:${MSSQL_PORT}:1433" \
            -v "${MSSQL_VOLUME}:/var/opt/mssql" \
            --restart=unless-stopped \
            -d \
            "$MSSQL_IMAGE" || \
            die "Error al crear el contenedor SQL Server.\nRevisa: docker logs $MSSQL_CONTAINER\nPosibles causas:\n  - Puerto $MSSQL_PORT ya ocupado (revisa con: ss -tlnp | grep $MSSQL_PORT)\n  - RAM insuficiente\n  - Imagen incompatible con la arquitectura $(uname -m)"

        log_info "Contenedor creado con volumen persistente '$MSSQL_VOLUME'."
    fi

    # ── Servicio systemd para el contenedor SQL ───────────────────────────────
    log_step "Registrando SQL Server como servicio systemd..."
    cat > /etc/systemd/system/rh-sql.service << SYSTEMD_SQL
[Unit]
Description=RecursosHumanosWeb — SQL Server 2025 Express (Docker)
After=docker.service network-online.target
Requires=docker.service
Wants=network-online.target

[Service]
Type=oneshot
RemainAfterExit=yes
ExecStart=/usr/bin/docker start ${MSSQL_CONTAINER}
ExecStop=/usr/bin/docker stop -t 20 ${MSSQL_CONTAINER}
Restart=on-failure
RestartSec=10s
TimeoutStartSec=90s
StandardOutput=journal
StandardError=journal
SyslogIdentifier=rh-sql

[Install]
WantedBy=multi-user.target
SYSTEMD_SQL

    systemctl daemon-reload
    systemctl enable rh-sql --quiet
    log_info "Servicio rh-sql.service registrado y habilitado."
    log_done "SQL Server 2025 Express configurado."
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 5 — ESPERAR QUE SQL SERVER ESTÉ LISTO
# ──────────────────────────────────────────────────────────────────────────────
wait_for_mssql() {
    log_section "PASO 5 — Esperando que SQL Server acepte conexiones"

    # Prueba de conectividad real: intenta ejecutar SELECT 1 contra el motor.
    # Es más confiable que buscar en logs (que pueden tener entradas viejas)
    # o esperar el healthcheck (que puede no estar configurado en la imagen).
    sql_is_ready() {
        docker exec "$MSSQL_CONTAINER" \
            /opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
            -C -Q "SELECT 1" \
            > /dev/null 2>&1
    }

    # Verificación inmediata — si el contenedor ya estaba corriendo, responde al toque
    log_step "Verificando conectividad (intentando conexión directa a SQL Server)..."
    if sql_is_ready; then
        log_done "SQL Server listo y aceptando conexiones."
        return 0
    fi

    log_step "SQL Server no respondió aún. Esperando hasta ${SQL_READY_TIMEOUT}s..."
    local elapsed=0

    while [[ $elapsed -lt $SQL_READY_TIMEOUT ]]; do
        sleep 5
        elapsed=$((elapsed + 5))

        # Verificar que el contenedor no se haya caído
        local state
        state=$(docker inspect --format='{{.State.Status}}' "$MSSQL_CONTAINER" 2>/dev/null || echo "unknown")
        if [[ "$state" == "exited" ]]; then
            log_error "El contenedor SQL Server se detuvo inesperadamente."
            log_error "Últimas líneas del log:"
            docker logs --tail 20 "$MSSQL_CONTAINER" 2>&1 | while IFS= read -r line; do
                log_error "  $line"
            done
            die "SQL Server se detuvo. Causas comunes:\n  - RAM insuficiente (requiere mínimo 2GB)\n  - Contraseña SA no cumple requisitos de complejidad\n  - Puerto $MSSQL_PORT ya en uso\nRevisa el log: docker logs $MSSQL_CONTAINER"
        fi

        printf "\r  ${YELLOW}⏳${RESET}  Esperando SQL Server... %3ds / %ds  (estado: %s)   " \
            "$elapsed" "$SQL_READY_TIMEOUT" "$state"

        if sql_is_ready; then
            echo ""
            log_done "SQL Server listo y aceptando conexiones (${elapsed}s)."
            return 0
        fi
    done

    echo ""
    log_error "SQL Server no respondió en ${SQL_READY_TIMEOUT}s."
    log_error "Diagnóstico:"
    log_error "  → Logs          : docker logs --tail 30 $MSSQL_CONTAINER"
    log_error "  → Estado        : docker inspect --format='{{.State.Status}}' $MSSQL_CONTAINER"
    log_error "  → RAM disponible: free -h"
    log_error "  → Prueba manual : docker exec $MSSQL_CONTAINER /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '$MSSQL_SA_PASSWORD' -C -Q 'SELECT 1'"
    die "Tiempo de espera agotado."
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 6 — INICIALIZACIÓN DE LA BASE DE DATOS
# ──────────────────────────────────────────────────────────────────────────────
init_database() {
    if [[ "$FLAG_SKIP_DB" == "true" ]]; then
        log_warn "PASO 6 omitido por --skip-db. Base de datos sin cambios."
        return 0
    fi

    log_section "PASO 6 — Inicializando la base de datos"

    # ── Helpers SQL ───────────────────────────────────────────────────────────
    run_sql() {
        docker exec -i "$MSSQL_CONTAINER" \
            /opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
            -C -b
    }
    run_sql_db() {
        docker exec -i "$MSSQL_CONTAINER" \
            /opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
            -d "$MSSQL_DB_NAME" -C -b
    }
    run_sql_inline() {
        docker exec "$MSSQL_CONTAINER" \
            /opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
            -d "$MSSQL_DB_NAME" -C -b -Q "$1"
    }

    # Deshabilita restricciones FK (necesario para inserts masivos)
    disable_fks() {
        log_step "Deshabilitando restricciones FK temporalmente..."
        local sql_nocheck
        sql_nocheck=$(docker exec "$MSSQL_CONTAINER" \
            /opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
            -d "$MSSQL_DB_NAME" -C -h -1 \
            -Q "SET NOCOUNT ON;
                SELECT 'ALTER TABLE [' + OBJECT_NAME(parent_object_id) +
                       '] NOCHECK CONSTRAINT [' + name + '];'
                FROM sys.foreign_keys
                ORDER BY OBJECT_NAME(parent_object_id);" \
            2>/dev/null | grep "^ALTER" || true)

        while IFS= read -r stmt; do
            [[ -z "$stmt" ]] && continue
            run_sql_inline "$stmt"
        done <<< "$sql_nocheck"
    }

    # Rehabilita y verifica TODAS las FKs
    enable_fks() {
        log_step "Rehabilitando y verificando restricciones FK..."
        local sql_check
        sql_check=$(docker exec "$MSSQL_CONTAINER" \
            /opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
            -d "$MSSQL_DB_NAME" -C -h -1 \
            -Q "SET NOCOUNT ON;
                SELECT 'ALTER TABLE [' + OBJECT_NAME(parent_object_id) +
                       '] WITH CHECK CHECK CONSTRAINT [' + name + '];'
                FROM sys.foreign_keys
                ORDER BY OBJECT_NAME(parent_object_id);" \
            2>/dev/null | grep "^ALTER" || true)

        local errors=0
        while IFS= read -r stmt; do
            [[ -z "$stmt" ]] && continue
            if ! run_sql_inline "$stmt" 2>/dev/null; then
                log_warn "FK con datos inconsistentes: $stmt"
                errors=$((errors + 1))
            fi
        done <<< "$sql_check"

        if [[ $errors -gt 0 ]]; then
            log_warn "$errors FK(s) no pudieron rehabilitarse — hay datos huérfanos."
            log_warn "Ejecuta el diagnóstico para ver cuáles: sudo bash db/setup.sh --check-db"
        else
            log_info "Todas las FK rehabilitadas y verificadas correctamente."
        fi
    }

    # ── Verificar si la BD ya existe ──────────────────────────────────────────
    log_step "Verificando si '$MSSQL_DB_NAME' ya existe..."
    local db_exists
    db_exists=$(docker exec "$MSSQL_CONTAINER" \
        /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
        -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name='${MSSQL_DB_NAME}'" \
        -h -1 2>/dev/null | tr -d '[:space:]' || echo "0")

    if [[ "$db_exists" == "1" ]]; then
        log_warn "La base de datos '$MSSQL_DB_NAME' ya existe."
        if [[ "$FLAG_RESET_DB" == "true" ]]; then
            log_warn "--reset-db activo: se recreará la BD sin preguntar."
        else
            read -rp "  ¿Eliminar y recrear la BD? Esto borrará TODOS los datos [s/N]: " ans
            if [[ ! "$ans" =~ ^[sS]$ ]]; then
                log_info "BD conservada. Usando datos existentes."
                return 0
            fi
        fi

        log_step "Eliminando '$MSSQL_DB_NAME' (cerrando conexiones activas)..."
        docker exec "$MSSQL_CONTAINER" \
            /opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
            -Q "ALTER DATABASE [${MSSQL_DB_NAME}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [${MSSQL_DB_NAME}];" 2>/dev/null || \
            die "No se pudo eliminar la BD. Detén el servicio primero:\n  systemctl stop $APP_SERVICE"
        log_info "BD eliminada correctamente."
    fi

    # ── ESTRATEGIA 1: .dacpac vía sqlpackage ──────────────────────────────────
    local DB_SCHEMA_METHOD="unknown"
    if [[ "$SQLPACKAGE_AVAILABLE" == "true" ]] && [[ -f "$DACPAC_FILE" ]]; then
        log_step "Estrategia 1: Publicando schema desde .dacpac → $DACPAC_FILE"

        # Crear la BD vacía primero (sqlpackage la necesita para publicar)
        docker exec "$MSSQL_CONTAINER" \
            /opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
            -Q "CREATE DATABASE [${MSSQL_DB_NAME}];" 2>/dev/null || true

        if sqlpackage \
            /Action:Publish \
            /SourceFile:"$DACPAC_FILE" \
            /TargetServerName:"localhost,${MSSQL_PORT}" \
            /TargetDatabaseName:"$MSSQL_DB_NAME" \
            /TargetUser:"sa" \
            /TargetPassword:"$MSSQL_SA_PASSWORD" \
            /TargetTrustServerCertificate:True \
            /p:BlockOnPossibleDataLoss=False 2>&1; then

            log_done "Schema aplicado correctamente desde .dacpac."
            DB_SCHEMA_METHOD="dacpac"
        else
            log_warn "sqlpackage falló. Intentando fallback con schemaDB.sql..."
            DB_SCHEMA_METHOD="fallback"
        fi
    else
        [[ "$SQLPACKAGE_AVAILABLE" == "false" ]] && log_warn "sqlpackage no disponible."
        [[ ! -f "$DACPAC_FILE" ]] && log_warn "No se encontró: $DACPAC_FILE"
        log_step "Usando fallback directo: schemaDB.sql"
        DB_SCHEMA_METHOD="fallback"
    fi

    # ── ESTRATEGIA 2: schemaDB.sql (fallback) ─────────────────────────────────
    if [[ "$DB_SCHEMA_METHOD" == "fallback" ]]; then
        log_step "Estrategia 2 (fallback): Ejecutando schemaDB.sql..."
        check_sql_files  # Verificar existencia de archivos
        if ! run_sql < "$SCHEMA_SQL"; then
            die "Error en schemaDB.sql. Revisa el log: docker logs $MSSQL_CONTAINER"
        fi
        log_info "schemaDB.sql → OK"
    fi

    # ── Inserts (Catálogos y datos iniciales) ──────────────────────────────────
    log_step "Preparando inserción de datos iniciales..."
    disable_fks

    log_step "Ejecutando InsertsDB.sql..."
    local tmp_inserts
    tmp_inserts=$(mktemp /tmp/inserts_clean.XXXXXX.sql)
    # Limpiamos sp_MSforeachtable si existe para usar nuestro manejo explícito de FKs
    grep -v "sp_MSforeachtable" "$INSERTS_SQL" > "$tmp_inserts" || true

    if ! run_sql_db < "$tmp_inserts"; then
        rm -f "$tmp_inserts"
        die "Error en InsertsDB.sql. Revisa los logs arriba."
    fi
    rm -f "$tmp_inserts"
    log_info "InsertsDB.sql → OK"

    enable_fks
    log_done "Base de datos '$MSSQL_DB_NAME' inicializada con éxito (método: ${DB_SCHEMA_METHOD})."
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 7 — PUBLICAR LA APP ASP.NET
#
# dotnet publish compila en Release y deposita los artefactos en APP_PUBLISH_DIR.
# Esto es lo que correrá Kestrel directamente — sin Docker, sin SDK en runtime.
# ──────────────────────────────────────────────────────────────────────────────
publish_aspnet_app() {
    log_section "PASO 7 — Publicando app ASP.NET (dotnet publish Release)"

    # Asegurar que dotnet esté en el PATH de esta sesión
    export DOTNET_ROOT="$DOTNET_INSTALL_DIR"
    export PATH="$PATH:$DOTNET_INSTALL_DIR:$DOTNET_INSTALL_DIR/tools"

    local dotnet_bin="$DOTNET_INSTALL_DIR/dotnet"
    [[ ! -x "$dotnet_bin" ]] && \
        die "No se encontró el binario de .NET en $dotnet_bin.\nVerifica que el Paso 2 completó correctamente."

    local dotnet_ver
    dotnet_ver=$("$dotnet_bin" --version 2>/dev/null || echo "desconocida")
    log_info ".NET SDK versión: $dotnet_ver"

    # ── Restaurar dependencias NuGet ──────────────────────────────────────────
    log_step "Restaurando paquetes NuGet (dotnet restore)..."
    cd "$ASPNET_DIR"
    local restore_output
    restore_output=$("$dotnet_bin" restore "$CSPROJ_FILE" 2>&1) || {
        echo "$restore_output"
        die "Error en dotnet restore.\nPosibles causas:\n  - Sin acceso a nuget.org\n  - Versión de .NET incompatible\n  - Archivo .csproj con errores\nEjecuta manualmente:\n  cd $ASPNET_DIR && dotnet restore $CSPROJ_FILE"
    }
    echo "$restore_output" | grep -E "(warning|error|Restored|Failed)" || true
    log_info "Paquetes NuGet restaurados."

    # ── Publicar en Release ───────────────────────────────────────────────────
    log_step "Compilando y publicando en: $APP_PUBLISH_DIR"
    log_info "(Configuración Release, sin SDK en runtime — solo los binarios necesarios)"

    # Limpiar publicación anterior si existe
    if [[ -d "$APP_PUBLISH_DIR" ]]; then
        log_warn "Directorio de publicación existente. Sobreescribiendo..."
        rm -rf "${APP_PUBLISH_DIR:?}"/*
    fi
    mkdir -p "$APP_PUBLISH_DIR"

    # Capturar output y código de salida por separado
    # grep filtraría warnings como errores — usamos el exit code real de dotnet
    local publish_output
    publish_output=$("$dotnet_bin" publish "$CSPROJ_FILE" \
        --configuration Release \
        --no-restore \
        --output "$APP_PUBLISH_DIR" \
        2>&1) || {
        echo "$publish_output"
        die "Error en dotnet publish.\nRevisa la salida arriba.\nEjecuta manualmente:\n  cd $ASPNET_DIR && dotnet publish $CSPROJ_FILE -c Release -o $APP_PUBLISH_DIR"
    }

    # Mostrar solo líneas relevantes del output (warnings y resultado final)
    echo "$publish_output" | grep -E "(warning|error|succeeded|failed|→)" || true

    # Verificar que el DLL principal existe
    if [[ ! -f "$APP_PUBLISH_DIR/${ASSEMBLY_NAME}.dll" ]]; then
        die "dotnet publish completó pero no se encontró el DLL principal.\nEsperado: $APP_PUBLISH_DIR/${ASSEMBLY_NAME}.dll\nContenido del directorio:\n  $(find "$APP_PUBLISH_DIR" -maxdepth 1 2>/dev/null | head -15)"
    fi

    # Asignar permisos correctos al directorio de publicación
    chown -R "$APP_USER":"$APP_USER" "$APP_PUBLISH_DIR" 2>/dev/null || \
        chown -R www-data:www-data "$APP_PUBLISH_DIR" 2>/dev/null || true
    chmod -R 750 "$APP_PUBLISH_DIR"

    log_done "App publicada en: $APP_PUBLISH_DIR/${ASSEMBLY_NAME}.dll"
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 8 — ARCHIVO DE VARIABLES DE ENTORNO PARA KESTREL
#
# ASP.NET Core lee variables de entorno con doble guion bajo (__) como
# separador de jerarquía JSON, sobreescribiendo appsettings.json.
# El archivo se protege con permisos 600 (solo root) y se referencia
# desde el servicio systemd con EnvironmentFile=
# ──────────────────────────────────────────────────────────────────────────────
generate_env_file() {
    log_section "PASO 8 — Variables de entorno (secretos de producción)"

    local env_file="/etc/recursoshumanosweb.env"

    if [[ -f "$env_file" ]]; then
        log_warn "Ya existe: $env_file"
        read -rp "  ¿Sobreescribirlo con los valores actuales del script? [s/N]: " ans
        if [[ ! "$ans" =~ ^[sS]$ ]]; then
            log_info "Archivo de entorno existente conservado."
            return 0
        fi
        cp "$env_file" "${env_file}.bak.$(date +%Y%m%d%H%M%S)"
        log_info "Backup creado: ${env_file}.bak.*"
    fi

    log_step "Generando $env_file..."
    cat > "$env_file" << ENV_CONTENT
# =============================================================================
#  /etc/recursoshumanosweb.env — Secretos de producción
#  Cargado por systemd (EnvironmentFile=) en el servicio recursoshumanosweb
#  ⚠  Permisos 600 — solo root puede leerlo. NUNCA versionar este archivo.
#  Generado por setup.sh v3.0.0 — $(date '+%Y-%m-%d %H:%M:%S')
# =============================================================================

# ── ASP.NET Core ──────────────────────────────────────────────────────────────
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://localhost:${APP_PORT}

# ── Cadena de conexión a SQL Server ───────────────────────────────────────────
# El nombre del contenedor Docker NO es accesible desde el host por nombre.
# Se usa localhost con el puerto mapeado (127.0.0.1:${MSSQL_PORT}).
ConnectionStrings__RecursosHumanosContext=Server=localhost,${MSSQL_PORT};Database=${MSSQL_DB_NAME};User Id=sa;Password=${MSSQL_SA_PASSWORD};Encrypt=False;TrustServerCertificate=True;

# ── SMTP / Email ──────────────────────────────────────────────────────────────
SmtpSettings__EmailSender=${SMTP_EMAIL_SENDER}
SmtpSettings__SmtpServer=${SMTP_SERVER}
SmtpSettings__Port=${SMTP_PORT}
SmtpSettings__PasswordApp=${SMTP_PASSWORD_APP}

# ── Rendimiento ───────────────────────────────────────────────────────────────
# Optimiza el GC en servidores con poca RAM
DOTNET_GCConserveMemory=7
DOTNET_ROOT=${DOTNET_INSTALL_DIR}
ENV_CONTENT

    # Solo root puede leerlo — contiene contraseñas
    chmod 600 "$env_file"
    chown root:root "$env_file"

    log_info "Permisos: 600 (solo root)"
    log_done "Archivo de entorno generado en $env_file"
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 9 — SERVICIO SYSTEMD PARA KESTREL (la app ASP.NET nativa)
# ──────────────────────────────────────────────────────────────────────────────
setup_app_service() {
    log_section "PASO 9 — Servicio systemd para Kestrel (app nativa)"

    local service_file="/etc/systemd/system/${APP_SERVICE}.service"

    log_step "Creando unidad systemd: $service_file"
    cat > "$service_file" << SYSTEMD_APP
# =============================================================================
#  systemd — ${APP_SERVICE}.service
#  Corre la app ASP.NET con Kestrel directamente en el servidor (sin Docker)
#  Generado por setup.sh v3.0.0
# =============================================================================

[Unit]
Description=RecursosHumanosWeb — ASP.NET MVC + API (Kestrel)
After=network-online.target rh-sql.service
Wants=network-online.target
Requires=rh-sql.service

[Service]
Type=simple
# Usuario sin privilegios de root para correr la app
User=${APP_USER}
Group=${APP_USER}
WorkingDirectory=${APP_PUBLISH_DIR}

# Binario de la app publicada
ExecStart=${DOTNET_INSTALL_DIR}/dotnet ${APP_PUBLISH_DIR}/${ASSEMBLY_NAME}.dll

# Cargar secretos y configuración de entorno desde archivo protegido (600)
EnvironmentFile=/etc/recursoshumanosweb.env

# Reinicio automático en caso de crash
Restart=always
RestartSec=10s
KillSignal=SIGINT
TimeoutStopSec=30s

# Seguridad adicional (hardening básico para producción)
PrivateTmp=true
ProtectSystem=full
NoNewPrivileges=true

# Logs disponibles con: journalctl -u ${APP_SERVICE} -f
StandardOutput=journal
StandardError=journal
SyslogIdentifier=${APP_SERVICE}

[Install]
WantedBy=multi-user.target
SYSTEMD_APP

    log_step "Recargando systemd y habilitando servicio..."
    systemctl daemon-reload
    systemctl enable "$APP_SERVICE" --quiet

    log_step "Iniciando servicio $APP_SERVICE..."
    if ! systemctl start "$APP_SERVICE"; then
        log_error "El servicio no arrancó correctamente."
        log_error "Diagnóstico:"
        systemctl status "$APP_SERVICE" --no-pager -l 2>/dev/null | tail -20 | while IFS= read -r line; do
            log_error "  $line"
        done
        die "Kestrel no pudo arrancar.\nRevisa:\n  journalctl -u $APP_SERVICE -n 50 --no-pager\nPosibles causas:\n  - Puerto $APP_PORT ya en uso\n  - Error en la cadena de conexión (SQL Server no disponible)\n  - DLL principal no encontrado en $APP_PUBLISH_DIR"
    fi

    # Esperar a que systemd confirme que el servicio está activo
    local retries=6
    until systemctl is-active --quiet "$APP_SERVICE" || [[ $retries -eq 0 ]]; do
        sleep 2; retries=$((retries - 1))
    done

    if systemctl is-active --quiet "$APP_SERVICE"; then
        log_done "Servicio $APP_SERVICE activo y corriendo en puerto $APP_PORT."
    else
        die "El servicio $APP_SERVICE no está activo.\nRevisa: journalctl -u $APP_SERVICE -n 50 --no-pager"
    fi
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 10 — NGINX COMO REVERSE PROXY
# ──────────────────────────────────────────────────────────────────────────────
configure_nginx() {
    log_section "PASO 10 — Nginx (reverse proxy → Kestrel :${APP_PORT})"

    command -v nginx &>/dev/null || \
        die "Nginx no está instalado. El Paso 1 debió haberlo instalado."

    local nginx_conf="/etc/nginx/sites-available/recursoshumanosweb"
    local nginx_link="/etc/nginx/sites-enabled/recursoshumanosweb"

    log_step "Generando configuración Nginx en $nginx_conf..."
    cat > "$nginx_conf" << NGINX_CONF
# =============================================================================
#  Nginx — RecursosHumanosWeb
#  Tráfico externo (80) → Kestrel (localhost:${APP_PORT})
#  Generado por setup.sh v3.0.0
# =============================================================================

server {
    listen 80;
    listen [::]:80;
    server_name ${NGINX_SERVER_NAME};

    client_max_body_size 50M;

    # Cabeceras de seguridad básicas
    add_header X-Frame-Options        "SAMEORIGIN"            always;
    add_header X-Content-Type-Options "nosniff"               always;
    add_header X-XSS-Protection       "1; mode=block"         always;
    add_header Referrer-Policy        "strict-origin-when-cross-origin" always;

    # Proxy hacia Kestrel
    location / {
        proxy_pass         http://localhost:${APP_PORT};
        proxy_http_version 1.1;
        proxy_set_header   Upgrade           \$http_upgrade;
        proxy_set_header   Connection        keep-alive;
        proxy_set_header   Host              \$host;
        proxy_set_header   X-Real-IP         \$remote_addr;
        proxy_set_header   X-Forwarded-For   \$proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
        proxy_connect_timeout 60s;
        proxy_send_timeout    60s;
        proxy_read_timeout    90s;
    }

    # Caché para archivos estáticos
    location ~* \.(css|js|ico|png|jpg|jpeg|gif|svg|woff|woff2|ttf|eot|map)$ {
        proxy_pass         http://localhost:${APP_PORT};
        proxy_http_version 1.1;
        proxy_set_header   Host \$host;
        expires            7d;
        add_header         Cache-Control "public, immutable";
    }

    # Bloquear acceso a archivos sensibles
    location ~ /\.(env|git|htaccess|vs) {
        deny all;
        return 404;
    }

    access_log /var/log/nginx/recursoshumanosweb_access.log;
    error_log  /var/log/nginx/recursoshumanosweb_error.log warn;
}
NGINX_CONF

    # Activar el sitio
    ln -sf "$nginx_conf" "$nginx_link"
    log_info "Sitio habilitado en sites-enabled."

    # Desactivar el sitio default para evitar conflicto en puerto 80
    if [[ -L "/etc/nginx/sites-enabled/default" ]]; then
        rm -f /etc/nginx/sites-enabled/default
        log_info "Sitio 'default' de Nginx desactivado."
    fi

    # Validar configuración antes de recargar
    log_step "Validando configuración de Nginx..."
    if ! nginx -t 2>&1; then
        die "La configuración de Nginx tiene errores de sintaxis.\nRevisa: $nginx_conf"
    fi

    log_step "Habilitando y recargando Nginx..."
    systemctl enable nginx --quiet
    systemctl reload nginx || systemctl start nginx

    log_done "Nginx configurado como reverse proxy en puerto 80."
}

# ──────────────────────────────────────────────────────────────────────────────
# RESUMEN FINAL
# ──────────────────────────────────────────────────────────────────────────────
print_summary() {
    local LOCAL_IP
    LOCAL_IP=$(ip -4 route get 1.1.1.1 2>/dev/null | awk '{print $7; exit}' || echo "desconocida")

    echo ""
    echo -e "${BOLD}${GREEN}╔══════════════════════════════════════════════════════════╗${RESET}"
    echo -e "${BOLD}${GREEN}║      ✅  INSTALACIÓN COMPLETADA EXITOSAMENTE             ║${RESET}"
    echo -e "${BOLD}${GREEN}╚══════════════════════════════════════════════════════════╝${RESET}"
    echo ""
    echo -e "  ${BOLD}Arquitectura desplegada:${RESET}"
    echo -e "    Internet → Nginx (:80) → Kestrel (:${APP_PORT}) → SQL Server Docker (:${MSSQL_PORT})"
    echo ""
    echo -e "  ${BOLD}Componentes:${RESET}"
    echo -e "    • .NET SDK               →  $DOTNET_INSTALL_DIR/dotnet"
    echo -e "    • App publicada          →  $APP_PUBLISH_DIR/${ASSEMBLY_NAME}.dll"
    echo -e "    • Kestrel (systemd)      →  ${APP_SERVICE}.service  (puerto $APP_PORT)"
    echo -e "    • SQL Server 2025        →  Contenedor '${MSSQL_CONTAINER}'  (127.0.0.1:${MSSQL_PORT})"
    echo -e "    • SQL servicio (systemd) →  rh-sql.service"
    echo -e "    • Nginx reverse proxy    →  Puerto 80 → localhost:${APP_PORT}"
    echo -e "    • Secretos               →  /etc/recursoshumanosweb.env  (chmod 600)"
    echo ""
    echo -e "  ${BOLD}Acceso a la aplicación:${RESET}"
    echo -e "    Local      : http://localhost"
    echo -e "    Red local  : http://${LOCAL_IP}"
    echo ""
    echo -e "  ${BOLD}Credenciales SQL Server:${RESET}"
    echo -e "    Servidor   : localhost,${MSSQL_PORT}  (solo loopback)"
    echo -e "    Usuario    : sa"
    echo -e "    Contraseña : ${MSSQL_SA_PASSWORD}"
    echo -e "    Base datos : ${MSSQL_DB_NAME}"
    echo ""
    echo -e "  ${BOLD}Comandos del día a día:${RESET}"
    echo -e "    Estado de la app        :  systemctl status ${APP_SERVICE}"
    echo -e "    Logs de la app en vivo  :  journalctl -u ${APP_SERVICE} -f"
    echo -e "    Estado de SQL Server    :  systemctl status rh-sql"
    echo -e "    Logs de SQL Server      :  docker logs ${MSSQL_CONTAINER} -f"
    echo -e "    Conectar a SQL Server   :  docker exec -it ${MSSQL_CONTAINER} /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '${MSSQL_SA_PASSWORD}' -C"
    echo -e "    Reiniciar la app        :  systemctl restart ${APP_SERVICE}"
    echo -e "    Reiniciar SQL Server    :  systemctl restart rh-sql"
    echo -e "    Logs de Nginx           :  tail -f /var/log/nginx/recursoshumanosweb_error.log"
    echo ""
    echo -e "  ${BOLD}Redeploy (actualizar la app sin tocar la BD):${RESET}"
    echo -e "    sudo bash db/setup.sh --redeploy"
    echo ""
    echo -e "  ${YELLOW}${BOLD}⚠  Próximos pasos recomendados para producción real:${RESET}"
    echo -e "    1. Cambiar MSSQL_SA_PASSWORD en este script y en /etc/recursoshumanosweb.env"
    echo -e "    2. Cambiar NGINX_SERVER_NAME por el dominio real y agregar HTTPS:"
    echo -e "       apt install certbot python3-certbot-nginx && certbot --nginx -d tu.dominio.com"
    echo -e "    3. Aplicar .NET en la sesión actual:"
    echo -e "       source /etc/profile.d/dotnet.sh"
    echo ""
}

# ──────────────────────────────────────────────────────────────────────────────
# MAIN
# ──────────────────────────────────────────────────────────────────────────────
main() {
    clear
    echo ""
    echo -e "${BOLD}${CYAN}"
    echo "   ██████╗ ██╗  ██╗    ██████╗ ███████╗ ██████╗ ██╗   ██╗██████╗ ███████╗ ██████╗ ███████╗"
    echo "   ██╔══██╗██║  ██║    ██╔══██╗██╔════╝██╔════╝ ██║   ██║██╔══██╗██╔════╝██╔═══██╗██╔════╝"
    echo "   ██████╔╝███████║    ██████╔╝█████╗  ██║      ██║   ██║██████╔╝███████╗██║   ██║███████╗"
    echo "   ██╔══██╗██╔══██║    ██╔══██╗██╔══╝  ██║      ██║   ██║██╔══██╗╚════██║██║   ██║╚════██║"
    echo "   ██║  ██║██║  ██║    ██║  ██║███████╗╚██████╗ ╚██████╔╝██║  ██║███████║╚██████╔╝███████║"
    echo "   ╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝  ╚═╝╚══════╝ ╚═════╝  ╚═════╝ ╚═╝  ╚═╝╚══════╝ ╚═════╝ ╚══════╝"
    echo ""
    echo -e "  RecursosHumanosWeb — Script de Instalación Automatizada v3.0.0${RESET}"
    echo -e "  TecNM Campus Monclova © 2026"
    echo ""

    # ── Pre-checks ─────────────────────────────────────────────────────────
    log_section "PRE-CHECKS"
    check_root
    detect_os
    check_ram
    check_internet
    check_sql_files
    check_aspnet_project
    check_ports

    # ── Modo redeploy rápido (sale temprano) ───────────────────────────────
    if [[ "$FLAG_REDEPLOY" == "true" ]]; then
        check_aspnet_project
        run_redeploy
    fi

    # ── Instalación completa ───────────────────────────────────────────────
    install_system_deps     # Paso 1: nginx, curl, gnupg, etc.
    install_dotnet          # Paso 2: .NET SDK global en /usr/share/dotnet
    install_docker          # Paso 3: Docker CE (con fallback para Debian 13)
    install_sqlpackage      # Paso 3.5: sqlpackage para publicar .dacpac
    setup_mssql_container   # Paso 4: contenedor SQL Server + servicio rh-sql
    wait_for_mssql          # Paso 5: esperar a que el motor SQL esté listo
    init_database           # Paso 6: schema + inserts (con protección de datos)
    publish_aspnet_app      # Paso 7: dotnet publish Release → /var/www/
    generate_env_file       # Paso 8: /etc/recursoshumanosweb.env (chmod 600)
    setup_app_service       # Paso 9: servicio systemd para Kestrel
    configure_nginx         # Paso 10: reverse proxy Nginx → Kestrel

    print_summary
}

main "$@"
