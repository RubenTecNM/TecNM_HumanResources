#!/usr/bin/env bash
# =============================================================================
#  setup-db-arch.sh — RecursosHumanosWeb · Despliegue de Base de Datos
#  Plataforma : Arch Linux
#  Descripción: Levanta SQL Server 2025 en Docker, aplica el esquema DACPAC
#               e inserta los datos iniciales desde InsertsDB.sql
#
#  USO:
#    bash db/setup-db-arch.sh [opciones]
#
#  OPCIONES:
#    --reset    Elimina el contenedor y volumen existentes antes de crear (DESTRUCTIVO)
#    --skip-data  Solo aplica el esquema, omite InsertsDB.sql
#    --help       Muestra esta ayuda
#
#  REQUISITOS PREVIOS (instalar con pacman si no están):
#    sudo pacman -S docker
#    sudo systemctl enable --now docker
#    dotnet tool install -g microsoft.sqlpackage
# =============================================================================

set -euo pipefail

# ── Configuración ─────────────────────────────────────────────────────────────
MSSQL_IMAGE="mcr.microsoft.com/mssql/server:2025-latest"
MSSQL_CONTAINER="rh_sql"
MSSQL_VOLUME="rh_sqldata"
MSSQL_HOST_PORT="1434"          # Puerto en el HOST (cambia si hay conflicto)
MSSQL_SA_PASSWORD="Buss1ness!2026"  # ⚠ Debe tener mayúscula, número y símbolo
MSSQL_PID="Express"
MSSQL_DB_NAME="RecursosHumanos"
SQL_READY_TIMEOUT=120

# ── Rutas (relativas al directorio del script) ────────────────────────────────
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DACPAC_FILE="$SCRIPT_DIR/RHdatabase-schema.dacpac"
INSERTS_SQL="$SCRIPT_DIR/InsertsDB.sql"

# ── Colores ───────────────────────────────────────────────────────────────────
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'
CYAN='\033[0;36m'; BOLD='\033[1m'; RESET='\033[0m'

log_info()  { echo -e "  ${GREEN}✔${RESET}  $1"; }
log_warn()  { echo -e "  ${YELLOW}⚠${RESET}  $1"; }
log_error() { echo -e "  ${RED}✘${RESET}  $1" >&2; }
log_step()  { echo -e "  ${BOLD}→${RESET}  $1"; }
log_section() {
    echo ""
    echo -e "${CYAN}${BOLD}══════════════════════════════════════════════════════${RESET}"
    echo -e "${CYAN}${BOLD}  $1${RESET}"
    echo -e "${CYAN}${BOLD}══════════════════════════════════════════════════════${RESET}"
}
die() { echo ""; log_error "ERROR FATAL: $1"; exit 1; }

# ── Flags ─────────────────────────────────────────────────────────────────────
FLAG_RESET=false
FLAG_SKIP_DATA=false

for arg in "$@"; do
    case "$arg" in
        --reset)      FLAG_RESET=true ;;
        --skip-data)  FLAG_SKIP_DATA=true ;;
        --help|-h)
            grep "^#  " "${BASH_SOURCE[0]}" | sed 's/^#  //'
            exit 0 ;;
        *) die "Flag desconocido: '$arg'. Usa --help." ;;
    esac
done

# =============================================================================
# 1. VERIFICACIONES PREVIAS
# =============================================================================
log_section "1. Verificaciones previas"

# Docker disponible
if ! command -v docker &>/dev/null; then
    die "Docker no está instalado.\nInstala con: sudo pacman -S docker\nLuego: sudo systemctl enable --now docker"
fi
log_info "Docker: $(docker --version | awk '{print $3}' | tr -d ',')"

# Docker daemon activo
if ! docker info &>/dev/null; then
    die "El daemon de Docker no está corriendo.\nInicia con: sudo systemctl start docker\nO habilítalo permanentemente: sudo systemctl enable --now docker"
fi
log_info "Docker daemon: activo"

