@'
# API REST

## Propósito
Exponer las operaciones principales del Sistema de Gestión de Licitaciones mediante una API REST versionada.

## Responsabilidades
La API permite operar proveedores, licitaciones, ofertas, niveles de aprobación y tipos de cambio.

También expone operaciones específicas como publicación de licitaciones, mejor oferta, conversión CRC/USD y determinación del nivel de aprobación.

## Dependencias
- Licitaciones.Application
- Licitaciones.Infrastructure
- ASP.NET Core
- OpenAPI
- Problem Details

## Entradas
- Parámetros de ruta.
- Query string.
- Contratos HTTP de entrada.

## Salidas
- DTO.
- Respuestas HTTP.
- Problem Details para errores controlados.

## Reglas
- 400: entrada o regla de negocio inválida.
- 404: recurso inexistente.
- 409: duplicado, concurrencia o conflicto conocido.
- 500: error inesperado o no reconocido.

## Errores
El manejador global evita exponer detalles técnicos de las excepciones.

## Pruebas
La API se verifica mediante FunctionalTests, IntegrationTests y pruebas específicas del manejador global de errores.

Para mayor detalle consultar [API](../api.md).
'@ | Set-Content .\docs\modulos\api-rest.md -Encoding UTF8
