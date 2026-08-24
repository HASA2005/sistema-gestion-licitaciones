# Historias de usuario

Este archivo mantiene las historias seleccionadas mediante Planning Game, sus
criterios verificables y la trazabilidad con Issues, pruebas, commits y Pull
Requests. El proyecto utiliza cuatro iteraciones XP y no emplea artefactos ni
terminología de Scrum.

## Convenciones

- Prioridad: alta, media o baja según valor y dependencias.
- Estimación: puntos relativos acordados antes de iniciar una historia.
- Estado: propuesta, seleccionada, en desarrollo o terminada.
- Una historia está terminada únicamente cuando sus criterios, pruebas,
  integración y documentación se encuentran satisfactorios.

## HU-01 Registrar proveedor

| Campo | Valor |
| --- | --- |
| Issue | [#1](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/1) |
| Iteración | Iteración XP 1 |
| Prioridad documentada al cierre | Alta; no quedó registrada antes de iniciar |
| Estimación inicial | No quedó registrada antes de iniciar; se documenta como hallazgo del proceso |
| Estado | Terminada |

**Historia**

Como encargado de compras, quiero registrar proveedores en el sistema, para
poder asociarlos posteriormente con las ofertas de las licitaciones.

### Criterios de aceptación y evidencia

| Criterio | Resultado | Evidencia principal |
| --- | --- | --- |
| El nombre es obligatorio y se rechazan valores vacíos | Cumplido | `Crear_ConNombreVacio_LanzaErrorControlado` y pruebas del formulario MVC |
| Se eliminan espacios laterales y se reducen espacios repetidos | Cumplido | `Crear_ConNombreValido_ConservaNombreLimpio` |
| La comparación ignora mayúsculas y minúsculas | Cumplido | `Crear_ConNombresEquivalentes_GeneraMismaNormalizacion` |
| Se normalizan representaciones Unicode equivalentes | Cumplido | `Crear_ConRepresentacionesUnicodeEquivalentes_GeneraMismaNormalizacion` |
| Solo se permiten letras, números, espacios, punto, coma y paréntesis | Cumplido | `Crear_ConCaracteresPermitidos_ConservaNombreValido` y `Crear_ConCaracteresNoPermitidos_LanzaErrorControlado` |
| El nombre normalizado es único | Cumplido | Servicio de aplicación, índice único PostgreSQL `ux_proveedores_nombre_normalizado` y pruebas con Testcontainers |
| Un duplicado produce un mensaje controlado | Cumplido | Respuesta API `409`, error junto al campo MVC y pruebas de concurrencia |
| Un registro válido produce confirmación | Cumplido | Respuesta API `201` y confirmación MVC mediante POST-Redirect-GET |

Las pruebas citadas se encuentran en:

- [pruebas de dominio](../tests/Licitaciones.UnitTests/Domain/Proveedores/ProveedorTests.cs);
- [pruebas de aplicación](../tests/Licitaciones.UnitTests/Application/Proveedores/RegistrarProveedorServiceTests.cs);
- [pruebas funcionales de API](../tests/Licitaciones.FunctionalTests/Api/Proveedores/RegistrarProveedorEndpointTests.cs);
- [pruebas funcionales de MVC](../tests/Licitaciones.FunctionalTests/Web/Proveedores/RegistrarProveedorWebTests.cs);
- [pruebas de persistencia](../tests/Licitaciones.IntegrationTests/Persistence/ProveedorRepositoryTests.cs).

### Trazabilidad de integración

| Alcance | Pull Request |
| --- | --- |
| Dominio y caso de uso | [#5](https://github.com/HASA2005/sistema-gestion-licitaciones/pull/5) |
| Persistencia EF Core y PostgreSQL | [#8](https://github.com/HASA2005/sistema-gestion-licitaciones/pull/8) |
| API REST versionada | [#9](https://github.com/HASA2005/sistema-gestion-licitaciones/pull/9) |
| Formulario y navegación MVC | [#10](https://github.com/HASA2005/sistema-gestion-licitaciones/pull/10) |

Las tareas técnicas que habilitaron la historia fueron TT-01 [#2](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/2),
TT-02 [#3](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/3),
TT-03 [#6](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/6)
y TT-04 [#11](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/11).

## HU-02 Crear licitación en estado Borrador

| Campo | Valor |
| --- | --- |
| Issue | [#16](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/16) |
| Iteración | Iteración XP 2 |
| Prioridad | Alta |
| Estimación inicial | 8 puntos |
| Estado | Terminada técnicamente; pendiente de integración en `main` |

**Historia**

Como encargado de compras, quiero crear una licitación en estado Borrador, para
preparar sus datos antes de publicarla y recibir ofertas.

### Decisiones de alcance

El Planning Game de la historia definió como obligatorios código, título,
presupuesto y fecha de cierre. Toda creación asigna `Borrador`; el cliente no
puede elegir el estado. Guardar un borrador no exige una fecha futura: esa regla
se aplicará en la historia de publicación, tal como establece el ciclo de
estados del documento oficial.

Se adoptó `numeric(18,2)` para CRC. Por ello se aceptan como máximo dos
decimales y un monto máximo de `9 999 999 999 999 999,99` CRC.
El código admite 100 caracteres y el título 200. Ambos se normalizan a Unicode
Form C y rechazan caracteres de control para mantener un contrato seguro entre
las interfaces, el dominio y PostgreSQL.

### Criterios de aceptación y evidencia

| Criterio | Resultado | Evidencia principal |
| --- | --- | --- |
| El código es obligatorio y se eliminan únicamente sus espacios laterales | Cumplido | `LicitacionTests` |
| La unicidad del código ignora mayúsculas y minúsculas | Cumplido | Servicio, índice `ux_licitaciones_codigo_normalizado` y prueba de carrera |
| Los espacios internos del código se conservan y siguen siendo significativos | Cumplido | `Crear_ConCodigoYTituloValidos_LimpiaExtremosSinAlterarContenido` |
| Código y título respetan límites y rechazan caracteres de control | Cumplido | Pruebas de borde en dominio, API, MVC y columnas `varchar` |
| El título es obligatorio y se almacena sin espacios laterales | Cumplido | Pruebas de dominio, API y MVC |
| El presupuesto es decimal, mayor que cero y compatible con `numeric(18,2)` | Cumplido | Dominio, restricción `CHECK`, formulario y PostgreSQL real |
| La fecha es obligatoria, se almacena en UTC y el formulario usa hora de Costa Rica | Cumplido | Pruebas unitarias, funcionales y de integración Web |
| Toda licitación nueva inicia en `Borrador` | Cumplido | Entidad, respuesta API y persistencia real |
| El identificador, la auditoría y `xmin` son administrados por el sistema | Cumplido | Modelo EF y conflicto real con dos contextos |
| Un duplicado produce un error controlado sin crear otra fila | Cumplido | API `409`, error MVC y prueba concurrente PostgreSQL |
| La creación está disponible en API y MVC con confirmación | Cumplido | `POST /api/v1/licitaciones` y `/licitaciones/crear` |

Las pruebas principales se encuentran en:

- [dominio](../tests/Licitaciones.UnitTests/Domain/Licitaciones/LicitacionTests.cs);
- [aplicación](../tests/Licitaciones.UnitTests/Application/Licitaciones/CrearLicitacionServiceTests.cs);
- [API funcional](../tests/Licitaciones.FunctionalTests/Api/Licitaciones/CrearLicitacionEndpointTests.cs);
- [Web funcional](../tests/Licitaciones.FunctionalTests/Web/Licitaciones/CrearLicitacionWebTests.cs);
- [repositorio PostgreSQL](../tests/Licitaciones.IntegrationTests/Persistence/LicitacionRepositoryTests.cs).

Publicar, cerrar, listar, editar, eliminar y gestionar ofertas permanecen fuera
de HU-02.

## Aprendizaje de estimación

La Iteración XP 1 no dejó una estimación relativa registrada antes del inicio de
HU-01. No se asignan puntos retrospectivos porque alterarían la evidencia real.
HU-02 corrigió ese hallazgo: la Issue #16 registró prioridad alta, estimación de
8 puntos y criterios verificables antes de comenzar. Los puntos se contabilizan
como velocidad únicamente después de integrar y aceptar la historia.
