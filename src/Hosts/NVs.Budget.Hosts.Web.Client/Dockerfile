FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/* 
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM node:20 AS node-builder
COPY ["budget-client", "/client"]
RUN npm i -g @angular/cli
WORKDIR /client
RUN npm ci
RUN npm run build


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY --exclude="budget-client" . .
RUN dotnet restore "NVs.Budget.Hosts.Web.Client.csproj"
RUN dotnet build "NVs.Budget.Hosts.Web.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NVs.Budget.Hosts.Web.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=node-builder /client/dist/budget-client/browser /app/wwwroot
ENTRYPOINT ["dotnet", "NVs.Budget.Hosts.Web.Client.dll"]
