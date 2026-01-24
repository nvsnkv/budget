ARG CLIENT_IMAGE
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/* 
USER $APP_UID
WORKDIR /app
EXPOSE 7237
EXPOSE 5153

# Client build stage - accepts build arg for client image
FROM ${CLIENT_IMAGE} AS client-assets

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
WORKDIR /src/src/Hosts/NVs.Budget.Hosts.Web.Server
RUN dotnet restore NVs.Budget.Hosts.Web.Server.csproj
RUN dotnet build NVs.Budget.Hosts.Web.Server.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish NVs.Budget.Hosts.Web.Server.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS preps-development


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=client-assets /client/dist/budget-client/browser /app/wwwroot
ENTRYPOINT ["dotnet", "NVs.Budget.Hosts.Web.Server.dll", "--launch-profile", "https"]
