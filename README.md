# Sistema de Gestión de Licitaciones

## Descripción

Aplicación académica para centralizar la gestión de proveedores, licitaciones,
ofertas, niveles de aprobación y tipos de cambio. La solución ofrece una
interfaz Web MVC y una API REST, con persistencia PostgreSQL mediante Entity
Framework Core.

## Estado actual

El sistema implementa:

- CRUD de proveedores;
- CRUD de licitaciones;
- creación en estado `Borrador` y publicación de licitaciones;
- CRUD de ofertas y restricción de una oferta por proveedor y licitación;
- consulta de mejor oferta y cálculo de ahorro;
- niveles de aprobación, validación de traslapes y determinación por monto;
- tipos de cambio CRC/USD, un único tipo activo y conversión CRC → USD;
- Web MVC y API;
- PostgreSQL y EF Core;
- Docker Compose y manifiestos Kubernetes;
- integración continua con GitHub Actions;
- pruebas UnitTests, FunctionalTests, IntegrationTests y EndToEndTests.

Las fechas se almacenan internamente en UTC y se presentan en
`America/Costa_Rica`.

## Tecnologías

- .NET 9.
- ASP.NET Core MVC.
- ASP.NET Core Minimal API.
- Entity Framework Core 9.
- PostgreSQL 16.
- xUnit.
- Testcontainers.
- Playwright y Chromium.
- Docker y Docker Compose.
- Kubernetes.
- GitHub Actions.

## Arquitectura

La solución separa el dominio, los casos de uso, la persistencia y las
interfaces de entrada:

- `Licitaciones.Domain`: entidades e invariantes.
- `Licitaciones.Application`: servicios, contratos y DTOs.
- `Licitaciones.Infrastructure`: EF Core, repositorios, migraciones y
  PostgreSQL.
- `Licitaciones.Web`: interfaz ASP.NET Core MVC.
- `Licitaciones.Api`: API REST Minimal API.

Consulte [Arquitectura general](docs/arquitectura-general.md) y [Modelo de
datos](docs/modelo-datos.md).

## Módulos

- **Proveedores:** catálogo y operaciones CRUD.
- **Licitaciones:** creación en Borrador, publicación y operaciones CRUD.
- **Ofertas:** propuestas, unicidad por proveedor/licitación, mejor oferta y
  ahorro.
- **Niveles de aprobación:** rangos, traslapes y determinación por monto.
- **Tipos de cambio:** administración de valores CRC/USD y conversión.

## Ejecución local

### Con Docker Compose

Desde la raíz del repositorio:

```powershell
docker compose up -d --build
docker compose ps
```

URLs:

- Web: <http://localhost:8080>
- API: <http://localhost:8081>
- Health Web: <http://localhost:8080/health>
- Health API: <http://localhost:8081/health>

Para revisar logs:

```powershell
docker compose logs
```

Para detener los servicios:

```powershell
docker compose down
```

`docker compose down -v` también elimina el volumen persistente y los datos de
PostgreSQL.

### Ejecución directa con .NET

Se requiere una cadena PostgreSQL en `ConnectionStrings:Licitaciones`. La
configuración segura y las migraciones se explican en [Desarrollo
local](docs/desarrollo-local.md). Después puede iniciarse Web o API con
`dotnet run --project` sobre el proyecto correspondiente.

## Pruebas

```powershell
dotnet test Licitaciones.sln
```

La última ejecución manual confirmada registró **195 pruebas correctas y 0
fallidas**.

## Documentación

- [Índice de documentación](docs/README.md)
- [Visión y alcance](docs/vision-alcance.md)
- [Historias de usuario](docs/historias-usuario.md)
- [Plan XP](docs/plan-xp.md)
- [Bitácora XP](docs/bitacora-xp.md)
- [Arquitectura general](docs/arquitectura-general.md)
- [Modelo de datos](docs/modelo-datos.md)
- [API](docs/api.md)
- [Pruebas](docs/pruebas.md)
- [Docker](docs/docker.md)
- [Kubernetes](docs/kubernetes.md)
- [Uso de IA](docs/uso-ia.md)
- [Integración de módulos](docs/integracion-modulos.md)
- [Desarrollo local](docs/desarrollo-local.md)
- [Módulo de licitaciones](docs/modulos/licitaciones.md)
- [Módulo de proveedores](docs/modulos/proveedores.md)
- [Colección API HTTP](docs/api.http)

