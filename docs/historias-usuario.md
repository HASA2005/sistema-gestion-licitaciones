# Historias de usuario

Este documento reÃºne las historias verificables del Sistema de GestiÃ³n de Licitaciones. El trabajo se organizÃ³ exclusivamente con Extreme Programming (XP), mediante iteraciones XP, TDD, RED-GREEN-REFACTOR, diseÃ±o simple, integraciÃ³n continua, small releases y feedback.

Los identificadores HU-01 a HU-08 son identificadores documentales. Las referencias a Issues y Pull Requests solo se incluyen cuando aparecen en la documentaciÃ³n existente o en el historial Git.

## HU-01 â€” Gestionar proveedores

**Como** encargado de compras, **quiero** registrar, consultar, listar, editar y eliminar proveedores, **para** mantener actualizado el catÃ¡logo que se usa en las ofertas.

**Referencia verificable:** Issue #1 para el registro inicial; el CRUD fue incorporado posteriormente mediante el incremento asociado a Issue #28.

**Estado:** implementada en Domain, Application, Infrastructure, API y Web.

### Criterios de aceptaciÃ³n

- El nombre es obligatorio, se limpia y se normaliza con Unicode Form C.
- Se rechazan caracteres no permitidos y se ignoran diferencias de mayÃºsculas, espacios redundantes y representaciones Unicode equivalentes al comparar.
- El nombre normalizado es Ãºnico, incluido el caso de solicitudes concurrentes.
- El registro vÃ¡lido se persiste en PostgreSQL y devuelve confirmaciÃ³n.
- El usuario puede listar, consultar, editar y eliminar proveedores desde Web y API mediante las operaciones CRUD actuales.
- Los errores esperables se presentan mediante respuestas controladas.

## HU-02 â€” Crear una licitaciÃ³n en Borrador

**Como** encargado de compras, **quiero** crear una licitaciÃ³n en estado Borrador, **para** preparar sus datos antes de publicarla.

**Referencia verificable:** Issue #16 y Pull Request #17.

**Estado:** implementada e integrada en `main`.

### Criterios de aceptaciÃ³n

- CÃ³digo, tÃ­tulo, presupuesto y fecha de cierre son recibidos y validados.
- El cÃ³digo normalizado es Ãºnico.
- El presupuesto es positivo y compatible con `numeric(18,2)`.
- La entidad inicia en estado `Borrador` y conserva auditorÃ­a UTC.
- La operaciÃ³n estÃ¡ disponible mediante API y Web.
- Un duplicado o dato invÃ¡lido no crea otra fila y se muestra de forma controlada.

## HU-03 â€” Publicar una licitaciÃ³n

**Como** encargado de compras, **quiero** publicar una licitaciÃ³n en Borrador, **para** habilitar el registro de ofertas.

**Referencia verificable:** Issue #18 y Pull Request #19.

**Estado:** implementada en el cÃ³digo actual.

### Criterios de aceptaciÃ³n

- Solo una licitaciÃ³n en Borrador puede publicarse.
- La fecha de cierre debe ser estrictamente futura.
- La publicaciÃ³n cambia el estado a `Publicada` y actualiza `UpdatedAt` en UTC.
- Los estados invÃ¡lidos, identificadores inexistentes y conflictos de concurrencia producen errores controlados.
- La operaciÃ³n estÃ¡ disponible mediante API y Web con antiforgery en MVC.

## HU-04 â€” Gestionar ofertas

**Como** encargado de compras, **quiero** crear, consultar, listar, editar y eliminar ofertas, **para** comparar propuestas de proveedores en licitaciones publicadas.

**Referencia verificable:** commit de implementaciÃ³n de ofertas asociado a Issue #22.

**Estado:** implementada en Domain, Application, Infrastructure, API y Web.

### Criterios de aceptaciÃ³n

- Solo se crean ofertas para licitaciones publicadas y no vencidas.
- El proveedor debe existir.
- El monto debe ser mayor que cero, tener como mÃ¡ximo dos decimales y no superar el presupuesto de la licitaciÃ³n.
- No puede existir mÃ¡s de una oferta del mismo proveedor para una licitaciÃ³n.
- La ediciÃ³n y eliminaciÃ³n respetan el estado y fecha de cierre de la licitaciÃ³n.
- La Web muestra cÃ³digos y nombres amigables sin exponer Guid tÃ©cnicos.
- Los identificadores se conservan internamente para rutas y operaciones.

