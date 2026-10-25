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

## Estado de la Fase 0 y Fase 1 (COMPLETADO)

### Fase 0: Inventario y línea base (COMPLETADO)

| # | Tarea | Verificación | Estado |
| :-- | :-- | :-- | :-- |
| 0.1 | Commit actual en refactor/screaming | git status limpio | ✅ HECHO |
| 0.2 | Tag de seguridad | git tag muestra el tag | ✅ HECHO |
| 0.3 | Build base | 0 errores | ✅ HECHO |
| 0.4 | Tests base | 45/45 correctas | ✅ HECHO |
| 0.5 | Inventario de vistas Razor | Lista obtenida | ⏳ PENDIENTE |
| 0.6 | Identificar vistas reemplazadas por Vue | Mapa Razor ↔ Vue | ⏳ PENDIENTE |
| 0.7 | Build Docker local | Imagen construye | ⏳ PENDIENTE |

### Fase 1: Endpoint create-config (COMPLETADO)

| # | Tarea | Verificación | Estado |
| :-- | :-- | :-- | :-- |
| 1.1 | Mover PermissionTypeCardConfigs de static en controller a servicio dedicado | Ya no es static en controller MVC | ✅ HECHO |
| 1.2 | Endpoint GET /api/permissions/create-config | Devuelve JSON con tipos filtrados por usuario, corte activo, canChooseRequester | ✅ HECHO |
| 1.3 | Test de GetCreateConfigurationAsync | Test verifica que tipo 5 solo aparece si es docente | ⏳ PENDIENTE (agregar test) |
| 1.4 | Test de integracion endpoint create-config | Llama al endpoint real, valida respuesta JSON | ⏳ PENDIENTE (agregar test) |
| 1.5 | Build + tests | 0 errores, tests verdes (incluidos nuevos) | ✅ HECHO |
| 1.6 | Commit fase 1 | Rama limpia | ✅ HECHO |

---

## Notas relevantes de las fases completadas

### Cambios commiteados en Fase 1
-  (nuevo): clases ,  y diccionario estático  movido del MVC controller
-  (nuevo): DTO de respuesta del endpoint create-config
-  (modificado): agregado 
-  (modificado): agregado , implementado  con filtrado de tipos por usuario (tipo 5 solo si es docente del departamento DOCENCIA)
-  (modificado): agregado endpoint 
-  (modificado): agregado  al crear  en tests
-  (modificado): documento actualizado

### Estado del servidor
- No se toco ningún otro proyecto ni servicio del servidor.
- Solo se modifico /root/itsmRH_GestionPermisos.
- rh-sql.service sigue con problemas de contenedor inexistente (preexistente, no relacionado).
- tunnel-opnsense.service activo (no tocado).

### Tag de seguridad
- safety/pre-api-pure-20261024 permite volver al estado actual si la migracion a API pura falla.

---

## Siguientes fases (planificado, no ejecutado aún)

### Fase 1 (pendiente): Tests adicionales para create-config
- Agregar test unitario para GetCreateConfigurationAsync (tipo 5 solo para docentes)
- Agregar test de integracion para el endpoint GET /api/permissions/create-config

### Fase 2: Frontend consume create-config (DÍA 3-4)
| # | Tarea | Archivos | Verificación | Estado |
| :-- | :-- | :-- | :-- | :-- |
| 2.1 | PermissionCreateView.vue llama a create-config | src/features/permissions/views/PermissionCreateView.vue | Al montarse, obtiene tipos del backend | ⏳ PENDIENTE |
| 2.2 | Eliminar TEACHER_ABSENCE_TYPE_ID = 5 hardcodeado | PermissionCreateView.vue | Ya no filtra por isDocente en JS | ⏳ PENDIENTE |
| 2.3 | Eliminar defaultTypeConfig hardcodeado | PermissionCreateView.vue | Configuracion viene del backend | ⏳ PENDIENTE |
| 2.4 | Eliminar canChooseRequester calculado con authStore | PermissionCreateView.vue | El backend lo dice en el response | ⏳ PENDIENTE |
| 2.5 | vue-tsc --noEmit (si hay node) | Frontend/ | Sin errores de tipos | ⏳ PENDIENTE |
| 2.6 | Build + tests backend | dotnet build + dotnet test | 0 errores, tests verdes | ✅ HECHO |
| 2.7 | Commit fase 2 | git commit | Rama limpia | ⏳ PENDIENTE |

