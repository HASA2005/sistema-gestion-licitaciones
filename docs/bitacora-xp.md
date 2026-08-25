# BitÃ¡cora de Extreme Programming

## Alcance de la bitÃ¡cora

Esta bitÃ¡cora resume la evoluciÃ³n verificable del proyecto mediante cuatro iteraciones XP. No inventa horas trabajadas, reuniones, decisiones de negocio ni resultados que no estÃ©n en el repositorio o en la evidencia de ejecuciÃ³n proporcionada para el cierre.

## IteraciÃ³n XP 1 â€” Base tÃ©cnica y proveedores

Se iniciÃ³ la soluciÃ³n modular con Domain, Application, Infrastructure, API, Web y proyectos de pruebas. La historia de proveedores incorporÃ³ validaciÃ³n, normalizaciÃ³n Unicode, duplicados, auditorÃ­a UTC y persistencia PostgreSQL.

La evidencia Git muestra secuencias TDD representativas de prueba e implementaciÃ³n para normalizaciÃ³n, nombres vacÃ­os, Unicode, caracteres permitidos, caso de uso, duplicados, repositorio, API y MVC. La persistencia se verificÃ³ con PostgreSQL y Testcontainers. Se configurÃ³ GitHub Actions para compilar y ejecutar la soluciÃ³n.

La documentaciÃ³n histÃ³rica registra el cierre mediante `v0.1.0`, el PR #15 y 46 casos correctos para ese corte. Esos datos corresponden a la primera liberaciÃ³n y no al estado final.

## IteraciÃ³n XP 2 â€” Licitaciones, publicaciÃ³n y E2E

La siguiente evoluciÃ³n agregÃ³ la creaciÃ³n de licitaciones en Borrador, reglas de cÃ³digo, tÃ­tulo, presupuesto, fecha, auditorÃ­a y concurrencia `xmin`. DespuÃ©s se implementÃ³ la transiciÃ³n explÃ­cita de Borrador a Publicada, con fecha futura, errores controlados, API, MVC, antiforgery y PRG.

El historial identifica la creaciÃ³n de licitaciones con Issue #16 y la publicaciÃ³n con Issue #18. La prueba de navegador se incorporÃ³ mediante Issue #20 usando Playwright, PostgreSQL y Testcontainers.

Para HU-01 existe evidencia detallada RED-GREEN-REFACTOR en commits. Para los incrementos de licitaciones y publicaciÃ³n se conserva evidencia de pruebas y validaciÃ³n, pero no se afirma un commit RED independiente cuando no estÃ¡ registrado como tal.

## IteraciÃ³n XP 3 â€” Ofertas, aprobaciÃ³n y tipos de cambio

Se incorporÃ³ el mÃ³dulo de ofertas con creaciÃ³n, consulta, listado, ediciÃ³n y eliminaciÃ³n. Las reglas verificadas incluyen licitaciÃ³n publicada y abierta, proveedor existente, monto positivo, lÃ­mite presupuestario y una oferta por proveedor y licitaciÃ³n. TambiÃ©n se agregÃ³ la consulta de mejor oferta, cÃ¡lculo de ahorro y clasificaciÃ³n.

El mÃ³dulo de niveles de aprobaciÃ³n agregÃ³ rangos monetarios, validaciÃ³n de traslapes y determinaciÃ³n automÃ¡tica del nivel segÃºn el monto.

El mÃ³dulo de tipos de cambio agregÃ³ CRUD, conversiÃ³n CRC â†’ USD y la restricciÃ³n de un Ãºnico registro activo, protegida por Ã­ndice Ãºnico filtrado en PostgreSQL.

El historial Git vincula estos incrementos con Issues #22, #24 y #26. El repositorio contiene pruebas unitarias, funcionales y de integraciÃ³n para las reglas correspondientes. No se reconstruyen aquÃ­ mÃ©tricas por historia que no estÃ©n registradas explÃ­citamente.

## IteraciÃ³n XP 4 â€” CRUD, infraestructura y estabilizaciÃ³n

Se completaron las operaciones CRUD de licitaciones y proveedores mediante el incremento asociado a Issue #28. Luego se incorporÃ³ Docker Compose con API, Web y PostgreSQL, asociado a Issue #30, y manifiestos Kubernetes con Deployments, Services, ConfigMap, Secret de ejemplo, StatefulSet, PVC y probes, asociados a Issue #32.

El rediseÃ±o Web asociado a Issue #34 consolidÃ³ la navegaciÃ³n y la presentaciÃ³n. La estabilizaciÃ³n asociada a Issue #36 corrigiÃ³ recorridos Web, registros DI, ediciÃ³n de proveedores, errores esperables y presentaciÃ³n de fechas.

Las fechas se almacenan internamente en UTC y se presentan mediante `America/Costa_Rica`. Las ofertas y sus detalles dejaron de mostrar Guid cuando existe informaciÃ³n descriptiva; los Guid se conservan para rutas y lÃ³gica.

La regresiÃ³n de tipos de cambio verificÃ³ que intentar activar un segundo registro no produce una pÃ¡gina genÃ©rica, conserva el formulario y mantiene un solo activo. TambiÃ©n se verificaron recorridos CRUD y opciones amigables para ofertas.

## Pruebas y cierre tÃ©cnico

El repositorio contiene UnitTests, FunctionalTests, IntegrationTests y EndToEndTests. La CI ejecuta la soluciÃ³n, usa Docker para Testcontainers y prepara Chromium para Playwright. La Ãºltima ejecuciÃ³n manual confirmada para este cierre registrÃ³ **195 pruebas correctas y 0 fallidas**.

No se registran horas trabajadas ni se calculan velocidades por puntos cuando la evidencia no las proporciona.

## DocumentaciÃ³n final

Durante la IteraciÃ³n XP 4 se completÃ³ la documentaciÃ³n final despuÃ©s de contrastar la implementaciÃ³n real, la infraestructura, las pruebas y el historial Git. Quedaron documentados el alcance, la arquitectura, el modelo de datos, la API, las pruebas, Docker, Kubernetes, la integraciÃ³n de mÃ³dulos, el uso de IA y los cinco mÃ³dulos funcionales.

