# Plan de Implementación — Cierre de Brechas Legacy → SPA

**Proyecto:** itsmRH_GestionPermisos (TecRHPermisos)
**Fecha:** 2026-08-18
**Objetivo:** Documentar la trazabilidad entre la versión deprecada (ASP.NET MVC) y la actual (API REST + SPA Vue 3), e identificar e implementar todo lo que falta para cerrar las brechas funcionales.

---

## 1. Inventario Legacy → API → Vue

### 1.1 Permisos (Permission)

| Funcionalidad legacy (MVC) | API actual (`/api/Permissions`) | SPA Vue | Estado |
|---|---|---|---|
| Index paginado con filtros (userName, areaId, fromDay, toDay, statusId) | GET lista paginada con filtros statusId/userName/fromDay/toDay | `PermissionsListView.vue` (filtros sin área) | ⚠️ Parcial |
| Visibilidad por rol (Admin/RH todo; Autorizador propios+área pendientes; Empleado solo suyos) | Sin scoping por rol | Listado role-aware solo en acciones | ❌ Falta en API |
| Crear permiso (tipos config-driven, fechas/horas, evidencia PDF, motivo) | POST multipart con confirmación de corte | `PermissionCreateView.vue` completa | ✅ Completo |
| Regla "autorizador no puede crear/editar" | No aplicada | UI lo bloquea parcialmente | ❌ Falta en API |
| Auto-aprobado SIN goce al exceder el corte | No re-evaluado (flujo inline de confirmación) | Confirma en 2 pasos | ❌ Falta en API |
| Editar permiso (solo Pending + corte Open) | PUT con ClearEvidence | `PermissionEditView.vue` completa | ✅ Completo |
| Autorizar (Admin/Autorizador, WithPay + comentario) | POST authorize (sin restricción de rol) | `PermissionAuthorizeView.vue` (solo aprobar) | ⚠️ Parcial |
| Rechazo manual | Endpoint acepta `approved=false` | No expuesto en UI | ❌ Falta en UI |
| Ver detalles (días, autorizador, comentario) | GET /{id} (sin datos de autorización) | `PermissionDetailsView.vue` (no muestra days/autorizedBy/comment) | ⚠️ Parcial |
| Ver/descargar evidencia (GetEvidence) | ❌ No existe | Sin descarga | ❌ Falta |
| Eliminar = invalidar (DeleteAjax con confirmación) | PUT /invalidate | `deactivatePermission` con confirmación | ✅ Completo |
| Stats/KPIs del panel (GetStats) | ❌ No existe | No existe | ❌ Falta |

### 1.2 Cortes de nómina (Cutoff)

| Funcionalidad legacy | API actual (`/api/Cutoffs`) | SPA Vue | Estado |
|---|---|---|---|
| Index paginado con filtros | GET lista paginada (dateFrom/dateTo/statusId) | `CutoffsListView.vue` | ✅ Completo |
| Crear periodo con validación (solapamiento, pendientes, cierre del anterior) | POST sin validaciones del servicio | `CutoffCreateView.vue` (Bootstrap) | ❌ Falta en API |
| Editar periodo | PUT sin validaciones | Sin vista de edición | ❌ Falta |
| Cerrar periodo (rechaza pendientes) | PUT /close sin rechazo de pendientes | `closeCutoff` | ❌ Falta en API |
| Invalidar periodo (cascada a permisos) | PUT /inactivate sin cascada | `inactivateCutoff` | ❌ Falta en API |
| Detalle con KPIs, gráficas y heatmap | GET analytics (sin heatmap) | `CutoffDetailsView.vue` | ⚠️ Parcial |
| Exportar Excel/CSV/JSON/XML | ❌ No existe | Links a `/Cutoff/*` (no proxados en Vite, falta XML) | ❌ Falta |
| Cierre automático (AutoCloseCutoffIfNeeded) | Worker `PeriodIntegrityWorker` | — | ✅ Completo |