### Fase 3: Endpoints de edición y autorización (DÍA 5-6)
| # | Tarea | Verificación | Estado |
| :-- | :-- | :-- | :-- |
| 3.1 | Endpoint GET /api/permissions/{id}/edit-config | Devuelve si se puede editar, mensaje de bloqueo, campos | ⏳ PENDIENTE |
| 3.2 | Test de edit-config | Valida respuestas para owner, admin, autorizador | ⏳ PENDIENTE |
| 3.3 | Endpoint GET /api/permissions/{id}/authorize-config | Devuelve canAuthorize, opciones de withPay | ⏳ PENDIENTE |
| 3.4 | Test de authorize-config | Valida reglas de autorizacion por rol | ⏳ PENDIENTE |
| 3.5 | PermissionEditView.vue consume edit-config | No calcula canEdit en JS | ⏳ PENDIENTE |
| 3.6 | PermissionAuthorizeView.vue consume authorize-config | No calcula permisos en JS | ⏳ PENDIENTE |
| 3.7 | Build + tests | 0 errores, tests verdes | ⏳ PENDIENTE |
| 3.8 | Commit fase 3 | Rama limpia | ⏳ PENDIENTE |

### Fase 4: Eliminar vistas Razor y controladores MVC (DÍA 7-8)
| # | Tarea | Verificación | Estado |
| :-- | :-- | :-- | :-- |
| 4.1 | Listar vistas Razor ya reemplazadas | Solo quedan vistas que el frontend Vue NO cubre | ⏳ PENDIENTE |
| 4.2 | Eliminar vistas Razor obsoletas | Archivos borrados | ⏳ PENDIENTE |
| 4.3 | Eliminar PermissionController (MVC) | Solo queda PermissionsApiController | ⏳ PENDIENTE |
| 4.4 | Eliminar otros controladores MVC obsoletos | Solo quedan API controllers | ⏳ PENDIENTE |
| 4.5 | Cambiar AddControllersWithViews → AddControllers | Compila sin Razor | ⏳ PENDIENTE |
| 4.6 | Verificar app.MapStaticFiles() | Sirve wwwroot con el SPA Vue | ⏳ PENDIENTE |
| 4.7 | Build + tests | 0 errores, 45+ tests verdes | ⏳ PENDIENTE |
| 4.8 | Commit fase 4 | Rama limpia | ⏳ PENDIENTE |

### Fase 5: Validación de integración completa (DÍA 9)
| # | Tarea | Verificación | Estado |
| :-- | :-- | :-- | :-- |
| 5.1 | Build Docker | Imagen construye | ⏳ PENDIENTE |
| 5.2 | Ejecutir contenedor local | Contenedor levanta sin errores | ⏳ PENDIENTE |
| 5.3 | Probar endpoints críticos | Respuestas JSON correctas | ⏳ PENDIENTE |
| 5.4 | Probar frontend SPA | Carga la aplicación Vue | ⏳ PENDIENTE |
| 5.5 | Crear permiso end-to-end | Se guarda en BD y se puede ver | ⏳ PENDIENTE |
| 5.6 | Tests finales | Todos verdes | ⏳ PENDIENTE |
| 5.7 | Commit final | Rama lista para PR | ⏳ PENDIENTE |

---

## Reglas de seguridad durante la operación

| Check | Frecuencia | Accion si falla |
| :-- | :-- | :-- |
| dotnet build sin errores | Después de CADA cambio | Revertir último cambio |
| dotnet test 45+ verdes | Después de CADA fase | No avanzar |
| Endpoint devuelve JSON esperado | Después de crear cada endpoint | Corregir contrato o servicio |
| Frontend no tiene logica de negocio duplicada | Después de Fase 2 y 3 | Mover logica al backend |
| docker build exitoso | Fase 5 | Revisar Dockerfile y wwwroot |
| Rollback posible | Siempre | git checkout safety/pre-api-pure-20261024 |

---

## Rollback de emergencia


