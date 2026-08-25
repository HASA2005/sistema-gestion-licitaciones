# Uso de inteligencia artificial

## Alcance y transparencia

Durante el desarrollo se utilizó asistencia de inteligencia artificial como apoyo al trabajo del equipo. Las herramientas utilizadas fueron **OpenAI Codex** y **ChatGPT**. También pudo utilizarse un asistente integrado en VS Code, sin atribuirle proveedor ni modelo concreto.

La IA fue una herramienta de apoyo: no sustituyó el criterio del equipo, la revisión académica, la validación funcional ni la responsabilidad sobre el código final.

## Actividades asistidas

OpenAI Codex y ChatGPT se utilizaron como apoyo para:

- analizar requisitos y convertirlos en historias y criterios verificables;
- proponer implementaciones respetando la separación por capas;
- generar y revisar código de Domain, Application, Infrastructure, API y Web;
- crear, revisar y ampliar pruebas unitarias, funcionales, de integración y E2E;
- interpretar errores de compilación, pruebas, Razor, EF Core y PostgreSQL;
- depurar flujos Web y estabilizar errores esperables;
- preparar configuraciones de Docker Compose y Kubernetes;
- revisar comandos y cambios de Git/GitHub, incluyendo ramas, commits y Pull Requests;
- redactar y revisar documentación técnica y académica.

ChatGPT también se utilizó para análisis de requisitos, revisión de
implementaciones, interpretación de errores, preparación y revisión de pruebas,
Docker y Kubernetes, Git/GitHub, documentación, validación funcional y
preparación para la defensa académica.

## Proceso de revisión humana

Las sugerencias se revisaron antes de incorporarse. El equipo mantuvo la decisión sobre alcance, reglas de negocio, prioridades, aceptación y forma de integración.

La validación incluyó:

- lectura y revisión de diferencias;
- compilación de la solución;
- ejecución de pruebas automatizadas;
- pruebas funcionales mediante solicitudes HTTP;
- pruebas manuales de formularios, navegación y presentación Web;
- ejecución de pruebas de integración con PostgreSQL;
- ejecución de pruebas E2E con Playwright cuando correspondía;
- revisión de Docker, Kubernetes y CI.

La última ejecución manual confirmada para el cierre registró 195 pruebas correctas y 0 fallidas. Ese resultado pertenece a la ejecución del proyecto y no se presenta como una afirmación autónoma de la IA.

## Relación con XP

La IA apoyó el ciclo TDD y RED-GREEN-REFACTOR cuando existía una prueba o criterio verificable para guiar el cambio. También ayudó en refactoring, simple design, integración continua, small releases y preparación de feedback. El equipo decidió qué sugerencias aceptar y comprobó que no alteraran las reglas de negocio sin justificación.

## Ejemplos de validación

- La normalización Unicode y los duplicados se comprobaron con pruebas de dominio, aplicación y PostgreSQL.
- Las fechas se verificaron para conservar UTC internamente y presentarse en `America/Costa_Rica`.
- Las restricciones de ofertas, niveles de aprobación y tipos de cambio se contrastaron con servicios, repositorios, migraciones y pruebas.
- Los errores Web de negocio se reprodujeron y se verificó que regresaran a un formulario o mostraran un mensaje controlado en lugar de una página genérica.
- Docker Compose, Kubernetes, health checks, probes y CI se revisaron contra sus archivos reales.

## Responsabilidad del equipo

Los integrantes son responsables del resultado final. Deben poder explicar, defender y mantener cualquier código generado o asistido por IA. La asistencia no constituye aprobación académica, funcional ni de seguridad.

No se introdujeron secretos ni credenciales reales por medio de sugerencias de IA. Las cadenas de conexión de los entornos se mantienen como configuración externa o valores de ejemplo.

## Límites de la evidencia

El repositorio no conserva una transcripción completa de cada interacción con la herramienta ni permite identificar qué sugerencia concreta produjo cada línea. Por ello este documento describe categorías de asistencia y controles realizados, pero no atribuye decisiones individuales, horas de trabajo, prioridades del cliente o resultados no verificables a la IA.

