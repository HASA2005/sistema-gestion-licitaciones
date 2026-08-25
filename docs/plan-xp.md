# Plan de Extreme Programming

## Propósito

El proyecto se desarrolló exclusivamente mediante Extreme Programming (XP). El plan usa iteraciones XP, historias de usuario, TDD, RED-GREEN-REFACTOR, simple design, refactoring, integración continua, small releases y feedback.

## Prácticas XP aplicadas

- Historias de usuario con criterios verificables.
- TDD cuando existe evidencia de pruebas seguidas por implementación en Git.
- RED-GREEN-REFACTOR para los incrementos con trazabilidad disponible.
- Diseño simple y separación Domain, Application, Infrastructure, API y Web.
- Integración continua con GitHub Actions.
- Small releases y Pull Requests pequeños.
- Pruebas automatizadas, pruebas de integración y pruebas E2E.
- Feedback mediante revisión manual de formularios, API y flujos Web.

## Iteración XP 1 — Proveedores y base técnica

**Objetivo:** entregar el primer recorrido vertical para registrar proveedores por API y Web, con persistencia real.

**Historias:** HU-01.

**Tareas técnicas verificables:** inicialización de la solución (#2), dependencias para pruebas (#3), persistencia (#6), integración continua (#11) y documentación de cierre (#14).

**Prácticas y evidencia:** TDD sobre normalización, validación, duplicados, persistencia, API y MVC; PostgreSQL con Testcontainers; workflow CI; small release `v0.1.0`.

**Criterio de salida:** recorrido de proveedores implementado, compilación y pruebas automatizadas satisfactorias, documentación de la iteración y CI configurada. La documentación histórica registra 46 casos para ese corte.

## Iteración XP 2 — Licitaciones, publicación y navegador

**Objetivo:** preparar licitaciones en Borrador, publicarlas y verificar el flujo Web completo.

**Historias:** HU-02 y HU-03.

**Tareas técnicas verificables:** creación de licitaciones (#16), publicación de licitaciones (#18) y prueba E2E del flujo Web (#20).

**Prácticas y evidencia:** TDD de invariantes, fechas UTC y concurrencia; PostgreSQL, API, MVC, antiforgery, PRG, OpenAPI y Playwright con Testcontainers.

**Criterio de salida:** creación en Borrador, publicación válida, fecha futura, control de concurrencia, pruebas funcionales e integración, y flujo E2E de navegador.

## Iteración XP 3 — Ofertas, aprobación y valores monetarios

**Objetivo:** completar la evaluación de propuestas y las reglas monetarias auxiliares.

**Historias:** HU-04, HU-05, HU-06 y HU-07.

**Tareas técnicas verificables:** ofertas (#22), niveles de aprobación (#24) y tipos de cambio (#26).

**Prácticas y evidencia:** diseño simple por servicios y repositorios, restricciones de PostgreSQL, validaciones de dominio, pruebas automatizadas y mensajes Web para errores de negocio esperables.

**Criterio de salida:** CRUD de ofertas, restricción única por licitación y proveedor, mejor oferta y ahorro; rangos de aprobación sin traslapes; tipo de cambio activo único y conversión CRC → USD.

## Iteración XP 4 — Operación completa, despliegue y estabilización

**Objetivo:** consolidar la experiencia Web, el CRUD restante, la ejecución contenedorizada, Kubernetes, CI y la documentación final.

**Historias:** HU-08 y los criterios operativos de HU-01, HU-04 y HU-07.

**Tareas técnicas verificables:** completar CRUD (#28), Docker Compose (#30), Kubernetes (#32), rediseño Web (#34) y estabilización final (#36).

**Prácticas y evidencia:** refactoring de presentación, manejo de errores Web, conversión de fechas a `America/Costa_Rica`, revisión manual, pruebas de regresión, Docker Compose, manifiestos Kubernetes, GitHub Actions y pruebas E2E.

**Criterio de salida:** operaciones visibles y navegables, errores esperables controlados, fechas presentadas en Costa Rica, infraestructura documentada, CI ejecutable y documentación académica actualizada. La última ejecución manual confirmada registró 195 pruebas correctas y 0 fallidas.

## Reglas de aceptación

Una historia se considera lista para cierre cuando sus criterios son verificables, las pruebas automatizadas relevantes pasan, el comportamiento observable es revisado y la documentación se actualiza. No se reconstruyen estimaciones, fechas o decisiones que no tengan evidencia.

