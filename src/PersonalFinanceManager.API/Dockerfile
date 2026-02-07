FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/PersonalFinanceManager.API/PersonalFinanceManager.API.csproj", "src/PersonalFinanceManager.API/"]
COPY ["src/PersonalFinanceManager.Application/PersonalFinanceManager.Application.csproj", "src/PersonalFinanceManager.Application/"]
COPY ["src/PersonalFinanceManager.Core/PersonalFinanceManager.Core.csproj", "src/PersonalFinanceManager.Core/"]
COPY ["src/PersonalFinanceManager.Infrastructure/PersonalFinanceManager.Infrastructure.csproj", "src/PersonalFinanceManager.Infrastructure/"]
RUN dotnet restore "src/PersonalFinanceManager.API/PersonalFinanceManager.API.csproj"
COPY . .
WORKDIR "/src/src/PersonalFinanceManager.API"
RUN dotnet build "./PersonalFinanceManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PersonalFinanceManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PersonalFinanceManager.API.dll"]
