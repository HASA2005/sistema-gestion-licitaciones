# Módulo de licitaciones

## Alcance actual

El módulo implementa HU-02: crear una licitación en estado `Borrador` desde API
o MVC. Ambas interfaces utilizan el mismo caso de uso y las mismas reglas de
dominio.

Todavía no implementa listado, consulta, edición, eliminación, publicación,
cierre, ofertas, mejor oferta, clasificación ni nivel de aprobación.

## Responsabilidades por capa

| Capa | Componentes | Responsabilidad |
| --- | --- | --- |
| Dominio | `Licitacion`, `EstadoLicitacion` | Validar datos, normalizar el código, generar identidad y auditoría, convertir fechas a UTC y asignar `Borrador`. |
| Aplicación | `CrearLicitacionService`, comando, resultado, repositorio y excepción | Orquestar la creación, prevenir duplicados y devolver un resultado independiente de HTTP/MVC. |
| Infraestructura | EF Core, configuraciones, repositorio y migración `CrearLicitaciones` | Persistir en PostgreSQL, sembrar estados y proteger integridad, unicidad y concurrencia. |
| API | `CrearLicitacionEndpoint` y contratos HTTP | Exponer `POST /api/v1/licitaciones` y transformar errores en Problem Details. |
| Web MVC | `LicitacionesController`, ViewModel y `Crear.cshtml` | Capturar datos, convertir la hora de Costa Rica a UTC y presentar validaciones y confirmación. |

## Datos de la entidad

| Propiedad | Comportamiento |
| --- | --- |
| `Id` | `Guid` generado por el dominio; no lo proporciona el usuario. |
| `Codigo` | Código visible de hasta 100 caracteres, Unicode Form C y sin espacios laterales; conserva caso y espacios internos. |
| `CodigoNormalizado` | Código limpio convertido con `ToUpperInvariant()` para la comparación. |
| `Titulo` | Obligatorio, Unicode Form C, máximo 200 caracteres y sin espacios laterales. |
| `PresupuestoEstimadoCrc` | `decimal` positivo, máximo dos decimales y dentro del rango de `numeric(18,2)`. |
| `FechaCierre` | Instante obligatorio almacenado en UTC. |
| `Estado` | Siempre `Borrador` al crear; no se recibe desde el cliente. |
| `CreatedAt` y `UpdatedAt` | Fechas UTC iguales durante la creación. |
| `Version` | `uint` asociado a `xmin` como token de concurrencia. |

Guardar un Borrador no requiere que `FechaCierre` sea futura. La historia de
publicación comprobará datos completos, presupuesto válido y fecha futura.

## Normalización y duplicados

La equivalencia funcional del código elimina espacios laterales y no distingue
mayúsculas de minúsculas. Además se aplica Unicode Form C para que caracteres
visualmente equivalentes tengan la misma representación técnica. No reduce
espacios internos ni aplica el alfabeto restringido del nombre de proveedor;
código y título sí rechazan caracteres de control.

| Entrada | `Codigo` | `CodigoNormalizado` |
| --- | --- | --- |
| `  Lic-2030-001  ` | `Lic-2030-001` | `LIC-2030-001` |
| `LiC  001` | `LiC  001` | `LIC  001` |

El servicio consulta primero por el código normalizado. El índice único
`ux_licitaciones_codigo_normalizado` resuelve además la carrera en la que dos
solicitudes consultan antes de que alguna guarde. El repositorio traduce esa
violación a `LicitacionDuplicadaException`.

## Persistencia

La migración `CrearLicitaciones` agrega:

- `estados_licitacion`, con `Borrador`, `Publicada` y `Cerrada` como datos
  semilla;
- `licitaciones`, con clave primaria `uuid`;
- presupuesto `numeric(18,2)` y restricción
  `ck_licitaciones_presupuesto_positivo`;
- fecha y auditoría `timestamp with time zone`;
- clave foránea restringida hacia el catálogo de estados;
- índice único de código normalizado;
- columnas `varchar(100)` para código y `varchar(200)` para título;
- `xmin` como versión de concurrencia.

La migración se aplica después de `CrearProveedores` y una prueba comprueba que
los proveedores existentes se conservan.

## API

### Crear licitación

```http
POST /api/v1/licitaciones
Content-Type: application/json
```

```json
{
  "codigo": "LIC-2030-001",
  "titulo": "Compra de equipo informático",
  "presupuestoEstimadoCrc": 1250000.50,
  "fechaCierre": "2030-10-15T18:30:00-06:00"
}
```

La respuesta `201 Created` incluye `id`, los valores limpios, la fecha UTC,
`estado: "Borrador"` y `mensaje: "Licitación creada correctamente."`. No se
envía `Location` hasta que exista el endpoint de consulta por identificador.

| Estado | Situación | `errorCode` |
| ---: | --- | --- |
| `400` | JSON o tipos incompatibles | `solicitud_json_invalida` |
| `409` | Código normalizado duplicado | `licitacion_codigo_duplicado` |
| `415` | Cuerpo con tipo diferente de JSON | `tipo_contenido_no_compatible` |
| `422` | Regla de dominio incumplida | `licitacion_datos_invalidos` |
| `500` | Error inesperado | `error_interno` |

## Interfaz MVC

| Método | Ruta | Comportamiento |
| --- | --- | --- |
| `GET` | `/licitaciones/crear` | Muestra el formulario y el aviso de estado Borrador. |
| `POST` | `/licitaciones/crear` | Valida antiforgery, ejecuta el caso de uso y aplica POST-Redirect-GET. |

El formulario usa `number` con paso `0.01` y `datetime-local`. Como este último
no contiene zona horaria, el controlador interpreta el valor explícitamente en
`America/Costa_Rica` y lo convierte a UTC; no depende de la zona del servidor.

Los errores aparecen junto al campo correspondiente, los valores se conservan
y un registro exitoso muestra la confirmación mediante `TempData`.

## Pruebas

HU-02 agrega 72 casos:

- 30 unitarios para dominio y aplicación;
- 35 funcionales para API y MVC;
- 7 de integración para EF, migraciones, PostgreSQL, API y Web.

La estrategia completa se encuentra en [Pruebas](../pruebas.md).
