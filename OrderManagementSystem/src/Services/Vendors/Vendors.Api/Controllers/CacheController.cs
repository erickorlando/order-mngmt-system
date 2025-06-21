using Microsoft.AspNetCore.Mvc;
using Common.Interfaces;
using Vendors.Application.DTOs;

namespace Vendors.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CacheController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheController> _logger;

    public CacheController(ICacheService cacheService, ILogger<CacheController> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    [HttpPost("set")]
    public async Task<IActionResult> SetValue([FromBody] CacheSetRequest request)
    {
        try
        {
            await _cacheService.SetAsync(request.Key, request.Value, request.Expiration);
            return Ok(new { message = "Valor almacenado en caché exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al almacenar valor en caché");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    [HttpGet("get/{key}")]
    public async Task<IActionResult> GetValue(string key)
    {
        try
        {
            var value = await _cacheService.GetAsync<object>(key);
            if (value == null)
            {
                return NotFound(new { message = "Clave no encontrada en caché" });
            }

            return Ok(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener valor del caché");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    [HttpDelete("remove/{key}")]
    public async Task<IActionResult> RemoveValue(string key)
    {
        try
        {
            await _cacheService.RemoveAsync(key);
            return Ok(new { message = "Clave eliminada del caché exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar clave del caché");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    [HttpGet("exists/{key}")]
    public async Task<IActionResult> KeyExists(string key)
    {
        try
        {
            var exists = await _cacheService.ExistsAsync(key);
            return Ok(new { key, exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar existencia de clave");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    [HttpGet("ttl/{key}")]
    public async Task<IActionResult> GetTimeToLive(string key)
    {
        try
        {
            var ttl = await _cacheService.GetTimeToLiveAsync(key);
            return Ok(new { key, ttl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener TTL de clave");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    [HttpPost("hash/set")]
    public async Task<IActionResult> SetHashValue([FromBody] HashSetRequest request)
    {
        try
        {
            await _cacheService.SetHashAsync(request.Key, request.Field, request.Value, request.Expiration);
            return Ok(new { message = "Campo hash almacenado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al almacenar campo hash");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    [HttpGet("hash/get/{key}/{field}")]
    public async Task<IActionResult> GetHashValue(string key, string field)
    {
        try
        {
            var value = await _cacheService.GetHashAsync<object>(key, field);
            if (value == null)
            {
                return NotFound(new { message = "Campo hash no encontrado" });
            }

            return Ok(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener campo hash");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    [HttpDelete("hash/remove/{key}/{field}")]
    public async Task<IActionResult> RemoveHashValue(string key, string field)
    {
        try
        {
            await _cacheService.RemoveHashAsync(key, field);
            return Ok(new { message = "Campo hash eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar campo hash");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    [HttpPost("flush")]
    public async Task<IActionResult> FlushDatabase()
    {
        try
        {
            await _cacheService.FlushDatabaseAsync();
            return Ok(new { message = "Base de datos Redis limpiada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar base de datos Redis");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDatabaseStats()
    {
        try
        {
            var size = await _cacheService.GetDatabaseSizeAsync();
            return Ok(new { databaseSize = size });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de la base de datos");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }
}

public class CacheSetRequest
{
    public string Key { get; set; } = string.Empty;
    public object Value { get; set; } = new();
    public TimeSpan? Expiration { get; set; }
}

public class HashSetRequest
{
    public string Key { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public object Value { get; set; } = new();
    public TimeSpan? Expiration { get; set; }
} 