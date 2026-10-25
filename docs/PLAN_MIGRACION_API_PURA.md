# Plan de Implementación: Backend API Pura + Screaming Architecture

**Proyecto:** itsmRH_GestionPermisos (TecRHPermisos)
**Rama:** refactor/screaming
**Fecha:** 2026-10-24
**Estado de partida:** Build 0 errores, 45/45 tests pasando
**Tag de seguridad:** safety/pre-api-pure-20261024

---

## Objetivo

Eliminar las vistas Razor/MVC y dejar el backend como API pura. El frontend Vue (ya migrado a src/features/) sirve como SPA unica consumiendo exclusivamente endpoints JSON. La logica de negocio queda centralizada en C# (backend), nunca en JavaScript.

---

## Arquitectura objetivo

Backend (API pura - ASP.NET Core 10)
├── Features/
│   ├── Permissions/          → PermissionsApiController (JSON)
│   ├── Users/                → UsersApiController (JSON)
│   ├── Cutoffs/              → CutoffsApiController (JSON)
│   ├── Areas/, Positions/, ...
│   └── Shared/Web/           → BaseController, filtros comunes
├── Shared/
│   ├── Domain/               → Entidades (BaseEntity, User, Permission...)
│   ├── Data/                 → HumanResourcesContext
│   ├── Common/               → Utilidades transversales
│   └── Notifications/
└── wwwroot/                  → Frontend Vue compilado (estaticos)

Frontend (Vue 3 + TypeScript SPA)
├── src/features/*/views/     → Pantallas (reemplazan Razor Views)
├── src/features/*/api/       → Llamadas HTTP al backend
├── src/shared/               → Componentes, composables, http client
└── src/app/                  → Router, App.vue, layouts

---

## Checklist de la Fase 0: Inventario y linea base

| # | Tarea | Comando / Accion | Verificacion | Estado |
| :-- | :-- | :-- | :-- | :-- |
| 0.1 | Commit actual en refactor/screaming | git add -A && git commit | git status limpio | ✅ HECHO |
| 0.2 | Tag de seguridad | git tag safety/pre-api-pure-20261024 | git tag muestra el tag | ✅ HECHO |
| 0.3 | Build base | dotnet build HumanResourcesProduction.sln -c Release --no-restore | 0 errores | ✅ HECHO |
| 0.4 | Tests base | dotnet test HumanResourcesProduction.sln -c Release --no-restore --no-build | 45/45 correctas | ✅ HECHO |
| 0.5 | Inventario de vistas Razor | find HumanResourcesProduction/Views -name "*.cshtml" | Lista obtenida | ⏳ PENDIENTE |
| 0.6 | Identificar vistas reemplazadas por Vue | Comparar con src/features/*/views/*.vue | Mapa Razor ↔ Vue | ⏳ PENDIENTE |
| 0.7 | Build Docker local | docker build -t hr-api-pure-test . | Imagen construye | ⏳ PENDIENTE |

---

## Notas relevantes de la Fase 0

### Cambios commiteados
- Todos los controladores movidos de Controllers/ a Features/ por dominio (25 controladores).
- Frontend reestructurado: src/features/ con 9 dominios, src/shared/ con componentes/composables/api compartidos.
- Se elimino la carpeta Controllers/ del host.
- Se creo docs/ARCHITECTURE_SCREAMING.md con las reglas de la arquitectura.

### Estado del servidor
- No se toco ningún otro proyecto ni servicio del servidor.
- Solo se modifico /root/itsmRH_GestionPermisos.
- rh-sql.service sigue con problemas de contenedor inexistente (preexistente, no relacionado).
- tunnel-opnsense.service activo (no tocado).

### Tag de seguridad
- safety/pre-api-pure-20261024 permite volver al estado actual si la migracion a API pura falla.

---

## Siguientes fases (planificado, no ejecutado aún)

### Fase 1: Endpoints de configuración (DÍA 2-3)
- Mover PermissionTypeCardConfigs de static en PermissionController a servicio dedicado.
- Crear GET /api/permissions/create-config que devuelva tipos filtrados por usuario, corte activo, canChooseRequester.
- Frontend consume este endpoint y elimina TEACHER_ABSENCE_TYPE_ID = 5 hardcodeado.

### Fase 2: Endpoints de edición y autorización (DÍA 5-6)
- GET /api/permissions/{id}/edit-config
- GET /api/permissions/{id}/authorize-config

### Fase 3: Eliminar vistas Razor y controladores MVC (DÍA 7-8)
- Borrar Views/Permission/*.cshtml y vistas obsoletas.
- Eliminar PermissionController (MVC), HomeController, TestController.
- Cambiar AddControllersWithViews() → AddControllers() en Program.cs.

### Fase 4: Validación de integración (DÍA 9)
- Build Docker, ejecutir contenedor, probar endpoints y flujo end-to-end.

---

## Reglas de seguridad durante la operación

| Check | Frecuencia | Accion si falla |
| :-- | :-- | :-- |
| dotnet build sin errores | Después de CADA cambio | Revertir último cambio |
| dotnet test 45+ verdes | Después de CADA fase | No avanzar |
| Rollback posible | Siempre | git checkout safety/pre-api-pure-20261024 |

---

## Rollback de emergencia


