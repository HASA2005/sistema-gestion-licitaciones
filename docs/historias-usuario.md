# Historias de usuario

Este documento reúne las historias verificables del Sistema de Gestión de Licitaciones. El trabajo se organizó exclusivamente con Extreme Programming (XP), mediante iteraciones XP, TDD, RED-GREEN-REFACTOR, diseño simple, integración continua, small releases y feedback.

Los identificadores HU-01 a HU-08 son identificadores documentales. Las referencias a Issues y Pull Requests solo se incluyen cuando aparecen en la documentación existente o en el historial Git.

## HU-01 — Gestionar proveedores

**Como** encargado de compras, **quiero** registrar, consultar, listar, editar y eliminar proveedores, **para** mantener actualizado el catálogo que se usa en las ofertas.

**Referencia verificable:** Issue #1 para el registro inicial; el CRUD fue incorporado posteriormente mediante el incremento asociado a Issue #28.

**Estado:** implementada en Domain, Application, Infrastructure, API y Web.

### Criterios de aceptación

- El nombre es obligatorio, se limpia y se normaliza con Unicode Form C.
- Se rechazan caracteres no permitidos y se ignoran diferencias de mayúsculas, espacios redundantes y representaciones Unicode equivalentes al comparar.
- El nombre normalizado es único, incluido el caso de solicitudes concurrentes.
- El registro válido se persiste en PostgreSQL y devuelve confirmación.
- El usuario puede listar, consultar, editar y eliminar proveedores desde Web y API mediante las operaciones CRUD actuales.
- Los errores esperables se presentan mediante respuestas controladas.

## HU-02 — Crear una licitación en Borrador

**Como** encargado de compras, **quiero** crear una licitación en estado Borrador, **para** preparar sus datos antes de publicarla.

**Referencia verificable:** Issue #16 y Pull Request #17.

**Estado:** implementada e integrada en `main`.

### Criterios de aceptación

- Código, título, presupuesto y fecha de cierre son recibidos y validados.
- El código normalizado es único.
- El presupuesto es positivo y compatible con `numeric(18,2)`.
- La entidad inicia en estado `Borrador` y conserva auditoría UTC.
- La operación está disponible mediante API y Web.
- Un duplicado o dato inválido no crea otra fila y se muestra de forma controlada.

## HU-03 — Publicar una licitación

**Como** encargado de compras, **quiero** publicar una licitación en Borrador, **para** habilitar el registro de ofertas.

**Referencia verificable:** Issue #18 y Pull Request #19.

**Estado:** implementada en el código actual.

### Criterios de aceptación

- Solo una licitación en Borrador puede publicarse.
- La fecha de cierre debe ser estrictamente futura.
- La publicación cambia el estado a `Publicada` y actualiza `UpdatedAt` en UTC.
- Los estados inválidos, identificadores inexistentes y conflictos de concurrencia producen errores controlados.
- La operación está disponible mediante API y Web con antiforgery en MVC.

## HU-04 — Gestionar ofertas

**Como** encargado de compras, **quiero** crear, consultar, listar, editar y eliminar ofertas, **para** comparar propuestas de proveedores en licitaciones publicadas.

**Referencia verificable:** commit de implementación de ofertas asociado a Issue #22.

**Estado:** implementada en Domain, Application, Infrastructure, API y Web.

### Criterios de aceptación

- Solo se crean ofertas para licitaciones publicadas y no vencidas.
- El proveedor debe existir.
- El monto debe ser mayor que cero, tener como máximo dos decimales y no superar el presupuesto de la licitación.
- No puede existir más de una oferta del mismo proveedor para una licitación.
- La edición y eliminación respetan el estado y fecha de cierre de la licitación.
- La Web muestra códigos y nombres amigables sin exponer Guid técnicos.
- Los identificadores se conservan internamente para rutas y operaciones.

## HU-05 — Determinar la mejor oferta

**Como** encargado de compras, **quiero** consultar la mejor oferta y el ahorro respecto al presupuesto, **para** apoyar la evaluación de las propuestas.

**Referencia verificable:** implementación de `OfertaService.MejorAsync` y endpoint `/api/v1/ofertas/licitacion/{licitacionId}/mejor`.

**Estado:** implementada.

### Criterios de aceptación

- Se consideran ofertas válidas dentro del presupuesto.
- La mejor oferta se determina por el menor monto.
- Se calcula el porcentaje de ahorro respecto al presupuesto.
- Se informa una clasificación según el ahorro.
- Si no existen ofertas válidas, se informa esa situación de forma controlada.

## HU-06 — Configurar niveles de aprobación

**Como** responsable de aprobación, **quiero** crear, listar, consultar, editar y eliminar niveles con rangos monetarios, **para** determinar quién debe aprobar una operación.

**Referencia verificable:** commit de implementación asociado a Issue #24.

**Estado:** implementada en Domain, Application, Infrastructure, API y Web.

### Criterios de aceptación

- El responsable es obligatorio.
- El monto mínimo es positivo.
- El monto máximo, si existe, no es menor que el mínimo.
- Los montos admiten como máximo dos decimales.
- Los rangos incluyen sus límites.
- El sistema determina automáticamente el nivel correspondiente a un monto.
- Los traslapes entre rangos se validan antes de guardar.

## HU-07 — Gestionar tipos de cambio

**Como** responsable financiero, **quiero** administrar tipos de cambio CRC/USD y convertir montos, **para** consultar valores monetarios en dólares.

**Referencia verificable:** commit de implementación asociado a Issue #26.

**Estado:** implementada en Domain, Application, Infrastructure, API y Web.

### Criterios de aceptación

- El valor CRC por USD debe ser positivo y tener como máximo dos decimales.
- Solo puede existir un tipo de cambio activo.
- Crear, editar, listar, consultar y eliminar respetan esa unicidad.
- La conversión CRC → USD utiliza el tipo de cambio activo.
- Un intento de activar un segundo registro produce una validación controlada y no deja dos registros activos.
- Los formularios Web conservan los datos y muestran errores esperables.

## HU-08 — Operar el sistema mediante CRUD y Web estabilizada

**Como** usuario del sistema, **quiero** recorrer las operaciones principales desde una interfaz clara y consistente, **para** trabajar sin exponer detalles técnicos ni recibir errores genéricos ante validaciones esperadas.

**Referencia verificable:** commit de CRUD asociado a Issue #28, rediseño Web asociado a Issue #34 y estabilización Web asociada a Issue #36.

**Estado:** implementada en el alcance actual.

### Criterios de aceptación

- Licitaciones y proveedores cuentan con operaciones CRUD en Web y API.
- Las ofertas y tipos de cambio cuentan con formularios, listados y acciones disponibles según sus reglas.
- Las fechas visibles se convierten a `America/Costa_Rica` y se mantienen en UTC internamente.
- Los formularios no exponen Guid cuando existe información descriptiva.
- Las excepciones de negocio esperables regresan al formulario o muestran un mensaje controlado, sin página genérica de error.
- Se conservan las rutas, reglas de negocio y acciones existentes.

## Evidencia transversal

La solución contiene proyectos de pruebas unitarias, funcionales, de integración y extremo a extremo. La última ejecución manual confirmada y proporcionada para este cierre registró **180 pruebas correctas y 0 fallidas**.

No se atribuyen a una historia decisiones de negocio que no estén respaldadas por el código, la documentación o el historial Git.
