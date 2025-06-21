FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Vendors/Vendors.Api/Vendors.Api.csproj", "src/Services/Vendors/Vendors.Api/"]
COPY ["src/Services/Vendors/Vendors.Application/Vendors.Application.csproj", "src/Services/Vendors/Vendors.Application/"]
COPY ["src/Services/Vendors/Vendors.Domain/Vendors.Domain.csproj", "src/Services/Vendors/Vendors.Domain/"]
COPY ["src/Services/Vendors/Vendors.Infrastructure/Vendors.Infrastructure.csproj", "src/Services/Vendors/Vendors.Infrastructure/"]
COPY ["src/BuildingBlocks/Common/Common.csproj", "src/BuildingBlocks/Common/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.csproj", "src/BuildingBlocks/EventBus/"]
RUN dotnet restore "src/Services/Vendors/Vendors.Api/Vendors.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Vendors/Vendors.Api"
RUN dotnet build "./Vendors.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Vendors.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Vendors.Api.dll"] 