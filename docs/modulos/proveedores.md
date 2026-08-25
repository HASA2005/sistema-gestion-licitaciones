# Módulo de proveedores

## Propósito

Mantener el catálogo de proveedores utilizados en las ofertas.

## Funcionalidades

- Registrar proveedores.
- Listar proveedores.
- Consultar un proveedor.
- Editar proveedores.
- Eliminar proveedores cuando no tengan ofertas asociadas.
- Exponer las operaciones mediante Web MVC y API.

## Datos principales

| Campo | Tipo lógico | Descripción |
|---|---|---|
| `Id` | `Guid` | Identificador generado por el dominio. |
| `Nombre` | `string` | Nombre limpio mostrado al usuario. |
| `NombreNormalizado` | `string` | Valor normalizado usado para comparar y asegurar unicidad. |
| `CreatedAt` | `DateTimeOffset` | Auditoría de creación en UTC. |
| `UpdatedAt` | `DateTimeOffset` | Auditoría de actualización en UTC. |
| `Version` | `uint` | Token de concurrencia asociado a `xmin`. |

## Reglas

- El nombre es obligatorio.
- Se eliminan espacios laterales y se reducen espacios repetidos.
- Se aplica Unicode Form C.
- Se permiten letras y números Unicode, espacios, punto, coma y paréntesis.
- El nombre normalizado se compara sin distinguir mayúsculas y minúsculas.
- El nombre normalizado es único.
- Los duplicados concurrentes se controlan mediante el caso de uso y el índice
  único de PostgreSQL.
- No se permite eliminar un proveedor que tenga ofertas asociadas.
- Las actualizaciones usan concurrencia optimista mediante `xmin` en la
  persistencia.

## Web

El registro inicial se expone mediante `ProveedoresController`:

- `GET /proveedores/registrar` muestra el formulario.
- `POST /proveedores/registrar` valida antiforgery, registra y redirige con
  confirmación.

El CRUD administrativo se expone mediante `CrudController`:

- `GET /gestion/proveedores` lista proveedores.
- `GET /gestion/proveedores/{id}` muestra el detalle.
- `GET /gestion/proveedores/{id}/editar` muestra el formulario de edición.
- `POST /gestion/proveedores/{id}/editar` guarda la edición.
- `POST /gestion/proveedores/{id}/eliminar` elimina cuando la regla lo
  permite.

Los errores de validación y duplicidad se conservan en el flujo Web sin
mostrar una página genérica.

## API

La documentación general está en [docs/api.md](../api.md). El módulo expone:

- `POST /api/v1/proveedores`;
- `GET /api/v1/proveedores`;
- `GET /api/v1/proveedores/{id}`;
- `PUT /api/v1/proveedores/{id}`;
- `DELETE /api/v1/proveedores/{id}`.

## Persistencia

La persistencia usa EF Core y PostgreSQL. La migración
`20260824003850_CrearProveedores` crea la tabla `proveedores`, su clave
primaria, campos de auditoría, columna `xmin` e índice único
`ux_proveedores_nombre_normalizado`.

## Pruebas

Las pruebas relacionadas cubren:

- Normalización y validación en `ProveedorTests`.
- Registro y duplicados en `RegistrarProveedorServiceTests`.
- Repositorio, unicidad, configuración del modelo y PostgreSQL en
  `ProveedorRepositoryTests` y `ProveedorModelConfigurationTests`.
- Registro Web, antiforgery, validaciones y duplicados en
  `RegistrarProveedorWebTests`.
- Listado, consulta, edición, eliminación y regresiones Web en
  `GestionarProveedoresWebTests`.
- Registro API y respuestas controladas en
  `RegistrarProveedorEndpointTests`.
- Recorridos con infraestructura real en las pruebas de integración Web y API.

El detalle general está en [docs/pruebas.md](../pruebas.md).