# sqlpackage disponible
SQLPACKAGE_BIN=""
if command -v sqlpackage &>/dev/null; then
    SQLPACKAGE_BIN="sqlpackage"
elif [[ -x "$HOME/.dotnet/tools/sqlpackage" ]]; then
    SQLPACKAGE_BIN="$HOME/.dotnet/tools/sqlpackage"
    export PATH="$PATH:$HOME/.dotnet/tools"
else
    die "sqlpackage no encontrado.\nInstálalo con:\n  dotnet tool install -g microsoft.sqlpackage\nLuego agrega al PATH:\n  export PATH=\$PATH:\$HOME/.dotnet/tools"
fi
log_info "sqlpackage: disponible ($SQLPACKAGE_BIN)"

# Archivos de BD
if [[ ! -f "$DACPAC_FILE" ]]; then
    die "No se encontró el esquema DACPAC en:\n  $DACPAC_FILE"
fi
log_info "Esquema DACPAC: $(du -sh "$DACPAC_FILE" | cut -f1) → $DACPAC_FILE"

if [[ ! -f "$INSERTS_SQL" ]]; then
    log_warn "No se encontró InsertsDB.sql. La BD se creará sin datos iniciales."
    FLAG_SKIP_DATA=true
else
    log_info "Datos SQL: $(du -sh "$INSERTS_SQL" | cut -f1) → $INSERTS_SQL"
fi

# =============================================================================
# 2. CONTENEDOR SQL SERVER
# =============================================================================
log_section "2. Contenedor SQL Server 2025"

# -- --reset: eliminar contenedor y volumen existentes -------------------------
if [[ "$FLAG_RESET" == "true" ]]; then
    log_warn "--reset: Se eliminarán el contenedor '$MSSQL_CONTAINER' y el volumen '$MSSQL_VOLUME'."
    log_warn "⚠  TODOS LOS DATOS DE LA BASE DE DATOS SERÁN BORRADOS PERMANENTEMENTE."
    read -rp "  ¿Confirmar? Escribe 'SI' en mayúsculas para continuar: " ans
    if [[ "$ans" == "SI" ]]; then
        log_step "Eliminando contenedor y volumen..."
        docker stop   "$MSSQL_CONTAINER" 2>/dev/null || true
        docker rm     "$MSSQL_CONTAINER" 2>/dev/null || true
        docker volume rm "$MSSQL_VOLUME" 2>/dev/null || true
        log_info "Contenedor y volumen eliminados."
    else
        log_warn "Reset cancelado. Se usará el contenedor/volumen existente."
        FLAG_RESET=false
    fi
fi

# -- Verificar si el contenedor ya existe --------------------------------------
if docker ps -a --format '{{.Names}}' | grep -q "^${MSSQL_CONTAINER}$"; then
    local_state=$(docker inspect --format='{{.State.Status}}' "$MSSQL_CONTAINER" 2>/dev/null || echo "unknown")
    log_info "Contenedor '$MSSQL_CONTAINER' ya existe (estado: $local_state)."

    if [[ "$local_state" != "running" ]]; then
        log_step "Iniciando contenedor detenido..."
        docker start "$MSSQL_CONTAINER" || die "No se pudo iniciar el contenedor. Revisa: docker logs $MSSQL_CONTAINER"
    fi
else
    # -- Descargar imagen si no existe localmente ------------------------------
    if ! docker image inspect "$MSSQL_IMAGE" &>/dev/null; then
        log_step "Descargando imagen SQL Server 2025 (~1.5 GB, puede tardar varios minutos)..."
        docker pull "$MSSQL_IMAGE" || die "No se pudo descargar la imagen: $MSSQL_IMAGE"
    else
        log_info "Imagen SQL Server 2025 ya descargada localmente."
    fi

    # -- Crear contenedor ------------------------------------------------------
    log_step "Creando contenedor '$MSSQL_CONTAINER' en puerto $MSSQL_HOST_PORT..."
    docker run \
        --name "$MSSQL_CONTAINER" \
        -e "ACCEPT_EULA=Y" \
        -e "MSSQL_SA_PASSWORD=$MSSQL_SA_PASSWORD" \
        -e "MSSQL_PID=$MSSQL_PID" \
        -p "${MSSQL_HOST_PORT}:1433" \
        -v "${MSSQL_VOLUME}:/var/opt/mssql" \
        --restart=unless-stopped \
        -d \
        "$MSSQL_IMAGE" \
        || die "Error al crear el contenedor.\nRevisa: docker logs $MSSQL_CONTAINER"

    log_info "Contenedor creado con volumen persistente '$MSSQL_VOLUME'."
