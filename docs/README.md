# DocumentaciÃ³n del Sistema de GestiÃ³n de Licitaciones

Este directorio contiene la documentaciÃ³n acadÃ©mica y tÃ©cnica final del
Sistema de GestiÃ³n de Licitaciones. Incluye su alcance, arquitectura, modelo
de datos, API, pruebas, infraestructura, integraciÃ³n de mÃ³dulos y uso de
inteligencia artificial.

## Estado actual

El sistema implementa:

- Proveedores.
- Licitaciones.
- Ofertas.
- Niveles de aprobaciÃ³n.
- Tipos de cambio.
- Web MVC.
- API.
- PostgreSQL.
- Docker.
- Kubernetes.
- IntegraciÃ³n continua con GitHub Actions.
- UnitTests.
- FunctionalTests.
- IntegrationTests.
- EndToEndTests.

La Ãºltima ejecuciÃ³n manual confirmada registrÃ³ **180 pruebas correctas y 0
fallidas**.

La etiqueta `v0.1.0` se conserva como referencia histÃ³rica del cierre de una
liberaciÃ³n anterior.

## Ãndice documental

- [VisiÃ³n y alcance](vision-alcance.md): problema, objetivos, usuarios,
  alcance incluido y exclusiones.
- [Historias de usuario](historias-usuario.md): historias, criterios y estados.
- [Plan XP](plan-xp.md): cuatro iteraciones XP, prÃ¡cticas y criterios de salida.
- [BitÃ¡cora XP](bitacora-xp.md): evoluciÃ³n incremental y evidencia del proceso.
- [Arquitectura general](arquitectura-general.md): capas, dependencias y
  proyectos de pruebas.
- [Modelo de datos](modelo-datos.md): entidades, relaciones, restricciones y
  migraciones.
- [IntegraciÃ³n de mÃ³dulos](integracion-modulos.md): colaboraciÃ³n persistida y
  servicios independientes.
- [API REST](api.md): endpoints, contratos y manejo de errores.
- [Pruebas automatizadas](pruebas.md): estrategia, proyectos, CI y E2E.
- [Docker](docker.md): servicios Compose, puertos, volumen y health checks.
- [Kubernetes](kubernetes.md): recursos, configuraciÃ³n, persistencia y probes.
- [Desarrollo local](desarrollo-local.md): configuraciÃ³n y ejecuciÃ³n local.
- [Uso de inteligencia artificial](uso-ia.md): herramientas, asistencia,
  revisiÃ³n humana y responsabilidad del equipo.

### DocumentaciÃ³n por mÃ³dulo

- [MÃ³dulo de licitaciones](modulos/licitaciones.md).
- [MÃ³dulo de proveedores](modulos/proveedores.md).
- [MÃ³dulo de ofertas](modulos/ofertas.md).
- [MÃ³dulo de niveles de aprobaciÃ³n](modulos/aprobaciones.md).
- [MÃ³dulo de tipos de cambio](modulos/tipos-cambio.md).

## Convenciones

- La metodologÃ­a utilizada es Extreme Programming.
- Las historias y tareas se relacionan con GitHub cuando existe una referencia
  verificable.
- Los cambios se integran mediante Pull Requests.
- Se utilizan Conventional Commits cuando corresponde.
- No se almacenan secretos ni credenciales reales en el repositorio.

