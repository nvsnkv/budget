FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM node:20 AS node-builder
COPY ["NVs.Budget.Hosts.Web.Client/budget-client", "/client"]
RUN npm i -g @angular/cli
WORKDIR /client
RUN npm ci
RUN npm run build


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY  --exclude="NVs.Budget.Hosts.Web.Client/budget-client" ["NVs.Budget.Hosts.Web.Client/NVs.Budget.Hosts.Web.Client.csproj", "NVs.Budget.Hosts.Web.Client/"]
RUN dotnet restore "NVs.Budget.Hosts.Web.Client/NVs.Budget.Hosts.Web.Client.csproj"
COPY . .
WORKDIR "/src/NVs.Budget.Hosts.Web.Client"
RUN dotnet build "NVs.Budget.Hosts.Web.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NVs.Budget.Hosts.Web.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=node-builder /client/dist/budget-client /app/wwwroot
ENTRYPOINT ["dotnet", "NVs.Budget.Hosts.Web.Client.dll"]
