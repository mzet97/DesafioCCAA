FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/DesafioCCAA/DesafioCCAA.sln", "."]
COPY ["src/DesafioCCAA/DesafioCCAA.Api/DesafioCCAA.Api.csproj", "DesafioCCAA.Api/"]
COPY ["src/DesafioCCAA/DesafioCCAA.Domain/DesafioCCAA.Domain.csproj", "DesafioCCAA.Domain/"]
COPY ["src/DesafioCCAA/DesafioCCAA.Infrastructure/DesafioCCAA.Infrastructure.csproj", "DesafioCCAA.Infrastructure/"]
RUN dotnet restore "DesafioCCAA.sln"

COPY ["src/DesafioCCAA/", "."]

WORKDIR "/src/DesafioCCAA.Api"
RUN dotnet build "DesafioCCAA.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/DesafioCCAA.Api"
RUN dotnet publish "DesafioCCAA.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-build

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DesafioCCAA.Api.dll"]