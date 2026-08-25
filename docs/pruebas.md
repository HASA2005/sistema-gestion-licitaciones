# Pruebas automatizadas

## Estrategia

La estrategia avanza de lo mÃ¡s aislado a lo mÃ¡s cercano al uso real:

```text
Unit â†’ Functional â†’ Integration â†’ E2E
```

Cada nivel verifica una responsabilidad distinta y la suite completa se
coordina desde `Licitaciones.sln`.

## UnitTests

Proyecto: `tests/Licitaciones.UnitTests`.

Prueba entidades Domain y servicios Application sin infraestructura externa.
Incluye reglas de:

- proveedores y normalizaciÃ³n;
- licitaciones, estados y publicaciÃ³n;
- ofertas y sus montos;
- niveles de aprobaciÃ³n y rangos;
- tipos de cambio;
- operaciones CRUD y casos de uso.

## FunctionalTests

Proyecto: `tests/Licitaciones.FunctionalTests`.

Usa `WebApplicationFactory`, hosts de prueba, repositorios en memoria y
solicitudes HTTP. Verifica:

- endpoints API;
- formularios Web MVC;
- validaciÃ³n antiforgery;
- redirecciones y respuestas HTTP;
- conservaciÃ³n de datos ante errores;
- validaciones de reglas de negocio;
- regresiones Web de proveedores y tipos de cambio;
- comportamiento funcional de ofertas.

## IntegrationTests

Proyecto: `tests/Licitaciones.IntegrationTests`.

Usa PostgreSQL real mediante Testcontainers y prueba:

- configuraciones del modelo EF Core;
- repositorios;
- migraciones;
- claves forÃ¡neas, Ã­ndices y restricciones;
- persistencia de licitaciones, proveedores y ofertas;
- recorridos API y Web con infraestructura real;
- concurrencia mediante `xmin` donde estÃ¡ configurada.

## EndToEndTests

Proyecto: `tests/Licitaciones.EndToEndTests`.

Usa Playwright con Chromium y PostgreSQL mediante Testcontainers. La prueba E2E
verifica un flujo Web real de crear una licitaciÃ³n y publicarla. La aplicaciÃ³n
Web se ejecuta sobre Kestrel y puede guardar evidencias de fallos en
`TestResults/e2e`.

## EjecuciÃ³n

Desde la raÃ­z:

```powershell
dotnet test Licitaciones.sln
```

La Ãºltima ejecuciÃ³n manual confirmada registrÃ³ **180 pruebas correctas y 0
fallidas**. Es el resultado de esa ejecuciÃ³n final confirmada, no una cifra
histÃ³rica fija para todas las ejecuciones futuras.

## Cobertura de cÃ³digo

El reporte de cobertura corresponde a la ejecuciÃ³n final del 25/08/2026. Fue
generado con coverlet mediante `XPlat Code Coverage`, procesado con
ReportGenerator y construido a partir de 8 reportes Cobertura combinados.

### Cobertura de lÃ­neas

| Ãrea | Cobertura de lÃ­neas |
|---|---:|
| SoluciÃ³n completa | 88.9 % |
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
la cobertura total de lÃ­neas supera el objetivo de 70 %. El criterio de 70 %
corresponde a la cobertura total de la soluciÃ³n; no se afirma que Web alcance
individualmente ese porcentaje.

Web presenta una cobertura individual menor, al igual que algunas Ã¡reas como
`OfertaService` y `NivelAprobacionService`. Esto no constituye un incumplimiento
del objetivo global de cobertura de lÃ­neas.

### Otras mÃ©tricas

| MÃ©trica | Resultado |
|---|---:|
| Branch coverage | 44.7 % |
| Method coverage | 66.9 % |

Branch coverage y method coverage se informan por separado. El criterio
principal indicado para esta evaluaciÃ³n corresponde a cobertura de lÃ­neas, por
lo que estas mÃ©tricas no se presentan como equivalentes a ella.

### ReproducciÃ³n

Ejecutar la cobertura con:

```powershell
dotnet test Licitaciones.sln --collect:"XPlat Code Coverage"
```

DespuÃ©s, combinar los reportes Cobertura generados y crear el resumen con
ReportGenerator. Una forma reproducible, desde la raÃ­z y con la herramienta
instalada, es:

```powershell
reportgenerator `
	-reports:"TestResults/**/coverage.cobertura.xml" `
	-targetdir:"TestResults/coverage-report" `
	-reporttypes:"Html;TextSummary"
```

Los porcentajes corresponden exclusivamente a la ejecuciÃ³n concreta del
25/08/2026 y no son cifras fijas para futuras ejecuciones.

## IntegraciÃ³n continua

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

Los artefactos se conservan durante 7 dÃ­as segÃºn la configuraciÃ³n actual del
workflow.

## Alcance de la evidencia

Los resultados numÃ©ricos deben asociarse siempre a una ejecuciÃ³n concreta. En
este documento se conserva Ãºnicamente la Ãºltima ejecuciÃ³n manual confirmada
indicada para el cierre. Las pruebas E2E dependen de Docker y de la instalaciÃ³n
de Chromium.


