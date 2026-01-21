FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG APP_UID=1000
WORKDIR /https
RUN chmod 755 /https
RUN dotnet dev-certs https -ep /https/aspnetapp.pfx -p "dev-password-do-not-use-in-production"

# Export certificate (CRT) from PFX file
RUN openssl pkcs12 -in /https/aspnetapp.pfx -out /https/aspnetapp.crt -nokeys -passin pass:"dev-password-do-not-use-in-production"

# Export private key (KEY) from PFX file
RUN openssl pkcs12 -in /https/aspnetapp.pfx -out /https/aspnetapp.key -nocerts -nodes -passin pass:"dev-password-do-not-use-in-production"

RUN chmod 644 /https/*
RUN chown $APP_UID:$APP_UID /https/*