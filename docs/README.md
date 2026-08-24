# Documentación del Sistema de Gestión de Licitaciones

Este directorio reúne la documentación viva del proyecto. Se actualiza de
manera incremental junto con las historias de usuario, tareas técnicas y
pequeñas liberaciones.

## Estado documentado

HU-01 — Registrar proveedor está integrada en `main`. Incluye dominio, caso de
uso, persistencia PostgreSQL, API, interfaz MVC y pruebas automatizadas. La
integración continua también se encuentra configurada.

Esto representa el alcance de la Iteración XP 1 y no significa que el sistema
completo de licitaciones esté terminado. `v0.1.0` permanece como candidata hasta
integrar el cierre y etiquetar el commit validado de `main`.

## Índice

- [Plan XP](plan-xp.md): objetivos de las cuatro iteraciones y plan de
  liberaciones.
- [Historias de usuario](historias-usuario.md): criterios de aceptación y
  trazabilidad.
- [Bitácora XP](bitacora-xp.md): evidencia, métricas y retrospectiva de la
  Iteración XP 1.
- [Estrategia de pruebas](pruebas.md): niveles, casos ejecutados y CI.
- [Módulo de proveedores](modulos/proveedores.md): diseño y contrato del primer
  recorrido vertical.
- [Desarrollo local](desarrollo-local.md): configuración segura de PostgreSQL y
  ejecución local.
- [Uso de inteligencia artificial](uso-ia.md): alcance, control humano y
  validación del uso de Codex.

## Evidencia técnica actual

- Plataforma: .NET 9.
- Persistencia: Entity Framework Core 9 y PostgreSQL 16.
- Interfaz Web: ASP.NET Core MVC.
- API: `POST /api/v1/proveedores`.
- Pruebas: 46 casos xUnit registrados.
- Integración: cinco pruebas usan PostgreSQL 16 mediante Testcontainers.
- CI: GitHub Actions compila en Release y ejecuta la solución completa.

## Convenciones

- La metodología utilizada es Extreme Programming.
- Las historias y tareas se relacionan con Issues de GitHub.
- Los cambios se integran mediante Pull Requests.
- Se favorecen commits claros con Conventional Commits.
- No se almacenan secretos ni credenciales reales en el repositorio.
