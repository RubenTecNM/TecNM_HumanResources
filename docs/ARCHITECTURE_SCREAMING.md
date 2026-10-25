# Arquitectura Screaming - HumanResourcesProduction

## Reglas obligatorias
1. Las rutas API/MVC NO cambian.
2. Una feature por PR. Commits atómicos.
3. Un solo DbContext (HumanResourcesContext) en Shared/Data.
4. No dividir entidades por feature. Shared/Domain es el kernel compartido.
5. No tocar esquema BD, triggers ni ejecutar upgrades.
6. Build verde tras cada movimiento.
7. Diff de OpenAPI debe ser idéntico al baseline en cada feature migrada.

## Estructura
- Features/<Dominio>/: Controllers, Services exclusivos, DTOs, ViewModels, Mapping, Views (solo si se justifica)
- Shared/Domain/: Entidades, BaseEntity
- Shared/Data/: DbContext, configuraciones
- Shared/Common/: utilidades transversales
- Shared/Web/: BaseController, filtros, extensiones web
- Shared/Notifications/: Email

## Orden de migración
ApiKeys -> Areas -> Positions -> PermissionTypes -> UserTypes -> Departments -> Users -> Permissions -> Cutoffs -> Sync -> Auth -> Admin -> Shared/Web

## Gates
- dotnet build HumanResourcesProduction.sln -c Release --no-restore sin errores
- Comparar OpenAPI contra docs/contracts/openapi-baseline.json
- Smoke test de la feature
