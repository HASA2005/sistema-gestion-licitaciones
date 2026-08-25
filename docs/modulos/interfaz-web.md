@'
# Interfaz Web

## Propósito
Proporcionar una interfaz ASP.NET Core MVC para operar el Sistema de Gestión de Licitaciones.

## Responsabilidades
Permite administrar proveedores, licitaciones, ofertas, niveles de aprobación y tipos de cambio mediante vistas, formularios y acciones MVC.

La lógica de negocio se mantiene en la capa Application y no en los controladores.

## Dependencias
- Licitaciones.Application
- Licitaciones.Infrastructure
- ASP.NET Core MVC
- Razor
- HTML, CSS y JavaScript

## Entradas
- Formularios Web.
- Parámetros de ruta.
- Acciones realizadas por el usuario.

## Salidas
- Vistas Razor.
- Mensajes de éxito, advertencia y error.
- Listados, formularios y detalles.

## Reglas
- CRC se conserva como moneda fuente de verdad.
- Las fechas se presentan en la zona horaria America/Costa_Rica.
- Las reglas de negocio son validadas por Application.
- Los errores esperados se muestran de forma controlada.

## Errores
Los errores de validación y negocio se presentan sin exponer información técnica interna.

## Pruebas
La interfaz se verifica mediante FunctionalTests y pruebas EndToEnd con Playwright.
'@ | Set-Content .\docs\modulos\interfaz-web.md -Encoding UTF8