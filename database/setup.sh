#!/usr/bin/env bash
# ==============================================================================
# itsmRH — Script de Instalación y Despliegue Automatizado
# TecNM Campus Monclova © 2026
# ==============================================================================
# Este script configura el entorno completo: dependencias, .NET 10 SDK,
# Docker Engine, SQL Server 2025 y la aplicación itsmRH.
# Permite confirmación interactiva [s/N] en cada paso o ejecución desatendida (-y).
# ==============================================================================

set -o pipefail

# --- Colores y Estilos ---
BOLD="\e[1m"
GREEN="\e[32m"
YELLOW="\e[33m"
CYAN="\e[36m"
RED="\e[31m"
RESET="\e[0m"

# --- Configuración por Defecto ---
AUTO_YES=false
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

MSSQL_CONTAINER="${MSSQL_CONTAINER:-nexsmart_central_db_sql}"
MSSQL_PORT="${MSSQL_PORT:-1433}"
MSSQL_SA_PASSWORD="${MSSQL_SA_PASSWORD:-Buss1ness.2026}"
MSSQL_DB_NAME="${MSSQL_DB_NAME:-HumanResourcesProduction}"
APP_CONTAINER="${APP_CONTAINER:-itsmrh_gestion_permisos_api}"
APP_PORT="${APP_PORT:-5013}"
APP_IMAGE="${APP_IMAGE:-itsmrh_gestion_permisos_api:refactor-screaming}"

# Parsear argumentos
while [[ $# -gt 0 ]]; do
    case "$1" in
        -y|--yes)
            AUTO_YES=true
            shift
            ;;
        -h|--help)
            echo "Uso: sudo bash $0 [opciones]"
            echo "Opciones:"
            echo "  -y, --yes    Ejecutar todos los pasos sin solicitar confirmación interactiva"
            echo "  -h, --help   Mostrar esta ayuda"
            exit 0
            ;;
        *)
            echo -e "${RED}Opción desconocida: $1${RESET}"
            exit 1
            ;;
    esac
done

# --- Funciones Auxiliares ---
log_info()    { echo -e "${CYAN}${BOLD}[INFO]${RESET} $1"; }
log_success() { echo -e "${GREEN}${BOLD}[OK]${RESET} $1"; }
log_warn()    { echo -e "${YELLOW}${BOLD}[AVISO]${RESET} $1"; }
log_error()   { echo -e "${RED}${BOLD}[ERROR]${RESET} $1"; }

ask_confirm() {
    local prompt="$1"
    local default="${2:-n}"

    if [ "$AUTO_YES" = true ]; then
        return 0
    fi

    if [ "$default" = "s" ]; then
        prompt="$prompt [S/n]: "
    else
        prompt="$prompt [s/N]: "
    fi

    read -r -p "$(echo -e "${BOLD}${YELLOW}?${RESET} $prompt")" response
    response="${response,,}" # lowercase

    if [ -z "$response" ]; then
        response="$default"
    fi

    if [[ "$response" =~ ^(s|si|y|yes)$ ]]; then
        return 0
    else
        return 1
    fi
}

check_root() {
    if [ "$EUID" -ne 0 ]; then
        log_error "Este script requiere permisos de superusuario (root)."
        echo "Ejecute: sudo bash $0"
        exit 1
    fi
}

banner() {
    echo -e "${BOLD}${CYAN}"
    echo "=========================================================================="
    echo "   itsmRH — Instalación y Configuración del Entorno (TecNM Monclova)"
    echo "=========================================================================="
    echo -e "${RESET}"
    echo -e "Directorio del repositorio: ${BOLD}$REPO_DIR${RESET}"
    echo -e "Modo interactivo: ${BOLD}$([ "$AUTO_YES" = true ] && echo "Desactivado (-y)" || echo "Activado")${RESET}"
    echo ""
}

# --- Paso 1: Dependencias del Sistema ---
step_system_dependencies() {
    echo ""
    log_info "Paso 1: Dependencias del sistema (curl, wget, ca-certificates, libicu, libssl)"
    if ask_confirm "¿Desea actualizar paquetes e instalar dependencias básicas del sistema?" "s"; then
        apt-get update -y
        apt-get install -y ca-certificates curl wget gnupg lsb-release libicu-dev libssl-dev
        log_success "Dependencias del sistema instaladas."
    else
        log_warn "Omitiendo instalación de dependencias del sistema."
    fi
}