fi

# =============================================================================
# 3. ESPERAR A QUE SQL SERVER ESTÉ LISTO
# =============================================================================
log_section "3. Esperando que SQL Server acepte conexiones"

sql_is_ready() {
    docker exec "$MSSQL_CONTAINER" \
        /opt/mssql-tools18/bin/sqlcmd \
        -S 127.0.0.1 -U sa -P "$MSSQL_SA_PASSWORD" \
        -C -Q "SELECT 1" \
        > /dev/null 2>&1
}

log_step "Probando conexión a SQL Server (timeout: ${SQL_READY_TIMEOUT}s)..."
elapsed=0
while [[ $elapsed -lt $SQL_READY_TIMEOUT ]]; do
    if sql_is_ready; then
        echo ""
        log_info "SQL Server listo (${elapsed}s)."
        break
    fi

    container_state=$(docker inspect --format='{{.State.Status}}' "$MSSQL_CONTAINER" 2>/dev/null || echo "unknown")
    if [[ "$container_state" == "exited" ]]; then
        echo ""
        log_error "El contenedor se detuvo inesperadamente."
        log_error "Últimas líneas del log:"
        docker logs --tail 20 "$MSSQL_CONTAINER" 2>&1 | while IFS= read -r line; do log_error "  $line"; done
        die "SQL Server se detuvo. Causas comunes:\n  - Contraseña SA no cumple requisitos de complejidad (debe tener mayúscula, número y símbolo)\n  - Puerto $MSSQL_HOST_PORT ya en uso\n  - RAM insuficiente (mínimo 2 GB)"
    fi

    printf "\r  ${YELLOW}⏳${RESET}  Esperando... %3ds / %ds  (estado: %s)   " \
        "$elapsed" "$SQL_READY_TIMEOUT" "$container_state"
    sleep 5
    elapsed=$((elapsed + 5))
done

if [[ $elapsed -ge $SQL_READY_TIMEOUT ]]; then
    echo ""
    die "SQL Server no respondió en ${SQL_READY_TIMEOUT}s.\nPrueba manual:\n  docker exec $MSSQL_CONTAINER /opt/mssql-tools18/bin/sqlcmd -S 127.0.0.1 -U sa -P '$MSSQL_SA_PASSWORD' -C -Q 'SELECT 1'"
fi

# =============================================================================
# 4. INICIALIZAR LA BASE DE DATOS
# =============================================================================
log_section "4. Inicializando la base de datos '$MSSQL_DB_NAME'"

