FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Hosts/NVs.Budget.Hosts.Web/NVs.Budget.Hosts.Web.csproj", "Hosts/NVs.Budget.Hosts.Web/"]
RUN dotnet restore "Hosts/NVs.Budget.Hosts.Web/NVs.Budget.Hosts.Web.csproj"
COPY . .
WORKDIR "/src/Hosts/NVs.Budget.Hosts.Web"
RUN dotnet build "NVs.Budget.Hosts.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NVs.Budget.Hosts.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NVs.Budget.Hosts.Web.dll"]
