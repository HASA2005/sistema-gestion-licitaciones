# Módulo de ofertas

## Propósito

Registrar y comparar propuestas económicas de proveedores para licitaciones.

## Datos principales

| Campo | Tipo lógico | Descripción |
|---|---|---|
| `Id` | `Guid` | Identificador generado por el dominio. |
| `LicitacionId` | `Guid` | Identificador de la licitación asociada. |
| `ProveedorId` | `Guid` | Identificador del proveedor asociado. |
| `MontoCrc` | `decimal` | Monto de la propuesta en CRC. |
| `CreatedAt` | `DateTimeOffset` | Auditoría de creación en UTC. |
| `UpdatedAt` | `DateTimeOffset` | Auditoría de actualización en UTC. |

## Funcionalidades

- Crear, listar, consultar, editar y eliminar ofertas.
- Filtrar el listado por `licitacionId` o `proveedorId` cuando se proporcionan.
- Consultar la mejor oferta.
- Calcular el ahorro respecto al presupuesto.
- Devolver una clasificación según el ahorro.

## Reglas

- La licitación debe existir, estar publicada y no estar vencida.
- El proveedor debe existir.
- `MontoCrc` debe ser mayor que cero y tener como máximo dos decimales.
- El monto no puede superar el presupuesto de la licitación.
- El índice único impide más de una oferta por combinación de licitación y
  proveedor.
- La mejor oferta es la de menor monto válido.
- En caso de empate, se selecciona la oferta con `CreatedAt` más antiguo.
- El ahorro se calcula como la diferencia entre presupuesto y oferta, dividida
  por el presupuesto y expresada como porcentaje.
- La clasificación real es: `Oferta válida sin ahorro` cuando el ahorro es 0,
  `Oferta conveniente` cuando es mayor o igual a 10 %, y `Oferta aceptable` en
  los demás casos válidos.
- Editar y eliminar también requieren que la licitación siga publicada y
  abierta.

## Web

El controlador `OfertasController` expone:

- `GET /ofertas`: listado amigable.
- `GET /ofertas/crear` y `POST /ofertas/crear`: creación.
- `GET /ofertas/{id}`: detalle.
- `GET /ofertas/{id}/editar` y `POST /ofertas/{id}/editar`: edición.
- `POST /ofertas/{id}/eliminar`: eliminación.
- `GET /ofertas/licitacion/{licitacionId}/mejor`: mejor oferta.

Los formularios usan selectores amigables de licitación y proveedor. Muestran
código, título y nombre, sin exponer Guid cuando existe información
principalmente descriptiva. Los Guid se mantienen internamente para el
model-binding y las rutas de acciones.

## API

La documentación general está en [docs/api.md](../api.md). El módulo expone:

- `POST /api/v1/ofertas`;
- `GET /api/v1/ofertas` con filtros opcionales `licitacionId` y `proveedorId`;
- `GET /api/v1/ofertas/{id}`;
- `PUT /api/v1/ofertas/{id}`;
- `DELETE /api/v1/ofertas/{id}`;
- `GET /api/v1/ofertas/licitacion/{licitacionId}/mejor`.

## Persistencia

La migración `20260824203636_CrearOfertas` crea `ofertas`, con monto
`numeric(18,2)`, restricción `ck_ofertas_monto_positivo`, índice único
`ux_ofertas_licitacion_proveedor` y claves foráneas.

- La FK hacia `Licitacion` usa `DeleteBehavior.Cascade`.
- La FK hacia `Proveedor` usa `DeleteBehavior.Restrict`.
- El servicio de aplicación bloquea eliminar una licitación con ofertas, por lo
  que el cascade no forma parte del flujo normal de eliminación expuesto.

## Pruebas

- UnitTests: `OfertaTests` y pruebas CRUD de aplicación.
- FunctionalTests: `OfertaServiceFunctionalTests`.
- IntegrationTests: `OfertaRepositoryTests`, con PostgreSQL y restricciones de
  persistencia.

El detalle general está en [docs/pruebas.md](../pruebas.md).
