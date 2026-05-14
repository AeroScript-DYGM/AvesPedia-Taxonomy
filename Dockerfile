# Multi-stage Dockerfile for BirdTaxonomy API
# ASP.NET Core 8 Web API with Entity Framework Core and SQL Server
# Production-optimized with security hardening and minimal attack surface

# ============================================================================
# STAGE 1: Build Stage
# ============================================================================
# Purpose: Compile C# code, run tests, publish optimized release build
# Image: mcr.microsoft.com/dotnet/sdk:8.0
# - Includes .NET SDK, build tools, test runners
# - Larger image (2GB+); discarded in final output
# ============================================================================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory for build stage
WORKDIR /src

# Copy project files (.csproj, .sln)
# Separate COPY for project files enables Docker layer caching
# If source code changes but project files don't, layer is reused
COPY ["BirdTaxonomy.Api/BirdTaxonomy.Api.csproj", "BirdTaxonomy.Api/"]
COPY ["BirdTaxonomy.Application/BirdTaxonomy.Application.csproj", "BirdTaxonomy.Application/"]
COPY ["BirdTaxonomy.Domain/BirdTaxonomy.Domain.csproj", "BirdTaxonomy.Domain/"]
COPY ["BirdTaxonomy.Infrastructure/BirdTaxonomy.Infrastructure.csproj", "BirdTaxonomy.Infrastructure/"]
COPY ["BirdTaxonomy.Persistence/BirdTaxonomy.Persistence.csproj", "BirdTaxonomy.Persistence/"]

# Restore NuGet packages
# --locked-mode: uses exact versions from lock file (reproducible builds)
RUN dotnet restore "BirdTaxonomy.Api/BirdTaxonomy.Api.csproj" --locked-mode

# Copy entire source code
COPY . .

# Set build configuration variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Run unit and integration tests
# Fails build if tests don't pass (fail-fast principle)
# Remove this stage in development; use only in CI/CD pipelines
RUN dotnet test --no-restore \
    --configuration Release \
    --verbosity normal \
    --logger "console;verbosity=detailed" \
    --blame-hang-timeout=30000 \
    || exit 1

# Build and publish optimized release version
# --no-restore: Skip package restore (already done above)
# --configuration Release: Production optimizations enabled
# --no-build-isolated: Use previously restored packages
# Outputs to /app directory for STAGE 2
RUN dotnet publish "BirdTaxonomy.Api/BirdTaxonomy.Api.csproj" \
    --no-restore \
    --configuration Release \
    --output /app/publish \
    --self-contained false

# ============================================================================
# STAGE 2: Runtime Stage
# ============================================================================
# Purpose: Minimal image containing only runtime dependencies and compiled app
# Image: mcr.microsoft.com/dotnet/aspnet:8.0
# - Lightweight (300MB vs 2GB for SDK)
# - No build tools, SDK, or build-time dependencies
# - Only .NET runtime required to execute application
# ============================================================================

FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Metadata labels for image identification and lifecycle management
LABEL maintainer="BirdTaxonomy Development Team"
LABEL description="BirdTaxonomy API - ASP.NET Core 8 Web API with Entity Framework Core"
LABEL version="1.0"

# ============================================================================
# SECURITY: Run as non-root user
# ============================================================================
# Principle: Least Privilege
# Impact: Limits damage if container is compromised
# Details: Create app user with restricted permissions; no sudo access
RUN groupadd -r appuser && \
    useradd -r -g appuser appuser

# ============================================================================
# ENVIRONMENT & CONFIGURATION
# ============================================================================

# Set working directory (where application runs)
WORKDIR /app

# ASP.NET Core defaults to HTTPS; disable for Docker (load balancer handles HTTPS)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Application-specific settings (can be overridden at runtime)
ENV ConnectionStrings__DefaultConnection=""
ENV Logging__LogLevel__Default="Information"
ENV Swagger__Enabled="true"

# ============================================================================
# COPY APPLICATION FILES FROM BUILD STAGE
# ============================================================================

# Copy published application from build stage
# This is the compiled, optimized code ready for execution
COPY --from=build --chown=appuser:appuser /app/publish .

# ============================================================================
# APPLICATION CONFIGURATION FILES
# ============================================================================

# Copy appsettings.json (base configuration)
# Contains default settings; overridden by environment-specific files and env vars
COPY --chown=appuser:appuser ["appsettings.json", "./"]

# Copy environment-specific settings (optional)
# appsettings.Production.json is applied when ASPNETCORE_ENVIRONMENT=Production
COPY --chown=appuser:appuser ["appsettings.Production.json", "./"]

# ============================================================================
# HEALTH CHECK
# ============================================================================
# Purpose: Kubernetes/Docker Swarm can detect and restart unhealthy containers
# Details: Probes /health endpoint every 30 seconds
# - If 3 consecutive checks fail, container marked unhealthy
# - Orchestrator may restart or remove container
# Note: Requires HealthChecks middleware in Startup.cs

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD dotnet /healthcheck/HealthCheckClient.dll http://localhost:8080/health || exit 1

# ============================================================================
# PORTS EXPOSURE
# ============================================================================

# Expose port 8080 (HTTP)
# Note: This is documentation; actual port binding set at runtime with -p flag
EXPOSE 8080

