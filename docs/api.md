# API REST

## Estado actual

La API usa rutas versionadas bajo `/api/v1`, DTO específicos, OpenAPI y
respuestas Problem Details. Actualmente permite registrar proveedores, crear
licitaciones en Borrador y publicar una licitación existente.

En ambiente `Development`, el documento OpenAPI está disponible en:

```text
/openapi/v1.json
```

## Colección reproducible

La colección [api.http](api.http) contiene el host local, encabezados y cuerpos
completos para registrar un proveedor, crear una licitación, publicar el
Borrador recién creado y consultar OpenAPI. Puede ejecutarse desde un editor
compatible con archivos `.http` después de iniciar la API en
`http://localhost:5018`.

## Endpoints implementados

| Método | Ruta | Éxito | Propósito |
| --- | --- | ---: | --- |
| `POST` | `/api/v1/proveedores` | `201` | Registrar un proveedor. |
| `POST` | `/api/v1/licitaciones` | `201` | Crear una licitación en Borrador. |
| `POST` | `/api/v1/licitaciones/{id}/publicar` | `200` | Cambiar una licitación válida de Borrador a Publicada. |

## Crear proveedor

```json
{
  "nombre": "Empresa Central"
}
```

Respuesta:

```json
{
  "mensaje": "Proveedor registrado correctamente."
}
```

Errores propios: `409 proveedor_duplicado` y
`422 proveedor_nombre_invalido`.

## Crear licitación

```json
{
  "codigo": "LIC-2030-001",
  "titulo": "Compra de equipo informático",
  "presupuestoEstimadoCrc": 1250000.50,
  "fechaCierre": "2030-10-15T18:30:00-06:00"
}
```

Respuesta abreviada:

```json
{
  "id": "2f24118b-6541-4c2c-8cd8-9c86e49299f1",
  "codigo": "LIC-2030-001",
  "titulo": "Compra de equipo informático",
  "presupuestoEstimadoCrc": 1250000.50,
  "fechaCierre": "2030-10-16T00:30:00+00:00",
  "estado": "Borrador",
  "mensaje": "Licitación creada correctamente."
}
```

El `id` de una respuesta real es un `Guid` generado y no vacío. El contrato de
entrada no contiene `estado`. `codigo` admite como máximo 100 caracteres y
`titulo` 200; ambos rechazan caracteres de control. `fechaCierre` debe ser ISO
8601 e incluir `Z` o un desplazamiento explícito, por ejemplo `-06:00`, para
que el resultado no dependa de la zona horaria del servidor.

Errores propios: `409 licitacion_codigo_duplicado` y
`422 licitacion_datos_invalidos`. Un cuerpo que no sea JSON produce
`415 tipo_contenido_no_compatible`; JSON inválido o una fecha sin zona produce
`400 solicitud_json_invalida`.

## Publicar licitación

```http
POST /api/v1/licitaciones/2f24118b-6541-4c2c-8cd8-9c86e49299f1/publicar
Accept: application/json
```

La operación no recibe cuerpo ni datos editables. El identificador forma parte
de la ruta y debe ser un UUID válido. Una publicación correcta devuelve
`200 OK`:

```json
{
  "id": "2f24118b-6541-4c2c-8cd8-9c86e49299f1",
  "codigo": "LIC-2030-001",
  "titulo": "Compra de equipo informático",
  "presupuestoEstimadoCrc": 1250000.50,
  "fechaCierre": "2030-10-16T00:30:00+00:00",
  "estado": "Publicada",
  "updatedAt": "2026-08-24T20:30:00+00:00",
  "mensaje": "Licitación publicada correctamente."
}
```

La fecha de cierre debe ser estrictamente posterior al instante de publicación.
El servidor actualiza `updatedAt` en UTC y usa `xmin` para impedir que una copia
obsoleta sobrescriba otra actualización.

| Estado | Situación | `errorCode` |
| ---: | --- | --- |
| `400` | El valor `{id}` no es un UUID | `identificador_licitacion_invalido` |
| `404` | No existe la licitación | `licitacion_no_encontrada` |
| `409` | La licitación no se encuentra en Borrador | `licitacion_estado_no_publicable` |
| `409` | Otra operación actualizó la misma fila | `licitacion_conflicto_concurrencia` |
| `422` | Los datos no permiten publicar o la fecha de cierre no es futura | `licitacion_datos_no_publicables` |
| `500` | Error inesperado | `error_interno` |

## Contrato Problem Details

Los errores controlados usan `application/problem+json` y contienen, como
mínimo:

```json
{
  "title": "Descripción breve",
  "status": 422,
  "detail": "Mensaje seguro y comprensible.",
  "errorCode": "codigo_estable",
  "correlationId": "identificador-de-la-solicitud"
}
```

El manejador global convierte JSON inválido en `400 solicitud_json_invalida` y
errores inesperados en `500 error_interno` sin exponer stack traces, rutas,
consultas, nombres internos de parámetros ni secretos.

## Pendiente

Todavía no están implementados listados, consulta general por identificador,
edición, eliminación, cierre, ofertas, mejor oferta, niveles de aprobación ni
tipos de cambio. Cuando existan listados deberán incorporar paginación,
filtrado y ordenamiento.

La autenticación y la autorización por rol todavía no están implementadas; las
rutas actuales son anónimas y deberán protegerse en una historia específica de
seguridad antes de un despliegue real.
