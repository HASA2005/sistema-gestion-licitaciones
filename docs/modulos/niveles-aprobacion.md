# Módulo de niveles de aprobación

## Propósito

Administrar rangos monetarios y determinar el responsable aplicable a un monto.

## Datos

| Campo | Tipo lógico | Descripción |
|---|---|---|
| `Id` | `Guid` | Identificador generado por el dominio. |
| `Responsable` | `string` | Responsable del nivel. |
| `MontoMinimoCrc` | `decimal` | Límite inferior del rango. |
| `MontoMaximoCrc` | `decimal?` | Límite superior opcional. |

La entidad no contiene campos de auditoría ni una referencia persistida a una
licitación u oferta.

## Funcionalidades

- Crear niveles.
- Listar niveles.
- Consultar un nivel.
- Editar niveles.
- Eliminar niveles.
- Determinar el nivel que incluye un monto.

## Reglas

- El responsable es obligatorio.
- El monto mínimo debe ser positivo.
- El monto máximo es opcional.
- Si existe, el máximo no puede ser menor que el mínimo.
- Los montos admiten como máximo dos decimales.
- Los rangos son inclusivos en ambos límites.
- No se permiten traslapes con otros rangos.
- La determinación busca dinámicamente el primer nivel cuyo rango incluye el
  monto consultado.

## Integración

No existe relación persistida con `Licitacion` u `Oferta`. La determinación
funciona como servicio de aplicación y API independiente; no se asigna
automáticamente un nivel a una licitación ni se guarda esa asignación.

## Web

`NivelesAprobacionController` expone el CRUD Web:

- `GET /niveles-aprobacion`;
- `GET /niveles-aprobacion/crear` y `POST /niveles-aprobacion/crear`;
- `GET /niveles-aprobacion/{id}/editar` y
  `POST /niveles-aprobacion/{id}/editar`;
- `POST /niveles-aprobacion/{id}/eliminar`.

No existe una acción Web específica para determinar el nivel por monto; esa
operación se expone mediante el servicio y la API.

## API

La documentación general está en [docs/api.md](../api.md). El módulo expone:

- `POST /api/v1/niveles-aprobacion`;
- `GET /api/v1/niveles-aprobacion`;
- `GET /api/v1/niveles-aprobacion/{id}`;
- `PUT /api/v1/niveles-aprobacion/{id}`;
- `DELETE /api/v1/niveles-aprobacion/{id}`;
- `GET /api/v1/niveles-aprobacion/determinar/{monto}`.

## Persistencia

La migración `20260824211656_CrearNivelesAprobacion` crea la tabla
`niveles_aprobacion`. Los campos monetarios se configuran como
`numeric(18,2)` y `Responsable` tiene longitud máxima de 150 caracteres en EF
Core.

## Pruebas

Las pruebas unitarias existentes cubren rangos válidos, límites, rangos
abiertos, montos inválidos y traslapes mediante `NivelAprobacionTests` y
`NivelAprobacionServiceTests`. El servicio también contiene las operaciones de
CRUD y determinación; no se localizó una clase funcional específica de este
módulo en el inventario de pruebas.

El detalle general está en [docs/pruebas.md](../pruebas.md).
