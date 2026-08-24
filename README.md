# Sistema de Gestión de Licitaciones

Aplicación académica desarrollada con .NET 9, ASP.NET Core, Entity Framework
Core y PostgreSQL mediante prácticas de Extreme Programming.

## Estado actual

La Iteración XP 1 implementa el registro de proveedores de extremo a extremo:

- reglas de dominio y normalización Unicode;
- caso de uso compartido por API y MVC;
- persistencia PostgreSQL con unicidad y control de concurrencia;
- `POST /api/v1/proveedores` con Problem Details;
- formulario `/proveedores/registrar` con antiforgery;
- 46 casos automatizados y CI con GitHub Actions.

La Iteración XP 2 agrega la creación de licitaciones en Borrador y la transición
a `Publicada` mediante API y MVC, persistencia `numeric(18,2)`, calendario en
hora de Costa Rica, estados sembrados, unicidad y concurrencia `xmin`. HU-02 ya
está integrada; HU-03 está terminada técnicamente y pendiente de integración.
La verificación consolidada ejecuta 145 casos: 64 unitarios, 65 funcionales y
16 de integración.

Las operaciones restantes se desarrollarán en incrementos posteriores. La
etiqueta `v0.1.0`, que identifica el cierre de la Iteración XP 1, ya está
publicada en GitHub.

## Requisitos

- SDK de .NET 9.
- Una instancia accesible de PostgreSQL para ejecutar API y Web.
- Docker Desktop, o un motor Docker compatible, para las pruebas de integración
  con Testcontainers.

## Verificación rápida

```powershell
dotnet restore .\Licitaciones.sln
dotnet build .\Licitaciones.sln --configuration Release --no-restore
dotnet test .\Licitaciones.sln --configuration Release --no-build --no-restore
```

La configuración segura de la base de datos y los comandos para iniciar API y
Web están en [Desarrollo local](docs/desarrollo-local.md).

## Documentación

Consulte el [índice de documentación](docs/README.md) para revisar el plan XP,
las historias, la bitácora, la API, las pruebas, los módulos y el uso de
inteligencia artificial.