### 1.3 Usuarios (User)

| Funcionalidad legacy | API actual (`/api/Users`) | SPA Vue | Estado |
|---|---|---|---|
| Index con filtros (nombre, correo, departamento, estado) y sort | GET con filtros name/email/departmentId/status (sin sort) | `UsersListView.vue` (sin departamento, userType crudo) | ⚠️ Parcial |
| Crear/editar con catálogos | POST/PUT | `UserFormView.vue` | ⚠️ Parcial (password vacío en PUT) |
| Detalle con estadísticas del usuario | GET /{id} (trae TotalPermissions etc.) | No hay vista de detalle (reusa edit) | ❌ Falta en UI |
| Eliminar (no auto-eliminación) | PUT /deactivate (sin guard) | Desactivar con confirmación | ❌ Falta en API |
| Reactivar | ❌ No existe | ❌ No existe | ⚠️ Opcional |
| Búsqueda AJAX de usuarios | GET /api/Users/search | `AppAutocomplete` | ✅ Completo |
| Validación duplicado de correo, guard de rol | ❌ No existe | — | ❌ Falta en API |

### 1.4 Departamentos (Department)

| Funcionalidad legacy | API actual | SPA Vue | Estado |
|---|---|---|---|
| CRUD con validación de duplicado | POST/PUT sin validación de duplicado | `DepartmentsListView.vue` | ❌ Falta en API |
| Toggle activo con confirmación | PUT toggle-active | Botón toggle (bug `isActive` vs `status`) | ⚠️ Bug |
| Paginación | Sin paginar | Sin paginar | ⚠️ Parcial |

### 1.5 API Keys

| Funcionalidad legacy | API actual | SPA Vue | Estado |
|---|---|---|---|
| Index con filtros (userName, fechas) | GET con filtro userName | `ApiKeysListView.vue` | ✅ Completo |
| Crear con reglas por tabla (checkboxes) | POST (UserId + Permissions) | `ApiKeyRulesEditor.vue` | ✅ Completo |
| Editar permisos por tabla | PUT `{id}/permissions` (reemplaza reglas) | Modal reglas + detalles | ✅ Completo |
| Mostrar key una sola vez + copiar | POST devuelve {Key} | Modal con copiar | ✅ Completo |
| Nota: la ruta correcta es `/api/ApiKeysApi` (token `[controller]`), no `/api/ApiKeys`. | | | |

### 1.6 Auditoría, Login, Reset Password

| Módulo | Estado |
|---|---|
| Auditoría (Index + Details) | ✅ Paridad total |
| Login con cookie + antiforgery | ✅ `LoginView.vue` + `stores/auth.ts` |
| Acceso denegado (403) | ✅ `ForbiddenView.vue` + ruta `/403` + guardas de rol por ruta |
| Reset password (3 pasos) | ✅ Completo |

### 1.7 CRUDs solo-API (sin UI en MVC ni SPA)

- Áreas (`/api/Areas`), Puestos (`/api/Positions`), Tipos de Usuario (`/api/UserTypes`), Tipos de Permiso (`/api/PermissionTypes`): endpoints completos, **sin vistas Vue** (nuevas).

---

## 2. Gap Analysis

### 2.1 Reglas de negocio que la API NO aplica (CRÍTICO — Fase 1)

