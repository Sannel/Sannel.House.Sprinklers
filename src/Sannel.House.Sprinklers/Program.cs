using Iot.Device.Board;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.OpenApi;
using MudBlazor.Services;
using Sannel.House;
using Sannel.House.Sprinklers;
using Sannel.House.Sprinklers.Components;
using Sannel.House.Sprinklers.Features.Common;
using Sannel.House.Sprinklers.Features.Messaging;
using Sannel.House.Sprinklers.Features.Notifications;
using Sannel.House.Sprinklers.Features.Schedules;
using Sannel.House.Sprinklers.Features.Sprinklers;
using Sannel.House.Sprinklers.Mappers;

if (!Directory.Exists("Data"))
{
    Directory.CreateDirectory("Data");
}

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile(Path.Combine("app_config", "appsettings.json"), true, true);
builder.Configuration.AddJsonFile(Path.Combine("app_config", $"appsettings.{builder.Environment.EnvironmentName}.json"), true, true);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("Data"));

builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p =>
    {
        p.AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        var list = builder.Configuration.GetSection("AllowedOrigins").Get<string[]?>() ?? [];
        p.WithOrigins(list);
    });
});

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration);

// JWT Bearer for API controllers and SignalR (external clients)
builder.Services.AddAuthentication()
    .AddMicrosoftIdentityWebApi(builder.Configuration);

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy(AuthPolicy.ZONE_READERS, p =>
        p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
         .RequireRole(Roles.Sprinklers.ZONE_READ, Roles.Sprinklers.ZONE_WRITE, Roles.ADMIN));
    o.AddPolicy(AuthPolicy.ZONE_METADATA_READER, p =>
        p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
         .RequireRole(Roles.Sprinklers.ZONE_READ, Roles.Sprinklers.ZONE_WRITE, Roles.ADMIN));
    o.AddPolicy(AuthPolicy.ZONE_TRIGGERS, p =>
        p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
         .RequireRole(Roles.Sprinklers.ZONE_TRIGGER, Roles.ADMIN));
    o.AddPolicy(AuthPolicy.ZONE_METADATA_WRITER, p =>
        p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
         .RequireRole(Roles.Sprinklers.ZONE_WRITE, Roles.ADMIN));
    o.AddPolicy(AuthPolicy.SCHEDULE_READERS, p =>
        p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
         .RequireRole(Roles.Sprinklers.SCHEDULE_READ, Roles.Sprinklers.SCHEDULE_WRITE, Roles.ADMIN));
    o.AddPolicy(AuthPolicy.SCHEDULE_SCHEDULERS, p =>
        p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
         .RequireRole(Roles.Sprinklers.SCHEDULE_WRITE, Roles.ADMIN));
});

static bool IsRunningOnRaspberryPi() => File.Exists("/dev/gpiomem");

builder.Services.AddApiVersioning(o => { o.ReportApiVersions = true; })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddDbContext<SprinklerDbContext>(i => i.UseSqlite("Data Source=Data/schedule.db"));

if (IsRunningOnRaspberryPi())
{
    builder.Services.AddSingleton<Board>(RaspberryPiBoard.Create());
    builder.Services.AddSingleton<ISprinklerHardware, OpenSprinklerHardware>();
}
else
{
    builder.Services.AddSingleton<ISprinklerHardware, FakeHardware>();
}

builder.Services.AddTransient<IClaimsTransformation, UserClaimsTransformation>();

// Internal mediator — scans assembly for all IRequestHandler<,> and IRequestHandler<> implementations
var assembly = typeof(Program).Assembly;
foreach (var type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
{
    foreach (var iface in type.GetInterfaces())
    {
        if (iface.IsGenericType)
        {
            var def = iface.GetGenericTypeDefinition();
            if (def == typeof(IRequestHandler<,>) || def == typeof(IRequestHandler<>))
                builder.Services.AddScoped(iface, type);
        }
    }
}
builder.Services.AddScoped<IMediator, Mediator>();

// Workers
builder.Services.AddSingleton<SprinklerWorker>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<SprinklerWorker>());
builder.Services.AddHostedService<ScheduleWorker>();

// Messaging
builder.Services.AddSingleton<HubMessageClient>();
builder.Services.AddSingleton<MQTTMessageClient>();
builder.Services.AddSingleton<IMessageClient>(sp =>
    new MultiMessageClient(
        sp.GetRequiredService<HubMessageClient>(),
        sp.GetRequiredService<MQTTMessageClient>()
    ));

// Mappers (Mapperly source-generated, used by handlers)
builder.Services.AddSingleton<ScheduleMapper>();
builder.Services.AddSingleton<ZoneInfoMapper>();

builder.Services.AddSignalR();
builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.UseAllOfToExtendReferenceSchemas();
    o.DocumentFilter<DocumentSecurityFilter>();
    o.OperationFilter<SecurityFilter>();
    o.MapType<TimeSpan>(() => new OpenApiSchema { Type = JsonSchemaType.String, Format = "00:00:00" });
    o.MapType<TimeSpan?>(() => new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Format = "00:00:00" });
});

builder.Services.Configure<MqttOptions>(builder.Configuration.GetSection("MQTT"));
builder.Services.AddSingleton<MQTTManager>();

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});

using var app = builder.Build();

// Must be first: tells ASP.NET Core to trust X-Forwarded-Proto/For from the K8s ingress
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Security headers on every response
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), microphone=(), payment=(), usb=()");
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "connect-src 'self' wss:; " +
        "img-src 'self' data: blob:; " +
        "font-src 'self' data:; " +
        "object-src 'none'; " +
        "form-action 'self'; " +
        "frame-ancestors 'self'");
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseCors();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SprinklerDbContext>();
    await context.Database.MigrateAsync();
}

app.UseSwagger(o =>
{
    o.RouteTemplate = "sprinkler/swagger/{documentName}/swagger.{json|yaml}";
});
app.UseSwaggerUI(o =>
{
    o.RoutePrefix = "sprinkler/swagger";
    o.OAuthAppName("AzureAd");
    o.OAuthClientId(builder.Configuration["AzureAd:ClientId"]);
    o.OAuthScopeSeparator(" ");
});

app.MapHub<MessageHub>("/sprinkler/hub");
await app.Services.GetRequiredService<MQTTManager>().StartAsync();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

await app.RunAsync();
