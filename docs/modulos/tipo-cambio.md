# Módulo de tipos de cambio

## Propósito

Administrar la referencia CRC/USD utilizada para conversiones monetarias.

## Datos

| Campo | Tipo lógico | Descripción |
|---|---|---|
| `Id` | `Guid` | Identificador generado por el dominio. |
| `CrcPorUsd` | `decimal` | Colones por cada dólar. |
| `Activo` | `bool` | Indica si es el tipo de cambio activo. |
| `CreatedAt` | `DateTimeOffset` | Auditoría de creación en UTC. |
| `UpdatedAt` | `DateTimeOffset` | Auditoría de actualización en UTC. |

## Funcionalidades

- Crear tipos de cambio.
- Listar tipos de cambio.
- Consultar un tipo de cambio.
- Editar valor y estado.
- Eliminar cuando la regla lo permite.
- Consultar el tipo de cambio activo.
- Convertir CRC → USD.

## Reglas

- `CrcPorUsd` debe ser mayor que cero.
- El valor admite como máximo dos decimales.
- Solo puede existir un tipo de cambio activo.
- El servicio intenta desactivar el tipo activo anterior antes de activar el
  nuevo registro. El índice único filtrado de PostgreSQL protege la unicidad
  ante conflictos de persistencia.
- La eliminación del tipo activo está bloqueada por el servicio.
- La conversión usa el tipo de cambio activo.
- La conversión no modifica los montos CRC persistidos.

## Integración

No existe una FK hacia licitaciones u ofertas. CRC continúa siendo la fuente
de verdad de los presupuestos y montos monetarios del dominio. La conversión
CRC → USD es una operación de servicio y no una relación persistida ni una
actualización de esos montos.

## Web

`TiposCambioController` y sus vistas exponen:

- `GET /tipos-cambio`: listado;
- `GET /tipos-cambio/crear` y `POST /tipos-cambio/crear`: crear;
- `GET /tipos-cambio/{id}/editar` y `POST /tipos-cambio/{id}/editar`: editar;
- `POST /tipos-cambio/{id}/eliminar`: eliminar.

Si se intenta activar un segundo registro y la persistencia rechaza la
operación, el controlador muestra una validación controlada y conserva el
formulario; no presenta la página genérica de error.

## API

La documentación general está en [docs/api.md](../api.md). El módulo expone:

- `POST /api/v1/tipos-cambio`;
- `GET /api/v1/tipos-cambio`;
- `GET /api/v1/tipos-cambio/activo`;
- `GET /api/v1/tipos-cambio/{id}`;
- `PUT /api/v1/tipos-cambio/{id}`;
- `DELETE /api/v1/tipos-cambio/{id}`;
- `GET /api/v1/tipos-cambio/convertir/{crc}`.

## Persistencia

La migración `20260824214000_CrearTiposCambio` crea la tabla
`tipos_cambio`, valores `numeric(18,2)`, la restricción
`ck_tipos_cambio_crc_positivo` y el índice único filtrado
`ux_tipos_cambio_activo` para `Activo = true`.

## Pruebas

Las pruebas unitarias cubren unicidad y conversión mediante
`TipoCambioServiceTests` y `TipoCambioTests`. Las pruebas funcionales cubren
CRUD y la regresión Web de segundo activo en
`GestionarTiposCambioWebTests`. No se localizó una clase específica de
IntegrationTests para este módulo en el inventario de pruebas.

El detalle general está en [docs/pruebas.md](../pruebas.md).