| # | Brecha | Severidad | Archivos | Estado |
|---|---|---|---|---|
| 1.1 | `GET /api/Permissions` sin scoping por rol | Alta | `PermissionsApiController.cs`, `PermissionService.cs` | ✅ F1 |
| 1.2 | `GET/PUT /api/Permissions/{id}` sin `CanUserAccessPermiso` | Alta | `PermissionsApiController.cs` | ✅ F1 |
| 1.3 | POST/PUT no aplican "autorizador no crea" | Alta | `PermissionsApiController.cs` | ✅ F1 |
| 1.4 | POST/PUT no re-evalúan auto-aprobado sin goce al exceder corte | Alta | `PermissionsApiController.cs`, `PermissionService.cs` | ✅ F1 |
| 1.5 | `authorize` sin restricción de rol/Pending; `WithPay` no aplica `aprobado && withPay` | Alta | `PermissionsApiController.cs`, `PermissionService.cs` | ✅ F1 |
| 1.6 | `POST/PUT /api/Cutoffs` no usan `ICutoffService` (overlap, pendientes, cierre anterior, transacción) | Alta | `CutoffsApiController.cs`, `CutoffService.cs` | ✅ F1 |
| 1.7 | `/close` sin rechazo de pendientes; `/inactivate` sin cascada | Alta | `CutoffsApiController.cs`, `CutoffService.cs` | ✅ F1 |
| 1.8 | Users: duplicado de email, guard de rol, auto-desactivación | Media | `UsersApiController.cs`, `UserService.cs` | ✅ F1 (+`/{id}/activate`) |
| 1.9 | Departments: duplicado de descripción | Media | `DepartmentsApiController.cs` | ✅ F1 |

### 2.2 Endpoints faltantes (Fase 2)

| # | Endpoint | Reemplaza | Prioridad | Estado |
|---|---|---|---|---|
| 2.1 | `GET /api/Permissions/{id}/evidence` | `Permission/GetEvidence/{id}` | Alta | ✅ F2 |
| 2.2 | `GET /api/Permissions/stats` | `GetStats` (KPIs) | Media | ✅ F2 |
| 2.3 | `areaId` en `GET /api/Permissions` | Filtro Index legacy | Alta | ✅ F2 |
| 2.4 | `sortOrder` en `GET /api/Users` | Sort Index legacy | Baja | ✅ F2 |
| 2.5 | `GET /api/Cutoffs/{id}/export?format=` | 4 exportaciones | Alta | ✅ F2 |
| 2.6 | `heatmap` en analytics de corte | Heatmap Details legacy | Media | ✅ F2 |
| 2.7 | `PUT /api/ApiKeys/{id}/permissions` | Editar reglas MVC | Media | ✅ F2 (ruta real `/api/ApiKeysApi`) |

### 2.3 Gaps del Frontend (Fase 3)

| # | Vista | Cambio | Estado |
|---|---|---|---|
| 3.1 | `PermissionDetailsView` | Mostrar days/autorizedBy/authorizationComment; fix `canEdit` | ✅ |
| 3.2 | `PermissionAuthorizeView` | Flujo de rechazo; mover a `api/permissions.ts` | ✅ |
| 3.3 | `PermissionsListView` | Filtro por Área | ✅ |
| 3.4 | `UsersListView` | Filtro departamento, userType traducido, detalle propio, reactivar | ✅ |
| 3.5 | `CutoffsListView` | CUTOFF_STATUS, rango "X–Y de Z", rol en cerrar | ✅ |
| 3.6 | `CutoffDetailsView` | Export XML vía API, badge estado, heatmap | ✅ |
| 3.7 | `CutoffCreateView` | Estilos TecNM, manejo error suggested-dates | ✅ |
| 3.8 | `ApiKeysListView` | Reglas por tabla (`ApiKeyRulesEditor.vue`) | ✅ |
| 3.9 | `DepartmentsListView` | Fix bug `isActive` vs `status`, paginación | ✅ |
| 3.10 | Login | Pantalla 403 (`ForbiddenView.vue`) | ✅ |
| 3.11 | `api/*.ts` | Consolidar módulos por entidad, eliminar código muerto | ✅ |

---

## 3. Plan de Ejecución

### Fase 0 · Consolidación frontend (previa)
1. Crear `api/users.ts`, `api/departments.ts`, `api/apikeys.ts`, `api/audit.ts`.
2. Conectar funciones tipadas existentes sin uso (`fetchCutoffs`, `fetchAreas`, etc.).
3. Eliminar muertos: `PlaceholderView.vue`, `AppAlert.vue`, `apiGet/apiPost`.

