# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore dependencies first (layer-cached)
COPY src/LiveExpert.Domain/LiveExpert.Domain.csproj           src/LiveExpert.Domain/
COPY src/LiveExpert.Application/LiveExpert.Application.csproj src/LiveExpert.Application/
COPY src/LiveExpert.Infrastructure/LiveExpert.Infrastructure.csproj src/LiveExpert.Infrastructure/
COPY src/LiveExpert.API/LiveExpert.API.csproj                 src/LiveExpert.API/
RUN dotnet restore src/LiveExpert.API/LiveExpert.API.csproj

# Copy source and publish
COPY src/ src/
WORKDIR /src/src/LiveExpert.API
RUN dotnet publish LiveExpert.API.csproj -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create non-root user
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

# Copy published output
COPY --from=build /app/publish .

# Data directory for SQLite DB (mounted as volume in production)
RUN mkdir -p /app/data /app/logs && chown -R appuser:appgroup /app

USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "LiveExpert.API.dll"]
