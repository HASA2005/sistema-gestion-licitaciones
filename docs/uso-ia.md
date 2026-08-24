# Uso de inteligencia artificial

## Herramienta utilizada

Durante la Iteración XP 1 se utilizó OpenAI Codex como herramienta de asistencia
para el desarrollo. Su participación no se considera autoría autónoma ni
reemplaza la responsabilidad del estudiante sobre el proyecto.

## Formas de asistencia

Codex se utilizó para:

- analizar requisitos y contrastarlos con la estructura del repositorio;
- proponer tareas pequeñas y secuencias RED-GREEN-REFACTOR;
- sugerir y preparar pruebas, código y documentación;
- interpretar errores de compilación y resultados de pruebas compartidos;
- revisar nombres, estructura, dependencias y separación por capas;
- proponer comandos Git, mensajes de commit y descripciones de Pull Requests;
- ayudar a configurar la integración continua.

Entre los casos concretos asistidos se encuentran la normalización Unicode del
nombre, el tratamiento de duplicados, la persistencia con PostgreSQL, los
contratos HTTP de la API, el formulario MVC y el workflow de GitHub Actions.

## Control y responsabilidad humana

El estudiante mantuvo la decisión sobre el alcance y el orden de trabajo,
revisó los cambios, verificó los resultados y realizó o autorizó su versionado.
La creación y fusión de Pull Requests se mantuvo bajo control humano.

Las sugerencias de la IA se contrastaron mediante:

- revisión de diferencias y estado de Git;
- compilación de la solución;
- ejecución de pruebas;
- observación de fallos esperados durante RED;
- nueva ejecución durante GREEN y después de cambios;
- validación de GitHub Actions antes de integrar a `main`.

Al cierre técnico se registraron 46 casos correctos: 23 unitarios, 17
funcionales y 6 de integración.

## Asistencia durante HU-02

En la Iteración XP 2, Codex ayudó a extraer del PDF las reglas aplicables a la
creación de licitaciones y a distinguirlas de las reglas de publicación. También
preparó cambios en dominio, aplicación, PostgreSQL, API, MVC, pruebas y
documentación para revisión del estudiante.

Entre las decisiones contrastadas se encuentran:

- no exigir una fecha futura al guardar un Borrador;
- interpretar `datetime-local` mediante `America/Costa_Rica` y almacenar UTC;
- usar `decimal` y `numeric(18,2)` sin redondeo silencioso;
- conservar los espacios internos del código porque el requisito solo elimina
  espacios laterales;
- impedir que API o MVC seleccionen un estado inicial diferente de `Borrador`;
- sembrar los estados y proteger el código mediante un índice único;
- limitar código y título para evitar errores del índice de PostgreSQL;
- exigir zona horaria explícita en las fechas recibidas por la API.

La validación detectó y corrigió la interpretación cultural de límites
decimales en MVC, el acceso al modelo de diseño requerido para inspeccionar
restricciones `CHECK` en EF Core 9, textos sin longitud máxima, fechas API sin
zona explícita y detalles técnicos en errores. Después de las correcciones se
ejecutaron 118 casos: 53 unitarios, 52 funcionales y 13 de integración.

## Asistencia durante HU-03

Codex ayudó a contrastar la Issue #18 con la documentación y el código de
HU-02, proponer criterios verificables y preparar para revisión los cambios de
dominio, aplicación, persistencia, API, MVC, pruebas y documentación. La
prioridad alta y la estimación inicial de 5 puntos proceden del Planning Game,
no de la herramienta.

Entre las decisiones revisadas se encuentran:

- modelar la publicación como una transición explícita de `Borrador` a
  `Publicada`, separada de la creación y sin aceptar campos editables;
- considerar inválida una fecha de cierre igual o anterior al instante de
  publicación;
- usar `TimeProvider` para comprobar el tiempo de forma determinista y guardar
  `UpdatedAt` en UTC;
- distinguir identificador inválido, licitación inexistente, estado no
  publicable, datos no publicables y conflicto de concurrencia mediante códigos
  Problem Details estables;
- comprobar la actualización con PostgreSQL real y dos contextos que compiten
  sobre el mismo `xmin`;
- usar una vista MVC de confirmación con datos de solo lectura, antiforgery y
  POST-Redirect-GET;
- redirigir la creación exitosa hacia la confirmación del Borrador recién
  creado, sin incorporar un listado general fuera del alcance.

Después de revisar y ejecutar la solución completa, el resultado consolidado es
de 145 casos: 64 unitarios, 65 funcionales y 16 de integración. Las decisiones
propuestas por la herramienta se aceptaron únicamente después de comprobarlas
mediante código, revisión y pruebas automatizadas.

## Ejemplos de corrección mediante evidencia

- Una prueba con dos representaciones Unicode visualmente equivalentes falló
  inicialmente; el resultado condujo a aplicar Unicode Form C.
- Las pruebas de nombres inválidos guiaron la definición explícita de los
  caracteres aceptados.
- Las pruebas de duplicidad llevaron a proteger la regla tanto en el caso de
  uso como mediante un índice único en PostgreSQL.
- Las pruebas de API y MVC verificaron el comportamiento observable antes de
  completar cada interfaz.

Estos ejemplos muestran que las respuestas de la IA fueron sometidas a
comprobación y podían requerir ajustes.

## Límites del uso de IA

- Codex no realizó una aprobación académica o funcional del proyecto.
- No sustituyó las pruebas ni la revisión humana.
- No determinó por sí solo prioridades, estimaciones o aceptación de
  interesados.
- No deben atribuirse a la IA decisiones del profesor o del cliente que no
  estén documentadas.
- No se deben introducir secretos, credenciales reales ni cadenas de conexión
  sensibles a partir de sugerencias de IA.

Este documento también fue preparado con asistencia de Codex y queda sujeto a
revisión y aceptación del estudiante antes de integrarse.