# --- Paso 2: .NET 10 SDK ---
step_dotnet() {
    echo ""
    log_info "Paso 2: .NET 10 SDK"
    local installed_ver
    installed_ver=$(dotnet --version 2>/dev/null || echo "")

    if [[ "$installed_ver" =~ ^10\. ]]; then
        log_success ".NET 10 SDK ya está instalado (versión: $installed_ver)."
        if ! ask_confirm "¿Desea reinstalar/actualizar .NET 10 SDK?" "n"; then
            return 0
        fi
    elif [ -n "$installed_ver" ]; then
        log_warn "Se detectó .NET $installed_ver, pero se requiere .NET 10.x LTS."
    fi

    if ask_confirm "¿Desea instalar .NET 10 SDK en /usr/share/dotnet?" "s"; then
        local temp_script="/tmp/dotnet-install.sh"
        wget -q https://dot.net/v1/dotnet-install.sh -O "$temp_script"
        chmod +x "$temp_script"
        mkdir -p /usr/share/dotnet
        "$temp_script" --channel 10.0 --install-dir /usr/share/dotnet
        rm -f "$temp_script"

        # Configurar PATH en profile.d
        cat << 'EOF' > /etc/profile.d/dotnet.sh
export DOTNET_ROOT=/usr/share/dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
EOF
        chmod +x /etc/profile.d/dotnet.sh
        export DOTNET_ROOT=/usr/share/dotnet
        export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools

        log_success ".NET 10 SDK instalado correctamente ($(dotnet --version 2>/dev/null || echo 'requiere nueva sesión')). "
    else
        log_warn "Omitiendo instalación de .NET 10 SDK."
    fi
}

# --- Paso 3: Docker Engine ---
step_docker() {
    echo ""
    log_info "Paso 3: Docker Engine"
    if command -v docker >/dev/null 2>&1; then
        local docker_ver
        docker_ver=$(docker --version)
        log_success "Docker ya está disponible: $docker_ver"
        if ! ask_confirm "¿Desea reinstalar o actualizar Docker CE?" "n"; then
            return 0
        fi
    fi

    if ask_confirm "¿Desea instalar Docker Engine oficial para Debian/Ubuntu?" "s"; then
        install -m 0755 -d /etc/apt/keyrings
        curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor --yes -o /etc/apt/keyrings/docker.gpg
        chmod a+r /etc/apt/keyrings/docker.gpg

        local codename
        codename=$(. /etc/os-release && echo "$VERSION_CODENAME")
        echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian $codename stable" \
            > /etc/apt/sources.list.d/docker.list

        apt-get update -y
        apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
        systemctl enable --now docker

        if [ -n "$SUDO_USER" ]; then
            usermod -aG docker "$SUDO_USER"
            log_info "Usuario $SUDO_USER agregado al grupo 'docker'."
        fi
        log_success "Docker Engine instalado y activo."
    else
        log_warn "Omitiendo instalación de Docker."
    fi
}

# --- Paso 4: Contenedor SQL Server 2025 Express ---
step_mssql_container() {
    echo ""
    log_info "Paso 4: Contenedor de Base de Datos (SQL Server 2025 Express)"
    if docker ps -a --format '{{.Names}}' | grep -Eq "^${MSSQL_CONTAINER}$"; then
        log_success "El contenedor '$MSSQL_CONTAINER' ya existe."
        if docker ps --format '{{.Names}}' | grep -Eq "^${MSSQL_CONTAINER}$"; then
            log_info "El contenedor '$MSSQL_CONTAINER' está actualmente corriendo."
        else
            log_warn "El contenedor '$MSSQL_CONTAINER' existe pero está detenido."
            if ask_confirm "¿Desea iniciar el contenedor '$MSSQL_CONTAINER'?" "s"; then
                docker start "$MSSQL_CONTAINER"
            fi
        fi
        return 0
    fi

    if ask_confirm "¿Desea desplegar un nuevo contenedor SQL Server 2025 Express con nombre '$MSSQL_CONTAINER' en puerto $MSSQL_PORT?" "s"; then
        docker run -d \
            --name "$MSSQL_CONTAINER" \
            --restart unless-stopped \
            -e 'ACCEPT_EULA=Y' \
            -e "MSSQL_SA_PASSWORD=$MSSQL_SA_PASSWORD" \
            -e 'MSSQL_PID=Express' \
            -p "${MSSQL_PORT}:1433" \
            mcr.microsoft.com/mssql/server:2025-latest

        log_success "Contenedor '$MSSQL_CONTAINER' iniciado en puerto $MSSQL_PORT."
    else
        log_warn "Omitiendo despliegue del contenedor SQL Server."
    fi
}

