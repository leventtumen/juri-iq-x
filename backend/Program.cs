using Microsoft.EntityFrameworkCore;
using JurisIQ.Backend.Data;
using JurisIQ.Backend.Configuration;
using JurisIQ.Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
// using Python.Runtime;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Database
builder.Services.AddDbContext<JurisIQDbContext>(options =>
    options.UseSqlite(appSettings.Database.ConnectionString));

// Services
builder.Services.AddSingleton(appSettings.Jwt);
builder.Services.AddSingleton(appSettings.Security);
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDocumentProcessingService>( provider => 
{
    var context = provider.GetRequiredService<JurisIQDbContext>();
    return new DocumentProcessingService(
        context, 
        appSettings.DocumentProcessing.SampleDocumentsPath,
        appSettings.DocumentProcessing.SupportedExtensions
    );
});

// Authentication
var jwtSettings = appSettings.Jwt;
var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5500", "http://127.0.0.1:5500")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Controllers and API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "JurisIQ API", 
        Version = "v1",
        Description = "AI-powered legal document search and analysis API"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: &quot;Authorization: Bearer {token}&quot;",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Initialize Python runtime
try
{
    // PythonEngine.Initialize();
    Console.WriteLine("Python runtime initialization skipped");
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Failed to initialize Python runtime: {ex.Message}");
}

var app = builder.Build();

// Database initialization
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<JurisIQDbContext>();
    
    // Ensure database is created
    context.Database.EnsureCreated();
    
    Console.WriteLine("Database created successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"Database initialization failed: {ex.Message}");
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "JurisIQ API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger at root
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Custom middleware
app.UseMiddleware<JurisIQ.Backend.Configuration.JwtMiddleware>();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

// Background service for daily document processing
var cts = new CancellationTokenSource();
var token = cts.Token;

Task.Run(async () =>
{
    while (!token.IsCancellationRequested)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var documentService = scope.ServiceProvider.GetRequiredService<IDocumentProcessingService>();
            
            await documentService.ProcessAllDocumentsAsync();
            Console.WriteLine($"Document processing completed at {DateTime.UtcNow}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in background document processing: {ex.Message}");
        }

        // Wait 24 hours before next run
        await Task.Delay(TimeSpan.FromHours(24), token);
    }
}, token);

// Ensure sample documents directory exists
var sampleDocsPath = appSettings.DocumentProcessing.SampleDocumentsPath;
if (!Directory.Exists(sampleDocsPath))
{
    Directory.CreateDirectory(sampleDocsPath);
    Console.WriteLine($"Created sample documents directory: {sampleDocsPath}");
}

try
{
    app.Run();
}
finally
{
    cts.Cancel();
    try
    {
        // PythonEngine.Shutdown();
        Console.WriteLine("Python runtime shutdown skipped");
    }
    catch
    {
        // Ignore shutdown errors
    }
}