# Dr. Device API

API REST construida con ASP.NET Core y .NET 10 para la plataforma Dr. Device, orientada a la gestión de reparaciones y servicios técnicos de dispositivos. Conecta clientes y técnicos a través de órdenes de trabajo, ofertas, especializaciones y categorización de equipos.

---

## Descripción del proyecto y stack

La solución sigue los principios de Clean Architecture (arquitectura limpia), separando responsabilidades en capas desacopladas con inversión de dependencias.

### Stack tecnológico

- **Plataforma y lenguaje:** .NET 10 (`net10.0`) 
- **Framework web:** ASP.NET Core Web API.
- **Acceso a datos y ORM:** Entity Framework Core 10
- **Base de datos:** PostgreSQL.
- **Autenticación y autorización:** JWT (JSON Web Tokens) firmados con HMAC SHA-256, transportados de forma segura mediante cookies `HttpOnly` (`access_token`).
- **Seguridad de contraseñas:** BCrypt para hash y verificación de credenciales.
- **Procesamiento en segundo plano:** [Quartz.NET](http://Quartz.NET) para tareas programadas de depuración periódica de usuarios.
- **Control de tráfico (rate limiting):** con particionamiento por IP para mitigar abusos en recuperación de contraseñas.
- **Integraciones externas:** Cliente HTTP tipado contra la API pública de Georef (`apis.datos.gob.ar`) para normalización y geocodificación de domicilios.
- **Contenedores y despliegue:** Dockerfile multi-etapa optimizado para despliegues en Railway y pipeline de CI/CD en GitHub Actions.

---

## Requisitos previos

Para compilar y ejecutar este proyecto en tu entorno local necesitás:

1. **.NET 10 SDK** (versión 10.0.x o superior).
   - Verificá la instalación con:
2. **PostgreSQL** (versión 15 o superior) en ejecución local o mediante contenedor Docker.
3. **Herramienta global de Entity Framework Core CLI:**
   ```bash
     dotnet tool install --global dotnet-ef
   ```
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


| Clave en appsettings                    | Variable de entorno (Railway / OS)        | Descripción                                                                                                                                                                                                            | Obligatoria                                    |
| :--------------------------------------- | :----------------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------- |
| `ConnectionStrings:DefaultConnection`   | `ConnectionStrings__DefaultConnection`    | Cadena de conexión para la base de datos PostgreSQL (incluye host, puerto, base, usuario y contraseña).                                                                                                                | Sí                                             |
| `Authentication:SecretForKey`           | `Authentication__SecretForKey`            | Clave secreta simétrica utilizada para firmar y validar los tokens JWT con algoritmo HMAC SHA-256. Debe tener **al menos 32 caracteres (256 bits)**, de lo contrario la aplicación arroja `InvalidOperationException`. | Sí                                             |
| `Authentication:Issuer`                 | `Authentication__Issuer`                  | Emisor esperado del token JWT (`ValidIssuer`). `JwtService` valida en el arranque que no esté vacío. En desarrollo suele ser la URL local (ej. `https://localhost:7169`) y en producción el dominio público de la API. | Sí                                             |
| `Authentication:Audience`               | `Authentication__Audience`                | Audiencia destinataria esperada para el token (`ValidAudience`). En el código base se define como `"Dr.Device"`. `JwtService` valida que no esté vacío.                                                                | Sí                                             |
| `UserCleanup:IntervalSeconds`           | `UserCleanup__IntervalSeconds`            | Intervalo en segundos con el que se dispara el trabajo programado de Quartz (`UserCleanupJob`). Debe ser un entero positivo.                                                                                           | No (default: 15 en appsettings, 180 en código) |
| `UserCleanup:RetentionDays`             | `UserCleanup__RetentionDays`              | Antigüedad en días tras la cual se purgan cuentas de usuario no verificadas o expiradas. Debe ser un entero positivo.                                                                                                  | No (default: 30)                               |
| `UserCleanup:BatchSize`                 | `UserCleanup__BatchSize`                  | Cantidad de registros procesados por lote en cada ejecución de limpieza. Debe ser mayor a 0.                                                                                                                           | No (default: 100)                              |
| `Logging:LogLevel:Default`              | `Logging__LogLevel__Default`              | Nivel mínimo de logging general de la aplicación.                                                                                                                                                                      | No (default: `Information`)                    |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Logging__LogLevel__Microsoft.AspNetCore` | Nivel de logging para el framework ASP.NET Core.                                                                                                                                                                       | No (default: `Warning`)                        |
| `AllowedHosts`                          | `AllowedHosts`                            | Patrones de host HTTP admitidos por la aplicación.                                                                                                                                                                     | No (default: `*`)                              |


> [!NOTE]
> Las variables de entorno de infraestructura en Railway como `PORT` son inyectadas automáticamente por la plataforma y consumidas por el `Dockerfile` (`ENV ASPNETCORE_URLS=http://+:${PORT:-8080}`).

### Comandos de user-secrets para desarrollo local

Para evitar almacenar contraseñas y claves privadas en archivos versionados por Git, ejecutá los siguientes comandos desde la raíz del repositorio:

```bash
# 1. Configurar la clave secreta para la firma de JWT (mínimo 32 caracteres)
dotnet user-secrets set "Authentication:SecretForKey" "dr-device-el-mejor-proyecto-del-mundo" --project src/Presentation

# 2. Configurar la cadena de conexión a PostgreSQL local
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=drdevice;Username=postgres;Password=postgres" --project src/Presentation

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
git clone [[ORCA_RICH_MD:48dec2c4b89ba9a10eb5688422d92d14:inline-html:%3Curl-del-repositorio%3E]]
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

La consola indicará las URLs en las que el servidor está escuchando (por defecto `http://localhost:5288` y `https://localhost:7176`).

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
