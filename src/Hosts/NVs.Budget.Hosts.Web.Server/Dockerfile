FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 7237
EXPOSE 5153

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
ENTRYPOINT ["dotnet", "NVs.Budget.Hosts.Web.Server.dll", "--launch-profile", "https"]
