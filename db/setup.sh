#!/usr/bin/env bash
# =============================================================================
#  setup.sh — TecRHPermisos · Script de Instalación Automatizada
#  Autor   : TecNM Campus Monclova
#  Versión : 1.0.0
#  Fecha   : 2026-03-19
#
#  DESCRIPCIÓN:
#    Instala y configura todo lo necesario para correr el sistema TecRHPermisos
#    en un servidor Debian/Ubuntu desde cero:
#      1. Dependencias del sistema
#      2. .NET 10 SDK (global, todos los usuarios)
#      3. Docker Engine
#      4. Contenedor SQL Server 2025 Express como demonio de systemd (root)
#      5. Inicialización de la base de datos (schema + inserts)
#
#  USO:
#    sudo bash setup.sh
#
#  REQUISITOS:
#    - Debian 11+ / Ubuntu 22.04+
#    - Acceso a internet
#    - Ejecutar como root o con sudo
# =============================================================================

set -euo pipefail

# ──────────────────────────────────────────────────────────────────────────────
# CONFIGURACIÓN — Editar si se desea personalizar
# ──────────────────────────────────────────────────────────────────────────────
DOTNET_CHANNEL="10.0"
DOTNET_INSTALL_DIR="/usr/share/dotnet"

MSSQL_IMAGE="mcr.microsoft.com/mssql/server:2025-latest"
MSSQL_CONTAINER="sql2025"
MSSQL_PORT="1433"
MSSQL_SA_PASSWORD="buss1ness!2026"   # Cambia esto en producción
MSSQL_PID="Express"

# Ruta al directorio db/ (donde están los .sql y este script)
DB_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SCHEMA_SQL="$DB_DIR/schemaDB.sql"
INSERTS_SQL="$DB_DIR/InsertsDB.sql"

# Tiempo máximo de espera para que SQL Server esté listo (segundos)
SQL_READY_TIMEOUT=90

# ──────────────────────────────────────────────────────────────────────────────
# UTILIDADES DE LOG
# ──────────────────────────────────────────────────────────────────────────────
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
BOLD='\033[1m'
RESET='\033[0m'

log_section() {
    echo ""
    echo -e "${CYAN}${BOLD}══════════════════════════════════════════════════════════${RESET}"
    echo -e "${CYAN}${BOLD}  $1${RESET}"
    echo -e "${CYAN}${BOLD}══════════════════════════════════════════════════════════${RESET}"
}

log_info()    { echo -e "  ${GREEN}✔${RESET}  $1"; }
log_warn()    { echo -e "  ${YELLOW}⚠${RESET}  $1"; }
log_error()   { echo -e "  ${RED}✘${RESET}  $1"; }
log_step()    { echo -e "  ${BOLD}→${RESET}  $1"; }
log_done()    { echo -e "\n${GREEN}${BOLD}  ✅  $1${RESET}\n"; }

# ──────────────────────────────────────────────────────────────────────────────
# VERIFICACIONES PREVIAS
# ──────────────────────────────────────────────────────────────────────────────
check_root() {
    if [[ "$EUID" -ne 0 ]]; then
        log_error "Este script debe ejecutarse como root o con sudo."
        echo "  Uso: sudo bash setup.sh"
        exit 1
    fi
}

check_os() {
    if ! (grep -qiE "debian|ubuntu" /etc/os-release 2>/dev/null); then
        log_warn "Sistema operativo no reconocido como Debian/Ubuntu. El script puede no funcionar correctamente."
    fi
}

check_internet() {
    log_step "Verificando conexión a internet..."
    if ! curl -sf --max-time 5 https://www.microsoft.com > /dev/null; then
        log_error "No hay conexión a internet o el servidor de Microsoft no es accesible."
        exit 1
    fi
    log_info "Conexión a internet: OK"
}

