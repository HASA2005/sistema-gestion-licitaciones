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

## Ajuste para historias posteriores

La Iteración XP 1 no dejó una estimación relativa registrada antes del inicio de
HU-01. No se asignan puntos retrospectivos porque alterarían la evidencia real.
A partir de la Iteración XP 2, cada historia deberá registrar prioridad,
estimación y criterios antes del primer commit RED. Esto permitirá calcular
velocidad en puntos y comparar lo planificado con lo terminado.
