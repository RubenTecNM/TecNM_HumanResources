# Plan de Implementación: Backend API Pura + Screaming Architecture

> [!NOTE]
> **Verificado y revisado por antigravity model**  
> Fecha de revisión y cierre de Fase 5: 2026-10-25  
> Estado de verificación: Migración a API Pura 100% completada. Build 0 errores, 53/53 tests unitarios pasando, imagen Docker construida con éxito, 10/10 pruebas de integración E2E superadas en contenedor real.

**Proyecto:** itsmRH_GestionPermisos (TecRHPermisos)  
**Rama:** `refactor/screaming`  
**Fecha:** 2026-10-24 (Actualizado 2026-10-25)  
**Estado actual:** Migración completada. Build 0 errores, 53/53 tests pasando, Docker validado  
**Tag de seguridad:** `safety/pre-api-pure-20261024`

---

## Objetivo

Eliminar las vistas Razor/MVC y dejar el backend como API pura. El frontend Vue (ubicado en `Frontend/src/features/`) sirve como SPA única consumiendo exclusivamente endpoints JSON. La lógica de negocio queda centralizada en C# (backend), nunca en JavaScript.

---

## Arquitectura objetivo

```
Backend (API pura - ASP.NET Core 10)
├── Features/
│   ├── Permissions/          → PermissionsApiController (JSON)
│   ├── Users/                → UsersApiController (JSON)
│   ├── Cutoffs/              → CutoffsApiController (JSON)
│   ├── Areas/, Positions/, ...
│   └── Shared/Web/           → TestController, filtros comunes
├── Shared/
│   ├── Domain/               → Entidades (BaseEntity, User, Permission...)
│   ├── Data/                 → HumanResourcesContext
│   ├── Common/               → Utilidades transversales
│   └── Notifications/
└── wwwroot/                  → Frontend Vue compilado (estáticos)

Frontend (Vue 3 + TypeScript SPA)
├── src/features/*/views/     → Pantallas (reemplazan Razor Views)
├── src/features/*/api/       → Llamadas HTTP al backend
├── src/shared/               → Componentes, composables, http client
└── src/app/                  → Router, App.vue, layouts
```

---

## Estado de las Fases Ejecutadas

### Fase 0: Inventario y línea base (COMPLETADO)

| # | Tarea | Verificación | Estado |
| :-- | :-- | :-- | :-- |
| 0.1 | Commit actual en refactor/screaming | git status limpio | ✅ HECHO |
| 0.2 | Tag de seguridad | git tag muestra el tag (`safety/pre-api-pure-20261024`) | ✅ HECHO |
| 0.3 | Build base | 0 errores | ✅ HECHO |
| 0.4 | Tests base | 45/45 correctas | ✅ HECHO |
| 0.5 | Documentación del plan | Commiteado en docs/ | ✅ HECHO |

---

### Fase 1: Endpoint create-config (COMPLETADO)

| # | Tarea | Verificación | Estado |
| :-- | :-- | :-- | :-- |
| 1.1 | Mover `PermissionTypeCardConfigs` de static en controller a servicio dedicado (`Core/Permissions/PermissionTypeConfig.cs`) | Ya no es static en controller MVC | ✅ HECHO |
| 1.2 | Endpoint `GET /api/permissions/create-config` | Devuelve JSON con tipos filtrados por usuario, corte activo, `canChooseRequester` | ✅ HECHO |
| 1.3 | Test unitario de `GetCreateConfigurationAsync` | Verifica que tipo 5 solo aparece si es docente y restringe solicitante | ✅ HECHO (cubierto en tests) |
| 1.4 | DTO `PermissionCreateConfigDto` | DTO limpio en `Core/DTOs` | ✅ HECHO |
| 1.5 | Build + tests | 0 errores, tests verdes | ✅ HECHO |
| 1.6 | Commit fase 1 | Commits `7c8148542` y `5aad1754e` | ✅ HECHO |

---

### Fase 2: Frontend consume create-config (COMPLETADO)

| # | Tarea | Archivos | Verificación | Estado |
| :-- | :-- | :-- | :-- | :-- |
| 2.1 | `PermissionCreateView.vue` llama a `create-config` | `Frontend/src/features/permissions/views/PermissionCreateView.vue` | Al montarse, obtiene tipos y corte del backend | ✅ HECHO |
| 2.2 | Eliminar `TEACHER_ABSENCE_TYPE_ID = 5` hardcodeado | `PermissionCreateView.vue` | Ya no filtra por isDocente en JS | ✅ HECHO |
| 2.3 | Eliminar `defaultTypeConfig` hardcodeado | `PermissionCreateView.vue` | Configuración viene del backend | ✅ HECHO |
| 2.4 | Eliminar `canChooseRequester` calculado con `authStore` | `PermissionCreateView.vue` | El backend lo indica en el response | ✅ HECHO |
| 2.5 | Tipos y API en frontend | `Frontend/src/features/permissions/api/permissions.ts` | Definición de `PermissionCreateConfigDto` y `fetchPermissionConfig` | ✅ HECHO |
| 2.6 | Commits fase 2 | `git log` | Commits `3dbbb4195` y `8bb67d4cd` | ✅ HECHO |

---

### Fase 3: Endpoints y Frontend de Edición y Autorización (COMPLETADO)

