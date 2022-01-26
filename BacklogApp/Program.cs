using BacklogApp.Managers;
using BacklogApp.Models.Options;
using BacklogApp.Repository;
using BacklogApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var logFactory = LoggerFactory.Create(logs => logs.AddConsole());
ILogger<Program> logger = logFactory.CreateLogger<Program>();

bool disableSpa = builder.Configuration.GetValue<bool>("DISABLE_SPA");
string corsDomain = builder.Configuration.GetValue<string>("CORS_DOMAIN");
string corsOrigin = string.IsNullOrEmpty(corsDomain) ? "http://localhost:8080" : corsDomain;

logger.LogInformation("DISABLE_SPA: {disableSpa}", disableSpa);
logger.LogInformation("CORS_DOMAIN: {corsDomain}", corsDomain);
logger.LogInformation("Current CORS origin: {corsOrigin}", corsOrigin);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

MapCheckOnlineRoute(app);
Configure(app, app.Environment);

app.Run();


#region ConfigureServices

void ConfigureServices(IServiceCollection services, ConfigurationManager configuration)
{
    ConfigureOptions(services, configuration);

    ConfigureDatabase(services, configuration);

    ConfigureCORS(services, configuration);

    ConfigureMainServices(services, configuration);
    ConfigureManagers(services);

    services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}

void ConfigureOptions(IServiceCollection services, ConfigurationManager configuration)
{
    services.Configure<PasswordOptions>(configuration.GetSection(nameof(PasswordOptions)));
    services.Configure<PasswordGeneratorOptions>(configuration.GetSection(nameof(PasswordGeneratorOptions)));
    services.Configure<EmailOptions>(configuration.GetSection(nameof(EmailOptions)));
    services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));
    services.Configure<RefreshTokenOptions>(configuration.GetSection(nameof(RefreshTokenOptions)));
}

void ConfigureDatabase(IServiceCollection services, ConfigurationManager configuration)
{
    string mongoConnectionString = configuration.GetConnectionString("MongoDB");
    var mongoConnection = new MongoUrlBuilder(mongoConnectionString);
    string mongoDatabaseName = mongoConnection.DatabaseName;

    var conventionPack = new ConventionPack
    {
        new CamelCaseElementNameConvention()
    };
    ConventionRegistry.Register("camelCase", conventionPack, t => true);

    services.AddTransient<IMongoDatabase>(_ =>
    {
        var mongoClient = new MongoClient(mongoConnectionString);
        IMongoDatabase db = mongoClient.GetDatabase(mongoDatabaseName);
        return db;
    });
    services.AddTransient<IGridFSBucket, GridFSBucket>();

    services.AddTransient<IUsersRepository, UsersRepository>();
    services.AddTransient<IProjectRepository, ProjectRepository>();
    services.AddTransient<ITaskRepository, TaskRepository>();
    services.AddTransient<IResourceRepository, ResourcesRepository>();
}

void ConfigureCORS(IServiceCollection services, ConfigurationManager configuration)
{
    services.AddCors(setup =>
        setup.AddDefaultPolicy(builder =>
            builder.WithOrigins(corsOrigin)
                .AllowCredentials()    
                .AllowAnyHeader()
                .AllowAnyMethod()
        ));
}

void ConfigureMainServices(IServiceCollection services, ConfigurationManager configuration)
{
    services.AddTransient<IPasswordHasher, PasswordHasher>();
    services.AddTransient<IPasswordGenerator, PasswordGenerator>();
    services.AddTransient<IEmailService, EmailService>();
    services.AddTransient<IDateTimeProvider, DateTimeProvider>();

    if(!disableSpa) {
        services.AddSpaStaticFiles(cfg => cfg.RootPath = "../clientapp/dist" );
    }

    //auth
    services.AddTransient<IJwtTokenFactory, JwtTokenFactory>();
    services.AddAuthorization();
    string issuer = configuration[$"{nameof(JwtOptions)}:{nameof(JwtOptions.Issuer)}"];
    string audience = configuration[$"{nameof(JwtOptions)}:{nameof(JwtOptions.Audience)}"];
    SymmetricSecurityKey signingKey = new(Encoding.UTF8.GetBytes(configuration[$"{nameof(JwtOptions)}:{nameof(JwtOptions.Key)}"]));
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opts =>
            opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = configuration[$"{nameof(JwtOptions)}:{nameof(JwtOptions.Audience)}"],
                IssuerSigningKey = signingKey
            });
}

void ConfigureManagers(IServiceCollection services)
{
    services.AddTransient<UsersManager>();
    services.AddTransient<ProjectsManager>();
    services.AddTransient<TasksManager>();
    services.AddTransient<ResourcesManager>();
}

#endregion


void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();

        app.UseSwagger();
        app.UseSwaggerUI();
    }

    if (!disableSpa)
    {
        app.UseSpaStaticFiles();
    }

    app.UseCors();

    app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });

    app.UseAuthentication();
    app.UseRouting();
    app.UseAuthorization();
    
    app.UseEndpoints(endpoints => endpoints.MapControllers());

    if (!disableSpa)
    {
        app.UseSpa(spa => {
            if (env.IsDevelopment())
            {
                Console.WriteLine("Using proxy to SPA dev server: http://localhost:8080");
                spa.UseProxyToSpaDevelopmentServer("http://localhost:8080");
            }
        });
    }
}


void MapCheckOnlineRoute(IEndpointRouteBuilder app)
{
    app.MapGet("/online", () => Results.Ok());
}

// Make the implicit Program class public so test projects can access it
//public partial class Program { }
// Set up InternalsVisibleTo in BacklogApp.csproj