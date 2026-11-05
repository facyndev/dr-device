# Etapa 1: Build y Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos de solución y proyectos para aprovechar la caché de Docker
COPY ["dr-device.slnx", "./"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Presentation/Presentation.csproj", "src/Presentation/"]

# Restaurar dependencias
RUN dotnet restore "src/Presentation/Presentation.csproj"

# Copiar todo el código fuente
COPY . .

# Compilar y publicar la capa de presentación
WORKDIR "/src/src/Presentation"
RUN dotnet publish "Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime liviano
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copiar artefactos publicados desde la etapa de compilación
COPY --from=build /app/publish .

# Railway inyecta la variable de entorno PORT automáticamente.
# Configuramos ASPNETCORE_URLS para enlazar al puerto inyectado (o 8080 por defecto).
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

ENTRYPOINT ["dotnet", "Presentation.dll"]
