# Estrategia y evidencia de pruebas

## Resultado acumulado

La solución contiene 89 métodos de prueba xUnit que producen 145 casos
ejecutados. Cada fila de `[InlineData]` cuenta como un caso independiente.

| Nivel | Casos |
| --- | ---: |
| Unitarias | 64 |
| Funcionales | 65 |
| Integración | 16 |
| **Total** | **145** |

## Herramientas y aislamiento

- xUnit ejecuta todos los proyectos de pruebas.
- Las pruebas unitarias no requieren infraestructura externa.
- Las pruebas funcionales usan `WebApplicationFactory` y reemplazan los
  repositorios por implementaciones en memoria.
- Las pruebas funcionales Web usan protección de datos efímera para procesar
  antiforgery sin depender de claves persistentes.
- Catorce pruebas de integración usan Testcontainers con `postgres:16-alpine`;
  las otras dos inspeccionan los metadatos del modelo EF Core.
- Docker debe estar iniciado para ejecutar la suite de integración completa.
- No existe todavía una prueba de navegador con Playwright o Selenium; la
  interfaz MVC se verifica mediante solicitudes HTTP y análisis del HTML.

## HU-01 — Pruebas unitarias

| Clase y escenario | Variantes | Casos |
| --- | --- | ---: |
| `ProveedorTests.Crear_ConNombreVacio_LanzaErrorControlado` | `null`, vacío y solo espacios | 3 |
| `ProveedorTests.Crear_ConRepresentacionesUnicodeEquivalentes_GeneraMismaNormalizacion` | Unicode compuesto y descompuesto | 1 |
| `ProveedorTests.Crear_ConCaracteresPermitidos_ConservaNombreValido` | número, punto, coma y paréntesis | 4 |
| `ProveedorTests.Crear_ConCaracteresNoPermitidos_LanzaErrorControlado` | `@`, `/`, `#` y `&` | 4 |
| `ProveedorTests.Crear_ConNombreValido_ConservaNombreLimpio` | espacios redundantes y Unicode descompuesto | 3 |
| `ProveedorTests.Crear_ConNombresEquivalentes_GeneraMismaNormalizacion` | diferencias de espacios y mayúsculas | 3 |
| `RegistrarProveedorServiceTests.Registrar_ConNombreValido_GuardaProveedorYDevuelveConfirmacion` | persistencia y mensaje | 1 |
| `RegistrarProveedorServiceTests.Registrar_ConNombreValido_GeneraIdentificadorYAuditoriaUtc` | `Guid`, UTC y fechas iniciales | 1 |
| `RegistrarProveedorServiceTests.Registrar_ConNombreDuplicado_LanzaErrorControladoYNoGuarda` | idéntico, espacios/mayúsculas y Unicode equivalente | 3 |
| **Total unitarias** | | **23** |

Archivos principales:

- [pruebas del dominio](../tests/Licitaciones.UnitTests/Domain/Proveedores/ProveedorTests.cs);
- [pruebas de aplicación](../tests/Licitaciones.UnitTests/Application/Proveedores/RegistrarProveedorServiceTests.cs).

## HU-02 — Pruebas unitarias

| Escenario | Variantes | Casos |
| --- | --- | ---: |
| Código ausente | `null`, vacío y espacios | 3 |
| Título ausente | `null`, vacío y espacios | 3 |
| Limpieza sin alterar espacios internos | código y título | 1 |
| Código equivalente | mayúsculas y espacios laterales | 3 |
| Longitudes máximas | código de 100 y título de 200 caracteres | 1 |
| Texto mayor al máximo | código y título | 2 |
| Caracteres de control | código y título | 2 |
| Unicode equivalente | código compuesto y descompuesto | 1 |
| Presupuesto no positivo | cero y negativo | 2 |
| Más de dos decimales | dos montos | 2 |
| Presupuesto superior al máximo | límite de `numeric(18,2)` | 1 |
| Presupuesto permitido | mínimo, ordinario y máximo | 3 |
| Fecha ausente | valor predeterminado | 1 |
| Creación válida | `Guid`, Borrador, UTC y auditoría | 1 |
| Servicio válido | consulta, guardado y resultado completo | 1 |
| Código duplicado | mayúsculas y espacios laterales | 3 |
| **Total HU-02 unitarias** | | **30** |

Archivos principales:

- [dominio de licitaciones](../tests/Licitaciones.UnitTests/Domain/Licitaciones/LicitacionTests.cs);
- [caso de uso](../tests/Licitaciones.UnitTests/Application/Licitaciones/CrearLicitacionServiceTests.cs).

## HU-03 — Pruebas unitarias

