# Mejoras del CacheService - Uso Directo de Redis

## Resumen de Cambios

Se ha mejorado el `CacheService` para utilizar Redis directamente en lugar de la abstracción genérica `IDistributedCache`. Esto proporciona mayor control y funcionalidades específicas de Redis.

## Cambios Principales

### 1. Dependencias Actualizadas
- **Antes**: `IDistributedCache` (abstracción genérica)
- **Ahora**: `IConnectionMultiplexer` de StackExchange.Redis (Redis directo)

### 2. Nuevas Funcionalidades Agregadas

#### Métodos Básicos Mejorados
- `GetAsync<T>()` - Con logging detallado y mejor manejo de errores
- `SetAsync<T>()` - Con configuración de TTL y logging
- `RemoveAsync()` - Con confirmación de eliminación

#### Métodos Específicos de Redis
- `ExistsAsync()` - Verificar si una clave existe
- `GetTimeToLiveAsync()` - Obtener el tiempo restante de vida de una clave

#### Operaciones de Hash
- `SetHashAsync()` - Almacenar campos en estructuras hash
- `GetHashAsync()` - Obtener campos de estructuras hash
- `RemoveHashAsync()` - Eliminar campos de estructuras hash

#### Métodos de Administración
- `FlushDatabaseAsync()` - Limpiar toda la base de datos Redis
- `GetDatabaseSizeAsync()` - Obtener el número de claves en la base de datos

### 3. Mejoras en la Configuración

#### ServiceCollectionExtensions
```csharp
// Configuración robusta de Redis con reintentos
services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var options = ConfigurationOptions.Parse(redisConnectionString);
    options.AbortConnect = false; // No fallar si Redis no está disponible
    options.ConnectRetry = 3;
    options.ReconnectRetryPolicy = new ExponentialRetry(5000);
    return ConnectionMultiplexer.Connect(options);
});
```

#### Configuración en appsettings.json
```json
{
  "Redis": {
    "InstanceName": "VendorsAPI"
  }
}
```

### 4. Características de Seguridad y Robustez

#### Manejo de Errores
- Try-catch en todas las operaciones
- Logging detallado de errores y operaciones
- Fallback graceful cuando Redis no está disponible

#### Nomenclatura de Claves
- Prefijo de instancia para evitar conflictos: `VendorsAPI:clave`
- Organización jerárquica de claves

#### Configuración de Conexión
- Reintentos automáticos de conexión
- Política de reconexión exponencial
- No bloqueo del inicio de la aplicación si Redis no está disponible

## Nuevo Controlador de Caché

Se ha creado `CacheController` con endpoints para:
- Operaciones básicas (GET, SET, DELETE)
- Verificación de existencia de claves
- Consulta de TTL
- Operaciones de hash
- Administración de la base de datos

### Endpoints Disponibles

```
POST   /api/cache/set              - Almacenar valor
GET    /api/cache/get/{key}        - Obtener valor
DELETE /api/cache/remove/{key}     - Eliminar clave
GET    /api/cache/exists/{key}     - Verificar existencia
GET    /api/cache/ttl/{key}        - Obtener TTL
POST   /api/cache/hash/set         - Almacenar campo hash
GET    /api/cache/hash/get/{key}/{field} - Obtener campo hash
DELETE /api/cache/hash/remove/{key}/{field} - Eliminar campo hash
POST   /api/cache/flush            - Limpiar base de datos
GET    /api/cache/stats            - Estadísticas de la base de datos
```

## Ventajas de los Cambios

1. **Mayor Control**: Acceso directo a todas las funcionalidades de Redis
2. **Mejor Rendimiento**: Sin capas de abstracción adicionales
3. **Funcionalidades Avanzadas**: Soporte para hashes, TTL, estadísticas
4. **Mejor Observabilidad**: Logging detallado de todas las operaciones
5. **Robustez**: Manejo robusto de errores y reconexiones
6. **Escalabilidad**: Configuración optimizada para entornos de producción

## Ejemplo de Uso

```csharp
// Almacenar un vendor en caché
var vendor = new VendorDto { Id = 1, Name = "Vendor 1" };
await _cacheService.SetAsync($"vendor:{vendor.Id}", vendor, TimeSpan.FromHours(1));

// Almacenar múltiples campos de un vendor en hash
await _cacheService.SetHashAsync("vendor:1", "name", vendor.Name);
await _cacheService.SetHashAsync("vendor:1", "email", vendor.Email);

// Verificar si existe
var exists = await _cacheService.ExistsAsync("vendor:1");

// Obtener TTL
var ttl = await _cacheService.GetTimeToLiveAsync("vendor:1");
```

## Migración

La interfaz `ICacheService` mantiene compatibilidad con los métodos básicos existentes, por lo que no se requieren cambios en el código que ya usa el servicio. Los nuevos métodos están disponibles como funcionalidades adicionales. 