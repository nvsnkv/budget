FROM mcr.microsoft.com/dotnet/sdk:8.0 AS debug
EXPOSE 7237
EXPOSE 5153
WORKDIR /budget/src/Hosts/NVs.Budget.Hosts.Web.Server
ENTRYPOINT ["dotnet", "watch", "run", "--project", "NVs.Budget.Hosts.Web.Server.csproj", "--", "--launch-profile", "https"]