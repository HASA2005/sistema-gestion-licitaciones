# Visión y alcance

## Problema

La gestión de procesos de licitación requiere mantener en un mismo sistema la
información de proveedores, licitaciones, ofertas y valores auxiliares para su
evaluación. Sin una gestión centralizada, estos datos y validaciones pueden
quedar dispersos, dificultando el seguimiento del proceso, la comparación de
propuestas y la conservación de reglas de integridad.

Este proyecto implementa un Sistema de Gestión de Licitaciones con una API,
una interfaz Web MVC y persistencia PostgreSQL.

## Objetivo general

Desarrollar un Sistema de Gestión de Licitaciones que permita gestionar:

- proveedores;
- licitaciones;
- ofertas;
- niveles de aprobación;
- tipos de cambio.

## Objetivos específicos

- Registrar, consultar, listar, editar y eliminar proveedores.
- Crear licitaciones en estado `Borrador` y publicarlas cuando cumplen las
  reglas establecidas.
- Gestionar ofertas asociadas a proveedores y licitaciones.
- Impedir más de una oferta del mismo proveedor para una licitación.
- Consultar la mejor oferta y calcular el ahorro respecto al presupuesto.
- Administrar niveles de aprobación con rangos monetarios sin traslapes.
- Determinar el nivel aplicable a partir de un monto.
- Administrar tipos de cambio CRC/USD y convertir montos CRC a USD.
- Mantener la integridad y persistencia de los datos mediante EF Core y
  PostgreSQL.
- Ofrecer acceso mediante Web MVC y API.
- Verificar el sistema con pruebas automatizadas, integración continua, Docker
  Compose y manifiestos Kubernetes.

## Usuarios y actores funcionales

El sistema se describe desde las siguientes responsabilidades funcionales:

- **Encargado de compras:** administra proveedores, licitaciones y ofertas.
- **Responsable de aprobación:** consulta y configura niveles de aprobación.
- **Usuario administrativo:** gestiona información operativa, incluyendo tipos
  de cambio y operaciones CRUD disponibles.

Estos nombres describen usos funcionales del sistema. El código actual no
implementa autenticación, cuentas, inicio de sesión, autorización ni permisos
por rol.

## Alcance incluido

El alcance implementado comprende:

- CRUD de proveedores.
- CRUD de licitaciones.
- Creación de licitaciones en estado `Borrador`.
- Publicación de licitaciones.
- CRUD de ofertas.
- Una sola oferta por combinación de proveedor y licitación.
- Consulta de mejor oferta.
- Cálculo de ahorro respecto al presupuesto.
- Niveles de aprobación.
- Validación de traslapes entre rangos.
- Determinación del nivel por monto.
- Tipos de cambio CRC/USD.
- Un único tipo de cambio activo.
- Conversión CRC → USD.
- Interfaz Web MVC con formularios, listados, detalles y manejo de errores
  esperables.
- API REST con OpenAPI, Problem Details y endpoint de salud.
- Persistencia PostgreSQL mediante EF Core.
- Docker Compose para API, Web y PostgreSQL.
- Manifiestos Kubernetes para API, Web y PostgreSQL.
- Pruebas UnitTests, FunctionalTests, IntegrationTests y EndToEndTests.
- Integración continua con GitHub Actions.
- Presentación de fechas en `America/Costa_Rica` y almacenamiento de fechas en
  UTC.

## Fuera de alcance

No se implementaron los siguientes elementos:

- autenticación;
- usuarios;
- roles;
- notificaciones;
- firma digital;
- integración con SICOP u otras plataformas gubernamentales;
- reportes avanzados;
- despliegue productivo real en Kubernetes.

## Límites verificables

El alcance se basa en las entidades, servicios, controladores, endpoints,
configuraciones, migraciones y pruebas existentes en el repositorio. No se
afirman reglas de negocio, actores técnicos o integraciones externas que no
estén representados en esos artefactos.
