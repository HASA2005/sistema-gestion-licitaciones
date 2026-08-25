@'
# Persistencia

## Propósito
Persistir la información del sistema utilizando Entity Framework Core 9 y PostgreSQL.

## Responsabilidades
Infrastructure contiene el DbContext, configuraciones de Entity Framework Core, repositorios y migraciones.

## Dependencias
- Entity Framework Core 9.
- PostgreSQL 16.
- Npgsql.
- Licitaciones.Domain.
- Licitaciones.Application.

## Entradas
Entidades y operaciones solicitadas por los servicios de Application.

## Salidas
Datos persistidos y recuperados desde PostgreSQL.

## Reglas
- Los montos utilizan precisión decimal.
- Se aplican índices y restricciones de unicidad.
- Las relaciones utilizan claves foráneas.
- Las fechas se almacenan en UTC.
- Se utiliza concurrencia optimista mediante xmin donde corresponde.
- Las restricciones de PostgreSQL complementan las reglas de Domain y Application.

## Errores
Los conflictos relevantes de persistencia son traducidos a errores controlados antes de llegar al usuario.

## Pruebas
Las IntegrationTests utilizan Testcontainers con PostgreSQL real para validar persistencia, migraciones, restricciones y repositorios.

Para mayor detalle consultar [Modelo de datos](../modelo-datos.md).
'@ | Set-Content .\docs\modulos\persistencia.md -Encoding UTF8