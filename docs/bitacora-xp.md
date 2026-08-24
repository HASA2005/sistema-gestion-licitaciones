# Bitácora de Extreme Programming

## Propósito y corte de evidencia

Esta bitácora registra el trabajo verificable de la Iteración XP 1. El corte
técnico corresponde a `main` en el commit `8be5992`, integrado el 23 de agosto
de 2026. El cierre documental se prepara el 24 de agosto de 2026 mediante la
tarea técnica [#14](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/14).

No se agregan estimaciones, opiniones del cliente ni resultados de aceptación
que no hayan quedado registrados durante el trabajo.

## Iteración XP 1

**Objetivo:** registrar un proveedor desde API o MVC, aplicar reglas comunes de
negocio y persistir el resultado en PostgreSQL.

**Periodo observado:** 23 y 24 de agosto de 2026.

### Trabajo seleccionado

| Tipo | Trabajo | Referencia | Resultado |
| --- | --- | --- | --- |
| Tarea técnica | Inicializar la solución .NET | [#2](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/2) | Integrada |
| Tarea técnica | Configurar dependencias mínimas para TDD | [#3](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/3) | Integrada |
| Historia | Registrar proveedor | [#1](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/1) | Integrada |
| Tarea técnica | Configurar persistencia de proveedores | [#6](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/6) | Integrada |
| Tarea técnica | Configurar integración continua | [#11](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/11) | Integrada |
| Tarea técnica | Documentar cierre y pequeña liberación | [#14](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/14) | En cierre |

Al cierre se documenta prioridad alta para todo el recorrido vertical. No quedó
evidencia de prioridad ni estimación relativa de HU-01 registrada antes de
empezar, por lo que esos datos no se reconstruyen retrospectivamente.

### Secuencia de entrega

1. Se creó la solución modular con proyectos para dominio, aplicación,
   infraestructura, API, Web y tres niveles de pruebas.
2. Se conectaron las dependencias mínimas para comenzar el desarrollo guiado
   por pruebas.
3. Se implementaron la entidad `Proveedor` y el caso de uso de registro.
4. Se agregó persistencia con EF Core, PostgreSQL, migración inicial, índice
   único y control de concurrencia.
5. Se expuso el registro mediante API REST y un formulario MVC con atributos
   básicos de accesibilidad verificados.
6. Se incorporaron pruebas con PostgreSQL real mediante Testcontainers.
7. Se configuró GitHub Actions para compilar y ejecutar las pruebas en cada
   Pull Request hacia `main` y en cada push a `main`.
8. Se preparó esta documentación y la candidata de liberación `v0.1.0`.

### Evidencia TDD representativa

| Comportamiento | RED | GREEN |
| --- | --- | --- |
| Normalización inicial del nombre | `8d31be4` | `be1bfe8` |
| Rechazo de nombres ausentes | `0a8617e` | `c251fb4` |
| Normalización Unicode | `65c42fa` | `399699a` |
| Caracteres permitidos | `72a4b78` | `bc2b4c8` |
| Registro mediante caso de uso | `f4f5edd` | `3d943b8` |
| Duplicados normalizados | `d5b47b1` | `10df144` |
| Repositorio PostgreSQL | `ac613a2` | `647f3f4` |
| Endpoint de registro | `1e96811` | `6655df2` |
| Formulario MVC | `a1e1f02` | `a9be373` |

La refactorización `7163bd8` introdujo una excepción específica para duplicados
y conservó el mismo mensaje de error. En el historial completo se identifican
25 secuencias de prueba seguidas por implementación; la tabla anterior conserva
una muestra suficiente para revisar el proceso.

### Integración continua y Pull Requests

| Pull Request | Alcance |
| --- | --- |
| [#4](https://github.com/HASA2005/sistema-gestion-licitaciones/pull/4) | Referencias mínimas para pruebas |
| [#5](https://github.com/HASA2005/sistema-gestion-licitaciones/pull/5) | Dominio y aplicación de proveedores |
| [#7](https://github.com/HASA2005/sistema-gestion-licitaciones/pull/7) | Dependencias EF Core y Testcontainers |
| [#8](https://github.com/HASA2005/sistema-gestion-licitaciones/pull/8) | Persistencia PostgreSQL |
| [#9](https://github.com/HASA2005/sistema-gestion-licitaciones/pull/9) | API de proveedores |
| [#10](https://github.com/HASA2005/sistema-gestion-licitaciones/pull/10) | Interfaz MVC de proveedores |
| [#12](https://github.com/HASA2005/sistema-gestion-licitaciones/pull/12) | Integración continua |

GitHub Actions terminó correctamente tanto para el Pull Request #12 como para
su integración en `main`.

### Métricas observadas

| Indicador | Resultado |
| --- | ---: |
| Historias de usuario integradas | 1 |
| Tareas técnicas integradas antes del cierre | 4 |
| Pull Requests integrados | 7 |
| Commits en el corte de `main` | 68 |
| Commits no merge | 61 |
| Commits de prueba | 29 |
| Casos de prueba correctos | 46 |
| Pruebas unitarias | 23 |
| Pruebas funcionales | 17 |
| Pruebas de integración | 6 |

Los 61 commits no merge se distribuyen en 29 `test`, 24 `feat`, 1 `fix`, 2
`refactor`, 3 `chore`, 1 `ci` y 1 `docs`.

### Velocidad

No es posible calcular velocidad en puntos porque HU-01 no tuvo una estimación
inicial registrada. El rendimiento observado fue una historia de usuario y
cuatro tareas técnicas integradas, con la tarea de cierre todavía en curso al
preparar este documento. A partir de la Iteración XP 2, los puntos se
registrarán en el Issue antes de comenzar para obtener una velocidad comparable.

### Retroalimentación y retrospectiva

**Funcionó bien**

- Los incrementos verticales mantuvieron alineadas las reglas de dominio, la
  persistencia, la API y la interfaz MVC.
- Las pruebas detectaron diferencias Unicode que visualmente parecían iguales.
- El índice único complementó la validación del servicio ante solicitudes
  concurrentes.
- Los Pull Requests y la CI dejaron una trazabilidad revisable.

**Dificultades observadas**

- El trabajo inicial no registró estimación ni velocidad en puntos.
- OneDrive mantuvo bloqueadas algunas carpetas durante cambios de rama.
- Windows Application Control bloqueó transitoriamente un ensamblado generado;
  una nueva ejecución confirmó que no era un defecto del código.
- La confirmación manual después de cada paso pequeño hizo más lento el flujo
  de desarrollo asistido.

**Acciones para la Iteración XP 2**

- Registrar prioridad, estimación y criterios antes de implementar HU-02.
- Ejecutar cada incremento completo de forma continua, incluyendo pruebas y
  verificación, sin pausas manuales entre RED y GREEN.
- Conservar evidencia verificable del comportamiento aunque se reduzcan las
  interrupciones durante el trabajo.
- Incorporar una primera prueba real de navegador con Playwright o Selenium.
- Configurar un reporte de cobertura antes del cierre de la siguiente
  iteración.

## Pequeña liberación de la Iteración XP 1

La documentación se integró en `main` mediante el PR #15, commit `2117778`. La
etiqueta anotada `v0.1.0` fue creada localmente sobre ese commit el 24 de agosto
de 2026. En el corte de esta bitácora todavía falta publicarla en GitHub, por lo
que no se presenta como una liberación remota disponible.

## Iteración XP 2 — trabajo en curso

**Objetivo:** preparar licitaciones en Borrador antes de publicar y recibir
ofertas.

**Inicio observado:** 24 de agosto de 2026.

### Planning Game

| Historia | Referencia | Prioridad | Estimación | Estado en el corte |
| --- | --- | --- | ---: | --- |
| Crear licitación en estado Borrador | [#16](https://github.com/HASA2005/sistema-gestion-licitaciones/issues/16) | Alta | 8 puntos | Terminada técnicamente; pendiente de integración |

Antes de programar se acordaron campos obligatorios, unicidad del código,
presupuesto CRC positivo, estado inicial, auditoría, concurrencia, API, MVC y el
alcance excluido. La fecha futura se reservó para publicar; no se impuso al
guardar un Borrador.

### Resultado técnico de HU-02

- Entidad y caso de uso compartido con estado inicial `Borrador`.
- Código normalizado mediante Unicode Form C, recorte lateral y comparación sin
  distinguir mayúsculas, con límites seguros para PostgreSQL.
- Presupuesto `decimal` y PostgreSQL `numeric(18,2)` con restricción positiva.
- Fecha capturada como hora de Costa Rica y almacenada en UTC.
- Catálogo sembrado con los tres estados oficiales.
- Índice único, manejo de carreras y concurrencia `xmin` probada con dos
  contextos simultáneos.
- `POST /api/v1/licitaciones` y formulario `/licitaciones/crear`.
- 72 casos nuevos: 30 unitarios, 35 funcionales y 7 de integración.
- Resultado acumulado local: 118 casos correctos.

Las pruebas de HU-02 se prepararon junto con su implementación en bloques por
capa. No se registra un commit RED independiente ni se afirma un fallo
intermedio que no haya quedado evidenciado. Los casos automatizados y
PostgreSQL real verifican el comportamiento final; la CI del Pull Request
confirmará el resultado en un entorno limpio.

Una revisión cruzada antes de integrar endureció longitudes, caracteres de
control, equivalencia Unicode, fechas API con zona explícita, respuestas `415`
y mensajes sin detalles técnicos. Las rutas continúan anónimas porque identidad
y autorización no forman parte de HU-02; esa deuda debe resolverse antes de un
despliegue real.

### Velocidad provisional

HU-02 fue planificada en 8 puntos. La velocidad aceptada de la Iteración XP 2
permanece en 0 puntos hasta integrar la historia y recibir una ejecución
satisfactoria de CI. Después de la integración será de 8 puntos, salvo que la
retroalimentación requiera reabrirla.

### Próximos ajustes

- Publicar y cerrar licitaciones se implementarán en historias separadas.
- Se debe crear una tarea técnica para la primera prueba de navegador con
  Playwright o Selenium.
- Se debe configurar cobertura y sus puertas mínimas antes de cerrar la
  Iteración XP 2.
- La retroalimentación de cliente sobre HU-02 aún no ha sido registrada.
