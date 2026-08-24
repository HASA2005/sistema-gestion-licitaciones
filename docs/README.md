# Documentación del Sistema de Gestión de Licitaciones

Este directorio reúne la documentación viva del proyecto. Se actualiza de
manera incremental junto con las historias de usuario, tareas técnicas y
pequeñas liberaciones.

## Estado documentado

HU-01 — Registrar proveedor está integrada en `main`. HU-02 — Crear licitación
en estado Borrador está terminada técnicamente en su rama y pendiente de
integración. Ambas incluyen dominio, aplicación, PostgreSQL, API, MVC y pruebas
automatizadas.

`v0.1.0` fue etiquetada localmente sobre el cierre de la Iteración XP 1 y está
pendiente de publicación en GitHub. El sistema completo todavía no está
terminado.

## Índice

- [Plan XP](plan-xp.md): objetivos de las cuatro iteraciones y plan de
  liberaciones.
- [Historias de usuario](historias-usuario.md): criterios de aceptación y
  trazabilidad.
- [Bitácora XP](bitacora-xp.md): evidencia, métricas y retrospectivas de las
  iteraciones XP.
- [Estrategia de pruebas](pruebas.md): niveles, casos ejecutados y CI.
- [API REST](api.md): endpoints, contratos, colección reproducible y errores.
- [Módulo de licitaciones](modulos/licitaciones.md): creación de Borradores,
  fechas, dinero y persistencia.
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
- API: creación de proveedores y licitaciones bajo `/api/v1`.
- Pruebas: 118 casos xUnit registrados.
- Integración: once pruebas usan PostgreSQL 16 mediante Testcontainers.
- CI: GitHub Actions compila en Release y ejecuta la solución completa.

## Convenciones

- La metodología utilizada es Extreme Programming.
- Las historias y tareas se relacionan con Issues de GitHub.
- Los cambios se integran mediante Pull Requests.
- Se favorecen commits claros con Conventional Commits.
- No se almacenan secretos ni credenciales reales en el repositorio.
