# Kubernetes

## Namespace

Todos los manifiestos usan el namespace `licitaciones`, definido en
`k8s/namespace.yaml`.

## API

`k8s/api-deployment.yaml` define el Deployment `licitaciones-api`:

- una réplica;
- imagen `licitaciones-api:latest`;
- puerto de contenedor `8080`;
- configuración desde `licitaciones-config`;
- cadena de conexión desde `licitaciones-db-secret`;
- readiness probe HTTP en `/health`, puerto 8080;
- liveness probe HTTP en `/health`, puerto 8080;
- requests: `100m` CPU y `128Mi` memoria;
- limits: `500m` CPU y `512Mi` memoria.

`k8s/api-service.yaml` expone el servicio `licitaciones-api` como `ClusterIP`,
con puerto 8080 dirigido al puerto 8080 del contenedor.

## Web

`k8s/app-deployment.yaml` define el Deployment `licitaciones-web`:

- una réplica;
- imagen `licitaciones-web:latest`;
- puerto de contenedor `8080`;
- configuración desde `licitaciones-config`;
- cadena de conexión desde `licitaciones-db-secret`;
- readiness probe HTTP en `/health`, puerto 8080;
- liveness probe HTTP en `/health`, puerto 8080;
- requests: `100m` CPU y `128Mi` memoria;
- limits: `500m` CPU y `512Mi` memoria.

`k8s/app-service.yaml` expone el servicio `licitaciones-web` como `ClusterIP`,
con puerto 8080 dirigido al puerto 8080 del contenedor.

## PostgreSQL

`k8s/postgres-statefulset.yaml` define el StatefulSet `postgres`:

- una réplica;
- imagen `postgres:16-alpine`;
- puerto 5432;
- Service headless llamado `postgres`;
- readiness y liveness probes con `pg_isready`;
- requests: `100m` CPU y `256Mi` memoria;
- limits: `500m` CPU y `512Mi` memoria;
- volumen montado en `/var/lib/postgresql/data` mediante el PVC
  `postgres-data`.

`k8s/postgres-service.yaml` define el Service headless con `clusterIP: None` y
puerto 5432.

`k8s/postgres-pvc.yaml` solicita:

- almacenamiento de 5Gi;
- acceso `ReadWriteOnce`.

## Configuración y secretos

`k8s/app-configmap.yaml` define:

- `ASPNETCORE_ENVIRONMENT=Production`;
- `POSTGRES_HOST=postgres`;
- `POSTGRES_PORT=5432`;
- `POSTGRES_DATABASE=licitaciones`.

`k8s/app-secret.example.yaml` es un ejemplo de Secret Opaque. Contiene
placeholders para `POSTGRES_USER`, `POSTGRES_PASSWORD` y
`CONNECTION_STRING`. Los valores reales deben configurarse fuera del
repositorio.

## Orden de aplicación

Por las referencias entre manifiestos, un orden razonable es:

1. `namespace.yaml`;
2. `app-configmap.yaml` y el Secret real basado en
   `app-secret.example.yaml`;
3. `postgres-pvc.yaml`, `postgres-service.yaml` y
   `postgres-statefulset.yaml`;
4. `api-deployment.yaml`, `api-service.yaml`, `app-deployment.yaml` y
   `app-service.yaml`.

Este orden se deduce de las referencias a namespace, ConfigMap, Secret, PVC y
Services. Los archivos contienen manifiestos, pero el repositorio no aporta
evidencia de un despliegue productivo real en un clúster Kubernetes.
