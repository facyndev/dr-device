# Dr. Device API

API REST construida con ASP.NET Core y .NET 10 para la plataforma Dr. Device, orientada a la gestión de reparaciones y servicios técnicos de dispositivos. Conecta clientes y técnicos a través de órdenes de trabajo, ofertas, especializaciones y categorización de equipos.

---

## Descripción del proyecto y stack

La solución sigue los principios de Clean Architecture (arquitectura limpia), separando responsabilidades en capas desacopladas con inversión de dependencias.

### Stack tecnológico

- **Plataforma y lenguaje:** .NET 10 (`net10.0`) y C# 13.
- **Framework web:** ASP.NET Core Web API.
- **Acceso a datos y ORM:** Entity Framework Core 10 con proveedor `Npgsql.EntityFrameworkCore.PostgreSQL`.
- **Base de datos:** PostgreSQL.
- **Autenticación y autorización:** JWT (JSON Web Tokens) firmados con HMAC SHA-256 (`Microsoft.AspNetCore.Authentication.JwtBearer`), transportados de forma segura mediante cookies `HttpOnly` (`access_token`).
- **Seguridad de contraseñas:** BCrypt (`BCrypt.Net-Next`) para hash y verificación de credenciales.
- **Procesamiento en segundo plano:** Quartz.NET (`Quartz.Extensions.Hosting`) para tareas programadas de depuración periódica de usuarios (`UserCleanupJob`).
- **Control de tráfico (rate limiting):** `Microsoft.AspNetCore.RateLimiting` con particionamiento por IP para mitigar abusos en recuperación de contraseñas.
- **Integraciones externas:** Cliente HTTP tipado contra la API pública de Georef (`apis.datos.gob.ar`) para normalización y geocodificación de domicilios.
- **Especificación de API:** `Microsoft.AspNetCore.OpenApi` nativo de .NET 10.
- **Contenedores y despliegue:** Dockerfile multi-etapa optimizado para despliegues en Railway y pipeline de CI/CD en GitHub Actions.

---

## Requisitos previos

Para compilar y ejecutar este proyecto en tu entorno local necesitás:

1. **.NET 10 SDK** (versión 10.0.x o superior).
   - Verificá la instalación con:
     ```bash
     dotnet --version
     ```
2. **PostgreSQL** (versión 15 o superior) en ejecución local o mediante contenedor Docker.
3. **Herramienta global de Entity Framework Core CLI:**
   ```bash
   dotnet tool install --global dotnet-ef
   ```
   *(Si ya la tenías instalada, podés actualizarla con `dotnet tool update --global dotnet-ef`).*
4. **Git** para clonar el repositorio.
5. *(Opcional)* **Docker** si deseás levantar PostgreSQL en contenedor o probar la imagen productiva localmente.

---

## Configuración

### Cómo carga .NET la configuración

ASP.NET Core utiliza una jerarquía de proveedores donde cada fuente sobreescribe a las anteriores según el orden de precedencia:

1. `appsettings.json` (valores base compartidos).
2. `appsettings.{Environment}.json` (sobreescrituras específicas para `Development`, `Production`, etc.).
3. **Secret Manager (`user-secrets`):** almacena credenciales sensibles fuera del árbol de directorios de Git (activo exclusivamente en modo `Development`).
4. **Variables de entorno del sistema:** tienen la precedencia más alta. En sistemas operativos y plataformas de alojamiento como Railway, las jerarquías de configuración que en JSON usan dos puntos `:` deben escribirse obligatoriamente con doble guion bajo `__`. Por ejemplo, `Authentication:SecretForKey` se define como `Authentication__SecretForKey`.

### Tabla de variables de configuración

A continuación se detallan todas las variables leídas por el código fuente en `Program.cs`, `JwtService.cs` y los componentes de infraestructura:

