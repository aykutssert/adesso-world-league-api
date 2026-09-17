# syntax=docker/dockerfile:1

# ---- build -------------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /source

# Restore first, with only the project files copied, so the layer cache survives source-only changes.
COPY global.json Directory.Build.props Directory.Packages.props AdessoWorldLeague.slnx ./
COPY src/AdessoWorldLeague.Domain/*.csproj src/AdessoWorldLeague.Domain/
COPY src/AdessoWorldLeague.Application/*.csproj src/AdessoWorldLeague.Application/
COPY src/AdessoWorldLeague.Infrastructure/*.csproj src/AdessoWorldLeague.Infrastructure/
COPY src/AdessoWorldLeague.Api/*.csproj src/AdessoWorldLeague.Api/
COPY tests/AdessoWorldLeague.UnitTests/*.csproj tests/AdessoWorldLeague.UnitTests/
COPY tests/AdessoWorldLeague.IntegrationTests/*.csproj tests/AdessoWorldLeague.IntegrationTests/
RUN dotnet restore src/AdessoWorldLeague.Api/AdessoWorldLeague.Api.csproj

COPY src/ src/
RUN dotnet publish src/AdessoWorldLeague.Api/AdessoWorldLeague.Api.csproj \
        --configuration Release \
        --no-restore \
        --output /app

# ---- tests ---------------------------------------------------------------------------------------
# Lets the whole suite run without a local .NET SDK: docker compose --profile test run --rm tests
FROM build AS test
WORKDIR /source
COPY tests/ tests/
RUN dotnet restore AdessoWorldLeague.slnx
ENTRYPOINT ["dotnet", "test", "--configuration", "Release"]

# ---- runtime -----------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# Team names are Turkish, so the image needs ICU rather than the invariant globalization fallback.
RUN apk add --no-cache icu-libs tzdata \
    && adduser --disabled-password --no-create-home --uid 64198 appuser
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_HTTP_PORTS=8080

COPY --from=build --chown=appuser:appuser /app ./
USER appuser
EXPOSE 8080

ENTRYPOINT ["dotnet", "AdessoWorldLeague.Api.dll"]
