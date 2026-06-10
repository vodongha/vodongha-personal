# ── Stage 1: Build ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/VodonghaPersonal.Shared/VodonghaPersonal.Shared.csproj src/VodonghaPersonal.Shared/
COPY src/VodonghaPersonal.Server/VodonghaPersonal.Server.csproj src/VodonghaPersonal.Server/
RUN dotnet restore src/VodonghaPersonal.Server/VodonghaPersonal.Server.csproj

COPY . .
RUN dotnet publish src/VodonghaPersonal.Server/VodonghaPersonal.Server.csproj -c Release -o /app/publish

# ── Stage 2: Runtime ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install fonts required by QuestPDF/SkiaSharp on Linux
# fonts-noto: full Unicode + Vietnamese support
# fonts-liberation: Arial-equivalent metrics
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
