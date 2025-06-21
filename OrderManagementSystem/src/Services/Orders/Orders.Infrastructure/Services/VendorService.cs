using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orders.Application.Interfaces;

namespace Orders.Infrastructure.Services;

public class VendorService : IVendorService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VendorService> _logger;
    private readonly string _vendorsApiUrl;

    public VendorService(HttpClient httpClient, IConfiguration configuration, ILogger<VendorService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _vendorsApiUrl = configuration["VendorsApi:BaseUrl"] ?? "https://localhost:7002";
    }

    public async Task<VendorInfo?> GetVendorByIdAsync(Guid vendorId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_vendorsApiUrl}/api/vendors/{vendorId}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var vendorDto = JsonSerializer.Deserialize<VendorDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (vendorDto != null)
                {
                    return new VendorInfo
                    {
                        Id = vendorDto.Id,
                        Name = vendorDto.Name,
                        Email = vendorDto.Email,
                        IsActive = vendorDto.IsActive
                    };
                }
            }
            else
            {
                _logger.LogWarning("Failed to get vendor {VendorId}. Status: {StatusCode}", vendorId, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vendor {VendorId}", vendorId);
        }

        return null;
    }

    private record VendorDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public bool IsActive { get; init; }
    }
} 