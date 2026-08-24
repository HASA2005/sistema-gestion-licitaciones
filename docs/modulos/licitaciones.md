# Módulo de licitaciones

## Alcance actual

El módulo implementa HU-02 para crear una licitación en estado `Borrador` y
HU-03 para publicarla desde API o MVC. Ambas interfaces utilizan los mismos
casos de uso y reglas de dominio. HU-02 está integrada en `main`; HU-03 está
terminada técnicamente en su rama y pendiente de integración.

Todavía no implementa listado, consulta general, edición, eliminación, cierre,
ofertas, mejor oferta, clasificación ni nivel de aprobación.

## Responsabilidades por capa

| Capa | Componentes | Responsabilidad |
| --- | --- | --- |
| Dominio | `Licitacion`, `EstadoLicitacion`, motivo y excepción de publicación | Validar datos, crear en `Borrador` y proteger la transición a `Publicada`, la fecha futura y la auditoría UTC. |
| Aplicación | Servicios de creación y publicación, contratos, repositorio y excepciones | Orquestar ambos casos de uso, usar un reloj inyectable y devolver resultados independientes de HTTP/MVC. |
| Infraestructura | EF Core, configuraciones, repositorio y migración `CrearLicitaciones` | Persistir, consultar por identificador, sembrar estados y proteger integridad, unicidad y concurrencia `xmin`. |
| API | Endpoints y DTO de creación y publicación | Exponer ambas operaciones bajo `/api/v1` y transformar errores en Problem Details. |
| Web MVC | `LicitacionesController`, ViewModels y vistas `Crear` y `Publicar` | Crear el Borrador, mostrar una confirmación de solo lectura, validar antiforgery y aplicar PRG. |

## Datos de la entidad

| Propiedad | Comportamiento |
| --- | --- |
| `Id` | `Guid` generado por el dominio; no lo proporciona el usuario. |
| `Codigo` | Código visible de hasta 100 caracteres, Unicode Form C y sin espacios laterales; conserva caso y espacios internos. |
| `CodigoNormalizado` | Código limpio convertido con `ToUpperInvariant()` para la comparación. |
| `Titulo` | Obligatorio, Unicode Form C, máximo 200 caracteres y sin espacios laterales. |
| `PresupuestoEstimadoCrc` | `decimal` positivo, máximo dos decimales y dentro del rango de `numeric(18,2)`. |
| `FechaCierre` | Instante obligatorio almacenado en UTC. |
| `Estado` | Inicia en `Borrador`; la publicación válida lo cambia a `Publicada`. No se recibe desde el cliente. |
| `CreatedAt` y `UpdatedAt` | Son iguales y UTC al crear; publicar conserva `CreatedAt` y actualiza `UpdatedAt` en UTC. |
| `Version` | `uint` asociado a `xmin` como token de concurrencia. |

Guardar un Borrador no requiere que `FechaCierre` sea futura. Al publicar se
comprueba que código, título, presupuesto y fecha sigan siendo válidos y que
`FechaCierre` sea estrictamente posterior al instante actual. La validación
ocurre antes de modificar la entidad, por lo que un intento inválido conserva
el estado y la auditoría anteriores.

## Publicación y concurrencia

`Licitacion.Publicar(fechaActual)` es la única operación que realiza la
transición `Borrador` → `Publicada`. Cualquier otro estado, incluida una
licitación ya `Publicada` o `Cerrada`, se rechaza con un mensaje controlado. El
servicio obtiene el instante de un `TimeProvider` para permitir pruebas
deterministas y lo convierte a UTC.

El repositorio obtiene la licitación por `Guid` con seguimiento de EF Core y
guarda la entidad modificada. Si otra operación cambió la misma fila después de
la lectura, PostgreSQL detecta el `xmin` obsoleto. El repositorio traduce
`DbUpdateConcurrencyException` a `LicitacionConcurrenciaException`; no se
exponen nombres de clases, consultas ni el valor de `xmin` al usuario.

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

HU-03 no requiere una migración adicional: `Publicada`, `updated_at` y `xmin`
ya formaban parte del esquema creado por HU-02.

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

### Publicar licitación

```http
POST /api/v1/licitaciones/{id}/publicar
Accept: application/json
```

La solicitud no recibe cuerpo. Una respuesta `200 OK` contiene `id`, código,
título, presupuesto, fecha de cierre UTC, `estado: "Publicada"`, `updatedAt`
UTC y `mensaje: "Licitación publicada correctamente."`.

| Estado | Situación | `errorCode` |
| ---: | --- | --- |
| `400` | Identificador que no es un UUID | `identificador_licitacion_invalido` |
| `404` | Licitación inexistente | `licitacion_no_encontrada` |
| `409` | Estado distinto de Borrador | `licitacion_estado_no_publicable` |
| `409` | Versión `xmin` obsoleta | `licitacion_conflicto_concurrencia` |
| `422` | Datos inválidos o fecha de cierre no futura | `licitacion_datos_no_publicables` |
| `500` | Error inesperado | `error_interno` |

## Interfaz MVC

| Método | Ruta | Comportamiento |
| --- | --- | --- |
| `GET` | `/licitaciones/crear` | Muestra el formulario y el aviso de estado Borrador. |
| `POST` | `/licitaciones/crear` | Valida antiforgery, crea el Borrador y redirige a su confirmación de publicación. |
| `GET` | `/licitaciones/{id}/publicar` | Consulta y muestra código, título, presupuesto, cierre en hora de Costa Rica y estado. |
| `POST` | `/licitaciones/{id}/publicar` | Valida antiforgery, intenta publicar y aplica POST-Redirect-GET. |

El formulario usa `number` con paso `0.01` y `datetime-local`. Como este último
no contiene zona horaria, el controlador interpreta el valor explícitamente en
`America/Costa_Rica` y lo convierte a UTC; no depende de la zona del servidor.

Los errores de creación aparecen junto al campo correspondiente y conservan los
valores. Después de crear, el navegador llega a la vista de publicación del
nuevo Borrador. Una publicación correcta muestra la confirmación mediante
`TempData`, actualiza el estado visible y oculta el botón para impedir otro
intento. Fecha vencida y concurrencia también se presentan mediante mensajes
seguros; un identificador inexistente devuelve `404`. Un POST sin antiforgery
devuelve `400` y no publica.

## Pruebas

HU-02 agrega 72 casos:

- 30 unitarios para dominio y aplicación;
- 35 funcionales para API y MVC;
- 7 de integración para EF, migraciones, PostgreSQL, API y Web.

HU-03 agrega pruebas unitarias de dominio y aplicación, funcionales de API y
MVC, y de integración con repositorio, API y Web sobre PostgreSQL real. El
incremento neto es de 27 casos y el total consolidado del proyecto queda en 145:
64 unitarios, 65 funcionales y 16 de integración.

La estrategia y el desglose consolidado se encuentran en
[Pruebas](../pruebas.md).
