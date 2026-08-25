# Docker

## Docker Compose

`docker-compose.yml` define tres servicios: PostgreSQL, API y Web.

### PostgreSQL

- Servicio: `postgres`.
- Imagen: `postgres:16-alpine`.
- Puerto interno y externo predeterminado: `5432`.
- Volumen persistente: `licitaciones-postgres`, montado en
  `/var/lib/postgresql/data`.
- Health check: `pg_isready` usando el usuario y la base configurados.

### API

- Servicio: `api`.
- Construcción: `Dockerfile.api`.
- Puerto interno: `8080`.
- Puerto externo predeterminado: `8081`.
- Health check: `curl -f http://localhost:8080/health`.
- Depende de PostgreSQL saludable.
- Usa `ASPNETCORE_ENVIRONMENT=Production` y la cadena de conexión externa.

### Web

- Servicio: `web`.
- Construcción: `Dockerfile.web`.
- Puerto interno: `8080`.
- Puerto externo predeterminado: `8080`.
- Health check: `curl -f http://localhost:8080/health`.
- Depende de PostgreSQL saludable.
- Usa `ASPNETCORE_ENVIRONMENT=Production` y la cadena de conexión externa.

Los Dockerfiles usan imágenes .NET 9 SDK para compilación y ASP.NET 9 para
runtime. El contenedor de runtime instala `curl` para los health checks.

## Variables configurables

- `POSTGRES_DB`, predeterminada `licitaciones`.
- `POSTGRES_USER`, predeterminada `postgres`.
- `POSTGRES_PASSWORD`, predeterminada `postgres` en Compose; no representa una
  credencial de producción.
- `POSTGRES_PORT`, predeterminada `5432`.
- `API_PORT`, predeterminada `8081`.
- `WEB_PORT`, predeterminada `8080`.

La API y Web reciben `ConnectionStrings__Licitaciones` construida para usar el
servicio `postgres`. No se documentan credenciales reales.

## Comandos

Construir e iniciar:

```powershell
docker compose up -d --build
```

Ver estado:

```powershell
docker compose ps
```

Ver logs:

```powershell
docker compose logs
```

Detener servicios:

```powershell
docker compose down
```

`docker compose down -v` elimina también el volumen
`licitaciones-postgres` y los datos persistidos de PostgreSQL.
