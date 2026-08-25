# Integración de módulos

## Vista general

Los módulos colaboran principalmente a través de servicios de Application y
repositorios. Las relaciones entre licitaciones, proveedores y ofertas sí se
persisten mediante claves foráneas. Los niveles de aprobación y los tipos de
cambio ofrecen capacidades de consulta y gestión independientes; el modelo no
persiste una asociación de esas entidades con una licitación u oferta.

## Proveedores + Ofertas

Una oferta requiere que exista el proveedor indicado por `ProveedorId`. El
`OfertaService` consulta el repositorio de proveedores antes de crear la oferta.
Si el proveedor no existe, la operación se rechaza.

La relación se persiste en `ofertas.proveedor_id` como clave foránea hacia
`proveedores.id`, con eliminación restringida para evitar eliminar un proveedor
que tenga ofertas.

## Licitaciones + Ofertas

Una oferta requiere una licitación existente, publicada y abierta. El servicio
verifica el estado `Publicada` y que la fecha de cierre no haya vencido.

El monto de la oferta debe:

- ser mayor que cero;
- tener como máximo dos decimales;
- no superar `PresupuestoEstimadoCrc` de la licitación.

La combinación `LicitacionId` + `ProveedorId` tiene un índice único, por lo que
no se permite más de una oferta del mismo proveedor para una licitación. La
relación se persiste mediante `ofertas.licitacion_id`. Está configurada con
`Cascade` a nivel de persistencia, pero el servicio de aplicación bloquea la
eliminación de licitaciones que contienen ofertas, por lo que ese cascade no
forma parte del flujo normal expuesto al usuario.

## Ofertas + mejor oferta

`OfertaService.MejorAsync` consulta las ofertas de una licitación, conserva las
que tienen monto mayor que cero y no superan el presupuesto, y selecciona la de
menor monto.

A partir de la oferta seleccionada se calcula el porcentaje de ahorro respecto
al presupuesto de la licitación. El servicio también devuelve una clasificación
según el ahorro. Si no hay ofertas válidas, devuelve la situación de ausencia
de oferta válida.

Esta integración es una consulta de servicio; no agrega una entidad o tabla
para almacenar una oferta ganadora.

## Licitaciones + niveles de aprobación

Los niveles de aprobación se administran mediante `NivelAprobacionService`.
Sus rangos validan límites, montos y traslapes, y `DeterminarAsync` busca el
nivel que incluye un monto.

La determinación se expone como servicio de aplicación y mediante el endpoint
`GET /api/v1/niveles-aprobacion/determinar/{monto}`. Actualmente no existe una
asociación persistida entre `NivelAprobacion` y `Licitacion`, `Oferta` o una
operación de aprobación. El sistema no asigna automáticamente un nivel a una
licitación ni guarda esa asignación.

## Tipos de cambio

`TipoCambioService` administra los tipos de cambio CRC/USD y expone la
conversión CRC → USD usando el tipo de cambio activo. El valor CRC continúa
siendo la fuente de verdad para presupuestos, montos de ofertas y demás valores
monetarios del dominio.

La entidad `TipoCambio` posee un índice único filtrado para permitir un solo
registro activo. El servicio consulta el activo y utiliza su valor
`CrcPorUsd` para convertir un monto CRC.

No existe una relación persistida entre `TipoCambio` y `Licitacion`, `Oferta`,
`Proveedor` o `NivelAprobacion`. La conversión es una operación de servicio/API,
no una modificación de los montos almacenados.

## Flujo completo del sistema

Un recorrido funcional posible con las capacidades implementadas es:

1. Registrar un proveedor mediante Web o API.
2. Crear una licitación con estado `Borrador`.
3. Publicar la licitación cuando sus datos y fecha de cierre son válidos.
4. Registrar ofertas de proveedores existentes para la licitación publicada y
   abierta.
5. Obtener la mejor oferta y el ahorro respecto al presupuesto.
6. Consultar el nivel de aprobación aplicable al monto mediante el servicio o
   su endpoint independiente.
7. Convertir un monto CRC a USD cuando se requiera, utilizando el tipo de
   cambio activo.

## Integraciones persistidas e independientes

### Persistidas

- `Oferta` → `Licitacion` mediante `LicitacionId`.
- `Oferta` → `Proveedor` mediante `ProveedorId`.
- `Licitacion.Estado` → catálogo de estados de licitación.
- Integridad, unicidad y montos mediante PostgreSQL y EF Core.

### Consultas o servicios independientes

- La selección de mejor oferta no se almacena como relación adicional.
- La determinación de nivel de aprobación no se asigna ni persiste en una
  licitación u oferta.
- La conversión CRC → USD no cambia los montos CRC almacenados.
- El tipo de cambio no tiene clave foránea hacia las entidades de contratación.

La interfaz Web y la API usan los mismos servicios de Application para aplicar
estas reglas, mientras Infrastructure implementa el acceso a PostgreSQL.
