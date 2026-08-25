# Uso de inteligencia artificial

## Alcance y transparencia

Durante el desarrollo se utilizÃ³ asistencia de inteligencia artificial como apoyo al trabajo del equipo. Las herramientas utilizadas fueron **OpenAI Codex** y **ChatGPT**. TambiÃ©n pudo utilizarse un asistente integrado en VS Code, sin atribuirle proveedor ni modelo concreto.

La IA fue una herramienta de apoyo: no sustituyÃ³ el criterio del equipo, la revisiÃ³n acadÃ©mica, la validaciÃ³n funcional ni la responsabilidad sobre el cÃ³digo final.

## Actividades asistidas

OpenAI Codex y ChatGPT se utilizaron como apoyo para:

- analizar requisitos y convertirlos en historias y criterios verificables;
- proponer implementaciones respetando la separaciÃ³n por capas;
- generar y revisar cÃ³digo de Domain, Application, Infrastructure, API y Web;
- crear, revisar y ampliar pruebas unitarias, funcionales, de integraciÃ³n y E2E;
- interpretar errores de compilaciÃ³n, pruebas, Razor, EF Core y PostgreSQL;
- depurar flujos Web y estabilizar errores esperables;
- preparar configuraciones de Docker Compose y Kubernetes;
- revisar comandos y cambios de Git/GitHub, incluyendo ramas, commits y Pull Requests;
- redactar y revisar documentaciÃ³n tÃ©cnica y acadÃ©mica.

ChatGPT tambiÃ©n se utilizÃ³ para anÃ¡lisis de requisitos, revisiÃ³n de
implementaciones, interpretaciÃ³n de errores, preparaciÃ³n y revisiÃ³n de pruebas,
Docker y Kubernetes, Git/GitHub, documentaciÃ³n, validaciÃ³n funcional y
preparaciÃ³n para la defensa acadÃ©mica.

## Proceso de revisiÃ³n humana

Las sugerencias se revisaron antes de incorporarse. El equipo mantuvo la decisiÃ³n sobre alcance, reglas de negocio, prioridades, aceptaciÃ³n y forma de integraciÃ³n.

La validaciÃ³n incluyÃ³:

- lectura y revisiÃ³n de diferencias;
- compilaciÃ³n de la soluciÃ³n;
- ejecuciÃ³n de pruebas automatizadas;
- pruebas funcionales mediante solicitudes HTTP;
- pruebas manuales de formularios, navegaciÃ³n y presentaciÃ³n Web;
- ejecuciÃ³n de pruebas de integraciÃ³n con PostgreSQL;
- ejecuciÃ³n de pruebas E2E con Playwright cuando correspondÃ­a;
- revisiÃ³n de Docker, Kubernetes y CI.

La Ãºltima ejecuciÃ³n manual confirmada para el cierre registrÃ³ 195 pruebas correctas y 0 fallidas. Ese resultado pertenece a la ejecuciÃ³n del proyecto y no se presenta como una afirmaciÃ³n autÃ³noma de la IA.

## RelaciÃ³n con XP

La IA apoyÃ³ el ciclo TDD y RED-GREEN-REFACTOR cuando existÃ­a una prueba o criterio verificable para guiar el cambio. TambiÃ©n ayudÃ³ en refactoring, simple design, integraciÃ³n continua, small releases y preparaciÃ³n de feedback. El equipo decidiÃ³ quÃ© sugerencias aceptar y comprobÃ³ que no alteraran las reglas de negocio sin justificaciÃ³n.

## Ejemplos de validaciÃ³n

- La normalizaciÃ³n Unicode y los duplicados se comprobaron con pruebas de dominio, aplicaciÃ³n y PostgreSQL.
- Las fechas se verificaron para conservar UTC internamente y presentarse en `America/Costa_Rica`.
- Las restricciones de ofertas, niveles de aprobaciÃ³n y tipos de cambio se contrastaron con servicios, repositorios, migraciones y pruebas.
- Los errores Web de negocio se reprodujeron y se verificÃ³ que regresaran a un formulario o mostraran un mensaje controlado en lugar de una pÃ¡gina genÃ©rica.
- Docker Compose, Kubernetes, health checks, probes y CI se revisaron contra sus archivos reales.

## Responsabilidad del equipo

Los integrantes son responsables del resultado final. Deben poder explicar, defender y mantener cualquier cÃ³digo generado o asistido por IA. La asistencia no constituye aprobaciÃ³n acadÃ©mica, funcional ni de seguridad.

No se introdujeron secretos ni credenciales reales por medio de sugerencias de IA. Las cadenas de conexiÃ³n de los entornos se mantienen como configuraciÃ³n externa o valores de ejemplo.

## LÃ­mites de la evidencia

El repositorio no conserva una transcripciÃ³n completa de cada interacciÃ³n con la herramienta ni permite identificar quÃ© sugerencia concreta produjo cada lÃ­nea. Por ello este documento describe categorÃ­as de asistencia y controles realizados, pero no atribuye decisiones individuales, horas de trabajo, prioridades del cliente o resultados no verificables a la IA.

