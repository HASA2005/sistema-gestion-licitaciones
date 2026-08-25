# Documentación del Sistema de Gestión de Licitaciones

Este directorio contiene la documentación académica y técnica final del
Sistema de Gestión de Licitaciones. Incluye su alcance, arquitectura, modelo
de datos, API, pruebas, infraestructura, integración de módulos y uso de
inteligencia artificial.

## Estado actual

El sistema implementa:

- Proveedores.
- Licitaciones.
- Ofertas.
- Niveles de aprobación.
- Tipos de cambio.
- Web MVC.
- API.
- PostgreSQL.
- Docker.
- Kubernetes.
- Integración continua con GitHub Actions.
- UnitTests.
- FunctionalTests.
- IntegrationTests.
- EndToEndTests.

La última ejecución manual confirmada registró **180 pruebas correctas y 0
fallidas**.

La etiqueta `v0.1.0` se conserva como referencia histórica del cierre de una
liberación anterior.

## Índice documental

- [Visión y alcance](vision-alcance.md): problema, objetivos, usuarios,
  alcance incluido y exclusiones.
- [Historias de usuario](historias-usuario.md): historias, criterios y estados.
- [Plan XP](plan-xp.md): cuatro iteraciones XP, prácticas y criterios de salida.
- [Bitácora XP](bitacora-xp.md): evolución incremental y evidencia del proceso.
- [Arquitectura general](arquitectura-general.md): capas, dependencias y
  proyectos de pruebas.
- [Modelo de datos](modelo-datos.md): entidades, relaciones, restricciones y
  migraciones.
- [Integración de módulos](integracion-modulos.md): colaboración persistida y
  servicios independientes.
- [API REST](api.md): endpoints, contratos y manejo de errores.
- [Pruebas automatizadas](pruebas.md): estrategia, proyectos, CI y E2E.
- [Docker](docker.md): servicios Compose, puertos, volumen y health checks.
- [Kubernetes](kubernetes.md): recursos, configuración, persistencia y probes.
- [Desarrollo local](desarrollo-local.md): configuración y ejecución local.
- [Uso de inteligencia artificial](uso-ia.md): herramientas, asistencia,
  revisión humana y responsabilidad del equipo.

### Documentación por módulo

- [Módulo de licitaciones](modulos/licitaciones.md).
- [Módulo de proveedores](modulos/proveedores.md).
- [Módulo de ofertas](modulos/ofertas.md).
- [Módulo de niveles de aprobación](modulos/aprobaciones.md).
- [Módulo de tipos de cambio](modulos/tipos-cambio.md).

## Convenciones

- La metodología utilizada es Extreme Programming.
- Las historias y tareas se relacionan con GitHub cuando existe una referencia
  verificable.
- Los cambios se integran mediante Pull Requests.
- Se utilizan Conventional Commits cuando corresponde.
- No se almacenan secretos ni credenciales reales en el repositorio.
