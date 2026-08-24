# Módulo de proveedores

## Alcance actual

El módulo implementa la HU-01: registrar un proveedor mediante API o interfaz
MVC. Ambas entradas reutilizan el mismo caso de uso y las mismas reglas de
dominio.

Actualmente no existen operaciones para listar, consultar, editar ni eliminar
proveedores. Tampoco se asocian todavía proveedores con ofertas.

## Responsabilidades por capa

| Capa | Componentes | Responsabilidad |
| --- | --- | --- |
| Dominio | `Proveedor` | Validar y limpiar el nombre, generar el identificador, establecer auditoría UTC y producir el nombre normalizado usado para detectar duplicados. |
| Aplicación | `RegistrarProveedorService`, `IProveedorRepository`, `RegistrarProveedorResultado`, `ProveedorDuplicadoException` | Orquestar el registro, comprobar duplicados, solicitar la persistencia y devolver una confirmación independiente del canal de entrada. |
| Infraestructura | `LicitacionesDbContext`, `ProveedorConfiguration`, `ProveedorRepository`, migración `CrearProveedores` | Persistir con EF Core y PostgreSQL, aplicar restricciones de integridad, configurar `xmin` y traducir violaciones de unicidad a un error de aplicación controlado. |
| API | `RegistrarProveedorEndpoint`, contratos y `ApiExceptionHandler` | Exponer `POST /api/v1/proveedores`, transformar resultados y errores en respuestas HTTP y documentar el contrato mediante OpenAPI. |
| Web MVC | `ProveedoresController`, `RegistrarProveedorViewModel`, `Registrar.cshtml` | Mostrar y procesar el formulario, validar antiforgery, conservar datos inválidos y presentar confirmaciones o errores. |

## Flujo de registro

1. La API o el controlador MVC recibe el nombre.
2. `RegistrarProveedorService` crea la entidad `Proveedor`.
3. La entidad aplica las reglas y genera `Nombre` y `NombreNormalizado`.
4. El servicio consulta el repositorio por `NombreNormalizado`.
5. Si ya existe, lanza `ProveedorDuplicadoException`.
6. Si no existe, solicita al repositorio agregar la entidad.
7. PostgreSQL protege definitivamente la unicidad mediante un índice único.
8. El servicio devuelve `Proveedor registrado correctamente.`

## Datos de la entidad

| Propiedad | Descripción |
| --- | --- |
| `Id` | `Guid` generado por el dominio; EF Core no genera su valor. |
| `Nombre` | Nombre limpio, conservando mayúsculas y minúsculas significativas. |
| `NombreNormalizado` | Nombre limpio convertido con `ToUpperInvariant()`, usado para comparar y asegurar unicidad. |
| `CreatedAt` | Fecha de creación convertida a UTC. |
| `UpdatedAt` | En el registro inicial tiene el mismo valor que `CreatedAt`. |
| `Version` | Valor `uint` asociado a `xmin` de PostgreSQL como token de concurrencia. |

El caso de uso devuelve únicamente `RegistrarProveedorResultado.Mensaje`; no
devuelve el identificador del proveedor.

## Reglas del nombre

- Es obligatorio; `null`, cadena vacía y solo espacios son inválidos.
- Se normaliza Unicode con NFC (`NormalizationForm.FormC`).
- Se eliminan los espacios iniciales y finales.
- Los grupos de espacios se reducen a un único espacio.
- Se admiten letras Unicode, caracteres numéricos Unicode, espacios, punto,
  coma y paréntesis.
- Caracteres como `@`, `/`, `#` y `&` son rechazados.
- La unicidad ignora diferencias de mayúsculas, espacios redundantes y
  representaciones Unicode equivalentes.
- No existe actualmente una longitud máxima configurada en el dominio ni en la
  base de datos.

Ejemplos:

| Entrada | `Nombre` | `NombreNormalizado` |
| --- | --- | --- |
| `  Empresa   Central  ` | `Empresa Central` | `EMPRESA CENTRAL` |
| `Cafe\u0301 Central` | `Café Central` | `CAFÉ CENTRAL` |

