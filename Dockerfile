FROM mcr.microsoft.com/dotnet/sdk:6.0 AS installer-env

COPY ./RPA.MIT.Notification.Function /src
RUN cd /src && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4.1.3-dotnet-isolated6.0-appservice
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]