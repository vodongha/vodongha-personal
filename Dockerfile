# ── Stage 1: Build ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY vodongha.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

# ── Stage 2: Runtime ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

RUN useradd -m appuser && chown -R appuser /app
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "vodongha.dll"]