| Escenario | Variantes | Casos |
| --- | --- | ---: |
| Publicación válida | transición a `Publicada`, auditoría UTC y conservación de datos | 1 |
| Fecha de cierre no futura | fecha igual y anterior al reloj | 2 |
| Segunda publicación | estado y auditoría permanecen sin cambios | 1 |
| Servicio válido | consulta, guardado y resultado completo | 1 |
| Consulta para confirmación | datos de solo lectura y ningún guardado | 1 |
| Identificador inexistente | publicación y consulta | 2 |
| Cierre vencido en aplicación | rechazo y ningún guardado | 1 |
| Conflicto de concurrencia | excepción segura sin detalles de EF o `xmin` | 1 |
| Cancelación | propagación del token a lectura y guardado | 1 |
| **Total HU-03 unitarias** | | **11** |

Archivos principales:

- [reglas de publicación](../tests/Licitaciones.UnitTests/Domain/Licitaciones/LicitacionTests.cs);
- [caso de uso de publicación](../tests/Licitaciones.UnitTests/Application/Licitaciones/PublicarLicitacionServiceTests.cs).

## HU-01 — Pruebas funcionales

### API

| Escenario | Variantes | Casos |
| --- | --- | ---: |
| Registro válido | `201`, limpieza, normalización y guardado | 1 |
| Nombre inválido | `null`, vacío, espacios y `@` | 4 |
| Nombre duplicado | `409` y contrato Problem Details | 1 |
| JSON inválido | cuerpo truncado y respuesta `400` | 1 |
| Error inesperado | `500` sin filtrar detalles técnicos | 1 |
| OpenAPI | respuestas `201`, `400`, `409`, `422` y `500` | 1 |
| **Total API** | | **9** |

### Web MVC

| Escenario | Variantes | Casos |
| --- | --- | ---: |
| Formulario de registro | estructura, accesibilidad y antiforgery | 1 |
| Navegación desde inicio | idioma español, menú colapsable y enlace | 1 |
| Registro válido | guardado, normalización, redirección y mensaje | 1 |
| POST sin antiforgery | `400` y ningún guardado | 1 |
| Nombre inválido | vacío, espacios y `@` | 3 |
| Nombre duplicado | equivalencia por espacios y mayúsculas | 1 |
| **Total Web** | | **8** |
| **Total funcionales** | API 9 + Web 8 | **17** |

Archivos principales:

- [pruebas funcionales de API](../tests/Licitaciones.FunctionalTests/Api/Proveedores/RegistrarProveedorEndpointTests.cs);
- [pruebas funcionales de Web](../tests/Licitaciones.FunctionalTests/Web/Proveedores/RegistrarProveedorWebTests.cs).

## HU-02 — Pruebas funcionales

| Canal y escenario | Variantes | Casos |
| --- | --- | ---: |
| API: creación válida | `201`, DTO completo y Borrador | 1 |
| API: datos ausentes o inválidos | campos requeridos, monto y precisión | 9 |
| API: textos fuera de contrato | longitudes y caracteres de control | 4 |
| API: código duplicado | `409` Problem Details | 1 |
| API: JSON o fecha sin zona inválidos | código, monto y fecha | 4 |
| API: tipo de contenido | `415` Problem Details | 1 |
| API: OpenAPI | contrato y respuestas | 1 |
| Web: formulario | campos, tipos, accesibilidad básica y antiforgery | 1 |
| Web: navegación | enlace desde inicio | 1 |
| Web: creación válida | UTC, guardado, PRG y confirmación | 1 |
| Web: campo inválido | código, título, monto y fecha | 6 |
| Web: longitud inválida | código y título | 2 |
| Web: precisión inválida | error junto al presupuesto | 1 |
| Web: código duplicado | error junto al código | 1 |
| Web: sin antiforgery | `400` y ningún guardado | 1 |
| **Total HU-02 funcionales** | API 21 + Web 14 | **35** |

Archivos principales:

- [API](../tests/Licitaciones.FunctionalTests/Api/Licitaciones/CrearLicitacionEndpointTests.cs);
- [Web](../tests/Licitaciones.FunctionalTests/Web/Licitaciones/CrearLicitacionWebTests.cs).

## HU-03 — Pruebas funcionales

### API

| Escenario | Casos |
| --- | ---: |
| Publicación válida con representación completa | 1 |
| Identificador que no es UUID | 1 |
| Licitación inexistente | 1 |
| Fecha de cierre vencida | 1 |
| Licitación ya publicada | 1 |
| Conflicto de concurrencia seguro | 1 |
| Contrato OpenAPI sin cuerpo y respuestas documentadas | 1 |
| **Total API** | **7** |

### Web MVC

| Escenario | Casos |
| --- | ---: |
| Confirmación con datos de solo lectura y antiforgery | 1 |
| Publicación válida, PRG, mensaje y botón oculto | 1 |
| Licitación inexistente | 1 |
| Fecha vencida sin guardado | 1 |
| Conflicto con mensaje seguro | 1 |
| POST sin antiforgery | 1 |
| **Total Web** | **6** |
| **Total HU-03 funcionales** | **13** |

Archivos principales:

- [API](../tests/Licitaciones.FunctionalTests/Api/Licitaciones/PublicarLicitacionEndpointTests.cs);
- [Web](../tests/Licitaciones.FunctionalTests/Web/Licitaciones/PublicarLicitacionWebTests.cs).