| Clave en appsettings | Variable de entorno (Railway / OS) | Descripción | Obligatoria | Origen recomendado |
| :--- | :--- | :--- | :--- | :--- |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | Cadena de conexión para la base de datos PostgreSQL (incluye host, puerto, base, usuario y contraseña). | Sí | `user-secrets` / `appsettings.Development.json` (local) o variable de entorno en Railway |
| `Authentication:SecretForKey` | `Authentication__SecretForKey` | Clave secreta simétrica utilizada para firmar y validar los tokens JWT con algoritmo HMAC SHA-256. Debe tener **al menos 32 caracteres (256 bits)**, de lo contrario la aplicación arroja `InvalidOperationException`. | Sí | `user-secrets` (local) o variable de entorno en Railway |
| `Authentication:Issuer` | `Authentication__Issuer` | Emisor esperado del token JWT (`ValidIssuer`). `JwtService` valida en el arranque que no esté vacío. En desarrollo suele ser la URL local (ej. `https://localhost:7169`) y en producción el dominio público de la API. | Sí | `appsettings.Development.json` (local) o variable de entorno en Railway |
| `Authentication:Audience` | `Authentication__Audience` | Audiencia destinataria esperada para el token (`ValidAudience`). En el código base se define como `"Dr.Device"`. `JwtService` valida que no esté vacío. | Sí | `appsettings.json` o variable de entorno en Railway si se desea personalizar |
| `UserCleanup:IntervalSeconds` | `UserCleanup__IntervalSeconds` | Intervalo en segundos con el que se dispara el trabajo programado de Quartz (`UserCleanupJob`). Debe ser un entero positivo. | No (default: 15 en appsettings, 180 en código) | `appsettings.json` o variable de entorno |
| `UserCleanup:RetentionDays` | `UserCleanup__RetentionDays` | Antigüedad en días tras la cual se purgan cuentas de usuario no verificadas o expiradas. Debe ser un entero positivo. | No (default: 30) | `appsettings.json` o variable de entorno |
| `UserCleanup:BatchSize` | `UserCleanup__BatchSize` | Cantidad de registros procesados por lote en cada ejecución de limpieza. Debe ser mayor a 0. | No (default: 100) | `appsettings.json` o variable de entorno |
| `Logging:LogLevel:Default` | `Logging__LogLevel__Default` | Nivel mínimo de logging general de la aplicación. | No (default: `Information`) | `appsettings.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Logging__LogLevel__Microsoft.AspNetCore` | Nivel de logging para el framework ASP.NET Core. | No (default: `Warning`) | `appsettings.json` |
| `AllowedHosts` | `AllowedHosts` | Patrones de host HTTP admitidos por la aplicación. | No (default: `*`) | `appsettings.json` |

> [!NOTE]
> Las variables de entorno de infraestructura en Railway como `PORT` son inyectadas automáticamente por la plataforma y consumidas por el `Dockerfile` (`ENV ASPNETCORE_URLS=http://+:${PORT:-8080}`).

### Comandos de user-secrets para desarrollo local

Para evitar almacenar contraseñas y claves privadas en archivos versionados por Git, ejecutá los siguientes comandos desde la raíz del repositorio:

```bash
# 1. Configurar la clave secreta para la firma de JWT (mínimo 32 caracteres)
dotnet user-secrets set "Authentication:SecretForKey" "<tu-clave-jwt-segura-de-al-menos-32-caracteres>" --project src/Presentation

# 2. Configurar la cadena de conexión a PostgreSQL local
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=drdevice;Username=<tu-usuario-postgres>;Password=<tu-password-postgres>" --project src/Presentation

# 3. (Opcional) Configurar el emisor local si no se toma desde appsettings.Development.json
dotnet user-secrets set "Authentication:Issuer" "https://localhost:7169" --project src/Presentation
```

Para verificar qué secretos tenés configurados localmente:

```bash
dotnet user-secrets list --project src/Presentation
```

---

## Cómo correr el proyecto en local

### 1. Clonar el repositorio y restaurar paquetes

```bash
git clone <url-del-repositorio>
cd dr-device
dotnet restore dr-device.slnx
```

### 2. Levantar la base de datos PostgreSQL

Asegurate de tener una instancia de PostgreSQL activa. Si preferís utilizar Docker:

```bash
docker run --name drdevice-postgres -e POSTGRES_DB=drdevice -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16-alpine
```

### 3. Aplicar las migraciones de base de datos

A diferencia del entorno productivo (donde las migraciones corren solas al inicio), en entorno de desarrollo deben aplicarse manualmente mediante la herramienta de Entity Framework Core:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Presentation
```

### 4. Iniciar la aplicación

Ejecutá el proyecto de presentación:

```bash
dotnet run --project src/Presentation
```

O utilizando un perfil específico definido en `launchSettings.json`:

```bash
dotnet run --project src/Presentation --launch-profile https
```

La consola indicará las URLs en las que el servidor está escuchando (por defecto `http://localhost:5288` y `https://localhost:7176`).

### 5. Documentación OpenAPI y prueba de endpoints

- **Especificación OpenAPI (JSON):** el proyecto incluye el paquete nativo `Microsoft.AspNetCore.OpenApi` y expone el documento de especificación en modo `Development` bajo la ruta:
  ```text
  http://localhost:5288/openapi/v1.json
  ```
