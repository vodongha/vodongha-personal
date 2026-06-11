# ── Stage 1: Build ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

# Directory.Build.props must be present before restore (provides TargetFramework)
COPY Directory.Build.props .

# Copy project files for restore layer caching
COPY Source/VodonghaPersonal.Shared/VodonghaPersonal.Shared.csproj Source/VodonghaPersonal.Shared/
COPY Source/VodonghaPersonal.Client/VodonghaPersonal.Client.csproj Source/VodonghaPersonal.Client/
COPY Source/VodonghaPersonal.Server/VodonghaPersonal.Server.csproj Source/VodonghaPersonal.Server/
RUN dotnet restore Source/VodonghaPersonal.Server/VodonghaPersonal.Server.csproj

COPY . .
RUN dotnet publish Source/VodonghaPersonal.Server/VodonghaPersonal.Server.csproj -c Release -o /app/publish

# ── Stage 2: Runtime ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# fonts-noto: full Unicode + Vietnamese support; fonts-liberation: Arial metrics for QuestPDF
RUN apt-get update && apt-get install -y --no-install-recommends \
    fonts-noto \
    fonts-liberation \
    libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

RUN useradd -m appuser && chown -R appuser /app
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "vodongha-personal.dll"]
