FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OrderManagementSystem/src/Services/Vendors/Vendors.Api/Vendors.Api.csproj", "OrderManagementSystem/src/Services/Vendors/Vendors.Api/"]
COPY ["OrderManagementSystem/src/Services/Vendors/Vendors.Application/Vendors.Application.csproj", "OrderManagementSystem/src/Services/Vendors/Vendors.Application/"]
COPY ["OrderManagementSystem/src/Services/Vendors/Vendors.Domain/Vendors.Domain.csproj", "OrderManagementSystem/src/Services/Vendors/Vendors.Domain/"]
COPY ["OrderManagementSystem/src/Services/Vendors/Vendors.Infrastructure/Vendors.Infrastructure.csproj", "OrderManagementSystem/src/Services/Vendors/Vendors.Infrastructure/"]
COPY ["OrderManagementSystem/src/BuildingBlocks/Common/Common.csproj", "OrderManagementSystem/src/BuildingBlocks/Common/"]
COPY ["OrderManagementSystem/src/BuildingBlocks/EventBus/EventBus.csproj", "OrderManagementSystem/src/BuildingBlocks/EventBus/"]
RUN dotnet restore "OrderManagementSystem/src/Services/Vendors/Vendors.Api/Vendors.Api.csproj"
COPY . .
WORKDIR "/src/OrderManagementSystem/src/Services/Vendors/Vendors.Api"
RUN dotnet build "./Vendors.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Vendors.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Vendors.Api.dll"] 