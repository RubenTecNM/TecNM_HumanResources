#!/usr/bin/env bash
# ============================================================
# Suite HTTP repetible — itsmRH GestionPermisos
# Fase 6: Verificación de endpoints (13 casos)
# Entorno: desarrollo :5270 (Development + BD DB_Development_HumanResources)
# Uso: API_KEY=<tu_key> CUTOFF_ID=<id_open> PERM_ID=<id_con_pdf> USER_ID=<id_usuario> bash verificar_endpoints.sh
# ============================================================

BASE="http://localhost:5270"
API_KEY="${API_KEY:-}"
CUTOFF_ID="${CUTOFF_ID:-1}"
PERM_ID="${PERM_ID:-1}"
USER_ID="${USER_ID:-2}"

PASS=0
FAIL=0
RESULTS=()

# ── Utilidades ────────────────────────────────────────────────────────────────

check() {
  local no="$1" label="$2" expected="$3" actual="$4"
  if [[ "$actual" == "$expected" ]]; then
    PASS=$((PASS+1))
    RESULTS+=("  [PASS] #${no} ${label} → ${actual}")
  else
    FAIL=$((FAIL+1))
    RESULTS+=("  [FAIL] #${no} ${label} → esperado ${expected}, obtenido ${actual}")
  fi
}

key_header() {
  if [[ -n "$API_KEY" ]]; then
    echo "-H \"X-API-KEY: ${API_KEY}\""
  fi
}

# ── Caso 1: Health ────────────────────────────────────────────────────────────
STATUS=$(curl -s -o /dev/null -w "%{http_code}" "${BASE}/health")
check 1 "GET /health → 200" "200" "$STATUS"

# ── Caso 2: Stats con API Key ─────────────────────────────────────────────────
STATUS=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "X-API-KEY: ${API_KEY}" \
  "${BASE}/api/Permissions/stats")
check 2 "GET /api/Permissions/stats (key) → 200" "200" "$STATUS"

# ── Caso 3: Listado paginado de permisos ─────────────────────────────────────
STATUS=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "X-API-KEY: ${API_KEY}" \
  "${BASE}/api/Permissions?page=1&pageSize=5")
check 3 "GET /api/Permissions?page=1&pageSize=5 → 200" "200" "$STATUS"

# ── Caso 4: POST permiso como Autorizador → 403 ───────────────────────────────
# Nota: requiere que la sesión cookie corresponda a un usuario de tipo Autorizador.
# Sin cookie de sesión del autorizador, la API retorna 401 (no autenticado).
# El script registra el código real para revisión manual.
STATUS=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "X-API-KEY: ${API_KEY}" \
  -H "Content-Type: application/json" \
  -X POST "${BASE}/api/Permissions" \
  -d '{"reason":"test","startDate":"2026-08-01","endDate":"2026-08-02","permissionTypeId":1}')
echo "  [INFO] #4 POST /api/Permissions sin sesión → ${STATUS} (esperado 401 sin sesión, 403 con sesión autorizador)"

# ── Caso 5: Auto-aprobado sin goce (permiso fuera de corte) ──────────────────
echo "  [INFO] #5 POST permiso empleado con fecha excediendo corte → requiere sesión empleado activa. Verificar manualmente o usar integration test."

# ── Caso 6: POST corte con traslape → 400 ────────────────────────────────────
STATUS=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "X-API-KEY: ${API_KEY}" \
  -H "Content-Type: application/json" \
  -X POST "${BASE}/api/Cutoffs" \
  -d "{\"startDate\":\"2020-01-01\",\"endDate\":\"2020-01-15\",\"createdByUserId\":1}")
check 6 "POST /api/Cutoffs traslape conocido → 400 o 500" "400" "$STATUS"

# ── Caso 7: PUT close de corte ────────────────────────────────────────────────
STATUS=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "X-API-KEY: ${API_KEY}" \
  -X PUT "${BASE}/api/Cutoffs/${CUTOFF_ID}/close")
echo "  [INFO] #7 PUT /api/Cutoffs/${CUTOFF_ID}/close → ${STATUS} (esperado 200 si corte Open; 400/404 si ya cerrado)"

# ── Caso 8: Export excel/csv/json/xml ─────────────────────────────────────────
for fmt in excel csv json xml; do
  STATUS=$(curl -s -o /dev/null -w "%{http_code}" \
    -H "X-API-KEY: ${API_KEY}" \
    "${BASE}/api/Cutoffs/${CUTOFF_ID}/export?format=${fmt}")
  check 8 "GET /api/Cutoffs/${CUTOFF_ID}/export?format=${fmt} → 200" "200" "$STATUS"
done

# ── Caso 9: Evidencia PDF ─────────────────────────────────────────────────────
STATUS=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "X-API-KEY: ${API_KEY}" \
  "${BASE}/api/Permissions/${PERM_ID}/evidence")
echo "  [INFO] #9 GET /api/Permissions/${PERM_ID}/evidence → ${STATUS} (esperado 200 + application/pdf si existe evidencia)"

# ── Caso 10: Ciclo deactivate → activate ──────────────────────────────────────
D=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "X-API-KEY: ${API_KEY}" \
  -X PUT "${BASE}/api/Users/${USER_ID}/deactivate")
A=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "X-API-KEY: ${API_KEY}" \
  -X PUT "${BASE}/api/Users/${USER_ID}/activate")
echo "  [INFO] #10 Ciclo deactivate/activate usuario ${USER_ID} → deactivate:${D} activate:${A} (esperado 200/200)"

# ── Caso 11: GET /api/ApiKeysApi sin cookie → 401 ────────────────────────────
STATUS=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "X-API-KEY: ${API_KEY}" \
  "${BASE}/api/ApiKeysApi")
check 11 "GET /api/ApiKeysApi sin cookie admin → 401" "401" "$STATUS"

# ── Caso 12: PUT /api/ApiKeysApi/{id}/permissions sin sesión ──────────────────
STATUS=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "X-API-KEY: ${API_KEY}" \
  -H "Content-Type: application/json" \
  -X PUT "${BASE}/api/ApiKeysApi/1/permissions" \
  -d '{"permissions":[]}')
echo "  [INFO] #12 PUT /api/ApiKeysApi/1/permissions sin sesión → ${STATUS} (esperado 401)"

# ── Caso 13: Catálogos ────────────────────────────────────────────────────────
for ep in Areas Departments PermissionTypes; do
  STATUS=$(curl -s -o /dev/null -w "%{http_code}" \
    -H "X-API-KEY: ${API_KEY}" \
    "${BASE}/api/${ep}")
  check 13 "GET /api/${ep} → 200" "200" "$STATUS"
done

# ── Resumen ────────────────────────────────────────────────────────────────────
echo ""
echo "══════════════════════════════════════════════════════"
echo " Resultados suite HTTP — $(date '+%Y-%m-%d %H:%M:%S')"
echo "══════════════════════════════════════════════════════"
for r in "${RESULTS[@]}"; do echo "$r"; done
echo "──────────────────────────────────────────────────────"
echo " PASS: ${PASS}  |  FAIL: ${FAIL}"
echo "══════════════════════════════════════════════════════"

if [[ $FAIL -gt 0 ]]; then
  exit 1
fi
