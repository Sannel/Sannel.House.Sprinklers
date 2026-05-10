using Iot.Device.Board;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Sannel.House;
using Sannel.House.Sprinklers;
using Sannel.House.Sprinklers.Features.Common;
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

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddMicrosoftIdentityWebApi(builder.Configuration);

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy(AuthPolicy.ZONE_READERS, p =>
        p.RequireRole(Roles.Sprinklers.ZONE_READ, Roles.Sprinklers.ZONE_WRITE, Roles.ADMIN));
    o.AddPolicy(AuthPolicy.ZONE_METADATA_READER, p =>
        p.RequireRole(Roles.Sprinklers.ZONE_READ, Roles.Sprinklers.ZONE_WRITE, Roles.ADMIN));
    o.AddPolicy(AuthPolicy.ZONE_TRIGGERS, p =>
        p.RequireRole(Roles.Sprinklers.ZONE_TRIGGER, Roles.ADMIN));
    o.AddPolicy(AuthPolicy.ZONE_METADATA_WRITER, p =>
        p.RequireRole(Roles.Sprinklers.ZONE_WRITE, Roles.ADMIN));
    o.AddPolicy(AuthPolicy.SCHEDULE_READERS, p =>
        p.RequireRole(Roles.Sprinklers.SCHEDULE_READ, Roles.Sprinklers.SCHEDULE_WRITE, Roles.ADMIN));
    o.AddPolicy(AuthPolicy.SCHEDULE_SCHEDULERS, p =>
        p.RequireRole(Roles.Sprinklers.SCHEDULE_WRITE, Roles.ADMIN));
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

// MediatR — scans the API assembly for all handlers
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.UseAllOfToExtendReferenceSchemas();
    o.DocumentFilter<DocumentSecurityFilter>();
    o.OperationFilter<SecurityFilter>();
    o.MapType<TimeSpan>(() => new OpenApiSchema { Type = "string", Format = "00:00:00", Reference = null, Nullable = false });
    o.MapType<TimeSpan?>(() => new OpenApiSchema { Type = "string", Format = "00:00:00", Reference = null, Nullable = true });
});

builder.Services.Configure<MqttOptions>(builder.Configuration.GetSection("MQTT"));
builder.Services.AddSingleton<MQTTManager>();

using var app = builder.Build();

app.UseDeveloperExceptionPage();
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

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
