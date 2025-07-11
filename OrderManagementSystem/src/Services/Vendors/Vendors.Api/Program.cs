using Vendors.Infrastructure.Extensions;
using Vendors.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Vendors.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Vendors API", 
        Version = "v1",
        Description = "API para gestión de vendedores con CQRS y Clean Architecture"
    });
    
    // Enable XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Database
builder.Services.AddDbContext<VendorsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Application Services
builder.Services.AddApplicationServices();

// Infrastructure Services (incluye Redis Cache desde Common)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<VendorsDbContext>()
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

// Add RabbitMQ Health Check only if RabbitMQ configuration exists
var rabbitMQSection = builder.Configuration.GetSection("RabbitMQ");
if (rabbitMQSection.Exists())
{
    var rabbitMQHost = rabbitMQSection["HostName"];
    var rabbitMQPort = rabbitMQSection["Port"];
    var rabbitMQUser = rabbitMQSection["UserName"];
    var rabbitMQPassword = rabbitMQSection["Password"];
    var rabbitMQVHost = rabbitMQSection["VirtualHost"];
    
    if (!string.IsNullOrEmpty(rabbitMQHost))
    {
        var rabbitMQConnectionString = $"amqp://{rabbitMQUser}:{rabbitMQPassword}@{rabbitMQHost}:{rabbitMQPort}{rabbitMQVHost}";
        builder.Services.AddHealthChecks().AddRabbitMQ(rabbitMQConnectionString, name: "rabbitmq");
    }
}

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vendors API V1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Health checks endpoint
app.MapHealthChecks("/health");

app.MapControllers();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VendorsDbContext>();
    context.Database.Migrate();
}

app.Run();