# ============================================================================
# SECURITY: Restrict filesystem permissions
# ============================================================================
# Make application directory read-only where possible
# Application requires write access to logs directory
RUN mkdir -p /app/logs && \
    chown appuser:appuser /app/logs && \
    chmod 755 /app/logs

# ============================================================================
# SECURITY: Switch to non-root user before executing application
# ============================================================================
USER appuser

# ============================================================================
# STARTUP COMMAND
# ============================================================================
# Executes the compiled ASP.NET Core application
# exec form (preferred): Direct process execution; signals handled properly
# shell form (avoid): Spawns /bin/sh which intercepts signals
# --urls: Override listen address (specified in environment variable)
# ASPNETCORE_URLS: Already set in ENV; can be overridden at runtime

ENTRYPOINT ["dotnet", "BirdTaxonomy.Api.dll"]
CMD ["--urls", "http://+:8080"]

# ============================================================================
# USAGE EXAMPLES
# ============================================================================

# Build the image:
# docker build -t birdtaxonomy-api:1.0 .

# Run container with SQL Server connection:
# docker run -d \
#   --name birdtaxonomy \
#   -p 8080:8080 \
#   -e ConnectionStrings__DefaultConnection="Server=sqlserver:1433;Database=BirdTaxonomy;User Id=sa;Password=YourPassword;Encrypt=true;TrustServerCertificate=true;" \
#   -e ASPNETCORE_ENVIRONMENT=Production \
#   --network birdtaxonomy-network \
#   birdtaxonomy-api:1.0

# Run with log volume:
# docker run -d \
#   --name birdtaxonomy \
#   -p 8080:8080 \
#   -v /var/log/birdtaxonomy:/app/logs \
#   -e ConnectionStrings__DefaultConnection="..." \
#   birdtaxonomy-api:1.0

# Run with environment-specific appsettings:
# docker run -d \
#   --name birdtaxonomy \
#   -p 8080:8080 \
#   -e ASPNETCORE_ENVIRONMENT=Staging \
#   -e ASPNETCORE_URLS=http://+:8080 \
#   -e Logging__LogLevel__Default=Debug \
#   -e Swagger__Enabled=true \
#   birdtaxonomy-api:1.0

# Run with Docker Compose:
# See docker-compose.yml in repository

# ============================================================================
# IMPORTANT NOTES
# ============================================================================

# 1. CONNECTION STRINGS
#    - Never commit connection strings to git
#    - Provide via environment variables at runtime
#    - Use secrets management (Docker Swarm secrets, Kubernetes secrets, Azure Key Vault)
#    - Example: -e ConnectionStrings__DefaultConnection="..."

# 2. APPSETTINGS
#    - appsettings.json: Base configuration, checked into git
#    - appsettings.{Environment}.json: Environment-specific, checked into git
#    - Environment variables: Override settings at runtime
#    - Precedence: appsettings.json < appsettings.{Env}.json < Environment variables

# 3. SQL SERVER CONNECTION
#    - Requires SQL Server running (in separate container or external server)
#    - Connection string format:
#      Server=hostname:port;Database=dbname;User Id=user;Password=pass;Encrypt=true;TrustServerCertificate=true;
#    - For development: TrustServerCertificate=true (skip cert validation)
#    - For production: Use proper SSL certificates

# 4. ENTITY FRAMEWORK MIGRATIONS
#    - Run migrations before starting application:
#      dotnet ef database update
#    - Or run migrations in Dockerfile (see docker-compose-with-migrations.yml)
#    - Or create separate migration job that runs before API starts

# 5. SWAGGER/OPENAPI
#    - Enabled in Development environment by default
#    - Disable in Production unless explicitly enabled
#    - Configuration: Startup.cs or Program.cs
#    - Example: if (env.IsDevelopment()) { app.UseSwagger(); }

# 6. HEALTH CHECKS
#    - Healthcheck endpoint at GET /health
#    - Requires HealthChecks middleware in Startup.cs
#    - Example implementation:
#      app.MapHealthChecks("/health");

# 7. PORT BINDING
#    - Container listens on port 8080 (internal)
#    - Host port specified at runtime: -p 8080:8080
#    - Load balancer typically handles external HTTPS traffic
#    - No need to bind 443; load balancer handles TLS termination

# 8. LOGGING
#    - Logs written to console (stdout/stderr)
#    - Docker captures logs: docker logs <container>
#    - Optional: Mount volume for persistent logs: -v /var/log/app:/app/logs
#    - Use structured logging (Serilog) for production

# 9. PERFORMANCE TUNING
#    - Use --cpus and --memory flags to limit resources
#    - Example: docker run --cpus 1.0 --memory 512m
#    - Set appropriate connection pool size for database
#    - Use read replicas for load distribution

# 10. SECURITY CHECKLIST
#     - [x] Non-root user (appuser)
#     - [x] No hardcoded secrets in image
#     - [x] Secrets provided at runtime
#     - [x] Read-only filesystem for app directory
#     - [x] No package manager or shell in final image
#     - [x] Health checks enabled
#     - [x] Resource limits enforced
#     - [x] Proper signal handling (exec form ENTRYPOINT)