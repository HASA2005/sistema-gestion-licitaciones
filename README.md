# Sistema de GestiÃ³n de Licitaciones

## DescripciÃ³n

AplicaciÃ³n acadÃ©mica para centralizar la gestiÃ³n de proveedores, licitaciones,
ofertas, niveles de aprobaciÃ³n y tipos de cambio. La soluciÃ³n ofrece una
interfaz Web MVC y una API REST, con persistencia PostgreSQL mediante Entity
Framework Core.

## Estado actual

El sistema implementa:

- CRUD de proveedores;
- CRUD de licitaciones;
- creaciÃ³n en estado `Borrador` y publicaciÃ³n de licitaciones;
- CRUD de ofertas y restricciÃ³n de una oferta por proveedor y licitaciÃ³n;
- consulta de mejor oferta y cÃ¡lculo de ahorro;
- niveles de aprobaciÃ³n, validaciÃ³n de traslapes y determinaciÃ³n por monto;
- tipos de cambio CRC/USD, un Ãºnico tipo activo y conversiÃ³n CRC â†’ USD;
- Web MVC y API;
- PostgreSQL y EF Core;
- Docker Compose y manifiestos Kubernetes;
- integraciÃ³n continua con GitHub Actions;
- pruebas UnitTests, FunctionalTests, IntegrationTests y EndToEndTests.

Las fechas se almacenan internamente en UTC y se presentan en
`America/Costa_Rica`.

## TecnologÃ­as

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

La soluciÃ³n separa el dominio, los casos de uso, la persistencia y las
interfaces de entrada:

- `Licitaciones.Domain`: entidades e invariantes.
- `Licitaciones.Application`: servicios, contratos y DTOs.
- `Licitaciones.Infrastructure`: EF Core, repositorios, migraciones y
  PostgreSQL.
- `Licitaciones.Web`: interfaz ASP.NET Core MVC.
- `Licitaciones.Api`: API REST Minimal API.

Consulte [Arquitectura general](docs/arquitectura-general.md) y [Modelo de
datos](docs/modelo-datos.md).

## MÃ³dulos

- **Proveedores:** catÃ¡logo y operaciones CRUD.
- **Licitaciones:** creaciÃ³n en Borrador, publicaciÃ³n y operaciones CRUD.
- **Ofertas:** propuestas, unicidad por proveedor/licitaciÃ³n, mejor oferta y
  ahorro.
- **Niveles de aprobaciÃ³n:** rangos, traslapes y determinaciÃ³n por monto.
- **Tipos de cambio:** administraciÃ³n de valores CRC/USD y conversiÃ³n.

## EjecuciÃ³n local

### Con Docker Compose

Desde la raÃ­z del repositorio:

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

`docker compose down -v` tambiÃ©n elimina el volumen persistente y los datos de
PostgreSQL.

### EjecuciÃ³n directa con .NET

Se requiere una cadena PostgreSQL en `ConnectionStrings:Licitaciones`. La
configuraciÃ³n segura y las migraciones se explican en [Desarrollo
local](docs/desarrollo-local.md). DespuÃ©s puede iniciarse Web o API con
`dotnet run --project` sobre el proyecto correspondiente.

## Pruebas

```powershell
dotnet test Licitaciones.sln
```

La Ãºltima ejecuciÃ³n manual confirmada registrÃ³ **180 pruebas correctas y 0
fallidas**.

## DocumentaciÃ³n

- [Ãndice de documentaciÃ³n](docs/README.md)
- [VisiÃ³n y alcance](docs/vision-alcance.md)
- [Historias de usuario](docs/historias-usuario.md)
- [Plan XP](docs/plan-xp.md)
- [BitÃ¡cora XP](docs/bitacora-xp.md)
- [Arquitectura general](docs/arquitectura-general.md)
- [Modelo de datos](docs/modelo-datos.md)
- [API](docs/api.md)
- [Pruebas](docs/pruebas.md)
- [Docker](docs/docker.md)
- [Kubernetes](docs/kubernetes.md)
- [Uso de IA](docs/uso-ia.md)
- [IntegraciÃ³n de mÃ³dulos](docs/integracion-modulos.md)
- [Desarrollo local](docs/desarrollo-local.md)
- [MÃ³dulo de licitaciones](docs/modulos/licitaciones.md)
- [MÃ³dulo de proveedores](docs/modulos/proveedores.md)
- [ColecciÃ³n API HTTP](docs/api.http)

