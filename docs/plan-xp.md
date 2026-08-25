# Plan de Extreme Programming

## PropÃ³sito

El proyecto se desarrollÃ³ exclusivamente mediante Extreme Programming (XP). El plan usa iteraciones XP, historias de usuario, TDD, RED-GREEN-REFACTOR, simple design, refactoring, integraciÃ³n continua, small releases y feedback.

## PrÃ¡cticas XP aplicadas

- Historias de usuario con criterios verificables.
- TDD cuando existe evidencia de pruebas seguidas por implementaciÃ³n en Git.
- RED-GREEN-REFACTOR para los incrementos con trazabilidad disponible.
- DiseÃ±o simple y separaciÃ³n Domain, Application, Infrastructure, API y Web.
- IntegraciÃ³n continua con GitHub Actions.
- Small releases y Pull Requests pequeÃ±os.
- Pruebas automatizadas, pruebas de integraciÃ³n y pruebas E2E.
- Feedback mediante revisiÃ³n manual de formularios, API y flujos Web.

## IteraciÃ³n XP 1 â€” Proveedores y base tÃ©cnica

**Objetivo:** entregar el primer recorrido vertical para registrar proveedores por API y Web, con persistencia real.

**Historias:** HU-01.

**Tareas tÃ©cnicas verificables:** inicializaciÃ³n de la soluciÃ³n (#2), dependencias para pruebas (#3), persistencia (#6), integraciÃ³n continua (#11) y documentaciÃ³n de cierre (#14).

**PrÃ¡cticas y evidencia:** TDD sobre normalizaciÃ³n, validaciÃ³n, duplicados, persistencia, API y MVC; PostgreSQL con Testcontainers; workflow CI; small release `v0.1.0`.

**Criterio de salida:** recorrido de proveedores implementado, compilaciÃ³n y pruebas automatizadas satisfactorias, documentaciÃ³n de la iteraciÃ³n y CI configurada. La documentaciÃ³n histÃ³rica registra 46 casos para ese corte.

## IteraciÃ³n XP 2 â€” Licitaciones, publicaciÃ³n y navegador

**Objetivo:** preparar licitaciones en Borrador, publicarlas y verificar el flujo Web completo.

**Historias:** HU-02 y HU-03.

**Tareas tÃ©cnicas verificables:** creaciÃ³n de licitaciones (#16), publicaciÃ³n de licitaciones (#18) y prueba E2E del flujo Web (#20).

**PrÃ¡cticas y evidencia:** TDD de invariantes, fechas UTC y concurrencia; PostgreSQL, API, MVC, antiforgery, PRG, OpenAPI y Playwright con Testcontainers.

**Criterio de salida:** creaciÃ³n en Borrador, publicaciÃ³n vÃ¡lida, fecha futura, control de concurrencia, pruebas funcionales e integraciÃ³n, y flujo E2E de navegador.

## IteraciÃ³n XP 3 â€” Ofertas, aprobaciÃ³n y valores monetarios

**Objetivo:** completar la evaluaciÃ³n de propuestas y las reglas monetarias auxiliares.

**Historias:** HU-04, HU-05, HU-06 y HU-07.

**Tareas tÃ©cnicas verificables:** ofertas (#22), niveles de aprobaciÃ³n (#24) y tipos de cambio (#26).

**PrÃ¡cticas y evidencia:** diseÃ±o simple por servicios y repositorios, restricciones de PostgreSQL, validaciones de dominio, pruebas automatizadas y mensajes Web para errores de negocio esperables.

**Criterio de salida:** CRUD de ofertas, restricciÃ³n Ãºnica por licitaciÃ³n y proveedor, mejor oferta y ahorro; rangos de aprobaciÃ³n sin traslapes; tipo de cambio activo Ãºnico y conversiÃ³n CRC â†’ USD.

## IteraciÃ³n XP 4 â€” OperaciÃ³n completa, despliegue y estabilizaciÃ³n

**Objetivo:** consolidar la experiencia Web, el CRUD restante, la ejecuciÃ³n contenedorizada, Kubernetes, CI y la documentaciÃ³n final.

**Historias:** HU-08 y los criterios operativos de HU-01, HU-04 y HU-07.

**Tareas tÃ©cnicas verificables:** completar CRUD (#28), Docker Compose (#30), Kubernetes (#32), rediseÃ±o Web (#34) y estabilizaciÃ³n final (#36).

**PrÃ¡cticas y evidencia:** refactoring de presentaciÃ³n, manejo de errores Web, conversiÃ³n de fechas a `America/Costa_Rica`, revisiÃ³n manual, pruebas de regresiÃ³n, Docker Compose, manifiestos Kubernetes, GitHub Actions y pruebas E2E.

**Criterio de salida:** operaciones visibles y navegables, errores esperables controlados, fechas presentadas en Costa Rica, infraestructura documentada, CI ejecutable y documentaciÃ³n acadÃ©mica actualizada. La Ãºltima ejecuciÃ³n manual confirmada registrÃ³ 195 pruebas correctas y 0 fallidas.

## Reglas de aceptaciÃ³n

Una historia se considera lista para cierre cuando sus criterios son verificables, las pruebas automatizadas relevantes pasan, el comportamiento observable es revisado y la documentaciÃ³n se actualiza. No se reconstruyen estimaciones, fechas o decisiones que no tengan evidencia.

