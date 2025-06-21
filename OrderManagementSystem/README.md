# Sistema de Gestión de Órdenes - Microservicios

Este proyecto implementa una arquitectura de microservicios utilizando C# con .NET 9, siguiendo los patrones CQRS con MediaTr, comunicación asíncrona con RabbitMQ, optimización de consultas con Redis Cache y almacenamiento en CosmosDB.

## Arquitectura

### Microservicios
- **Orders Service**: Gestión de órdenes y pedidos
- **Vendors Service**: Gestión de vendedores

### Tecnologías Utilizadas
- **.NET 9**: Framework principal
- **CQRS + MediaTr**: Patrón de comandos y consultas
- **RabbitMQ**: Comunicación asíncrona entre microservicios
- **Redis**: Cache distribuido para optimizar consultas
- **CosmosDB**: Base de datos NoSQL para datos no estructurados
- **SQL Server**: Base de datos relacional principal
- **Entity Framework Core**: ORM para acceso a datos
- **Docker**: Containerización de servicios

## Estructura del Proyecto

```
OrderManagementSystem/
├── src/
│   ├── BuildingBlocks/
│   │   ├── Common/           # Entidades base compartidas
│   │   └── EventBus/         # Implementación de RabbitMQ
│   └── Services/
│       ├── Orders/           # Microservicio de Órdenes
│       │   ├── Orders.Api/
│       │   ├── Orders.Application/
│       │   ├── Orders.Domain/
│       │   └── Orders.Infrastructure/
│       └── Vendors/          # Microservicio de Vendedores
│           ├── Vendors.Api/
│           ├── Vendors.Application/
│           ├── Vendors.Domain/
│           └── Vendors.Infrastructure/
└── tests/
```

## Requisitos Previos

- Docker Desktop
- .NET 9 SDK
- Visual Studio 2022 o VS Code

## Ejecución del Proyecto

### Opción 1: Usando Docker Compose (Recomendado)

1. **Clonar el repositorio**
   ```bash
   git clone <repository-url>
   cd OrderManagementSystem
   ```

2. **Ejecutar la infraestructura**
   ```bash
   docker-compose up -d
   ```

3. **Acceder a los servicios**
   - Orders API: http://localhost:7001
   - Vendors API: http://localhost:7002
   - RabbitMQ Management: http://localhost:15672 (admin/admin123)
   - Redis: localhost:6379
   - SQL Server: localhost:1433
   - CosmosDB Emulator: https://localhost:8081

### Opción 2: Ejecución Local

1. **Configurar la infraestructura**
   ```bash
   # SQL Server
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
   
   # Redis
   docker run -p 6379:6379 -d redis:7-alpine
   
   # RabbitMQ
   docker run -e RABBITMQ_DEFAULT_USER=admin -e RABBITMQ_DEFAULT_PASS=admin123 -p 5672:5672 -p 15672:15672 -d rabbitmq:3-management-alpine
   
   # CosmosDB Emulator
   docker run -p 8081:8081 -d mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest
   ```

2. **Ejecutar las migraciones**
   ```bash
   # Orders Service
   cd src/Services/Orders/Orders.Api
   dotnet ef database update
   
   # Vendors Service
   cd ../../Vendors/Vendors.Api
   dotnet ef database update
   ```

3. **Ejecutar los servicios**
   ```bash
   # Orders Service
   cd src/Services/Orders/Orders.Api
   dotnet run
   
   # Vendors Service (en otra terminal)
   cd src/Services/Vendors/Vendors.Api
   dotnet run
   ```

## Endpoints de la API

### Orders Service (http://localhost:7001)

- `GET /api/orders` - Obtener resumen de órdenes
- `GET /api/orders/{id}` - Obtener detalles de una orden
- `POST /api/orders` - Crear nueva orden
- `GET /api/orders/customer/{customerId}` - Órdenes por cliente
- `GET /api/orders/vendor/{vendorId}` - Órdenes por vendedor
- `GET /health` - Health check

### Vendors Service (http://localhost:7002)

- `GET /api/vendors` - Obtener todos los vendedores
- `GET /api/vendors/{id}` - Obtener vendedor por ID
- `POST /api/vendors` - Crear nuevo vendedor
- `GET /health` - Health check

## Patrones Implementados

### CQRS (Command Query Responsibility Segregation)
- **Commands**: Operaciones de escritura (CreateOrderCommand)
- **Queries**: Operaciones de lectura (GetOrderDetailQuery)
- **Handlers**: Procesamiento de comandos y consultas

### Event-Driven Architecture
- **Integration Events**: Eventos de integración entre microservicios
- **Event Bus**: Implementación con RabbitMQ
- **Event Handlers**: Procesamiento de eventos

### Clean Architecture
- **Domain Layer**: Entidades y lógica de negocio
- **Application Layer**: Casos de uso y orquestación
- **Infrastructure Layer**: Acceso a datos y servicios externos
- **API Layer**: Controllers y configuración

## Configuración

### Connection Strings
Los servicios utilizan las siguientes configuraciones:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=OrdersDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true",
    "Redis": "localhost:6379"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "admin",
    "Password": "admin123",
    "ExchangeName": "orders.exchange",
    "QueueName": "orders.queue"
  }
}
```

## Monitoreo y Observabilidad

- **Health Checks**: Endpoints `/health` en cada servicio
- **Logging**: Logs estructurados con Serilog
- **Metrics**: Métricas de rendimiento y disponibilidad

## Desarrollo

### Agregar Nuevos Endpoints
1. Crear Command/Query en la capa Application
2. Implementar Handler correspondiente
3. Agregar endpoint en el Controller
4. Configurar mapeo en AutoMapper si es necesario

### Agregar Nuevos Eventos
1. Crear IntegrationEvent en la capa Application
2. Publicar evento en el Handler correspondiente
3. Implementar EventHandler en el servicio receptor

## Troubleshooting

### Problemas Comunes

1. **Error de conexión a SQL Server**
   - Verificar que el contenedor esté ejecutándose
   - Comprobar credenciales en appsettings.json

2. **Error de conexión a Redis**
   - Verificar que Redis esté ejecutándose en el puerto 6379

3. **Error de conexión a RabbitMQ**
   - Verificar que RabbitMQ esté ejecutándose
   - Comprobar credenciales en la configuración

4. **Error de migración**
   - Ejecutar `dotnet ef database update` en el proyecto correspondiente

## Contribución

1. Fork el proyecto
2. Crear una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abrir un Pull Request

## Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo `LICENSE` para más detalles. 