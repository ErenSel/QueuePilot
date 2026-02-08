FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/QueuePilot.Api/QueuePilot.Api.csproj", "src/QueuePilot.Api/"]
COPY ["src/QueuePilot.Application/QueuePilot.Application.csproj", "src/QueuePilot.Application/"]
COPY ["src/QueuePilot.Domain/QueuePilot.Domain.csproj", "src/QueuePilot.Domain/"]
COPY ["src/QueuePilot.Infrastructure/QueuePilot.Infrastructure.csproj", "src/QueuePilot.Infrastructure/"]
RUN dotnet restore "src/QueuePilot.Api/QueuePilot.Api.csproj"
COPY . .
WORKDIR "/src/src/QueuePilot.Api"
RUN dotnet build "QueuePilot.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "QueuePilot.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QueuePilot.Api.dll"]