### Fase 1 · Reglas de negocio backend (crítico)
- **Permisos:** scoping por rol en listado, `CanUserAccessPermiso` en GET/PUT, prohibir creación de autorizadores, invocar el flujo del servicio para auto-aprobado, endurecer `authorize`.
- **Cortes:** refactorizar `CutoffsApiController` para usar `ICutoffService` (crear, editar, cerrar, invalidar) con sus validaciones.
- **Usuarios/Departamentos:** duplicados, guard de rol, no auto-desactivación.

### Fase 2 · Endpoints faltantes
- Evidencia PDF, stats, areaId, exportaciones, heatmap, permisos de API keys.

### Fase 3 · Frontend
- Los cambios de la sección 2.3.

### Fase 4 · (Opcional) CRUDs nuevos en UI
- Áreas, Puestos, Tipos de Usuario, Tipos de Permiso.

### Fase 5 · Calidad
- Ampliar `RecursosHumanosWeb.Tests` (autorize, close cascada, evidencia, overlap).
- Verificación final: `dotnet build`, `vue-tsc --noEmit`, `vite build`.

## 3.1. Evidencia de verificación en vivo

- **Fase 1 (reglas)**: 0 errores de compilación; probado por HTTP en `localhost:5270`:
  - POST permiso por autorizador → 403; permiso de empleado sobre corte activo → `Pending`.
  - Permiso con `confirmed=true` que excede el corte → auto-aprobado `Approved` + `withPay=false` (permiso 100003).
  - Cierre de corte → pendientes rechazados en cascada (permisos 100002, 100004).
  - Traslape de periodos → 400; borrado lógico conservado.
- **Fase 2 (endpoints)**: `stats`, `evidence` (PDF real), `areaId`, `sortOrder`, `export` (excel/csv/json/xml), `heatmap` en analytics, `PUT /api/ApiKeysApi/{id}/permissions` verificados vía HTTP.
- **Fase 3 (frontend)**: `vue-tsc --noEmit` y `vite build` sin errores; smoke test en Vite `:5173` (index 200, proxy `/health` 200, `/api/Permissions/stats` 200, `/api/Cutoffs?page=1` 200, `/api/Cutoffs/{id}/export?format=csv` 200, `/api/ApiKeysApi` 401 sin cookie admin — comportamiento correcto).
- **Usuarios**: ciclo desactivar → activar verificado en el usuario 61 (estado restaurado).
- Ambiente: backend dev `:5270` (Development), producción `:5011` sin alterar; BD `DB_Production_HumanResources`.
- Quirks detectados: ruta de API Keys es `/api/ApiKeysApi`; redondeo de `DateTime` en SQL para cortes (usar día posterior al `EndDate` previo al crear periodos).

---

## 5. Plan de verificación de paridad y pruebas (Fase 6)

> Detalle completo en Obsidian vault → `itsmRH_GestionPermisos/plan/` (`01-paridad-vistas-legacy-vue.md`, `02-tests-backend-y-endpoints.md`, `03-ejecucion-y-acta.md`). Ubicación del vault: `/home/axel/Documents/Obsidian-Vault`.

### 5.1 Matriz de paridad Legacy MVC → SPA Vue (resumen)