## HU-01 — Pruebas de integración

| Área y escenario | Infraestructura verificada | Casos |
| --- | --- | ---: |
| Modelo EF | tabla, clave, campos requeridos, índice único y concurrencia | 1 |
| Repositorio: agregar y consultar | persistencia, consulta y `xmin` en PostgreSQL | 1 |
| Repositorio: duplicado concurrente | índice único y traducción del conflicto | 1 |
| Migración inicial | `CrearProveedores` sobre una base vacía | 1 |
| API real | HTTP, aplicación, EF Core y PostgreSQL | 1 |
| Web real | antiforgery, MVC, aplicación, EF Core y PostgreSQL | 1 |
| **Total integración** | | **6** |

Archivos principales:

- [configuración del modelo](../tests/Licitaciones.IntegrationTests/Persistence/ProveedorModelConfigurationTests.cs);
- [repositorio](../tests/Licitaciones.IntegrationTests/Persistence/ProveedorRepositoryTests.cs);
- [migración](../tests/Licitaciones.IntegrationTests/Persistence/MigracionesTests.cs);
- [API con infraestructura real](../tests/Licitaciones.IntegrationTests/Api/Proveedores/RegistrarProveedorApiTests.cs);
- [Web con infraestructura real](../tests/Licitaciones.IntegrationTests/Web/Proveedores/RegistrarProveedorWebTests.cs).

## HU-02 — Pruebas de integración

| Área | Infraestructura verificada | Casos |
| --- | --- | ---: |
| Modelo EF | campos, precisión, restricciones, índices, FK de estado y `xmin` | 1 |
| Repositorio | persistencia y consulta exacta en PostgreSQL | 1 |
| Duplicado concurrente | índice único y traducción controlada | 1 |
| Actualización concurrente | dos contextos y excepción por `xmin` obsoleto | 1 |
| Evolución de migraciones | agrega licitaciones sin perder proveedores | 1 |
| API real | HTTP, aplicación, EF Core y PostgreSQL | 1 |
| Web real | calendario, antiforgery, zona horaria y PostgreSQL | 1 |
| **Total HU-02 integración** | | **7** |

Archivos principales:

- [modelo EF](../tests/Licitaciones.IntegrationTests/Persistence/LicitacionModelConfigurationTests.cs);
- [repositorio](../tests/Licitaciones.IntegrationTests/Persistence/LicitacionRepositoryTests.cs);
- [migraciones](../tests/Licitaciones.IntegrationTests/Persistence/MigracionesTests.cs);
- [API real](../tests/Licitaciones.IntegrationTests/Api/Licitaciones/CrearLicitacionApiTests.cs);
- [Web real](../tests/Licitaciones.IntegrationTests/Web/Licitaciones/CrearLicitacionWebTests.cs).

## HU-03 — Pruebas de integración

| Área | Infraestructura verificada | Casos nuevos |
| --- | --- | ---: |
| Repositorio | publicación, auditoría UTC y cambio de `xmin` en PostgreSQL | 1 |
| API real | HTTP, aplicación, EF Core, estado persistido y `xmin` | 1 |
| Web real | antiforgery, PRG, aplicación, PostgreSQL y `xmin` | 1 |
| **Total HU-03 integración** | | **3** |

La prueba concurrente ya contabilizada en HU-02 se refactorizó para ejecutar
dos publicaciones sobre copias distintas. Ahora también verifica la traducción
segura de `DbUpdateConcurrencyException` y confirma con un tercer contexto que
prevalece el primer cambio, sin contabilizar el mismo caso dos veces.

Archivos principales:

- [repositorio y concurrencia](../tests/Licitaciones.IntegrationTests/Persistence/LicitacionRepositoryTests.cs);
- [API real](../tests/Licitaciones.IntegrationTests/Api/Licitaciones/PublicarLicitacionApiTests.cs);
- [Web real](../tests/Licitaciones.IntegrationTests/Web/Licitaciones/PublicarLicitacionWebTests.cs).

## Ejecución local

Desde la raíz del repositorio:

```powershell
dotnet restore .\Licitaciones.sln
dotnet build .\Licitaciones.sln --no-restore
dotnet test .\Licitaciones.sln --no-build --no-restore
```

Por nivel:

```powershell
dotnet test .\tests\Licitaciones.UnitTests\Licitaciones.UnitTests.csproj
dotnet test .\tests\Licitaciones.FunctionalTests\Licitaciones.FunctionalTests.csproj
dotnet test .\tests\Licitaciones.IntegrationTests\Licitaciones.IntegrationTests.csproj
```

## Integración continua

El workflow [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) se ejecuta
en cada Pull Request dirigido a `main` y en cada push a `main`. Usa Ubuntu y
.NET 9, restaura dependencias, compila en Release, verifica Docker, ejecuta la
solución completa y publica los archivos TRX como artefacto durante siete días.

El job tiene un tiempo máximo de 20 minutos. Aunque los proyectos incluyen el
colector de cobertura, todavía no existe un reporte ni una puerta de cobertura
configurada en la CI.
