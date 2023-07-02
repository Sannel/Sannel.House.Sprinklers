using Iot.Device.Board;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Sannel.House;
using Sannel.House.Sprinklers;
using Sannel.House.Sprinklers.Core;
using Sannel.House.Sprinklers.Core.Hardware;
using Sannel.House.Sprinklers.Core.Schedules;
using Sannel.House.Sprinklers.Core.Zones;
using Sannel.House.Sprinklers.Infrastructure;
using Sannel.House.Sprinklers.Infrastructure.Hardware;
using Sannel.House.Sprinklers.Infrastructure.Options;
using Sannel.House.Sprinklers.Infrastructure.Schedules;
using Sannel.House.Sprinklers.Infrastructure.Zones;
using Sannel.House.Sprinklers.Mappers;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile(Path.Combine("app_config", "appsettings.json"), true, true);
builder.Configuration.AddJsonFile(Path.Combine("app_config", $"appsettings.{builder.Environment.EnvironmentName}.json"), true, true);
if (OperatingSystem.IsLinux())
{
	builder
		.Configuration
		.AddJsonFile(
			Path.Combine(Path.DirectorySeparatorChar.ToString(), "etc", "Sannel", "House", "sprinklers.json"),
			false,
			true
		);
}
builder.Host.UseSystemd();
if (OperatingSystem.IsLinux())
{
	builder.Services.AddDataProtection()
		.PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.DirectorySeparatorChar.ToString(), "var", "lib", "Sannel", "House", "Sprinklers", "data")));
}

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddAuthentication(o =>
{
	o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
	.AddMicrosoftIdentityWebApi(builder.Configuration);

builder.Services.AddAuthorization(o =>
{
	o.AddPolicy(AuthPolicy.ZONE_READERS, p =>
		p.RequireRole(
			Roles.Sprinklers.ZONE_READ,
			Roles.Sprinklers.ZONE_WRITE,
			Roles.ADMIN
		));
	o.AddPolicy(AuthPolicy.ZONE_METADATA_READER, p =>
		p.RequireRole(
			Roles.Sprinklers.ZONE_READ,
			Roles.Sprinklers.ZONE_WRITE,
			Roles.ADMIN
		));
	o.AddPolicy(AuthPolicy.ZONE_TRIGGERS, p =>
		p.RequireRole(
			Roles.Sprinklers.ZONE_TRIGGER,
			Roles.ADMIN
		));
	o.AddPolicy(AuthPolicy.ZONE_METADATA_WRITER, p =>
		p.RequireRole(
			Roles.Sprinklers.ZONE_WRITE,
			Roles.ADMIN
		));
	o.AddPolicy(AuthPolicy.SCHEDULE_READERS, p =>
		p.RequireRole(
			Roles.Sprinklers.SCHEDULE_READ,
			Roles.Sprinklers.SCHEDULE_WRITE,
			Roles.ADMIN
		));
	o.AddPolicy(AuthPolicy.SCHEDULE_SCHEDULERS, p =>
		p.RequireRole(
			Roles.Sprinklers.SCHEDULE_WRITE,
			Roles.ADMIN
		));
});

static bool IsRunningOnRaspberryPi()
{
	const string piPartialProcessorName = "BCM";
	var processorNameFile = "/proc/cpuinfo";

	if (!File.Exists(processorNameFile))
	{
		return false;
	}

	var processorName = File.ReadAllText(processorNameFile);
	return processorName.Contains(piPartialProcessorName);
}

builder.Services.AddApiVersioning(o =>
	{
		o.ReportApiVersions = true;
	})
	.AddMvc()
	.AddApiExplorer(options =>
	{
		// add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
		// note: the specified format code will format the version as "'v'major[.minor][-status]"
		options.GroupNameFormat = "'v'VVV";

		// note: this option is only necessary when versioning by url segment. the SubstitutionFormat
		// can also be used to control the format of the API version in route templates
		options.SubstituteApiVersionInUrl = true;
	});


// Add services to the container.
if (OperatingSystem.IsLinux())
{
	builder.Services.AddDbContext<SprinklerDbContext>(i => i.UseSqlite("Data Source=/var/lib/Sannel/House/Sprinklers/schedule.db"));
}
else
{
	builder.Services.AddDbContext<SprinklerDbContext>(i => i.UseSqlite("Data Source=schedule.db"));
}
builder.Services.AddSingleton<Board>(RaspberryPiBoard.Create());
if (IsRunningOnRaspberryPi())
{
	builder.Services.AddSingleton<ISprinklerHardware, OpenSprinklerHardware>();
}
else
{
	builder.Services.AddSingleton<ISprinklerHardware, FakeHardware>();
}
builder.Services.AddTransient<IClaimsTransformation, UserClaimsTransformation>();
builder.Services.AddSingleton<SprinklerService>();
builder.Services.AddTransient<IScheduleRepository, ScheduleRepository>();
builder.Services.AddTransient<ILoggerRepository, LoggerRepository>();
builder.Services.AddTransient<IZoneRepository, ZoneRepository>();
builder.Services.AddSingleton<HubMessageClient>();
builder.Services.AddSingleton<MQTTMessageClient>();
builder.Services.AddSingleton<IMessageClient>(sp =>
		new MultiMessageClient(
			sp.GetRequiredService<HubMessageClient>(),
			sp.GetRequiredService<MQTTMessageClient>()
		));
builder.Services.AddSingleton<ScheduleMapper>();
builder.Services.AddSingleton<ZoneInfoMapper>();
builder.Services.AddSingleton<CoreMapper>();
builder.Services.AddTransient<IZoneService, ZoneService>();
builder.Services.AddHostedService<SprinklerService>(s => s.GetRequiredService<SprinklerService>());
builder.Services.AddHostedService<ScheduleService>();

builder.Services.AddSignalR();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

using (var scope = app.Services.CreateScope())
{
	var context = scope.ServiceProvider.GetRequiredService<SprinklerDbContext>();
	await context.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
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

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
