# Pruebas automatizadas

## Estrategia

La estrategia avanza de lo más aislado a lo más cercano al uso real:

```text
Unit → Functional → Integration → E2E
```

Cada nivel verifica una responsabilidad distinta y la suite completa se
coordina desde `Licitaciones.sln`.

## UnitTests

Proyecto: `tests/Licitaciones.UnitTests`.

Prueba entidades Domain y servicios Application sin infraestructura externa.
Incluye reglas de:

- proveedores y normalización;
- licitaciones, estados y publicación;
- ofertas y sus montos;
- niveles de aprobación y rangos;
- tipos de cambio;
- operaciones CRUD y casos de uso.

## FunctionalTests

Proyecto: `tests/Licitaciones.FunctionalTests`.

Usa `WebApplicationFactory`, hosts de prueba, repositorios en memoria y
solicitudes HTTP. Verifica:

- endpoints API;
- formularios Web MVC;
- validación antiforgery;
- redirecciones y respuestas HTTP;
- conservación de datos ante errores;
- validaciones de reglas de negocio;
- regresiones Web de proveedores y tipos de cambio;
- comportamiento funcional de ofertas.

## IntegrationTests

Proyecto: `tests/Licitaciones.IntegrationTests`.

Usa PostgreSQL real mediante Testcontainers y prueba:

- configuraciones del modelo EF Core;
- repositorios;
- migraciones;
- claves foráneas, índices y restricciones;
- persistencia de licitaciones, proveedores y ofertas;
- recorridos API y Web con infraestructura real;
- concurrencia mediante `xmin` donde está configurada.

## EndToEndTests

Proyecto: `tests/Licitaciones.EndToEndTests`.

Usa Playwright con Chromium y PostgreSQL mediante Testcontainers. La prueba E2E
verifica un flujo Web real de crear una licitación y publicarla. La aplicación
Web se ejecuta sobre Kestrel y puede guardar evidencias de fallos en
`TestResults/e2e`.

## Ejecución

Desde la raíz:

```powershell
dotnet test Licitaciones.sln
```

La última ejecución manual confirmada registró **195 pruebas correctas y 0
fallidas**. Es el resultado de esa ejecución final confirmada, no una cifra
histórica fija para todas las ejecuciones futuras.

## Cobertura de código

El reporte de cobertura corresponde a la ejecución final del 25/08/2026. Fue
generado con coverlet mediante `XPlat Code Coverage`, procesado con
ReportGenerator y construido a partir de 8 reportes Cobertura combinados.

### Cobertura de líneas

| Área | Cobertura de líneas |
|---|---:|
| Solución completa | 88.9 % |
| `Licitaciones.Domain` | 95.8 % |
| `Licitaciones.Application` | 84.1 % |
| `Licitaciones.Infrastructure` | 96.5 % |
| `Licitaciones.Api` | 92.2 % |
| `Licitaciones.Web` | 52.9 % |

El resultado combinado registra:

- Covered lines: 2597.
- Uncovered lines: 322.
- Coverable lines: 2919.

Domain supera el objetivo de 80 %, Application supera el objetivo de 80 % y
la cobertura total de líneas supera el objetivo de 70 %. El criterio de 70 %
corresponde a la cobertura total de la solución; no se afirma que Web alcance
individualmente ese porcentaje.

Web presenta una cobertura individual menor, al igual que algunas áreas como
`OfertaService` y `NivelAprobacionService`. Esto no constituye un incumplimiento
del objetivo global de cobertura de líneas.

### Otras métricas

| Métrica | Resultado |
|---|---:|
| Branch coverage | 44.7 % |
| Method coverage | 66.9 % |

Branch coverage y method coverage se informan por separado. El criterio
principal indicado para esta evaluación corresponde a cobertura de líneas, por
lo que estas métricas no se presentan como equivalentes a ella.

### Reproducción

Ejecutar la cobertura con:

```powershell
dotnet test Licitaciones.sln --collect:"XPlat Code Coverage"
```

Después, combinar los reportes Cobertura generados y crear el resumen con
ReportGenerator. Una forma reproducible, desde la raíz y con la herramienta
instalada, es:

```powershell
reportgenerator `
	-reports:"TestResults/**/coverage.cobertura.xml" `
	-targetdir:"TestResults/coverage-report" `
	-reporttypes:"Html;TextSummary"
```

Los porcentajes corresponden exclusivamente a la ejecución concreta del
25/08/2026 y no son cifras fijas para futuras ejecuciones.

## Integración continua

El workflow `.github/workflows/ci.yml`:

- se ejecuta en Ubuntu;
- configura .NET 9;
- restaura dependencias;
- compila en Release;
- verifica Docker;
- instala Chromium para Playwright;
- ejecuta la suite completa;
- publica artefactos TRX y evidencias E2E;
- tiene un timeout de 30 minutos para el trabajo.

Los artefactos se conservan durante 7 días según la configuración actual del
workflow.

## Alcance de la evidencia

Los resultados numéricos deben asociarse siempre a una ejecución concreta. En
este documento se conserva únicamente la última ejecución manual confirmada
indicada para el cierre. Las pruebas E2E dependen de Docker y de la instalación
de Chromium.


