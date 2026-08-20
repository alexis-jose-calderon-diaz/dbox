## Why

La CLI actual ofrece varias formas redundantes de pedir JSON, ingresar datos y
consultar el schema, lo que aumenta la superficie del contrato para scripts y
agentes. La salida debe ser JSON estable por defecto, con una unica forma de
enviar payloads estructurados y consultas paginables para recorrer catalogos
grandes.

## What Changes

- **BREAKING** Hacer que las respuestas exitosas y los errores operacionales
  sean siempre JSON; eliminar `--output text` y `--output json`.
- **BREAKING** Eliminar el alias `dbox activity --schema`; conservar solo
  `dbox activity schema`.
- **BREAKING** Requerir `--json <objeto>` para `activity add` y `activity
  update`, eliminando las opciones de campos individuales.
- **BREAKING** Mover los filtros de `activity list` al payload JSON opcional y
  retirar `--type` y `--status` como opciones de terminal.
- Mantener `init` y `activity schema` sin payload, y conservar ayuda textual
  nativa mediante `--help`.
- Mantener los IDs de `get`, `update` y `delete` como argumentos posicionales.
- Agregar `activity count`, con filtros JSON opcionales, para devolver la
  cantidad de actividades que coinciden.
- Agregar `--skip` y `--take` a `activity list`; ordenar los resultados por
  `created_at ASC` y `id ASC` como desempate determinista.

## Capabilities

### New Capabilities
- `activity-count`: Contar actividades del catalogo, opcionalmente filtradas
  por tipo y estado.

### Modified Capabilities
- `cli-contract`: Sustituir los formatos de salida seleccionables por un
  contrato JSON operacional obligatorio y retirar aliases redundantes.
- `activity-crud`: Recibir cuerpos y filtros mediante JSON, paginar listados y
  cambiar el orden observable de las actividades.
- `activity-contract`: Exponer el schema solo mediante el subcomando canonico
  y retirar sus variantes de alias y formato.

## Impact

- Actualizar `PROJECT.md`, las specs principales y todas las pruebas que
  describen el contrato de CLI actual.
- Afectar la composicion y ejecucion de comandos, el parseo de inputs, el
  escritor de salida/error y el repositorio de actividades.
- Agregar el comando y pruebas de conteo, y extender la consulta de listado
  con ordenamiento y paginacion.
- No se agregan dependencias ni cambia el esquema SQLite.
