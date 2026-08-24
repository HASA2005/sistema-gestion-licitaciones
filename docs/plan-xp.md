# Plan de Extreme Programming

## Propósito

El proyecto se desarrolla exclusivamente mediante Extreme Programming (XP).
El plan organiza cuatro iteraciones cortas, pequeñas liberaciones ejecutables,
TDD, integración continua, diseño simple, refactorización y retroalimentación.
El alcance de cada iteración se confirma mediante Planning Game antes de
programar y puede ajustarse usando evidencia de la iteración anterior.

## Reglas de trabajo

1. Crear o seleccionar una historia con prioridad, estimación y criterios de
   aceptación verificables.
2. Dividirla en el incremento vertical más pequeño que aporte valor.
3. Escribir una prueba que falle por la razón esperada (RED).
4. Implementar el mínimo necesario para aprobarla (GREEN).
5. Refactorizar sin cambiar el comportamiento observable.
6. Integrar mediante commits pequeños y Pull Request.
7. Mantener compilación, pruebas y GitHub Actions satisfactorios.
8. Actualizar trazabilidad, bitácora y documentación del módulo.
9. Obtener retroalimentación y ajustar la siguiente iteración.

## Plan de liberación

El siguiente plan expresa objetivos y candidatos. Las historias definitivas de
cada iteración se acuerdan al comenzar esa iteración.

| Iteración | Objetivo de valor | Candidatos principales | Liberación |
| --- | --- | --- | --- |
| XP 1 | Registrar un proveedor por API y MVC con persistencia real | Estructura modular, HU-01, PostgreSQL, CI básica | `v0.1.0` |
| XP 2 | Preparar y administrar licitaciones antes de recibir ofertas | Crear licitación en Borrador, unicidad de código, presupuesto y fecha; transiciones en una historia separada; primera prueba de navegador | `v0.2.0` |
| XP 3 | Registrar y evaluar ofertas válidas | Ofertas, restricciones por estado y vencimiento, mejor oferta, clasificación y niveles de aprobación | `v0.3.0` |
| XP 4 | Completar operación, despliegue y entrega | Tipo de cambio y CRC/USD, operaciones restantes, UX, Docker Compose, Kubernetes, pruebas E2E y documentación final | `v1.0.0` |

Las tareas de calidad, documentación, CI y contenedores se adelantarán cuando
reduzcan riesgo; no se reservarán íntegramente para la última iteración.

## Plan de la Iteración XP 1

**Periodo observado:** 23 y 24 de agosto de 2026.

**Objetivo:** entregar un recorrido demostrable que registre un proveedor desde
MVC o API, aplique las mismas reglas de negocio y persista en PostgreSQL.

| Trabajo | Referencia | Prioridad | Estado al cierre |
| --- | --- | --- | --- |
| TT-01 Inicializar estructura .NET | [#2](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/2) | Alta | Terminado |
| TT-02 Configurar dependencias para TDD | [#3](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/3) | Alta | Terminado |
| HU-01 Registrar proveedor | [#1](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/1) | Alta | Terminado |
| TT-03 Configurar persistencia de proveedores | [#6](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/6) | Alta | Terminado |
| TT-04 Configurar integración continua básica | [#11](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/11) | Alta | Terminado |
| TT-05 Documentar cierre y pequeña liberación | [#14](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/14) | Alta | Terminado |

### Criterios de salida

- [x] HU-01 cumple sus criterios en Domain, Application, Infrastructure, API y
  Web.
- [x] PostgreSQL 16 se verifica mediante Testcontainers.
- [x] La solución compila en Release sin errores ni advertencias.
- [x] Las 46 pruebas se ejecutan satisfactoriamente.
- [x] GitHub Actions pasa tanto en Pull Request como en `main`.
- [x] La documentación de cierre se integra en `main` mediante el PR #15.
- [x] Se crea localmente la etiqueta `v0.1.0` desde el commit de cierre.
- [ ] Se publica la etiqueta `v0.1.0` en GitHub.

## Plan de la Iteración XP 2

**Inicio observado:** 24 de agosto de 2026.

**Objetivo:** permitir preparar licitaciones antes de publicarlas y fortalecer
la calidad automatizada del recorrido Web.

| Trabajo | Referencia | Prioridad | Estimación | Estado |
| --- | --- | --- | ---: | --- |
| HU-02 Crear licitación en Borrador | [#16](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/16) | Alta | 8 puntos | Terminada técnicamente; pendiente de integración |
| Primera prueba real de navegador | Tarea técnica por crear | Alta | Por estimar | Pendiente |
| Reporte y puertas de cobertura | Tarea técnica por crear | Alta | Por estimar | Pendiente |

HU-02 incluye código único, título, presupuesto CRC positivo, fecha de cierre,
estado inicial `Borrador`, auditoría, concurrencia, PostgreSQL, API y MVC. La
publicación y el cierre se tratarán como historias posteriores por poseer reglas
de transición propias.

Los 8 puntos solo se sumarán a la velocidad observada cuando HU-02 esté
integrada en `main` y su CI sea satisfactoria.
