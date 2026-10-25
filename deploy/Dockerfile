# Stage 1: Build Vue frontend
FROM node:22-alpine AS frontend-build
RUN corepack enable && corepack prepare pnpm@9 --activate
WORKDIR /app/Frontend
COPY Frontend/package.json Frontend/pnpm-lock.yaml Frontend/pnpm-workspace.yaml ./
RUN pnpm install --frozen-lockfile
COPY Frontend/src ./src
COPY Frontend/index.html Frontend/tsconfig* Frontend/vite.config.ts Frontend/vitest.config.ts* Frontend/.env* ./

# Flag condicional: por defecto NO corre tests en build de produccion
ARG RUN_FRONTEND_TESTS=false
RUN if [ "$RUN_FRONTEND_TESTS" = "true" ]; then \
      echo "🧪 Ejecutando pruebas unitarias de Frontend..." && \
      pnpm test:run; \
    else \
      echo "⏭️ Tests de Frontend omitidos (RUN_FRONTEND_TESTS=false)"; \
    fi

RUN pnpm build

# Stage 2: Build .NET backend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["HumanResourcesProduction/HumanResourcesProduction.csproj", "HumanResourcesProduction/"]
COPY ["HumanResourcesProduction.Core/HumanResourcesProduction.Core.csproj", "HumanResourcesProduction.Core/"]
COPY ["HumanResourcesProduction.Data/HumanResourcesProduction.Data.csproj", "HumanResourcesProduction.Data/"]
COPY ["HumanResourcesProduction.Crosscutting/HumanResourcesProduction.Crosscutting.csproj", "HumanResourcesProduction.Crosscutting/"]
COPY ["HumanResourcesProduction.Services/HumanResourcesProduction.Services.csproj", "HumanResourcesProduction.Services/"]
RUN dotnet restore "HumanResourcesProduction/HumanResourcesProduction.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/HumanResourcesProduction"
RUN dotnet build "HumanResourcesProduction.csproj" -c Release -o /app/build

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "HumanResourcesProduction.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=frontend-build /app/Frontend/dist ./wwwroot
COPY db/inserts.sql ./db/inserts.sql
COPY db/schema_objects.sql ./db/schema_objects.sql
COPY db/upgrade ./db/upgrade

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HumanResourcesProduction.dll"]
