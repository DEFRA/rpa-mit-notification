FROM mcr.microsoft.com/dotnet/sdk:6.0 AS production

RUN mkdir -p /home/dotnet/RPA.MIT.Notification.Function.Tests/
RUN mkdir -p /home/dotnet/RPA.MIT.Notification.Function.Tests/ /home/dotnet/RPA.MIT.Notification.Function/
COPY --chown=dotnet:dotnet ./RPA.MIT.Notification.Function.Tests/*.csproj ./RPA.MIT.Notification.Function.Tests/
RUN dotnet restore ./RPA.MIT.Notification.Function.Tests/RPA.MIT.Notification.Function.Tests.csproj
COPY --chown=dotnet:dotnet ./RPA.MIT.Notification.Function/*.csproj ./RPA.MIT.Notification.Function/
RUN dotnet restore ./RPA.MIT.Notification.Function/RPA.MIT.Notification.Function.csproj