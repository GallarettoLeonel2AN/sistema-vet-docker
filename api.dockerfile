# Etapa 1: Compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solo los archivos de proyecto para restaurar paquetes (aprovecha el caché de Docker)
COPY ["PerrosPeligrososApi/PerrosPeligrososApi.csproj", "PerrosPeligrososApi/"]
RUN dotnet restore "PerrosPeligrososApi/PerrosPeligrososApi.csproj"

# Copiar el resto del código fuente
COPY . .
WORKDIR "/src/PerrosPeligrososApi"

# Publicar la aplicación
RUN dotnet publish "PerrosPeligrososApi.csproj" -c Release -o /app/publish

# Etapa 2: Imagen final (solo con lo necesario para ejecutar)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PerrosPeligrososApi.dll"]