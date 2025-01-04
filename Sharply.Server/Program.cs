using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sharply.Server.AutoMapper;
using Sharply.Server.Data;
using Sharply.Server.Interfaces;
using Sharply.Server.Services;
using Sharply.Server.SignalR;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration
ConfigureAppSettings(builder);
ConfigureUrls(builder);

// Services
ConfigureServices(builder.Services, builder.Configuration);

// Build the application
var app = builder.Build();

// Middleware
ConfigureMiddleware(app);

// Run the application
app.Run();

#region Configuration Methods

void ConfigureAppSettings(WebApplicationBuilder builder)
{
    DotNetEnv.Env.Load();
    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();
}

void ConfigureUrls(WebApplicationBuilder builder)
{
    // Define application URLs
    var serverUri = builder.Configuration["ServerSettings:ServerUri"];
    if (string.IsNullOrEmpty(serverUri))
        serverUri = "http://localhost:9000";

    builder.WebHost.UseUrls(serverUri);
}

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Database
    services.AddDbContext<SharplyDbContext>(options =>
        options.UseSqlite(configuration.GetConnectionString("DefaultConnection"), options =>
		{
			options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
		}));

    // OpenAPI (Swagger)
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    // Authentication
    var jwtKey = configuration["Jwt:Key"] ?? "DEFAULT";
    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey
        };
    });

    // Controllers
    services.AddControllers();

    // AutoMapper
    services.AddAutoMapper(typeof(MappingProfile));

    // SignalR
    services.AddSignalR();

    // Custom services
    services.AddSingleton(typeof(ISharplyContextFactory<>), typeof(SharplyContextFactory<>));
    services.AddSingleton<IUserService, UserService>();
    services.AddSingleton<IServerService, ServerService>();
    services.AddSingleton<IChannelService, ChannelService>();
    services.AddSingleton<IUserTrackerService, UserTrackerService>();
	services.AddSingleton<IMessageService, MessageService>();
}

void ConfigureMiddleware(WebApplication app)
{
    // Swagger for API documentation
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapOpenApi();
    }

    // Middleware pipeline
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Map SignalR hubs
    app.MapHub<MessageHub>("/hubs/messages");
    app.MapHub<UserHub>("/hubs/users");
}

#endregion

