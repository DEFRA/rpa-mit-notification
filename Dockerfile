FROM mcr.microsoft.com/dotnet/sdk:6.0 AS Production

RUN dotnet_sdk_version=3.1.409 && curl -SL --output dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Sdk/$dotnet_sdk_version/dotnet-sdk-$dotnet_sdk_version-linux-x64.tar.gz && mkdir -p /usr/share/dotnet && tar -ozxf dotnet.tar.gz -C /usr/share/dotnet && rm dotnet.tar.gz

COPY ./RPA.MIT.Notification.Function /src/dotnet-function-app/

RUN mkdir -p /home/site/wwwroot

WORKDIR /src/dotnet-function-app

RUN dotnet publish --output /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4.1.3-dotnet-isolated6.0-appservice

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
  AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=Production ["/home/site/wwwroot", "/home/site/wwwroot"]