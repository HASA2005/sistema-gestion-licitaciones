# API REST

## Base y configuración

La API usa la base `/api/v1`. En ambiente Development, `Program.cs` mapea el
documento OpenAPI en `/openapi/v1.json`. También expone `GET /health`.

Las respuestas de error controladas usan `application/problem+json` cuando son
procesadas por los endpoints específicos o por el manejador global. El
manejador global asigna `400` para solicitudes inválidas, `404` para ofertas no
encontradas, `409` para duplicados reconocidos y `500` para excepciones no
traducidas.

## Licitaciones

| Método | Ruta | Propósito |
|---|---|---|
| `POST` | `/api/v1/licitaciones` | Crear una licitación en `Borrador`. |
| `GET` | `/api/v1/licitaciones` | Listar licitaciones. |
| `GET` | `/api/v1/licitaciones/{id}` | Consultar una licitación por Guid. |
| `PUT` | `/api/v1/licitaciones/{id}` | Editar código, título, presupuesto y fecha de cierre. |
| `DELETE` | `/api/v1/licitaciones/{id}` | Eliminar una licitación si la regla de aplicación lo permite. |
| `POST` | `/api/v1/licitaciones/{id}/publicar` | Publicar una licitación en Borrador. |

La creación recibe `codigo`, `titulo`, `presupuestoEstimadoCrc` y
`fechaCierre`. La respuesta exitosa es `201`. El código y título se validan,
el presupuesto debe ser positivo y la fecha API usa un desplazamiento o `Z`.

La publicación recibe el identificador en la ruta y no recibe cuerpo. Devuelve
`200` cuando es válida. Sus respuestas específicas verificables incluyen `400`
para identificador inválido, `404` para licitación inexistente, `409` para
estado no publicable o conflicto de concurrencia y `422` para datos no
publicables.

Los duplicados de código producen `409` con `licitacion_codigo_duplicado` en
el endpoint de creación. Los datos inválidos producen `422` con
`licitacion_datos_invalidos`.

Ejemplo mínimo de creación:

```json
{
  "codigo": "LIC-2030-001",
  "titulo": "Compra de equipo informático",
  "presupuestoEstimadoCrc": 1250000.50,
  "fechaCierre": "2030-10-15T18:30:00-06:00"
}
```

## Proveedores

| Método | Ruta | Propósito |
|---|---|---|
| `POST` | `/api/v1/proveedores` | Registrar un proveedor. |
| `GET` | `/api/v1/proveedores` | Listar proveedores. |
| `GET` | `/api/v1/proveedores/{id}` | Consultar un proveedor por Guid. |
| `PUT` | `/api/v1/proveedores/{id}` | Editar el nombre. |
| `DELETE` | `/api/v1/proveedores/{id}` | Eliminar un proveedor permitido. |

El registro recibe `{ "nombre": "Empresa Central" }` y devuelve `201`. El
nombre duplicado produce `409` con `proveedor_duplicado`; los datos inválidos
producen `422` con `proveedor_nombre_invalido`.

Las operaciones CRUD genéricas devuelven `200` para consultas y `204` para
actualizaciones o eliminaciones exitosas, según el endpoint.

## Ofertas

| Método | Ruta | Propósito |
|---|---|---|
| `POST` | `/api/v1/ofertas` | Crear una oferta. |
| `GET` | `/api/v1/ofertas` | Listar ofertas; admite `licitacionId` y `proveedorId` opcionales. |
| `GET` | `/api/v1/ofertas/{id}` | Consultar una oferta por Guid. |
| `PUT` | `/api/v1/ofertas/{id}` | Editar el monto de una oferta. |
| `DELETE` | `/api/v1/ofertas/{id}` | Eliminar una oferta. |
| `GET` | `/api/v1/ofertas/licitacion/{licitacionId}/mejor` | Obtener la mejor oferta. |

La solicitud de creación y actualización usa `LicitacionId`, `ProveedorId` y
`MontoCrc`. La creación exitosa devuelve `201`; la actualización devuelve
`200`; la eliminación devuelve `204`.

La aplicación valida licitación publicada y abierta, proveedor existente,
monto positivo, límite presupuestario y unicidad de proveedor por licitación.
Los errores de regla se procesan como solicitud inválida por el manejador
global; una oferta inexistente produce `404`.

## Niveles de aprobación

| Método | Ruta | Propósito |
|---|---|---|
| `POST` | `/api/v1/niveles-aprobacion` | Crear un nivel. |
| `GET` | `/api/v1/niveles-aprobacion` | Listar niveles. |
| `GET` | `/api/v1/niveles-aprobacion/{id}` | Consultar un nivel. |
| `PUT` | `/api/v1/niveles-aprobacion/{id}` | Editar un nivel. |
| `DELETE` | `/api/v1/niveles-aprobacion/{id}` | Eliminar un nivel. |
| `GET` | `/api/v1/niveles-aprobacion/determinar/{monto}` | Determinar el nivel para un monto. |

La solicitud usa `Responsable`, `MontoMinimoCrc` y `MontoMaximoCrc` opcional.
La aplicación valida montos y traslapes. Las consultas devuelven `200`, la
creación `201` y las operaciones de actualización/eliminación `204` cuando
terminan correctamente.

## Tipos de cambio

| Método | Ruta | Propósito |
|---|---|---|
| `POST` | `/api/v1/tipos-cambio` | Crear un tipo de cambio. |
| `GET` | `/api/v1/tipos-cambio` | Listar tipos de cambio. |
| `GET` | `/api/v1/tipos-cambio/activo` | Consultar el tipo activo. |
| `GET` | `/api/v1/tipos-cambio/{id}` | Consultar un tipo por Guid. |
| `PUT` | `/api/v1/tipos-cambio/{id}` | Editar valor y estado activo. |
| `DELETE` | `/api/v1/tipos-cambio/{id}` | Eliminar un tipo permitido. |
| `GET` | `/api/v1/tipos-cambio/convertir/{crc}` | Convertir un monto CRC a USD. |

La solicitud usa `CrcPorUsd` y `Activo`. La aplicación valida valor positivo y
mantiene un único activo. La creación devuelve `201`, las consultas `200` y
las operaciones de actualización/eliminación `204` cuando corresponda.

## Health y OpenAPI

- `GET /health`: health check de la aplicación.
- `GET /openapi/v1.json`: documento OpenAPI cuando el ambiente es
  `Development`.

## Problem Details

Los endpoints de creación de proveedores, creación de licitaciones y
publicación definen respuestas Problem Details específicas. El manejador global
agrega `title`, `status`, `detail`, `errorCode` y `correlationId` para errores
procesados globalmente, sin exponer detalles técnicos de la excepción.

## Colección reproducible

[api.http](api.http) contiene solicitudes para probar parte del contrato API.
Los endpoints CRUD adicionales están definidos en el código de la API aunque
no todos tengan una solicitud en esa colección.
