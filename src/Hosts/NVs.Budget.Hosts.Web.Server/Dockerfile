# syntax=docker/dockerfile:1

FROM node:24 AS client-build
WORKDIR /client
COPY src/Controllers/NVs.Budget.Controllers.Web.Client/budget-client/package.json \
     src/Controllers/NVs.Budget.Controllers.Web.Client/budget-client/package-lock.json ./
RUN npm ci --no-fund --no-audit
COPY src/Controllers/NVs.Budget.Controllers.Web.Client/budget-client/ .
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
# .git is intentionally part of the context: GitVersion.MsBuild derives the
# assembly version from git tags during publish
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish src/Hosts/NVs.Budget.Hosts.Web.Server/NVs.Budget.Hosts.Web.Server.csproj \
    -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

FROM base AS final
COPY --from=publish /app/publish .
COPY --from=client-build /client/dist/budget-client/browser ./wwwroot
ENTRYPOINT ["dotnet", "NVs.Budget.Hosts.Web.Server.dll", "--launch-profile", "https"]
