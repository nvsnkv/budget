FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NVs.Budget.Hosts.Web.Client/NVs.Budget.Hosts.Web.Client.csproj", "NVs.Budget.Hosts.Web.Client/"]
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
ENTRYPOINT ["dotnet", "NVs.Budget.Hosts.Web.Client.dll"]
