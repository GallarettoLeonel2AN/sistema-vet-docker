# Etapa 1: Compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar paquetes
COPY ["SistemaVetIng/SistemaVetIng.csproj", "SistemaVetIng/"]
COPY ["PerrosPeligrososApi/PerrosPeligrososApi.csproj", "PerrosPeligrososApi/"]
RUN dotnet restore "SistemaVetIng/SistemaVetIng.csproj"

# Copiar el resto del código fuente
COPY . .
WORKDIR "/src/SistemaVetIng"

# Publicar la aplicación
RUN dotnet publish "SistemaVetIng.csproj" -c Release -o /app/publish

# Etapa 2: Imagen final
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SistemaVetIng.dll"]