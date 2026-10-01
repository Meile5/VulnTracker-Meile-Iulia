FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json VulnTracker.sln ./
COPY server/VulnTracker.Domain/*.csproj         server/VulnTracker.Domain/
COPY server/VulnTracker.Application/*.csproj    server/VulnTracker.Application/
COPY server/VulnTracker.Infrastructure/*.csproj server/VulnTracker.Infrastructure/
COPY server/VulnTracker.Api/*.csproj            server/VulnTracker.Api/
COPY server/VulnTracker.Tests/*.csproj          server/VulnTracker.Tests/
RUN dotnet restore

COPY server/ server/
RUN dotnet publish server/VulnTracker.Api -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "VulnTracker.Api.dll"]