| Módulo | Legacy | SPA Vue | Estado |
|---|---|---|---|
| Permisos | Index · Create · Edit · Details · Authorize · Delete | `PermissionsListView` · `Create` · `Edit` (incluye delete) · `Details` · `Authorize` | ✅ Completo |
| Cortes | Index · Create · **Edit** · Details · Close · Invalidate · Export×4 | `CutoffsListView` · `Create` · **`CutoffEditView`** (nuevo, F6) · `Details` (export + heatmap) | ✅ Completo tras F6 |
| Usuarios | Index · Create · Edit · **Details** · Search · Delete | `UsersListView` (filtro dept + reactivar) · `UserFormView` · **modal detalle** (nuevo, F6) | ✅ Completo tras F6 |
| Departamentos | Index · Create · Edit | `DepartmentsListView` (modal CRUD) | ✅ Completo |
| API Keys | Index · Create (reglas) · Edit · Details · Delete | `ApiKeysListView` + `ApiKeyRulesEditor` (ruta `/api/ApiKeysApi`) | ✅ Completo |
| Auditoría | Index · _Details | `AuditLogListView` + modal inspect | ✅ Completo |
| Cuenta | Login · AccessDenied · Reset×3 | `LoginView` · `ForbiddenView` (+guardas rol router) · `Forgot`/`Reset` | ✅ Completo |
| Home/Privacy | index → redirige a Permisos | — (sin equivalente necesario) | ✅ N/A |
| Limpieza | — | `AppAlert.vue`, `apiGet`/`apiPost` muertos | ✅ Limpiado en F6 |

### 5.2 Tests a añadir (`RecursosHumanosWeb.Tests`, xUnit, net10.0)

- **CutoffService**: traslape (3 condiciones), no cierra activo con pendientes, `CloseCutoffAsync` cascada sin goce, `InactivateCutoffAsync` cascada (status 3 + `Status=false`), normalización `EndDate` (`+1d -1tick`).
- **PermissionService**: `AuthorizePermissionAsync` aprueba con/sin goce, `CanUserAccessPermiso` (owner/admin/autorizador-área), scoping de autorizador en paged, excluye `InvalidatedByPeriod`, `UpdatePermissionAsync` solo Pending+Open.
- **UserService** (nuevo archivo): email duplicado create/update, no auto-desactivación, `CanUserManageUsers`.
- Ejecución: `dotnet test RecursosHumanosWeb.Tests` → 0 fallos.

### 5.3 Suite HTTP de endpoints (repetible)

- Script `docs/scripts/verificar_endpoints.sh` (bash + curl, `X-API-KEY`) contra `:5270`.
- Casos: health, stats, listados paginados, 403 autorizador crea, auto-aprobado sin goce, traslape cortes → 400, cierre cascada, export excel/csv/json/xml, evidencia PDF, ciclo activate/deactivate, reglas API key.

### 5.4 Cierre

- `dotnet build` (0 errores), `vue-tsc --noEmit` (0), `vite build`, smoke test Vite `:5173`.
- Resultados en `plan/03-ejecucion-y-acta.md` del vault.

---

## 4. Reglas de negocio de referencia (para implementación)

1. **Visibilidad de permisos** (`PermissionService.GetPermissionsPagedAsync`):
   - Admin/RH: todo.
   - Autorizador: propios (cualquier corte) O (misma Área Y corte activo Y Pending).
   - Empleado: solo los suyos.
   - Siempre excluye `InvalidatedByPeriod` (3).
2. **Crear/editar permiso**:
   - Corte activo (Open) obligatorio; `StartDate < cutoff.StartDate` → error.
   - `EndDate > cutoff.EndDate && !confirmed` → confirmación 2 pasos.
   - Si tras confirmar la fecha excede el corte → **auto-aprobado SIN goce** (Approved, WithPay=false, AuthorizedAt=now).
   - Autorizadores no pueden crear ni editar.
3. **Editar**: solo si Pending y corte Open; al editar re-evalúa la regla anterior.
4. **Autorizar**: solo Admin/Autorizador (Autorizador solo de su área); `WithPay = aprobado && withPay`; rechazo fuerza `WithPay=false` y status Rejected.
5. **Cortes**: no solapar periodos; al cerrar, rechazar pendientes (sin goce); al invalidar, cascada a permisos (status 3 + Status=false). No editar/cerrar si hay pendientes sin decidir.
6. **Borrado lógico** en todo el sistema (nunca DELETE físico).
