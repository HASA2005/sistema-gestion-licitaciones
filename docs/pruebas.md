# Estrategia y evidencia de pruebas

## Resultado de la Iteración XP 1

La solución contiene 27 métodos de prueba xUnit que producen 46 casos
ejecutados. Cada fila de `[InlineData]` cuenta como un caso independiente.

| Nivel | Casos |
| --- | ---: |
| Unitarias | 23 |
| Funcionales | 17 |
| Integración | 6 |
| **Total** | **46** |

## Herramientas y aislamiento

- xUnit ejecuta todos los proyectos de pruebas.
- Las pruebas unitarias no requieren infraestructura externa.
- Las pruebas funcionales usan `WebApplicationFactory` y reemplazan
  `IProveedorRepository` por una implementación en memoria.
- Las pruebas funcionales Web usan protección de datos efímera para procesar
  antiforgery sin depender de claves persistentes.
- Cinco pruebas de integración usan Testcontainers con `postgres:16-alpine`;
  la sexta inspecciona los metadatos del modelo EF Core.
- Docker debe estar iniciado para ejecutar la suite de integración completa.
- No existe todavía una prueba de navegador con Playwright o Selenium; la
  interfaz MVC se verifica mediante solicitudes HTTP y análisis del HTML.

## Matriz de pruebas unitarias

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

## Matriz de pruebas funcionales

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

## Matriz de pruebas de integración

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