| # | Tarea | Verificación / Archivos | Estado |
| :-- | :-- | :-- | :-- |
| 3.1 | Endpoint `GET /api/permissions/{id}/edit-config` | Devuelve `canEdit`, `blockReason`, campos habilitados y catálogo de tipos | ✅ HECHO (`2af05e099`) |
| 3.2 | Endpoint `GET /api/permissions/{id}/authorize-config` | Devuelve `canAuthorize`, `blockReason`, `canChangeWithPay` y opciones de goce | ✅ HECHO (`2af05e099`) |
| 3.3 | DTOs backend de Fase 3 | `PermissionEditConfigDto.cs` y `PermissionAuthorizeConfigDto.cs` | ✅ HECHO (`2af05e099`) |
| 3.4 | Cliente API Frontend | `fetchEditConfig` y `fetchAuthorizeConfig` en `Frontend/.../api/permissions.ts` | ✅ HECHO (revisado y verificado) |
| 3.5 | `PermissionEditView.vue` consume `edit-config` | Valida `canEdit` del backend, bloquea edición si está cerrado o no es pendiente, configura tipos dinámicamente | ✅ HECHO |
| 3.6 | `PermissionAuthorizeView.vue` consume `authorize-config` | Valida `canAuthorize`, bloquea radio de `withPay` si no es administrador, opciones dinámicas | ✅ HECHO |
| 3.7 | Tests unitarios Fase 3 | Cobertura en `PermissionServiceTests.cs` (8 nuevos tests de edit, authorize y create config) | ✅ HECHO (53/53 pasando) |
| 3.8 | Verificación y Commit Fase 3 | Build 0 errores, suite completa 53/53 superada | ✅ HECHO |

---

### Fase 4: Eliminar vistas Razor y controladores MVC obsoletos (COMPLETADO)

| # | Tarea | Verificación | Estado |
| :-- | :-- | :-- | :-- |
| 4.1 | Listar vistas Razor ya reemplazadas por Vue | Confirmado: SPA Vue cubre flujos de negocio y autenticación | ✅ HECHO |
| 4.2 | Eliminar vistas Razor obsoletas | Directorio completo `HumanResourcesProduction/Views` eliminado | ✅ HECHO |
| 4.3 | Eliminar `PermissionController` (MVC) legado | Eliminado; `PermissionsApiController` maneja el 100% de la lógica de permisos | ✅ HECHO |
| 4.4 | Eliminar otros controladores MVC obsoletos | Eliminados: `ApiKeyController`, `AuditLogController`, `ResetPasswordController`, `CutoffController`, `DepartmentsController`, `HomeController`, `BaseController`, `UserController`. `AccountController` migrado a `ControllerBase` (`[ApiController]`) con respuestas JSON | ✅ HECHO |
| 4.5 | Cambiar `AddControllersWithViews` → `AddControllers` | `Program.cs` migrado a `AddControllers()`, `app.MapControllers()`; eliminado filtro `AuthenticationDataAttribute` | ✅ HECHO |
| 4.6 | Verificar `app.MapStaticFiles()` / SPA fallback | Configurado `app.UseStaticFiles()` y `app.MapFallbackToFile("index.html")` | ✅ HECHO |
| 4.7 | Build + tests | 0 errores, 53/53 tests unitarios superados | ✅ HECHO |
| 4.8 | Commit fase 4 | Commit `ecf44bad0` en `refactor/screaming` | ✅ HECHO |

---

### Fase 5: Validación de integración completa y Docker (COMPLETADO)

| # | Tarea | Verificación | Estado |
| :-- | :-- | :-- | :-- |
| 5.1 | Build Docker | Multi-stage build (.NET 10 + Node 22 pnpm) exitoso con `.dockerignore` optimizado | ✅ HECHO |
| 5.2 | Ejecutar contenedor local | Contenedor levantado y conectado a red central `nexsmart_shared_network` y SQL Server | ✅ HECHO |
| 5.3 | Probar endpoints críticos | `/health`, `/Account/Login`, `/Account/Me`, `/api/Permissions/create-config`, `/api/Departments`, `/api/Users` probados y respondiendo JSON | ✅ HECHO |
| 5.4 | Probar frontend SPA | Vite SPA compilado, servido en `/` y fallback a rutas profundas (`/permissions/create`) funcionando | ✅ HECHO |
| 5.5 | Flujo de autenticación y antiforgery | Validación manual con `_antiforgery.ValidateRequestAsync()`, login exitoso y rechazo de credenciales inválidas | ✅ HECHO |
| 5.6 | Tests finales | 53/53 tests unitarios superados y 10/10 pruebas de integración E2E aprobadas | ✅ HECHO |
| 5.7 | Commit final | Rama limpia, lista para PR | ✅ HECHO |

---

## Reglas de seguridad durante la operación

| Check | Frecuencia | Acción si falla |
| :-- | :-- | :-- |
| dotnet build sin errores | Después de CADA cambio | Revertir último cambio |
| dotnet test verdes | Después de CADA fase | No avanzar |
| Endpoint devuelve JSON esperado | Después de crear cada endpoint | Corregir contrato o servicio |
| Frontend no tiene lógica de negocio duplicada | Después de Fase 2 y 3 | Mover lógica al backend |
| docker build exitoso | Fase 5 | Revisar Dockerfile y wwwroot |
| Rollback posible | Siempre | `git checkout safety/pre-api-pure-20261024` |

---

## Rollback de emergencia

```bash
git checkout safety/pre-api-pure-20261024
git checkout -b recovery/fix-issue
```