## Errores controlados

| Situación | Resultado |
| --- | --- |
| Nombre ausente o compuesto solo por espacios | `ArgumentException`: `El nombre del proveedor es obligatorio.` |
| Nombre con caracteres no permitidos | `ArgumentException`: `El nombre del proveedor contiene caracteres no permitidos.` |
| Nombre normalizado ya registrado | `ProveedorDuplicadoException`: `Ya existe un proveedor con el mismo nombre.` |

## API

### Registrar proveedor

```http
POST /api/v1/proveedores
Content-Type: application/json
```

Solicitud:

```json
{
  "nombre": "Empresa Central"
}
```

Respuesta satisfactoria, HTTP `201 Created`:

```json
{
  "mensaje": "Proveedor registrado correctamente."
}
```

### Respuestas de error

Las respuestas de error usan `application/problem+json` e incluyen
`errorCode` y `correlationId`.

| Estado | Situación | `errorCode` |
| ---: | --- | --- |
| `400 Bad Request` | El cuerpo no contiene JSON válido. | `solicitud_json_invalida` |
| `409 Conflict` | Ya existe un proveedor equivalente. | `proveedor_duplicado` |
| `422 Unprocessable Entity` | El nombre incumple una regla del dominio. | `proveedor_nombre_invalido` |
| `500 Internal Server Error` | Se produjo un error inesperado. | `error_interno` |

La respuesta `500` no expone el mensaje técnico de la excepción. En ambiente de
desarrollo, OpenAPI documenta las respuestas `201`, `400`, `409`, `422` y `500`.

## Interfaz MVC

| Método | Ruta | Comportamiento |
| --- | --- | --- |
| `GET` | `/proveedores/registrar` | Devuelve el formulario HTML de registro. |
| `POST` | `/proveedores/registrar` | Valida el token antiforgery y ejecuta el caso de uso. |

El formulario:

- contiene el campo obligatorio `Nombre`;
- incluye ayuda sobre los caracteres permitidos;
- usa atributos de accesibilidad como `aria-required` y `aria-describedby`;
- incluye un token antiforgery;
- presenta los errores junto al campo;
- conserva el valor enviado cuando la validación falla;
- aplica POST-Redirect-GET después de un registro válido;
- muestra `Proveedor registrado correctamente.` mediante `TempData`.

Un POST sin token antiforgery responde `400 Bad Request` y no guarda datos.

## Persistencia

La migración `CrearProveedores` crea la tabla `proveedores`:

| Columna | Tipo PostgreSQL | Restricción |
| --- | --- | --- |
| `id` | `uuid` | Clave primaria `pk_proveedores`, no nula |
| `nombre` | `text` | No nula |
| `nombre_normalizado` | `text` | No nula |
| `created_at` | `timestamp with time zone` | No nula |
| `updated_at` | `timestamp with time zone` | No nula |
| `xmin` | `xid` | Token de versión, no nulo |

El índice `ux_proveedores_nombre_normalizado` es único.

## Duplicados y concurrencia

El servicio realiza una comprobación previa mediante
`ExisteConNombreNormalizadoAsync`, pero esa comprobación no sustituye la
restricción de la base: dos solicitudes concurrentes pueden observar que el
nombre todavía no existe.

El índice único de PostgreSQL resuelve esa condición de carrera. Si
`SaveChangesAsync` recibe una violación de unicidad correspondiente a
`ux_proveedores_nombre_normalizado`, el repositorio:

1. separa la entidad del contexto;
2. traduce el error de PostgreSQL a `ProveedorDuplicadoException`;
3. evita exponer detalles de infraestructura a las capas superiores.

`xmin` está configurado como token de concurrencia para futuras operaciones de
actualización, aunque el módulo actual todavía no implementa actualizaciones.

## Configuración

API y Web requieren la clave `ConnectionStrings:Licitaciones`. La cadena no se
almacena en el repositorio. La configuración local y la aplicación de
migraciones se explican en [Desarrollo local](../desarrollo-local.md).
