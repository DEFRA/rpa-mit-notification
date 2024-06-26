# development
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development

RUN mkdir -p /home/dotnet/RPA.MIT.Notification.Function.Tests/
RUN mkdir -p /home/dotnet/RPA.MIT.Notification.Function.Tests/ /home/dotnet/RPA.MIT.Notification.Function/
COPY --chown=dotnet:dotnet ./RPA.MIT.Notification.Function.Tests/*.csproj ./RPA.MIT.Notification.Function.Tests/
RUN dotnet restore ./RPA.MIT.Notification.Function.Tests/RPA.MIT.Notification.Function.Tests.csproj
COPY --chown=dotnet:dotnet ./RPA.MIT.Notification.Function/*.csproj ./RPA.MIT.Notification.Function/
RUN dotnet restore ./RPA.MIT.Notification.Function/RPA.MIT.Notification.Function.csproj

COPY ./RPA.MIT.Notification.Function /src
RUN cd /src && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=development ["/home/site/wwwroot", "/home/site/wwwroot"]


# production
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS production

COPY ./RPA.MIT.Notification.Function /src
RUN cd /src && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=production ["/home/site/wwwroot", "/home/site/wwwroot"]