- **Interfaz visual (Swagger UI):** la aplicación no incluye `Swashbuckle.AspNetCore.SwaggerUI` en sus dependencias, por lo que la ruta `/swagger` no está disponible de forma nativa. Podés importar la URL del archivo JSON (`/openapi/v1.json`) en herramientas como Postman, Insomnia o Scalar, o bien utilizar el archivo de solicitudes HTTP integrado en el proyecto: [Presentation.http](file:///src/Presentation/Presentation.http).

---

## Deploy en Railway

El proyecto incluye un `Dockerfile` optimizado en dos etapas (compilación en SDK 10 y ejecución en ASP.NET 10 runtime) preparado para Railway.

### Variables requeridas en el servicio de Railway

Dentro del panel de configuración de tu servicio en Railway, en la pestaña **Variables**, debés configurar:

1. **`ASPNETCORE_ENVIRONMENT`:**
   - Valor: `Production`
   - **Crítico:** al iniciar con este entorno, el código en `Program.cs` ejecuta automáticamente `dbContext.Database.Migrate()`, aplicando las migraciones pendientes en PostgreSQL. Además, restringe endpoints de prueba como `/api/auth/reset-password`.
2. **`ConnectionStrings__DefaultConnection`:**
   - Valor: cadena de conexión apuntando al servicio de PostgreSQL aprovisionado en Railway.
   - Podés componerla utilizando las variables provistas por el plugin de Postgres en Railway:
     ```text
     Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}}
     ```
3. **`Authentication__SecretForKey`:**
   - Valor: clave secreta aleatoria de alta entropía (mínimo 32 caracteres).
4. **`Authentication__Issuer`:**
   - Valor: URL pública HTTPS asignada a tu servicio en Railway (ejemplo: `https://dr-device-api-production.up.railway.app`).
5. **`Authentication__Audience`:**
   - Valor: `Dr.Device` (o el nombre que defina tu frontend).

### Puerto y URL del contenedor

Railway inyecta automáticamente la variable `PORT`. La directiva en el `Dockerfile`:
```dockerfile
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
```
se encarga de enlazar Kestrel al puerto dinámico proporcionado sin requerir configuración manual.

### Pipeline de integración y despliegue continuo (CI/CD)

El archivo `.github/workflows/deploy.yml` automatiza la integración y despliegue:
- En cada `pull_request` a `main` o `dev`, compila y valida la solución en .NET 10.
- Al fusionar (`push`) cambios en la rama `main`, instala Railway CLI y ejecuta `railway up --service "${{ secrets.RAILWAY_SERVICE }}" --ci --detach`.
- Requiere tener cargados en los secretos del repositorio de GitHub (`Repository Secrets`):
  - `RAILWAY_SERVICE`: nombre o ID del servicio en el proyecto de Railway.
  - `RAILWAY_TOKEN`: token de autenticación generado en Railway.

---

## Estructura de carpetas

El repositorio está organizado según los principios de Clean Architecture:

```text
dr-device/
├── .github/
│   └── workflows/
│       └── deploy.yml             # Pipeline de CI/CD para compilación y deploy en Railway
├── src/
│   ├── Domain/                    # Capa de dominio (núcleo empresarial sin dependencias)
│   │   ├── Entities/              # Entidades principales (User, Order, Offer, Category, etc.)
│   │   ├── Exceptions/            # Excepciones de reglas de negocio
│   │   └── Interfaces/            # Contratos de repositorios (IUserRepository, IOrderRepository)
│   ├── Application/               # Casos de uso y lógica de aplicación
│   │   ├── Interfaces/            # Contratos de servicios (IAuthService, IOrderService, etc.)
│   │   ├── Models/                # DTOs y modelos de solicitud/respuesta (Requests, Responses)
│   │   └── Services/              # Implementaciones de negocio (AuthService, OrderService, etc.)
│   ├── Infrastructure/            # Acceso a datos y servicios externos
│   │   ├── Data/                  # DbContext de EF Core y mapeo de entidades
│   │   ├── Jobs/                  # Tareas en segundo plano con Quartz (UserCleanupJob)
│   │   ├── Migrations/            # Migraciones versionadas de EF Core para PostgreSQL
│   │   └── Services/              # Proveedores de hashing (BCrypt), JWT y cliente Georef
│   └── Presentation/              # Punto de entrada HTTP (ASP.NET Core Web API)
│       ├── Controllers/           # Controladores REST expuestos
│       ├── Middlewares/           # Manejadores de excepciones y pipeline HTTP
│       ├── Properties/            # Configuración de perfiles de inicio (launchSettings.json)
│       ├── appsettings.json       # Configuración base
│       ├── appsettings.Development.json # Sobreescrituras de entorno local
│       └── Program.cs             # Registro de servicios y composición del pipeline
├── Dockerfile                     # Construcción multi-etapa para producción
└── dr-device.slnx                 # Archivo de solución XML de .NET
```
