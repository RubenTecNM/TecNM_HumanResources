# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy csproj and restore
COPY ["RecursosHumanosWeb/RecursosHumanosWeb.csproj", "RecursosHumanosWeb/"]
RUN dotnet restore "RecursosHumanosWeb/RecursosHumanosWeb.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/RecursosHumanosWeb"
RUN dotnet build "RecursosHumanosWeb.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "RecursosHumanosWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY db/inserts.sql ./db/inserts.sql

# Install dependencies for SQL Client if needed
# RUN apt-get update && apt-get install -y libicu-dev && rm -rf /var/lib/apt/lists/*

# Configuración de red para el contenedor
ENV ASPNETCORE_URLS=http://+:5011
EXPOSE 5011

ENTRYPOINT ["dotnet", "RecursosHumanosWeb.dll"]
