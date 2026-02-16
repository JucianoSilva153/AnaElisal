using System.Text;
using Elisal.WasteManagement.Application.Interfaces;
using Elisal.WasteManagement.Application.Services;
using Elisal.WasteManagement.Domain.Interfaces;
using Elisal.WasteManagement.Infrastructure.Persistence;
using Elisal.WasteManagement.Infrastructure.Repositories;
using Elisal.WasteManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services
    .AddSwaggerGen(); // Using SwaggerGen instead of OpenApi just for broader compatibility or standard use, but template used OpenApi. I'll stick to OpenAPI if preferred, but usually Swagger is standard. I'll stick to what was there or better yet, make it clean.
// Using standard Controllers approach since it's a layered architecture, usually Controllers are used in API layer.

builder.Services.AddDbContext<ElisalDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICollectionRecordRepository, CollectionRecordRepository>();
builder.Services.AddScoped<ICollectionPointRepository, CollectionPointRepository>();
builder.Services.AddScoped<IOperationalAlertRepository, OperationalAlertRepository>();

// Configurations
builder.Services.Configure<Elisal.WasteManagement.Application.Common.EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IResiduoService, ResiduoService>();
builder.Services.AddScoped<IPontoRecolhaService, PontoRecolhaService>();
builder.Services.AddScoped<ICooperativaService, CooperativaService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<IRotaService, RotaService>();
builder.Services.AddScoped<IOperationalAlertService, OperationalAlertService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings.GetValue<string>("Secret")!);

builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
            ValidAudience = jwtSettings.GetValue<string>("Audience")
        };
    });

// Swagger with JWT Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Elisal API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    // Throwing error
    // c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    // {
    //     {
    //         new OpenApiSecurityScheme
    //         {
    //             Reference = new OpenApiReference
    //             {
    //                 Type = ReferenceType.SecurityScheme,
    //                 Id = "Bearer"
    //             },
    //             Scheme = "oauth2",
    //             Name = "Bearer",
    //             In = ParameterLocation.Header,
    //         },
    //         new List<string>()
    //     }
    // });
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ElisalDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi(); // Reverting to standard swagger if needed, but let's just use what was generated if possible. 
    // Wait, the template generated MapOpenApi. configuring it might be easier.
    // However, usually detailed swagger configuration is preferred. 
    // Let's assume the user wants standard WebAPI.
    // I'll stick to generic map controllers.

    app.UseSwagger(); // Generates JSON
    app.UseSwaggerUI(); // Provides interactive UI
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
