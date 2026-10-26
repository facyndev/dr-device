Readme de Dr. Device

## Limpieza de cuentas eliminadas

Quartz.NET ejecuta `UserCleanupJob` mientras la API está encendida. La primera
ejecución ocurre después del intervalo configurado. Cada ejecución elimina
físicamente como máximo un lote, empezando por las cuentas más antiguas.

En `src/Presentation/appsettings.json`:

```json
"UserCleanup": {
  "IntervalSeconds": 15,
  "RetentionDays": 30,
  "BatchSize": 100
}
```

Todos los valores deben ser positivos. Los cambios requieren reiniciar la API.
También pueden configurarse con variables de entorno, por ejemplo
`UserCleanup__BatchSize`.

Solo se eliminan cuentas cuyo `DeletedAt` sea menor o igual que la fecha actual
UTC menos `RetentionDays`. Reactivar una cuenta pone `DeletedAt` en null y la
excluye de la limpieza. Las cuentas con órdenes propias, órdenes asignadas u
ofertas se conservan; sus relaciones actuales impiden el borrado. Las
especializaciones y los tokens de restablecimiento se eliminan mediante las
cascadas existentes.

El job no se superpone consigo mismo dentro del mismo scheduler. La sentencia
de PostgreSQL selecciona y elimina el lote de forma atómica, omitiendo filas
bloqueadas mediante `FOR UPDATE SKIP LOCKED`. No se acumulan ejecuciones
perdidas mientras la API está apagada; los usuarios vencidos siguen siendo
elegibles en la siguiente ejecución. Cada instancia de API tiene su propio
scheduler y límite de lote.

