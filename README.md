
## Dirección en el registro

`POST /api/auth/register` requiere estos campos adicionales:

```json
{
  "address": "Av. Santa Fe 260",
  "province": "Córdoba",
  "locality": "Córdoba"
}
```

Se agregan al resto de los datos del registro. `Address` es el nombre correcto
 del atributo; `Location` se calcula en el servidor y no se recibe del cliente.
Georef se consulta en `https://apis.datos.gob.ar/georef/api/direcciones`
con provincia y localidad como filtros. Si devuelve varias coincidencias, se usa
la primera del array, que debe tener coordenadas válidas. La dirección normalizada y `location.latitude` /
`location.longitude` se devuelven tanto al registrarse como al iniciar sesión.

Una dirección sin resultados o cuya primera coincidencia no tiene coordenadas válidas devuelve HTTP 400.
Una falla, respuesta inválida o timeout de Georef devuelve HTTP 503 y no crea
la cuenta. El timeout de consulta es de 10 segundos. Georef estima coordenadas:
no verifica la existencia física del inmueble ni que el usuario viva allí.
Documentación: https://www.argentina.gob.ar/georef/normalizacion-de-direcciones

La migración `AddUserAddressAndLocation` agrega Address, Latitude y Longitude
a Users. Son columnas opcionales para conservar las cuentas existentes; en
los nuevos registros la dirección es obligatoria. Aplicar antes de iniciar:

```powershell
dotnet ef database update --project src/Infrastructure --startup-project src/Presentation
```

## Actualizar la cuenta

`PATCH /api/users/me` actualiza la cuenta autenticada y devuelve HTTP 204.
Enviar solo los campos que se quieran modificar. Los omitidos o null conservan
su valor, excepto `phone: null`, que borra el teléfono. Ejemplo con todos los campos:

```json
{
  "firstName": "María",
  "lastName": "Pérez",
  "email": "maria@example.com",
  "phone": "3415555555",
  "address": "Pellegrini 4444",
  "province": "Santa Fe",
  "locality": "Rosario"
}
```

El ID se obtiene de la sesión. Conserva rol y contraseña. Para cambiar la dirección,
enviar address, province y locality juntos; solo entonces se consulta Georef y se
guardan las coordenadas del primer resultado.
Un correo ocupado por otra cuenta o una cuenta eliminada devuelve 409;
datos inválidos devuelven 400 y una falla de Georef devuelve 503.
Las validaciones se completan antes de modificar la cuenta.


## Consultar la cuenta

`GET /api/users/me` devuelve HTTP 200 con el `UserDTO` de la cuenta autenticada:
ID, nombre, apellido, email, teléfono, rol, fechas de creación y actualización,
dirección y ubicación. No incluye contraseña ni tokens. El ID se obtiene de
la sesión y se consultan los datos actuales en la base de datos.
Devuelve 401 sin autenticación o si la cuenta fue eliminada, y 404 si ya no
existe. La respuesta lleva `Cache-Control: no-store`.
