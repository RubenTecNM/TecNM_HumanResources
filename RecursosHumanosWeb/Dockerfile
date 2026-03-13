FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copiar csproj y restaurar dependencias
COPY RecursosHumanosWeb/*.csproj ./RecursosHumanosWeb/
RUN dotnet restore ./RecursosHumanosWeb/RecursosHumanosWeb.csproj

# Copiar todo y publicar
COPY . .
RUN dotnet publish ./RecursosHumanosWeb/RecursosHumanosWeb.csproj -c Release -o out

# Imagen de ejecuci&oacute;n
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 80
ENTRYPOINT ["dotnet", "RecursosHumanosWeb.dll"]