## HU-05 â€” Determinar la mejor oferta

**Como** encargado de compras, **quiero** consultar la mejor oferta y el ahorro respecto al presupuesto, **para** apoyar la evaluaciÃ³n de las propuestas.

**Referencia verificable:** implementaciÃ³n de `OfertaService.MejorAsync` y endpoint `/api/v1/ofertas/licitacion/{licitacionId}/mejor`.

**Estado:** implementada.

### Criterios de aceptaciÃ³n

- Se consideran ofertas vÃ¡lidas dentro del presupuesto.
- La mejor oferta se determina por el menor monto.
- Se calcula el porcentaje de ahorro respecto al presupuesto.
- Se informa una clasificaciÃ³n segÃºn el ahorro.
- Si no existen ofertas vÃ¡lidas, se informa esa situaciÃ³n de forma controlada.

## HU-06 â€” Configurar niveles de aprobaciÃ³n

**Como** responsable de aprobaciÃ³n, **quiero** crear, listar, consultar, editar y eliminar niveles con rangos monetarios, **para** determinar quiÃ©n debe aprobar una operaciÃ³n.

**Referencia verificable:** commit de implementaciÃ³n asociado a Issue #24.

**Estado:** implementada en Domain, Application, Infrastructure, API y Web.

### Criterios de aceptaciÃ³n

- El responsable es obligatorio.
- El monto mÃ­nimo es positivo.
- El monto mÃ¡ximo, si existe, no es menor que el mÃ­nimo.
- Los montos admiten como mÃ¡ximo dos decimales.
- Los rangos incluyen sus lÃ­mites.
- El sistema determina automÃ¡ticamente el nivel correspondiente a un monto.
- Los traslapes entre rangos se validan antes de guardar.

## HU-07 â€” Gestionar tipos de cambio

**Como** responsable financiero, **quiero** administrar tipos de cambio CRC/USD y convertir montos, **para** consultar valores monetarios en dÃ³lares.

**Referencia verificable:** commit de implementaciÃ³n asociado a Issue #26.

**Estado:** implementada en Domain, Application, Infrastructure, API y Web.

### Criterios de aceptaciÃ³n

- El valor CRC por USD debe ser positivo y tener como mÃ¡ximo dos decimales.
- Solo puede existir un tipo de cambio activo.
- Crear, editar, listar, consultar y eliminar respetan esa unicidad.
- La conversiÃ³n CRC â†’ USD utiliza el tipo de cambio activo.
- Un intento de activar un segundo registro produce una validaciÃ³n controlada y no deja dos registros activos.
- Los formularios Web conservan los datos y muestran errores esperables.

## HU-08 â€” Operar el sistema mediante CRUD y Web estabilizada

**Como** usuario del sistema, **quiero** recorrer las operaciones principales desde una interfaz clara y consistente, **para** trabajar sin exponer detalles tÃ©cnicos ni recibir errores genÃ©ricos ante validaciones esperadas.

**Referencia verificable:** commit de CRUD asociado a Issue #28, rediseÃ±o Web asociado a Issue #34 y estabilizaciÃ³n Web asociada a Issue #36.

**Estado:** implementada en el alcance actual.

### Criterios de aceptaciÃ³n

- Licitaciones y proveedores cuentan con operaciones CRUD en Web y API.
- Las ofertas y tipos de cambio cuentan con formularios, listados y acciones disponibles segÃºn sus reglas.
- Las fechas visibles se convierten a `America/Costa_Rica` y se mantienen en UTC internamente.
- Los formularios no exponen Guid cuando existe informaciÃ³n descriptiva.
- Las excepciones de negocio esperables regresan al formulario o muestran un mensaje controlado, sin pÃ¡gina genÃ©rica de error.
- Se conservan las rutas, reglas de negocio y acciones existentes.

## Evidencia transversal

La soluciÃ³n contiene proyectos de pruebas unitarias, funcionales, de integraciÃ³n y extremo a extremo. La Ãºltima ejecuciÃ³n manual confirmada y proporcionada para este cierre registrÃ³ **195 pruebas correctas y 0 fallidas**.

No se atribuyen a una historia decisiones de negocio que no estÃ©n respaldadas por el cÃ³digo, la documentaciÃ³n o el historial Git.