# --- Paso 5: Inicialización de la Base de Datos ---
step_init_database() {
    echo ""
    log_info "Paso 5: Inicialización de la Base de Datos (HumanResourcesProduction)"

    if ! docker ps --format '{{.Names}}' | grep -Eq "^${MSSQL_CONTAINER}$"; then
        log_warn "El contenedor '$MSSQL_CONTAINER' no está corriendo. Omitiendo inicialización SQL."
        return 0
    fi

    if ask_confirm "¿Desea verificar y aplicar los scripts SQL (schema_objects.sql e inserts.sql) en '$MSSQL_CONTAINER'?" "s"; then
        log_info "Esperando a que SQL Server responda..."
        local ready=false
        for i in {1..15}; do
            if docker exec "$MSSQL_CONTAINER" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" >/dev/null 2>&1; then
                ready=true
                break
            fi
            sleep 2
        done

        if [ "$ready" = false ]; then
            log_error "SQL Server no respondió a tiempo. Verifique credenciales o logs con: docker logs $MSSQL_CONTAINER"
            return 1
        fi

        log_success "Conexión a SQL Server exitosa."

        # Asegurar base de datos
        docker exec "$MSSQL_CONTAINER" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
            -Q "IF DB_ID('$MSSQL_DB_NAME') IS NULL CREATE DATABASE [$MSSQL_DB_NAME];"

        # Copiar y ejecutar inserts.sql si el usuario lo solicita
        if [ -f "$REPO_DIR/database/inserts.sql" ]; then
            if ask_confirm "¿Desea ejecutar 'database/inserts.sql' para poblar catálogos base?" "s"; then
                docker cp "$REPO_DIR/database/inserts.sql" "$MSSQL_CONTAINER:/tmp/inserts.sql"
                docker exec "$MSSQL_CONTAINER" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -d "$MSSQL_DB_NAME" -i /tmp/inserts.sql
                log_success "inserts.sql ejecutado correctamente."
            fi
        fi

        # Copiar y ejecutar schema_objects.sql si el usuario lo solicita
        if [ -f "$REPO_DIR/database/schema_objects.sql" ]; then
            if ask_confirm "¿Desea ejecutar 'database/schema_objects.sql' para crear SPs y funciones?" "s"; then
                docker cp "$REPO_DIR/database/schema_objects.sql" "$MSSQL_CONTAINER:/tmp/schema_objects.sql"
                docker exec "$MSSQL_CONTAINER" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -d "$MSSQL_DB_NAME" -i /tmp/schema_objects.sql
                log_success "schema_objects.sql ejecutado correctamente."
            fi
        fi
    else
        log_warn "Omitiendo inicialización de la base de datos."
    fi
}

# --- Paso 6: Build y Despliegue de la Aplicación ---
step_deploy_app() {
    echo ""
    log_info "Paso 6: Construcción y Despliegue de la Aplicación (Docker)"

    if [ ! -f "$REPO_DIR/docker/Dockerfile" ]; then
        log_warn "No se encontró '$REPO_DIR/docker/Dockerfile'. Omitiendo build de la app."
        return 0
    fi

    if ask_confirm "¿Desea compilar la imagen Docker '$APP_IMAGE' (incluyendo tests de frontend)?" "s"; then
        log_info "Iniciando docker build (esto puede tomar 2-4 minutos)..."
        docker build \
            -t "$APP_IMAGE" \
            -f "$REPO_DIR/docker/Dockerfile" \
            "$REPO_DIR" \
            --build-arg RUN_FRONTEND_TESTS=true

        log_success "Imagen Docker construida exitosamente."

        if ask_confirm "¿Desea desplegar o reiniciar el contenedor '$APP_CONTAINER' en puerto $APP_PORT?" "s"; then
            if docker ps -a --format '{{.Names}}' | grep -Eq "^${APP_CONTAINER}$"; then
                log_info "Deteniendo contenedor anterior '$APP_CONTAINER'..."
                docker stop "$APP_CONTAINER" >/dev/null 2>&1 || true
                docker rm "$APP_CONTAINER" >/dev/null 2>&1 || true
            fi

            docker run -d \
                --name "$APP_CONTAINER" \
                --restart unless-stopped \
                -p "${APP_PORT}:8080" \
                "$APP_IMAGE"

            sleep 3
            if docker ps --format '{{.Names}}' | grep -Eq "^${APP_CONTAINER}$"; then
                log_success "Contenedor '$APP_CONTAINER' ejecutándose en http://localhost:$APP_PORT"
            else
                log_error "El contenedor no pudo iniciar. Revise con: docker logs $APP_CONTAINER"
            fi
        fi
    else
        log_warn "Omitiendo build y despliegue del contenedor de la aplicación."
    fi
}

# --- Resumen Final ---
summary() {
    echo ""
    echo -e "${BOLD}${GREEN}=========================================================================="
    echo "                      INSTALACIÓN FINALIZADA"
    echo -e "==========================================================================${RESET}"
    echo ""
    echo -e "  ${BOLD}Resumen de Servicios:${RESET}"
    echo -e "    • Motor SQL Server : Contenedor '${MSSQL_CONTAINER}' (Puerto ${MSSQL_PORT})"
    echo -e "    • Base de Datos    : ${MSSQL_DB_NAME}"
    echo -e "    • App itsmRH       : Contenedor '${APP_CONTAINER}' (Puerto ${APP_PORT})"
    echo ""
    echo -e "  ${BOLD}Comandos de Diagnóstico Útiles:${RESET}"
    echo -e "    • Logs de la App   : docker logs -f ${APP_CONTAINER}"
    echo -e "    • Logs de SQL      : docker logs -f ${MSSQL_CONTAINER}"
    echo -e "    • Probar conexión  : curl -I http://localhost:${APP_PORT}"
    echo ""
}

# --- Main ---
main() {
    check_root
    banner
    step_system_dependencies
    step_dotnet
    step_docker
    step_mssql_container
    step_init_database
    step_deploy_app
    summary
}

main "$@"