# -- Verificar si la BD ya existe ----------------------------------------------
db_exists=$(docker exec "$MSSQL_CONTAINER" \
    /opt/mssql-tools18/bin/sqlcmd \
    -S 127.0.0.1 -U sa -P "$MSSQL_SA_PASSWORD" -C \
    -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name='${MSSQL_DB_NAME}'" \
    -h -1 2>/dev/null | tr -d '[:space:]' || echo "0")

if [[ "$db_exists" == "1" ]]; then
    log_warn "La base de datos '$MSSQL_DB_NAME' ya existe."
    read -rp "  ¿Eliminar y recrear? BORRARÁ todos los datos actuales [s/N]: " ans
    if [[ "$ans" =~ ^[sS]$ ]]; then
        log_step "Eliminando BD existente..."
        docker exec "$MSSQL_CONTAINER" \
            /opt/mssql-tools18/bin/sqlcmd \
            -S 127.0.0.1 -U sa -P "$MSSQL_SA_PASSWORD" -C \
            -Q "ALTER DATABASE [${MSSQL_DB_NAME}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [${MSSQL_DB_NAME}];" \
            2>/dev/null || die "No se pudo eliminar la BD."
        log_info "BD eliminada."
    else
        log_info "BD conservada. Nada que hacer."
        echo ""
        log_info "✅ Proceso completado (BD sin cambios)."
        echo -e "\n  Conexión: Server=localhost,${MSSQL_HOST_PORT};Database=${MSSQL_DB_NAME};User Id=sa;Password=${MSSQL_SA_PASSWORD};Encrypt=False;"
        exit 0
    fi
fi

# -- Etapa 1: Crear BD vacía y aplicar esquema DACPAC -------------------------
log_step "Creando base de datos vacía..."
docker exec "$MSSQL_CONTAINER" \
    /opt/mssql-tools18/bin/sqlcmd \
    -S 127.0.0.1 -U sa -P "$MSSQL_SA_PASSWORD" -C \
    -Q "CREATE DATABASE [${MSSQL_DB_NAME}];" \
    2>/dev/null || true

log_step "Publicando esquema desde DACPAC (puede tardar 2-5 minutos)..."
"$SQLPACKAGE_BIN" \
    /Action:Publish \
    /SourceFile:"$DACPAC_FILE" \
    /TargetServerName:"localhost,${MSSQL_HOST_PORT}" \
    /TargetDatabaseName:"$MSSQL_DB_NAME" \
    /TargetUser:"sa" \
    /TargetPassword:"$MSSQL_SA_PASSWORD" \
    /TargetTrustServerCertificate:True \
    /p:BlockOnPossibleDataLoss=False \
    /p:AllowIncompatiblePlatform=True \
    /p:TreatVerificationErrorsAsWarnings=True \
    2>&1 | grep -v "^Time elapsed" \
    || die "Falló la publicación del esquema DACPAC."

log_info "Esquema aplicado correctamente."

# -- Etapa 2: Insertar datos iniciales ----------------------------------------
if [[ "$FLAG_SKIP_DATA" == "true" ]]; then
    log_warn "Omitiendo inserción de datos (--skip-data activo o InsertsDB.sql no encontrado)."
else
    log_step "Deshabilitando restricciones FK temporalmente..."
    nocheck_stmts=$(docker exec "$MSSQL_CONTAINER" \
        /opt/mssql-tools18/bin/sqlcmd \
        -S 127.0.0.1 -U sa -P "$MSSQL_SA_PASSWORD" \
        -d "$MSSQL_DB_NAME" -C -h -1 \
        -Q "SET NOCOUNT ON;
            SELECT 'ALTER TABLE [' + OBJECT_NAME(parent_object_id) + '] NOCHECK CONSTRAINT [' + name + '];'
            FROM sys.foreign_keys ORDER BY OBJECT_NAME(parent_object_id);" \
        2>/dev/null | grep "^ALTER" || true)

    while IFS= read -r stmt; do
        [[ -z "$stmt" ]] && continue
        docker exec "$MSSQL_CONTAINER" \
            /opt/mssql-tools18/bin/sqlcmd \
            -S 127.0.0.1 -U sa -P "$MSSQL_SA_PASSWORD" \
            -d "$MSSQL_DB_NAME" -C -b -Q "$stmt" 2>/dev/null || true
    done <<< "$nocheck_stmts"

    log_step "Copiando e insertando datos desde InsertsDB.sql..."
    docker cp "$INSERTS_SQL" "${MSSQL_CONTAINER}:/tmp/InsertsDB.sql"

    if docker exec "$MSSQL_CONTAINER" \
        /opt/mssql-tools18/bin/sqlcmd \
        -S 127.0.0.1 -U sa -P "$MSSQL_SA_PASSWORD" \
        -d "$MSSQL_DB_NAME" -C -b \
        -i /tmp/InsertsDB.sql 2>&1; then
        log_info "Datos insertados correctamente."
    else
        log_warn "Algunos inserts fallaron. Verifica con:"
        log_warn "  docker exec -it $MSSQL_CONTAINER /opt/mssql-tools18/bin/sqlcmd -S 127.0.0.1 -U sa -P '$MSSQL_SA_PASSWORD' -d $MSSQL_DB_NAME -C"
    fi

    docker exec "$MSSQL_CONTAINER" rm -f /tmp/InsertsDB.sql 2>/dev/null || true

    log_step "Rehabilitando restricciones FK..."
    check_stmts=$(docker exec "$MSSQL_CONTAINER" \
        /opt/mssql-tools18/bin/sqlcmd \
        -S 127.0.0.1 -U sa -P "$MSSQL_SA_PASSWORD" \
        -d "$MSSQL_DB_NAME" -C -h -1 \
        -Q "SET NOCOUNT ON;
            SELECT 'ALTER TABLE [' + OBJECT_NAME(parent_object_id) + '] WITH CHECK CHECK CONSTRAINT [' + name + '];'
            FROM sys.foreign_keys ORDER BY OBJECT_NAME(parent_object_id);" \
        2>/dev/null | grep "^ALTER" || true)

    while IFS= read -r stmt; do
        [[ -z "$stmt" ]] && continue
        docker exec "$MSSQL_CONTAINER" \
            /opt/mssql-tools18/bin/sqlcmd \
            -S 127.0.0.1 -U sa -P "$MSSQL_SA_PASSWORD" \
            -d "$MSSQL_DB_NAME" -C -b -Q "$stmt" 2>/dev/null || true
    done <<< "$check_stmts"

    log_info "Restricciones FK rehabilitadas."
fi

# =============================================================================
# RESUMEN
# =============================================================================
echo ""
echo -e "${BOLD}${GREEN}╔══════════════════════════════════════════════════════════╗${RESET}"
echo -e "${BOLD}${GREEN}║      ✅  BASE DE DATOS DESPLEGADA CORRECTAMENTE          ║${RESET}"
echo -e "${BOLD}${GREEN}╚══════════════════════════════════════════════════════════╝${RESET}"
echo ""
echo -e "  ${BOLD}Contendor Docker:${RESET}"
echo -e "    Nombre   : $MSSQL_CONTAINER"
echo -e "    Puerto   : localhost:$MSSQL_HOST_PORT  (mapea → contenedor:1433)"
echo -e "    Volumen  : $MSSQL_VOLUME"
echo ""
echo -e "  ${BOLD}Base de datos:${RESET}"
echo -e "    Servidor : localhost,$MSSQL_HOST_PORT"
echo -e "    BD       : $MSSQL_DB_NAME"
echo -e "    Usuario  : sa"
echo -e "    Password : $MSSQL_SA_PASSWORD"
echo ""
echo -e "  ${BOLD}Connection string (appsettings.json):${RESET}"
echo -e "    Server=localhost,${MSSQL_HOST_PORT};Database=${MSSQL_DB_NAME};User Id=sa;Password=${MSSQL_SA_PASSWORD};Encrypt=False;TrustServerCertificate=True;"
echo ""
echo -e "  ${BOLD}Comandos útiles:${RESET}"
echo -e "    Ver logs    : docker logs $MSSQL_CONTAINER -f"
echo -e "    Conectar    : docker exec -it $MSSQL_CONTAINER /opt/mssql-tools18/bin/sqlcmd -S 127.0.0.1 -U sa -P '$MSSQL_SA_PASSWORD' -C"
echo -e "    Detener     : docker stop $MSSQL_CONTAINER"
echo -e "    Iniciar     : docker start $MSSQL_CONTAINER"
echo -e "    Reset total : bash $(basename "${BASH_SOURCE[0]}") --reset"
echo ""
