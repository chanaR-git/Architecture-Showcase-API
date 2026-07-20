using Chinese_sale_api.Configurations;
using Chinese_sale_api.Data;
using Chinese_sale_api.Middlewares;
using Chinese_sale_api.Repositories;
using Chinese_sale_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using projectApiAngular.Repositories;
using Serilog;
using StackExchange.Redis;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

//cors
builder.Services.AddCors(options=>
{
    options.AddPolicy("allowlocalhost",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod();
        });
});

// ����� Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/app-log.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

// ����� Serilog ������ ��Logging �� ASP.NET
builder.Host.UseSerilog();
//***

// Add services to the container.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));

// Configure Rate Limiting
var rateLimitConfig = new RateLimitConfig
{
    MaxRequests = builder.Configuration.GetValue<int>("RateLimit:MaxRequests", 100),
    WindowSeconds = builder.Configuration.GetValue<int>("RateLimit:WindowSeconds", 60),
    ExemptPaths = builder.Configuration.GetSection("RateLimit:ExemptPaths").Get<List<string>>() 
        ?? new List<string>
        {
            "/health",
            "/swagger",
            "/swagger/",
            "/api/auth/login",
            "/api/auth/register"
        }
};
builder.Services.AddSingleton(rateLimitConfig);

JwtSettings? jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
RedisSettings? redisSettings = builder.Configuration.GetSection("RedisSettings").Get<RedisSettings>();
// Register RedisSettings as singleton so it can be injected

if (jwtSettings is null || string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    throw new InvalidOperationException("Missing or invalid JwtSettings in configuration.");
}
if (redisSettings is null || string.IsNullOrWhiteSpace(redisSettings.Host))
{
    throw new InvalidOperationException("Missing or invalid RedisSettings in configuration.");
}

builder.Services.AddSingleton(redisSettings );
// Configure Redis
// builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
//         ConnectionMultiplexer.Connect($"{redisSettings.Host}:{redisSettings.Port},password={redisSettings.Password},abortConnect=false"));
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<Program>>();
    try
    {
        var config = ConfigurationOptions.Parse($"{redisSettings?.Host}:{redisSettings?.Port}");
        config.Password = redisSettings.Password;
        config.AbortOnConnectFail = false; // Don't crash if Redis is down
        config.ConnectTimeout = 5000;
        config.SyncTimeout = 3000;

        var connection = ConnectionMultiplexer.Connect(config);
        logger.LogInformation("Connected to Redis at {Host}:{Port} (Password set: {HasPassword})", redisSettings?.Host, redisSettings?.Port, !string.IsNullOrEmpty(redisSettings?.Password));
        return connection;
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to connect to Redis. Caching will fall back to database.");
        // Return a dummy connection that will fail gracefully
        throw;
    }
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Define the security settings (Security Definition)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Please enter the token only (without the word Bearer)"
    });

    // Apply the definition to all requests (Security Requirement)
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


// Authentication / JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = !string.IsNullOrEmpty(jwtSettings?.Issuer),
        ValidateAudience = !string.IsNullOrEmpty(jwtSettings?.Audience),
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidAudience = jwtSettings?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

//DI
builder.Services.AddScoped<IGiftRepository, GiftRepository>();
builder.Services.AddScoped<IGiftService, GiftService>();
builder.Services.AddScoped<IDonorRepository,DonorRepository>();
builder.Services.AddScoped<IDonorService,DonorService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IPurchasesRepository, PurchasesRepository>();
builder.Services.AddScoped<IPurchasesService, PurchasesService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBasketService, BasketService>();
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<IBasketService, BasketService>();
builder.Services.AddSingleton<ITokenService,TokenService>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddScoped<ILotteryService, LotteryService>();
builder.Services.AddScoped<IZIPService, ZIPService>();
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();
builder.Services.AddHttpContextAccessor();

var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ChineseSaleDbContext>(options =>
    options.UseSqlServer(defaultConn, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(2), null);
    }));

var loggerForDb = builder.Logging;
var tempLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Startup");
tempLogger.LogInformation("Using database connection: {Conn}", defaultConn);


var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RateLimitMiddleware>();
app.UseMiddleware<RequestLog>();
app.UseMiddleware<GiftAlreadyAsignedMiddleware>();
app.UseCors("allowlocalhost");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure static files for Assets folder
var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
if (!Directory.Exists(assetsPath))
{
    Directory.CreateDirectory(assetsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(assetsPath),
    RequestPath = "/assets"
});

app.UseStaticFiles();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        var payload = System.Text.Json.JsonSerializer.Serialize(new { error = "An unexpected error occurred." });
        await context.Response.WriteAsync(payload);
    });
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
