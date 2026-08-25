# Bitácora de Extreme Programming

## Alcance de la bitácora

Esta bitácora resume la evolución verificable del proyecto mediante cuatro iteraciones XP. No inventa horas trabajadas, reuniones, decisiones de negocio ni resultados que no estén en el repositorio o en la evidencia de ejecución proporcionada para el cierre.

## Iteración XP 1 — Base técnica y proveedores

Se inició la solución modular con Domain, Application, Infrastructure, API, Web y proyectos de pruebas. La historia de proveedores incorporó validación, normalización Unicode, duplicados, auditoría UTC y persistencia PostgreSQL.

La evidencia Git muestra secuencias TDD representativas de prueba e implementación para normalización, nombres vacíos, Unicode, caracteres permitidos, caso de uso, duplicados, repositorio, API y MVC. La persistencia se verificó con PostgreSQL y Testcontainers. Se configuró GitHub Actions para compilar y ejecutar la solución.

La documentación histórica registra el cierre mediante `v0.1.0`, el PR #15 y 46 casos correctos para ese corte. Esos datos corresponden a la primera liberación y no al estado final.

## Iteración XP 2 — Licitaciones, publicación y E2E

La siguiente evolución agregó la creación de licitaciones en Borrador, reglas de código, título, presupuesto, fecha, auditoría y concurrencia `xmin`. Después se implementó la transición explícita de Borrador a Publicada, con fecha futura, errores controlados, API, MVC, antiforgery y PRG.

El historial identifica la creación de licitaciones con Issue #16 y la publicación con Issue #18. La prueba de navegador se incorporó mediante Issue #20 usando Playwright, PostgreSQL y Testcontainers.

Para HU-01 existe evidencia detallada RED-GREEN-REFACTOR en commits. Para los incrementos de licitaciones y publicación se conserva evidencia de pruebas y validación, pero no se afirma un commit RED independiente cuando no está registrado como tal.

## Iteración XP 3 — Ofertas, aprobación y tipos de cambio

Se incorporó el módulo de ofertas con creación, consulta, listado, edición y eliminación. Las reglas verificadas incluyen licitación publicada y abierta, proveedor existente, monto positivo, límite presupuestario y una oferta por proveedor y licitación. También se agregó la consulta de mejor oferta, cálculo de ahorro y clasificación.

El módulo de niveles de aprobación agregó rangos monetarios, validación de traslapes y determinación automática del nivel según el monto.

El módulo de tipos de cambio agregó CRUD, conversión CRC → USD y la restricción de un único registro activo, protegida por índice único filtrado en PostgreSQL.

El historial Git vincula estos incrementos con Issues #22, #24 y #26. El repositorio contiene pruebas unitarias, funcionales y de integración para las reglas correspondientes. No se reconstruyen aquí métricas por historia que no estén registradas explícitamente.

## Iteración XP 4 — CRUD, infraestructura y estabilización

Se completaron las operaciones CRUD de licitaciones y proveedores mediante el incremento asociado a Issue #28. Luego se incorporó Docker Compose con API, Web y PostgreSQL, asociado a Issue #30, y manifiestos Kubernetes con Deployments, Services, ConfigMap, Secret de ejemplo, StatefulSet, PVC y probes, asociados a Issue #32.

El rediseño Web asociado a Issue #34 consolidó la navegación y la presentación. La estabilización asociada a Issue #36 corrigió recorridos Web, registros DI, edición de proveedores, errores esperables y presentación de fechas.

Las fechas se almacenan internamente en UTC y se presentan mediante `America/Costa_Rica`. Las ofertas y sus detalles dejaron de mostrar Guid cuando existe información descriptiva; los Guid se conservan para rutas y lógica.

La regresión de tipos de cambio verificó que intentar activar un segundo registro no produce una página genérica, conserva el formulario y mantiene un solo activo. También se verificaron recorridos CRUD y opciones amigables para ofertas.

## Pruebas y cierre técnico

El repositorio contiene UnitTests, FunctionalTests, IntegrationTests y EndToEndTests. La CI ejecuta la solución, usa Docker para Testcontainers y prepara Chromium para Playwright. La última ejecución manual confirmada para este cierre registró **180 pruebas correctas y 0 fallidas**.

No se registran horas trabajadas ni se calculan velocidades por puntos cuando la evidencia no las proporciona.

## Documentación final

Durante la Iteración XP 4 se completó la documentación final después de contrastar la implementación real, la infraestructura, las pruebas y el historial Git. Quedaron documentados el alcance, la arquitectura, el modelo de datos, la API, las pruebas, Docker, Kubernetes, la integración de módulos, el uso de IA y los cinco módulos funcionales.
