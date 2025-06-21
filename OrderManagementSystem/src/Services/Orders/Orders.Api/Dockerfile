FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OrderManagementSystem/src/Services/Orders/Orders.Api/Orders.Api.csproj", "OrderManagementSystem/src/Services/Orders/Orders.Api/"]
COPY ["OrderManagementSystem/src/Services/Orders/Orders.Application/Orders.Application.csproj", "OrderManagementSystem/src/Services/Orders/Orders.Application/"]
COPY ["OrderManagementSystem/src/Services/Orders/Orders.Domain/Orders.Domain.csproj", "OrderManagementSystem/src/Services/Orders/Orders.Domain/"]
COPY ["OrderManagementSystem/src/BuildingBlocks/Common/Common.csproj", "OrderManagementSystem/src/BuildingBlocks/Common/"]
RUN dotnet restore "OrderManagementSystem/src/Services/Orders/Orders.Api/Orders.Api.csproj"
COPY . .
WORKDIR "/src/OrderManagementSystem/src/Services/Orders/Orders.Api"
RUN dotnet build "./Orders.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Orders.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orders.Api.dll"]