check_sql_files() {
    log_step "Verificando archivos SQL necesarios..."
    local missing=0
    if [[ ! -f "$SCHEMA_SQL" ]]; then
        log_error "No se encontró: $SCHEMA_SQL"
        missing=1
    fi
    if [[ ! -f "$INSERTS_SQL" ]]; then
        log_error "No se encontró: $INSERTS_SQL"
        missing=1
    fi
    if [[ "$missing" -eq 1 ]]; then
        log_error "Archivos SQL faltantes. Asegúrese de ejecutar el script desde dentro del directorio db/."
        exit 1
    fi
    log_info "Archivos SQL encontrados: schemaDB.sql, InsertsDB.sql"
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 1 — DEPENDENCIAS DEL SISTEMA
# ──────────────────────────────────────────────────────────────────────────────
install_system_deps() {
    log_section "PASO 1 — Instalando dependencias del sistema"

    log_step "Actualizando índice de paquetes APT..."
    apt-get update -qq

    log_step "Instalando paquetes base (curl, wget, gnupg, libicu, libssl, ca-certificates)..."
    apt-get install -y -qq \
        curl wget gnupg ca-certificates \
        libicu-dev libssl-dev \
        apt-transport-https software-properties-common \
        2>&1 | grep -E "^(Get|Inst|Conf|Err)" || true

    log_done "Dependencias del sistema instaladas."
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 2 — INSTALACIÓN DE .NET 10 SDK
# ──────────────────────────────────────────────────────────────────────────────
install_dotnet() {
    log_section "PASO 2 — Instalando .NET $DOTNET_CHANNEL SDK"

    if command -v dotnet &>/dev/null; then
        local installed_version
        installed_version=$(dotnet --version 2>/dev/null || echo "desconocida")
        log_warn ".NET ya está instalado (versión: $installed_version). Omitiendo reinstalación."
        return 0
    fi

    log_step "Descargando el instalador oficial de .NET desde Microsoft..."
    local tmp_installer
    tmp_installer=$(mktemp /tmp/dotnet-install.XXXXXX.sh)
    wget -q --show-progress -O "$tmp_installer" https://dot.net/v1/dotnet-install.sh
    chmod +x "$tmp_installer"

    log_step "Creando directorio de instalación global: $DOTNET_INSTALL_DIR"
    mkdir -p "$DOTNET_INSTALL_DIR"

    log_step "Instalando .NET $DOTNET_CHANNEL en $DOTNET_INSTALL_DIR (esto puede tomar varios minutos)..."
    "$tmp_installer" --channel "$DOTNET_CHANNEL" --install-dir "$DOTNET_INSTALL_DIR" 2>&1 | \
        grep -E "^(dotnet-install|Downloading|Extracting|Installing)" || true
    rm -f "$tmp_installer"

    log_step "Configurando variables de entorno globales en /etc/profile.d/dotnet.sh..."
    cat > /etc/profile.d/dotnet.sh << 'DOTNET_ENV'
# .NET SDK - Variables de entorno globales (TecRHPermisos)
export DOTNET_ROOT=/usr/share/dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
DOTNET_ENV
    chmod 644 /etc/profile.d/dotnet.sh

    # Cargar en la sesión actual
    export DOTNET_ROOT="$DOTNET_INSTALL_DIR"
    export PATH="$PATH:$DOTNET_INSTALL_DIR:$DOTNET_INSTALL_DIR/tools"

    local version
    version=$("$DOTNET_INSTALL_DIR/dotnet" --version 2>/dev/null || echo "instalado")
    log_done ".NET $version instalado globalmente."
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 3 — INSTALACIÓN DE DOCKER ENGINE
# ──────────────────────────────────────────────────────────────────────────────
install_docker() {
    log_section "PASO 3 — Instalando Docker Engine"

    if command -v docker &>/dev/null; then
        log_warn "Docker ya está instalado ($(docker --version)). Omitiendo reinstalación."
        return 0
    fi

    log_step "Importando clave GPG del repositorio oficial de Docker..."
    install -m 0755 -d /etc/apt/keyrings
    curl -fsSL https://download.docker.com/linux/debian/gpg | \
        gpg --dearmor -o /etc/apt/keyrings/docker.gpg 2>/dev/null || \
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | \
        gpg --dearmor -o /etc/apt/keyrings/docker.gpg
    chmod a+r /etc/apt/keyrings/docker.gpg

    log_step "Agregando repositorio oficial de Docker a APT..."
    local os_id arch codename
    os_id=$(. /etc/os-release && echo "${ID:-debian}")
    arch=$(dpkg --print-architecture)
    codename=$(. /etc/os-release && echo "${VERSION_CODENAME:-bookworm}")

    echo "deb [arch=${arch} signed-by=/etc/apt/keyrings/docker.gpg] \
https://download.docker.com/linux/${os_id} ${codename} stable" \
        > /etc/apt/sources.list.d/docker.list

    log_step "Actualizando APT e instalando Docker CE..."
    apt-get update -qq
    apt-get install -y -qq \
        docker-ce docker-ce-cli containerd.io \
        docker-buildx-plugin docker-compose-plugin \
        2>&1 | grep -E "^(Get|Inst|Conf|Err)" || true

    log_step "Habilitando y arrancando el servicio Docker..."
    systemctl enable docker --quiet
    systemctl start docker

    log_done "Docker $(docker --version | awk '{print $3}' | tr -d ',') instalado y activo."
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 4 — CONTENEDOR SQL SERVER 2025 COMO DEMONIO SYSTEMD (ROOT)
# ──────────────────────────────────────────────────────────────────────────────
setup_mssql_container() {
    log_section "PASO 4 — Configurando SQL Server 2025 como servicio systemd"

    # Descargar imagen primero
    log_step "Descargando imagen de SQL Server 2025: $MSSQL_IMAGE"
    log_info "  (Imagen ~1.5 GB — puede tardar varios minutos según la conexión)"
    docker pull "$MSSQL_IMAGE"

    # Eliminar contenedor previo si existe
    if docker ps -a --format '{{.Names}}' | grep -q "^${MSSQL_CONTAINER}$"; then
        log_warn "Contenedor '$MSSQL_CONTAINER' ya existe. Deteniéndolo y eliminándolo para recrear..."
        docker stop "$MSSQL_CONTAINER" 2>/dev/null || true
        docker rm   "$MSSQL_CONTAINER" 2>/dev/null || true
    fi

    log_step "Creando contenedor SQL Server '$MSSQL_CONTAINER' en puerto $MSSQL_PORT..."
    docker run \
        --name "$MSSQL_CONTAINER" \
        -e "ACCEPT_EULA=Y" \
        -e "MSSQL_SA_PASSWORD=$MSSQL_SA_PASSWORD" \
        -e "MSSQL_PID=$MSSQL_PID" \
        -p "${MSSQL_PORT}:1433" \
        --restart=always \
        -d \
        "$MSSQL_IMAGE"

    log_info "Contenedor '$MSSQL_CONTAINER' creado con --restart=always (se reinicia automáticamente con Docker)."

    # ── Crear unidad systemd que gestiona el contenedor como demonio de root ──
    log_step "Creando unidad systemd: /etc/systemd/system/sql2025.service"
    cat > /etc/systemd/system/sql2025.service << SYSTEMD_UNIT
[Unit]
Description=SQL Server 2025 Express — TecRHPermisos
Documentation=https://hub.docker.com/_/microsoft-mssql-server
After=docker.service network-online.target
Requires=docker.service
Wants=network-online.target

[Service]
Type=forking
# Arrancar el contenedor si está detenido; no crear uno nuevo
ExecStart=/usr/bin/docker start ${MSSQL_CONTAINER}
ExecStop=/usr/bin/docker stop -t 15 ${MSSQL_CONTAINER}
ExecReload=/usr/bin/docker restart ${MSSQL_CONTAINER}
# Reintentos automáticos en fallo
Restart=on-failure
RestartSec=10s
# Seguridad: el servicio corre como root (requiere acceso a Docker)
User=root
StandardOutput=journal
StandardError=journal
SyslogIdentifier=sql2025

[Install]
WantedBy=multi-user.target
SYSTEMD_UNIT

    log_step "Recargando systemd y habilitando servicio sql2025..."
    systemctl daemon-reload
    systemctl enable sql2025 --quiet
    systemctl start sql2025

    log_done "Servicio sql2025 registrado y activo vía systemd."
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 5 — ESPERAR A QUE SQL SERVER ESTÉ LISTO
# ──────────────────────────────────────────────────────────────────────────────
wait_for_mssql() {
    log_section "PASO 5 — Esperando que SQL Server esté disponible"

    log_step "Esperando que el motor SQL Server acepte conexiones (máx. ${SQL_READY_TIMEOUT}s)..."
    local elapsed=0
    local ready=false

    while [[ $elapsed -lt $SQL_READY_TIMEOUT ]]; do
        if docker logs "$MSSQL_CONTAINER" 2>&1 | grep -q "SQL Server is now ready for client connections"; then
            ready=true
            break
        fi
        sleep 3
        elapsed=$((elapsed + 3))
        echo -ne "  ${YELLOW}⏳${RESET}  Tiempo transcurrido: ${elapsed}s / ${SQL_READY_TIMEOUT}s...\r"
    done
    echo ""

    if [[ "$ready" == "true" ]]; then
        log_done "SQL Server está listo y aceptando conexiones."
    else
        log_error "SQL Server no respondió en ${SQL_READY_TIMEOUT}s. Verifique los logs:"
        log_error "  docker logs $MSSQL_CONTAINER"
        exit 1
    fi
}

# ──────────────────────────────────────────────────────────────────────────────
# PASO 6 — INICIALIZACIÓN DE LA BASE DE DATOS
# ──────────────────────────────────────────────────────────────────────────────
init_database() {
    log_section "PASO 6 — Inicializando la base de datos"

    local sqlcmd="docker exec -i $MSSQL_CONTAINER /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P \"$MSSQL_SA_PASSWORD\" -C -b"

    # ── Verificar si la BD ya existe ──
    log_step "Verificando si la base de datos ya existe..."
    local db_exists
    db_exists=$(docker exec "$MSSQL_CONTAINER" \
        /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q \
        "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = 'TecRHPermisos'" \
        -h -1 2>/dev/null | tr -d '[:space:]' || echo "0")

    if [[ "$db_exists" == "1" ]]; then
        log_warn "La base de datos 'TecRHPermisos' ya existe."
        read -rp "  ¿Deseas recrearla? Esto borrará todos los datos [s/N]: " confirm
        if [[ ! "$confirm" =~ ^[sS]$ ]]; then
            log_info "Inicialización omitida. Se conservan los datos existentes."
            return 0
        fi
        log_step "Eliminando base de datos existente..."
        docker exec "$MSSQL_CONTAINER" \
            /opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q \
            "ALTER DATABASE TecRHPermisos SET SINGLE_USER WITH ROLLBACK IMMEDIATE; \
             DROP DATABASE TecRHPermisos;" 2>/dev/null || true
    fi

    # ── Ejecutar schema ──
    log_step "Ejecutando: schemaDB.sql (estructura de tablas y relaciones)..."
    docker exec -i "$MSSQL_CONTAINER" \
        /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -b \
        < "$SCHEMA_SQL"
    log_info "schemaDB.sql ejecutado correctamente."

    # ── Ejecutar inserts ──
    log_step "Ejecutando: InsertsDB.sql (datos iniciales de catálogos y usuarios)..."
    docker exec -i "$MSSQL_CONTAINER" \
        /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -b \
        < "$INSERTS_SQL"
    log_info "InsertsDB.sql ejecutado correctamente."

    log_done "Base de datos 'TecRHPermisos' inicializada con éxito."
}

# ──────────────────────────────────────────────────────────────────────────────
# RESUMEN FINAL
# ──────────────────────────────────────────────────────────────────────────────
print_summary() {
    echo ""
    echo -e "${BOLD}${GREEN}╔══════════════════════════════════════════════════════════╗${RESET}"
    echo -e "${BOLD}${GREEN}║        ✅  INSTALACIÓN COMPLETADA EXITOSAMENTE           ║${RESET}"
    echo -e "${BOLD}${GREEN}╚══════════════════════════════════════════════════════════╝${RESET}"
    echo ""
    echo -e "  ${BOLD}Componentes instalados:${RESET}"
    echo -e "    • .NET SDK ${DOTNET_CHANNEL}  →  ${DOTNET_INSTALL_DIR}/dotnet"
    echo -e "    • Docker Engine          →  $(docker --version 2>/dev/null | head -1)"
    echo -e "    • SQL Server 2025        →  Contenedor '${MSSQL_CONTAINER}' en puerto ${MSSQL_PORT}"
    echo -e "    • Servicio systemd       →  sql2025.service (habilitado para root)"
    echo ""
    echo -e "  ${BOLD}Datos de conexión SQL Server:${RESET}"
    echo -e "    Servidor  : localhost,${MSSQL_PORT}"
    echo -e "    Usuario   : sa"
    echo -e "    Contraseña: ${MSSQL_SA_PASSWORD}"
    echo -e "    BD        : TecRHPermisos"
    echo ""
    echo -e "  ${BOLD}Comandos útiles:${RESET}"
    echo -e "    Verificar servicio : systemctl status sql2025"
    echo -e "    Ver logs SQL       : docker logs ${MSSQL_CONTAINER}"
    echo -e "    Conectar al motor  : docker exec -it ${MSSQL_CONTAINER} /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '${MSSQL_SA_PASSWORD}' -C"
    echo -e "    Reiniciar servicio : systemctl restart sql2025"
    echo ""
    echo -e "  ${YELLOW}⚠  Recuerda aplicar las variables de entorno de .NET en la sesión actual:${RESET}"
    echo -e "    ${BOLD}source /etc/profile.d/dotnet.sh${RESET}"
    echo ""
}

# ──────────────────────────────────────────────────────────────────────────────
# MAIN
# ──────────────────────────────────────────────────────────────────────────────
main() {
    echo ""
    echo -e "${BOLD}${CYAN}"
    echo "  ████████╗███████╗ ██████╗    ██████╗ ██╗  ██╗"
    echo "     ██╔══╝██╔════╝██╔════╝    ██╔══██╗██║  ██║"
    echo "     ██║   █████╗  ██║         ██████╔╝███████║"
    echo "     ██║   ██╔══╝  ██║         ██╔══██╗██╔══██║"
    echo "     ██║   ███████╗╚██████╗    ██║  ██║██║  ██║"
    echo "     ╚═╝   ╚══════╝ ╚═════╝    ╚═╝  ╚═╝╚═╝  ╚═╝"
    echo ""
    echo -e "  TecRHPermisos — Script de Instalación Automatizada v1.0${RESET}"
    echo -e "  TecNM Campus Monclova © 2026"
    echo ""

    check_root
    check_os
    check_internet
    check_sql_files

    install_system_deps
    install_dotnet
    install_docker
    setup_mssql_container
    wait_for_mssql
    init_database

    print_summary
}

main "$@"
