# Desarrollo local

Las aplicaciones Web y API necesitan una conexión PostgreSQL configurada con la
clave `ConnectionStrings:Licitaciones`. La cadena de conexión es un secreto de
cada entorno y no debe guardarse en `appsettings.json`, archivos `.env` ni
commits.

## Configurar PostgreSQL en PowerShell

Desde la raíz del repositorio, solicite la cadena sin escribirla dentro del
comando ni dejarla en el historial:

```powershell
$env:ConnectionStrings__Licitaciones = Read-Host "Cadena de conexión PostgreSQL"
```

La base indicada debe existir y ser accesible. Aplique las migraciones de EF
Core antes de utilizar la aplicación:

```powershell
dotnet ef database update `
  --project .\src\Licitaciones.Infrastructure\Licitaciones.Infrastructure.csproj `
  --startup-project .\src\Licitaciones.Web\Licitaciones.Web.csproj
```

Luego puede iniciar la interfaz MVC:

```powershell
dotnet run --project .\src\Licitaciones.Web\Licitaciones.Web.csproj
```

Las pantallas implementadas están disponibles en `/proveedores/registrar` y
`/licitaciones/crear`. La API usa la misma variable de entorno y expone
`POST /api/v1/proveedores` y `POST /api/v1/licitaciones`.

Cuando termine la sesión, elimine la variable del proceso actual:

```powershell
Remove-Item Env:ConnectionStrings__Licitaciones
```

## Verificación

Docker Desktop debe estar iniciado para las pruebas que usan Testcontainers.

```powershell
dotnet build .\Licitaciones.sln
dotnet test .\Licitaciones.sln --no-build
```
