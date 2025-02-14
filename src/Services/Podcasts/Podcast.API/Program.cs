using System.Text;
using System.Reflection;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Podcast.Infrastructure.Http;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.IdentityModel.Tokens;
using Podcast.Infrastructure.Services.Interfaces;
using Podcast.Infrastructure.Services;
using Podcast.Common;
using Microsoft.AspNetCore.Identity;
using Podcast.API;
using Stripe;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Database and storage related-services
var dbConnectionString = builder.Configuration.GetConnectionString("PodcastDb") ?? throw new InvalidOperationException("Missing connection string configuration");
builder.Services.AddSqlServer<PodcastDbContext>(dbConnectionString, b => b.MigrationsAssembly("Podcast.Infrastructure"));

var queueConnectionString = builder.Configuration.GetConnectionString("FeedQueue") ?? throw new InvalidOperationException("Missing feed queue configuration");

builder.Services.AddSingleton(new QueueClient(queueConnectionString, "feed-queue"));
builder.Services.AddHttpClient<IFeedClient, FeedClient>();
builder.Services.AddHttpClient<ShowClient>();

// Authentication and authorization-related services
// Comment back in if testing authentication
//builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);
//builder.Services.AddAuthorizationBuilder().AddPolicy("modify_feeds", policy => policy.RequireScope("API.Access"));

#region Identity
/*builder.Services.AddDefaultIdentity<User>(config =>
{
    config.SignIn.RequireConfirmedEmail = true;
    config.Tokens.ProviderMap.Add("PodcastEmailConfirmation",
        new TokenProviderDescriptor(
            typeof(PodcastEmailConfirmation<User>)));
    config.Tokens.EmailConfirmationTokenProvider = "PodcastEmailConfirmation";
})
.AddRoles<Role>()
.AddEntityFrameworkStores<PodcastDbContext>()
.AddDefaultTokenProviders();*/

builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.User.RequireUniqueEmail = true;
    options.Tokens.ProviderMap.Add("PodcastEmailConfirmation",
        new TokenProviderDescriptor(
            typeof(PodcastEmailConfirmation<User>)));
    options.Tokens.EmailConfirmationTokenProvider = "PodcastEmailConfirmation";

    //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1d);
    //options.Lockout.MaxFailedAccessAttempts = 5;
})
  .AddEntityFrameworkStores<PodcastDbContext>()
  .AddDefaultTokenProviders();

var jwtTokenConfig = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? throw new InvalidOperationException("Missing jwt configuration");
builder.Services.AddSingleton(jwtTokenConfig);

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
  .AddJwtBearer(options =>
  {
	  options.TokenValidationParameters = new TokenValidationParameters
	  {
		  ValidateIssuer = true,
		  ValidIssuer = jwtTokenConfig.Issuer,
		  ValidAudience = jwtTokenConfig.Audience,
		  ValidateAudience = true,
		  ValidateLifetime = true,
		  ValidateIssuerSigningKey = true,
		  IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtTokenConfig.Secret)),
	  };
  });
#endregion

#region Policy-based authorization
builder.Services.AddAuthorization();
//builder.Services.AddAuthorization(config =>
//{
//    config.AddPolicy(Policies.IsAdmin, Policies.IsAdminPolicy());
//    config.AddPolicy(Policies.IsUser, Policies.IsUserPolicy());
//    config.AddPolicy(Policies.IsUser, Policies.IsReadOnlyPolicy());
//});

#endregion

builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddTransient<ITokenService, Podcast.Infrastructure.Services.TokenService>();
builder.Services.AddTransient<PodcastEmailConfirmation<User>>();

builder.Services.AddTransient<IStripeService, StripeService>();

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
builder.Services.AddSignalR();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// OpenAPI and versioning-related services
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            Description =
                "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
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
                new string[] { }
            }
        }
    );
});
builder.Services.Configure<SwaggerGeneratorOptions>(opts =>
{
    opts.InferSecuritySchemes = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(2, 0);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = new HeaderApiVersionReader("api-version");
});

builder.Services.AddOutputCache();

builder.Services.AddCors(setup =>
{
    setup.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Rate-limiting and output caching-related services
builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("feeds", options =>
{
    options.PermitLimit = 5;
    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    options.QueueLimit = 0;
    options.Window = TimeSpan.FromSeconds(2);
    options.AutoReplenishment = false;
}));

var serviceName = builder.Environment.ApplicationName;
var serviceVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
var serviceResource =
        ResourceBuilder
         .CreateDefault()
         .AddService(serviceName: serviceName, serviceVersion: serviceVersion);

var azureMonitorConnectionString = builder.Configuration.GetConnectionString("AzureMonitor");

var enableMonitor = !string.IsNullOrWhiteSpace(azureMonitorConnectionString);

if (enableMonitor)
{

    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing =>
        tracing.SetResourceBuilder(serviceResource)
        .AddAzureMonitorTraceExporter(o =>
        {
            o.ConnectionString = azureMonitorConnectionString;
        })
        .AddOtlpExporter()
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
    ).WithMetrics(metrics =>
    {
        metrics
        .SetResourceBuilder(serviceResource)
        .AddPrometheusExporter()
        .AddAzureMonitorMetricExporter(o =>
        {
            o.ConnectionString = azureMonitorConnectionString;
        })
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEventCountersInstrumentation(ec =>
        {
            ec.AddEventSources("Microsoft.AspNetCore.Hosting");
        });
    });

    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging
        .SetResourceBuilder(serviceResource)
        .AddAzureMonitorLogExporter(o =>
        {
            o.ConnectionString = azureMonitorConnectionString;
        })
        .AttachLogsToActivityEvent();
    });
}

var app = builder.Build();

await EnsureDbAsync(app.Services);

// Register required middlewares
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", ".NET Podcasts API");
});

app.UseCors();
app.UseRateLimiter();
app.UseOutputCache();
app.UseAuthentication();
app.UseAuthorization();

if (enableMonitor)
    app.MapPrometheusScrapingEndpoint();

app.MapGet("/version", () => serviceVersion);

var versionSet = app.NewApiVersionSet()
                    .HasApiVersion(1.0)
                    .HasApiVersion(2.0)
                    .ReportApiVersions()
                    .Build();

var shows = app.MapGroup("/shows");
var categories = app.MapGroup("/categories");
var episodes = app.MapGroup("/episodes");
var feeds = app.MapGroup("/feeds");
var account = app.MapGroup("/account");
var stripe = app.MapGroup("/stripe");
var files = app.MapGroup("/files");

shows
    .MapShowsApi()
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(1.0)
    .MapToApiVersion(2.0);

categories
    .MapCategoriesApi()
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(1.0)
    .MapToApiVersion(2.0);

episodes
    .MapEpisodesApi()
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(1.0)
    .MapToApiVersion(2.0);

account
    .MapAccountApi()
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(1.0)
    .MapToApiVersion(2.0);

stripe
    .MapStripeApi()
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(1.0)
    .MapToApiVersion(2.0);
files
    .MapFilesApi()
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(1.0)
    .MapToApiVersion(2.0);

var feedIngestionEnabled = app.Configuration.GetValue<bool>("Features:FeedIngestion");

if (feedIngestionEnabled)
{
    feeds
        .MapFeedsApi()
        .WithApiVersionSet(versionSet)
        .MapToApiVersion(2.0)
        .RequireRateLimiting("feeds");
}

app.Run();

static async Task EnsureDbAsync(IServiceProvider sp)
{
    await using var db = sp.CreateScope().ServiceProvider.GetRequiredService<PodcastDbContext>();
    await db.Database.MigrateAsync();
}