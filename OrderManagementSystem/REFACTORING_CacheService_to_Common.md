# Refactorización: CacheService movido a Common

## Resumen

Se ha refactorizado el `CacheService` para eliminar la duplicación de código entre los proyectos `Orders` y `Vendors`, moviendo la implementación al proyecto `Common` para su reutilización.

## Cambios Realizados

### 1. Nuevo Proyecto Common

#### Archivos Creados:
- `src/BuildingBlocks/Common/Interfaces/ICacheService.cs` - Interfaz del servicio de caché
- `src/BuildingBlocks/Common/Services/CacheService.cs` - Implementación del servicio de caché
- `src/BuildingBlocks/Common/Extensions/ServiceCollectionExtensions.cs` - Extensiones para registro de servicios

#### Dependencias Agregadas:
- `Microsoft.Extensions.Configuration.Abstractions`
- `Microsoft.Extensions.Logging.Abstractions`
- `StackExchange.Redis`

### 2. Eliminación de Código Duplicado

#### Archivos Eliminados:
- `src/Services/Vendors/Vendors.Infrastructure/Services/CacheService.cs`
- `src/Services/Vendors/Vendors.Application/Interfaces/ICacheService.cs`
- `src/Services/Orders/Orders.Infrastructure/Services/CacheService.cs`
- `src/Services/Orders/Orders.Application/Interfaces/ICacheService.cs`

#### Dependencias Removidas:
- `Microsoft.Extensions.Caching.StackExchangeRedis` de ambos proyectos Infrastructure

### 3. Actualización de Referencias

#### Vendors.Infrastructure.csproj:
```xml
<ProjectReference Include="..\..\..\BuildingBlocks\Common\Common.csproj" />
```

#### Orders.Infrastructure.csproj:
```xml
<ProjectReference Include="..\..\..\BuildingBlocks\Common\Common.csproj" />
```

### 4. Actualización de Configuración

#### ServiceCollectionExtensions Actualizados:
- **Vendors**: Ahora usa `services.AddRedisCache(configuration)` desde Common
- **Orders**: Ahora usa `services.AddRedisCache(configuration)` desde Common

#### Program.cs Actualizados:
- Removida configuración duplicada de Redis
- Ahora se maneja centralmente desde Common

#### appsettings.json Actualizados:
- **Vendors**: `"InstanceName": "VendorsAPI"`
- **Orders**: `"InstanceName": "OrdersAPI"`

### 5. Actualización de Usos

#### CacheController:
- Actualizado para usar `using Common.Interfaces;`

## Beneficios de la Refactorización

### 1. **Eliminación de Duplicación**
- Un solo lugar para mantener el código del CacheService
- Reducción de código duplicado entre proyectos

### 2. **Consistencia**
- Misma implementación de caché en todos los servicios
- Configuración centralizada y consistente

### 3. **Mantenibilidad**
- Cambios en el CacheService se aplican automáticamente a todos los proyectos
- Un solo punto de actualización para mejoras y correcciones

### 4. **Reutilización**
- Fácil agregar el CacheService a nuevos proyectos
- Patrón consistente para servicios compartidos

### 5. **Separación de Responsabilidades**
- Common contiene solo funcionalidades compartidas
- Cada servicio se enfoca en su dominio específico

## Estructura Final

```
src/
├── BuildingBlocks/
│   └── Common/
│       ├── Interfaces/
│       │   └── ICacheService.cs
│       ├── Services/
│       │   └── CacheService.cs
│       └── Extensions/
│           └── ServiceCollectionExtensions.cs
└── Services/
    ├── Orders/
    │   └── Orders.Infrastructure/
    │       └── Extensions/
    │           └── ServiceCollectionExtensions.cs (actualizado)
    └── Vendors/
        └── Vendors.Infrastructure/
            └── Extensions/
                └── ServiceCollectionExtensions.cs (actualizado)
```

## Uso en Nuevos Proyectos

Para agregar el CacheService a un nuevo proyecto:

1. **Agregar referencia al proyecto Common:**
```xml
<ProjectReference Include="..\..\..\BuildingBlocks\Common\Common.csproj" />
```

2. **Usar la extensión en ServiceCollectionExtensions:**
```csharp
using Common.Extensions;

public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
{
    services.AddRedisCache(configuration);
    // ... otros servicios
    return services;
}
```

3. **Configurar en appsettings.json:**
```json
{
  "ConnectionStrings": {
    "Redis": "redis:6379"
  },
  "Redis": {
    "InstanceName": "MiNuevoAPI"
  }
}
```

4. **Usar en el código:**
```csharp
using Common.Interfaces;

public class MiServicio
{
    private readonly ICacheService _cacheService;
    
    public MiServicio(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }
}
```

## Configuración de Instancias

Cada servicio tiene su propio nombre de instancia para evitar conflictos en Redis:

- **VendorsAPI**: Claves con prefijo `VendorsAPI:`
- **OrdersAPI**: Claves con prefijo `OrdersAPI:`

Esto permite que múltiples servicios compartan la misma instancia de Redis sin conflictos de claves.

## Migración Completa

La refactorización mantiene compatibilidad total con el código existente. No se requieren cambios en:

- Código que usa `ICacheService`
- Configuración de Redis existente
- Health checks
- Funcionalidades de caché

Todos los cambios son internos y transparentes para el código que consume el servicio. 