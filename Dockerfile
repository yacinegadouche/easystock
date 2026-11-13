# syntax=docker/dockerfile:1

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY src/EasyStock.Web/EasyStock.Web.csproj src/EasyStock.Web/
RUN dotnet restore src/EasyStock.Web/EasyStock.Web.csproj

COPY src/ src/
RUN dotnet publish src/EasyStock.Web/EasyStock.Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app .

ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
EXPOSE 8080

USER $APP_UID
ENTRYPOINT ["dotnet", "EasyStock.Web.dll"]