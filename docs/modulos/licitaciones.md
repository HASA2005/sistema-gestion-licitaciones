# Módulo de licitaciones

## Propósito

Gestionar el ciclo de vida básico de una licitación, desde su creación en
Borrador hasta su publicación y sus operaciones administrativas.

## Funcionalidades

- Crear una licitación en estado `Borrador`.
- Listar licitaciones.
- Consultar el detalle.
- Editar una licitación.
- Eliminarla cuando las reglas lo permitan.
- Publicarla.
- Exponer estas operaciones mediante Web MVC y API.

## Datos principales

| Campo | Tipo lógico | Descripción |
|---|---|---|
| `Id` | `Guid` | Identificador generado por el dominio. |
| `Codigo` | `string` | Código visible de máximo 100 caracteres. |
| `CodigoNormalizado` | `string` | Código usado para comprobar unicidad. |
| `Titulo` | `string` | Título obligatorio de máximo 200 caracteres. |
| `PresupuestoEstimadoCrc` | `decimal` | Presupuesto en CRC. |
| `FechaCierre` | `DateTimeOffset` | Fecha de cierre almacenada en UTC. |
| `Estado` | `EstadoLicitacion` | `Borrador`, `Publicada` o `Cerrada`. |
| `CreatedAt` | `DateTimeOffset` | Auditoría de creación en UTC. |
| `UpdatedAt` | `DateTimeOffset` | Auditoría de actualización en UTC. |
| `Version` | `uint` | Token de concurrencia asociado a `xmin`. |

## Reglas

- El código es obligatorio, se normaliza y es único mediante
  `CodigoNormalizado`.
- El título es obligatorio.
- Código y título rechazan caracteres de control y respetan sus longitudes
  máximas.
- El presupuesto debe ser positivo, tener como máximo dos decimales y estar
  dentro del rango configurado para `numeric(18,2)`.
- La fecha de cierre es obligatoria y se almacena en UTC.
- Toda licitación nueva inicia en `Borrador`.
- Solo una licitación en `Borrador` puede publicarse.
- Al publicar, la fecha de cierre debe ser estrictamente futura.
- No se puede editar reduciendo el presupuesto por debajo del monto de una
  oferta existente.
- La eliminación se bloquea cuando existen ofertas asociadas. Aunque la FK
  licitación-oferta está configurada con `DeleteBehavior.Cascade` en la
  persistencia, el servicio de aplicación aplica esta regla previa.
- La actualización usa concurrencia optimista mediante `xmin`.

## Web

El controlador MVC expone:

| Método | Ruta | Acción |
|---|---|---|
| `GET` | `/licitaciones/crear` | Mostrar el formulario de creación. |
| `POST` | `/licitaciones/crear` | Crear el Borrador y redirigir a publicación. |
| `GET` | `/licitaciones/{id}/publicar` | Mostrar confirmación de publicación. |
| `POST` | `/licitaciones/{id}/publicar` | Publicar con antiforgery y PRG. |
| `GET` | `/gestion/licitaciones` | Listar licitaciones. |
| `GET` | `/gestion/licitaciones/{id}` | Mostrar detalle. |
| `GET` | `/gestion/licitaciones/{id}/editar` | Mostrar edición. |
| `POST` | `/gestion/licitaciones/{id}/editar` | Guardar edición. |
| `POST` | `/gestion/licitaciones/{id}/eliminar` | Eliminar si no tiene ofertas. |

Las fechas visibles se presentan en `America/Costa_Rica`.

## API

La documentación general está en [docs/api.md](../api.md). El módulo expone:

- `POST /api/v1/licitaciones`;
- `GET /api/v1/licitaciones`;
- `GET /api/v1/licitaciones/{id}`;
- `PUT /api/v1/licitaciones/{id}`;
- `DELETE /api/v1/licitaciones/{id}`;
- `POST /api/v1/licitaciones/{id}/publicar`.

## Persistencia

La persistencia usa EF Core y PostgreSQL. La migración
`20260824164851_CrearLicitaciones` crea el catálogo de estados y la tabla
`licitaciones`, junto con presupuesto `numeric(18,2)`, fecha y auditoría,
restricción de presupuesto positivo, índice de código normalizado, claves
foráneas y la columna `xmin` de concurrencia.

## Pruebas

Las pruebas relacionadas se encuentran en:

- UnitTests: `LicitacionTests`, `CrearLicitacionServiceTests` y
  `PublicarLicitacionServiceTests`.
- FunctionalTests: `CrearLicitacionWebTests` y
  `PublicarLicitacionWebTests`, además de las pruebas de endpoints API de
  creación y publicación.
- IntegrationTests: configuración del modelo, repositorio, migraciones, API y
  Web con PostgreSQL.
- EndToEndTests: `FlujoLicitacionE2eTests`, que verifica el flujo Web de crear y
  publicar una licitación con Playwright.

El detalle general está en [docs/pruebas.md](../pruebas.md).
